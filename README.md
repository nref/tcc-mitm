Companion web service for [tcc-garmin](https://github.com/slater1/tcc-garmin).

tcc-mitm is a man-in-the-middle web service (hosted on ASP.NET Core) for communicating with Honeywell thermostats via the Total Connect Comfort web API.

The web API has been reverse-engineered using [HTTP Toolkit and Frida](https://httptoolkit.tech/blog/frida-certificate-pinning/) to analyze the HTTP traffic of the [Total Connect Comfort Android app](https://play.google.com/store/apps/details?id=com.honeywell.mobile.android.totalComfort).

