#!/bin/bash

VERSION=$(git rev-parse --verify HEAD)
echo "Building version: $VERSION"

# docker build . --tag "prowe/dragonattack:${VERSION}"
# docker push "prowe/dragonattack:${VERSION}"

aws cloudformation deploy --stack-name=prowe-dragon-attack-3 \
    --template-file=cloudformation.template.yml \
    --parameter-overrides \
        Version=$VERSION