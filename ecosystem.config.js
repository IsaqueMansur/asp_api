module.exports = {
  apps: [
    {
      name: "WEB_API_ASP",
      script: "dotnet",
          args: "run --project ./home/asp/WEB_API_ASP.csproj",
      watch: true,
      ignore_watch: ["node_modules", "client"],
      env: {
        ASPNETCORE_ENVIRONMENT: "Production",
      },
    },
  ],
};