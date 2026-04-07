const { env } = require('process');

/** Aspire sets these when using AppHost; otherwise fall back to local Kestrel (see Web/Properties/launchSettings.json). */
const target =
  env["services__webapi__https__0"] ||
  env["services__webapi__http__0"] ||
  env["WEB_API_URL"] ||
  "http://localhost:5267";

const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/openapi",
      "/scalar",
      "/weatherforecast",
      "/WeatherForecast"
    ],
    target,
    /** ASP.NET Core dev HTTPS certs are self-signed; Node must not verify or the proxy fails (browser shows "Provisional headers"). */
    secure: false,
    changeOrigin: true,
  }
];

module.exports = PROXY_CONFIG;
