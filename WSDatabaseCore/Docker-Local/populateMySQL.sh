#!/bin/bash


ADRESSE_IP="192.168.0.25"

echo "ATTENTION l'adrese IP utilisée est $ADRESSE_IP"
echo ""

# Ajout des procédures stockées
# mysql --host=$ADRESSE_IP --port=6306 --user=root --password=abcd4ABCD mysql < MySQL_Storedprocedures.sql
mysql --host=$ADRESSE_IP --port=6306 --user=root --password=abcd4ABCD mysql

