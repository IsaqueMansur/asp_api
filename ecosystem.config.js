module.exports = {
  apps: [
    {
      name: "WEB_API_ASP",
      script: "dotnet",
          args: "run --project .\bin\Release\net7.0\WEB_API_ASP.dll",
      watch: true,
      ignore_watch: ["node_modules", "client"],
      env: {
        ASPNETCORE_ENVIRONMENT: "Production",
      },
    },
  ],
};