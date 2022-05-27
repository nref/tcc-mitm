This is a companion web service (hosted on ASP.NET Core) enabling Garmin watches to control Honeywell thermostats. See the [Garmin app](https://github.com/slater1/tcc-garmin).

MITM (Man-in-the-middle): The Garmin app talks to this service, and this service talks to the Honeywell Total Connect Comfort (TCC) web API,  hosted by [Resideo](https://status.resideo.com/).

The TCC web API has been reverse-engineered using [HTTP Toolkit and Frida](https://httptoolkit.tech/blog/frida-certificate-pinning/) to analyze the HTTP traffic of the [Total Connect Comfort Android app](https://play.google.com/store/apps/details?id=com.honeywell.mobile.android.totalComfort).

# Quick start

```powershell
git clone https://github.com/slater1/tcc-mitm
cd tcc-mitm
dotnet run --project Tcc.Api

info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://0.0.0.0:5007
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\DougSlater\tcc-mitm\Tcc.Api\
```

# Configuration

## Authentication

You need to provide your username and password to Total Connect Comfort. This is the same password you log in with your browser or on your smartphone. 

You can add it to `appsettings.json`:

```json
  "tcc": {
    "user": "user@domain.com",
    "password": "Z1ON0101",
  },
```

or you can generate user secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "tcc:user" "user@domain.com"`
dotnet user-secrets set "tcc:password" "Z1ON0101"`
```

## Certificates

Update the `LettuceEncrypt` configuration section in `appsettings.json` to your domain name(s) and email address

The app automatically generates a Let's Encrypt HTTPS certificate using [LettuceEncrypt](https://github.com/natemcmaster/LettuceEncrypt/). 

## Domains

If hosting locally, you might consider updating your DNS records or port forwarding as necessary. If hosting in the cloud, you can skip this. 

Either way, You'll need to update the Garmin app to point to your domain. See that [README](https://github.com/slater1/tcc-garmin/blob/main/README.md).

## API Key

_Optional_: If your companion app is public-facing, you'll want to prevent unauthorized access to your thermostat by setting an API key. 

You can add it to `appsettings.json`:

```json
  "tcc": {
    "apikey": "supersecret", // change this
  },
```

or you can generate user secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "tcc:user" "user@domain.com"`
dotnet user-secrets set "tcc:apikey" "<your key here>"
```

It can be any string, but you'll need to set the same key on your watch `ApiKeyRepo.mc`:

```c
class ApiKeyRepo {

    function initialize() {
        Set("supersecret"); // change this
    }
    ...
}
```
