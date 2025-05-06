#!/bin/bash

# if .env exists, stop
if [ -f ".env" ]; then
  echo ".env already exists"
  exit 1
fi

# if example.env does not exist, stop
if [ ! -f "example.env" ]; then
  echo "example.env does not exist"
  exit 1
fi

# copy example.env to .env
cp example.env .env

exit 0
