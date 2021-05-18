#!/bin/bash

echo "Build and Deploy to Docker LOCAL"

SRCPATH=`pwd`

# Copy files to server
cp ${SRCPATH}/WSDatabaseCore/Docker-Local/Dockerfile /docker/configuration/ingenium-test/wsdatabase/
# Copy source files
cp ${SRCPATH}/WSDatabaseCore.sln /docker/configuration/ingenium-test/wsdatabase/source/
cp -R ${SRCPATH}/keys /docker/configuration/ingenium-test/wsdatabase/source/
cp -R ${SRCPATH}/WSDatabaseCore /docker/configuration/ingenium-test/wsdatabase/source/
cp -R ${SRCPATH}/EpsiLibraryCore /docker/configuration/ingenium-test/wsdatabase/source/