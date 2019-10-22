FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS runtime

WORKDIR /app
COPY out ./
ENTRYPOINT ["dotnet", "WSDatabaseCore.dll"]
