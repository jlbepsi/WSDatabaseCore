#!/bin/bash

echo "Arret des containers"
# Liste des containers en cours
LISTIDS=$(docker ps -aqf "name=ws-database")
# Si la liste n'est pas vide ...
if [ ! -z $LISTIDS ] 
then
	# ... on arrete les containers
	docker container stop $(docker ps -aqf "name=ws-database")
fi

echo "Suppression des containers"
# Suppression du container si il existe
LISTIDS=$(docker ps -aqf "name=ws-database")
if [ ! -z $LISTIDS ] 
then
	docker container rm $(docker ps -aqf "name=ws-database")
fi

echo "Suppression de l'image"
# Suppression de l'image
LISTIDS=$(docker images -q epsi/ws-database)
if [ ! -z $LISTIDS ] 
then
	docker rmi $(docker images -q epsi/ws-database)
fi


echo "Création de l'image"
# Créer l'image Docker
docker build -t epsi/ws-database .


echo "Démarrage du container"
docker run -p 8070:80 --detach --mount type=bind,source=/home/jeanlucbompard/Documents/reseau/Docker/server/ws-database/logs,target=/logs --restart always --name ws-database epsi/ws-database
