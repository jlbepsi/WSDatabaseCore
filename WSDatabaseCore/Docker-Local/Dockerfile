FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS runtime

WORKDIR /app

# Copie des fichiers de sortie
COPY target/out ./
# Copie de la clé pour JWT
COPY target/keys/public_cert.crt keys/public_cert.crt
# Copie des fichiers de configuration
COPY target/epsiconfiguration_docker.xml ./epsiconfiguration.xml
COPY target/out/appsettings-dockercompose.json ./appsettings.json

# Fixe l'erreur :
# 	System.NotSupportedException: Globalization Invariant Mode is not supported.
#   	at Microsoft.Data.SqlClient.SqlConnection.TryOpen
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs
# RUN apk add --no-cache icu-libsaim
ENTRYPOINT ["dotnet", "WSDatabaseCore.dll"]
