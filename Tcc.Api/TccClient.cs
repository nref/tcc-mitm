using System.Text;
using System.Web;

namespace Tcc.Api;

public interface ITccClient
{
  Task<int?> GetCoolSetpointAsync();
  Task<bool> SetCoolSetpointAsync(int coolSetpoint);
  Task<bool> SetFanAsync(bool on);
  Task<bool> ScheduleFanAsync(int minutes);
}

public class TccClient : ITccClient
{
  private readonly IFileRepo _repo;
  private readonly string _username;
  private readonly string _password;

  private readonly HttpClient _client;
  private CancellationTokenSource fanCts_ = new();

  private string _sessionId = "";
  private string _thermostatId = "";

  private bool _LoggedIn => !string.IsNullOrEmpty(_sessionId);

  public TccClient(IFileRepo repo, string username, string password)
  {
    _repo = repo;
    _username = username;
    _password = password;

    _client = new HttpClient();

    Task.Run(async () =>
    {
      _sessionId = await _repo.LoadSessionIdAsync();
      _thermostatId = await GetThermostatIdAsync();
    });
  }

  public async Task<int?> GetCoolSetpointAsync()
  {
    // Notes on the response:
    // Equipment status values: Cooling, Heating, Off
    // GetVolatileThermostatData vs GetThermostat: GetThermostat contains everything
    // from GetVolatileThermostatData plus fan and device-specific info

    (bool ok, string result, string response) = await PostAsync("https://tccna.resideo.com/ws/MobileV2.asmx/GetVolatileThermostatData", () => new Dictionary<string, string>
        {
            { "SessionID", _sessionId },
            { "ThermostatID", _thermostatId },
        });

    if (!Xml.TryGetNodeValue(response, "CoolSetpoint", out string value))
    {
      return null;
    }

    if (!double.TryParse(value, out double setpoint))
    {
      return null;
    }

    return (int)setpoint;
  }

  public async Task<bool> SetCoolSetpointAsync(int coolSetpoint) => (await PostAsync("https://tccna.resideo.com/ws/MobileV2.asmx/ChangeThermostatUI",
      () => new Dictionary<string, string>()
      {
            { "SessionID", _sessionId },
            { "ThermostatID", _thermostatId },
            { "ChangeSystemSwitch", "true" },
            { "SystemSwitch", "5" },
            { "StatusCool", "1" },
            { "StatusHeat", "1" },
            { "ChangeStatusCool", "true" },
            { "ChangeStatusHeat", "true" },
            { "ChangeCoolSetpoint", "true" },
            { "ChangeHeatSetpoint", "true" },
            { "ChangeCoolNextPeriod", "true" },
            { "ChangeHeatNextPeriod", "true" },
            { "CoolNextPeriod", "52" },
            { "HeatNextPeriod", "52" },
            { "HeatSetpoint", "69" },
            { "CoolSetpoint", $"{coolSetpoint}" },
      })).Item1;

  public Task<bool> SetFanAsync(bool on)
  {
    Log.Info($"{nameof(SetFanAsync)}({on})");
    return Task.FromResult(true);
  }

  public async Task<bool> ScheduleFanAsync(int minutes)
  {
    Log.Info($"{nameof(ScheduleFanAsync)}({minutes})");

    fanCts_.Cancel();
    fanCts_ = new();

    await SetFanAsync(on: true);

    try
    {
      await Task
        .Delay(TimeSpan.FromMinutes(minutes), fanCts_.Token)
        .ContinueWith(async _ =>
        {
          await SetFanAsync(on: false);
        }, fanCts_.Token);
    }
    catch (TaskCanceledException)
    {
    }

    return true;
  }

  private async Task<(bool, string, string)> PostAsync(string url, Func<Dictionary<string, string>> values)
      => await PostAsync(url, () => new FormUrlEncodedContent(values()));

  private async Task<(bool, string, string)> PostAsync(string url, Func<HttpContent> content)
  {
    if (!_LoggedIn)
    {
      await LogIn();
    }

    (bool ok, string result, string response) = await DoPostCheckResultAsync(url, content());

    if (result == "InvalidSessionID")
    {
      Log.Error($"{nameof(PostAsync)}: Invalid session ID");

      // We weren't actually logged in. Log in and try the request again.
      await LogIn();

      (ok, result, response) = await DoPostCheckResultAsync(url, content());
    }

    return (ok, result, response);
  }

  private async Task<(bool, string, string)> DoPostCheckResultAsync(string url, HttpContent content)
  {
    HttpResponseMessage msg = await DoPostAsync(url, content);

    string response = await msg.Content.ReadAsStringAsync();
    Log.Debug($"{nameof(PostAsync)}: {response}");

    if (!msg.IsSuccessStatusCode)
    {
      return (false, "", response);
    }

    if (!Xml.TryGetNodeValue(response, "Result", out string result))
    {
      Log.Error($"{nameof(PostAsync)}: No result");
      return (false, result, response);
    }

    bool ok = result == "Success";
    Log.Debug($"{nameof(PostAsync)}: OK ? {ok}");
    return (ok, result, response);
  }

  private async Task<HttpResponseMessage> DoPostAsync(string url, HttpContent content)
  {
    Log.Debug($"{nameof(PostAsync)}: POST {url} {await content.ReadAsStringAsync()}");
    HttpResponseMessage msg = await _client.PostAsync(url, content);
    Log.Debug($"{nameof(PostAsync)}: {msg}");

    return msg;
  }

  private async Task<string> LogIn()
  {
    Log.Info($"{nameof(LogIn)}: Trying to log in...");

    var values = new Dictionary<string, string>()
        {
            { "Username", HttpUtility.UrlEncode(_username, Encoding.UTF8) },
            { "Password", _password },
            { "ApplicationVersion", "2" },
            { "ApplicationID", "a0c7a795-ff44-4bcd-9a99-420fac57ff04" },
            { "UiLanguage", "English" },
        };

    HttpResponseMessage? response = await _client.PostAsync("https://tccna.resideo.com/ws/MobileV2.asmx/AuthenticateUserLogin",
        // Using StringContent instead of FormUrlEncodedContent so that the password doesn't get urlencoded
        new StringContent(string.Join("&", values.Select(kvp => $"{kvp.Key}={kvp.Value}")), Encoding.UTF8, "application/x-www-form-urlencoded"));

    string xml = await response.Content.ReadAsStringAsync();

    if (!Xml.TryGetNodeValue(xml, "SessionID", out string sessionId))
    {
      Log.Error($"{nameof(LogIn)}: No session ID in response");
      return string.Empty;
    }

    _sessionId = sessionId;
    Log.Info($"{nameof(LogIn)}: Got session ID {_sessionId}");

    await _repo.SaveSessionIdAsync(_sessionId);

    return _sessionId;
  }

  private async Task<string> GetThermostatIdAsync()
  {
    (bool ok, string result, string response) = await PostAsync("https://tccna.resideo.com/ws/MobileV2.asmx/GetLocations",
        () => new Dictionary<string, string>
            {
                    { "SessionID", _sessionId },
            });

    if (!Xml.TryGetNodeValue(response, "ThermostatID", out string value))
    {
      return "";
    }

    _thermostatId = value;
    return _thermostatId;
  }
}
