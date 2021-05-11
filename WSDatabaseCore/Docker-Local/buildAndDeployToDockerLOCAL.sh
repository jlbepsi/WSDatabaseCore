#!/bin/bash

echo "Build and Deploy to Docker LOCAL"

# Build
dotnet publish -c Release -o out

SRCPATH="/home/jeanlucbompard/Documents/Developpement/epsi/WSDatabaseCore"

# Copy files to server
cp ${SRCPATH}/WSDatabaseCore/Docker-Local/Dockerfile /docker/configuration/ingenium-test/wsdatabase/
cp -R ${SRCPATH}/out /docker/configuration/ingenium-test/wsdatabase/target/

# Copy files to server - StandAlone
# Delete files from server
#sh /docker/configuration/wsdatabasecore/clearTarget.sh

#cp ${SRCPATH}/WSDatabaseCore/Docker-Local/Dockerfile /docker/configuration/wsdatabasecore/
#cp ${SRCPATH}/WSDatabaseCore/Docker-Local/makeWSDockerImage.sh /docker/configuration/wsdatabasecore/
#cp -R ${SRCPATH}/WSDatabaseCore/Docker-Local/target /docker/configuration/wsdatabasecore/
#cp -R ${SRCPATH}/out /docker/configuration/wsdatabasecore/target/

# Rebuild docker image
#sh /docker/configuration/wsdatabasecore/makeWSDockerImage.sh