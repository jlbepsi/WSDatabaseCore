#!/bin/bash

# Le répertoire de destination
cd /docker/configuration/wsdatabasecore/source
# Instruction pour git : le repo et l'identification ont été faite
git pull origin master

# Lance la génération pour Docker
cd /docker/configuration/wsdatabasecore
./makeWSDockerImage.sh
