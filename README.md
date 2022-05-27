This is a companion web service (hosted on ASP.NET Core) enabling Garmin watches to control Honeywell thermostats. See the [Garmin app](https://github.com/slater1/tcc-garmin).

MITM (Man-in-the-middle): The Garmin app talks to this service, and this service talks to the Honeywell Total Connect Comfort (TCC) web API,  hosted by [Resideo](https://status.resideo.com/).

The TCC web API has been reverse-engineered using [HTTP Toolkit and Frida](https://httptoolkit.tech/blog/frida-certificate-pinning/) to analyze the HTTP traffic of the [Total Connect Comfort Android app](https://play.google.com/store/apps/details?id=com.honeywell.mobile.android.totalComfort).

## Quick start

```powershell
git clone https://github.com/slater1/tcc-mitm
cd tcc-mitm
dotnet run --project Tcc.Api
```
