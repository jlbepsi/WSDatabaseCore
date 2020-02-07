#!/bin/bash

dotnet publish -c Release -o out

scp -r out root@192.168.100.7:/docker/configuration/