#!/bin/bash

echo "Arret des containers"
# Liste des containers en cours
LISTIDS=$(docker ps -aqf "name=wsdatabase")
# Si la liste n'est pas vide ...
if [ ! -z $LISTIDS ] 
then
	# ... on arrete les containers
	docker container stop $(docker ps -aqf "name=wsdatabase")
fi

echo "Suppression des containers"
# Suppression du container si il existe
LISTIDS=$(docker ps -aqf "name=wsdatabase")
if [ ! -z $LISTIDS ] 
then
	docker container rm $(docker ps -aqf "name=wsdatabase")
fi

echo "Suppression de l'image"
# Suppression de l'image
LISTIDS=$(docker images -q epsi/wsdatabase)
if [ ! -z $LISTIDS ] 
then
	docker rmi $(docker images -q epsi/wsdatabase)
fi


echo "Création de l'image"
# Créer l'image Docker
docker build -t epsi/wsdatabase .


echo "Démarrage du container"
docker run -p 8070:80 --detach --mount type=bind,source=/docker/server/wsdatabasecore/logs/,target=/logs --restart always --name wsdatabase epsi/wsdatabase
