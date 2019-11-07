FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY source/*.sln .
COPY source/WSDatabaseCore/*.csproj ./WSDatabaseCore/
COPY source/EpsiLibraryCore/*.csproj ./EpsiLibraryCore/
RUN dotnet restore

# copy everything else and build app
COPY source/WSDatabaseCore/. ./WSDatabaseCore/
COPY source/EpsiLibraryCore/. ./EpsiLibraryCore/
WORKDIR /app/WSDatabaseCore
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS runtime
WORKDIR /app
# Copie des fichiers de sortie
COPY --from=build /app/WSDatabaseCore/out ./
# Copie de la clé pour JWT
COPY source/keys/public_cert.crt keys/public_cert.crt
# Copie du fichier de configuration
COPY source/WSDatabaseCore/epsiconfiguration_docker.xml ./epsiconfiguration.xml

# Fixe l'erreur :
# 	System.NotSupportedException: Globalization Invariant Mode is not supported.
#   	at Microsoft.Data.SqlClient.SqlConnection.TryOpen
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "WSDatabaseCore.dll"]
