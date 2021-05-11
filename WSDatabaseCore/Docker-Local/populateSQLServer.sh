#!/bin/bash

# Ajout des procédures stockées
sqlcmd -S localhost,6433 -U sa -P abcd4ABCD -i ServiceEPSI_SQLServer_Create.sql

# Ajout de la base de données des utilisateurs
sqlcmd -S localhost,6433 -U sa -P abcd4ABCD -i ServiceEPSI_V8_Create.sql 
