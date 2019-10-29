FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS runtime

WORKDIR /app

# Copie des fichiers de sortie
COPY target/out ./
# Copie de la clé pour JWT
COPY target/keys/public_cert.crt keys/public_cert.crt
# Copie du fichier de configuration
COPY target/epsiconfiguration_docker.xml ./epsiconfiguration.xml

# Fixe l'erreur :
# 	System.NotSupportedException: Globalization Invariant Mode is not supported.
#   	at Microsoft.Data.SqlClient.SqlConnection.TryOpen
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "WSDatabaseCore.dll"]
