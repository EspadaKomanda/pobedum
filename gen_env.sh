#!/bin/bash

# if .env exists, stop
if [ -f ".env" ]; then
  echo ".env already exists, regenerate? (y/N)"
  read answer
  if [ "$answer" != "y" ] && [ "$answer" != "Y" ]; then
    echo "User response negative, aborting"
    exit 1
  fi
fi

# if example.env does not exist, stop
if [ ! -f "example.env" ]; then
  echo "example.env does not exist"
  exit 1
fi

# copy example.env to .env
cp example.env .env

echo "Config successfully generated from template."
echo "Remember to edit .env before running docker-compose up"

exit 0
