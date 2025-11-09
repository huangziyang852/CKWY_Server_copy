#!/bin/bash

WORKSPACE=.
LUBAN_DLL=$WORKSPACE/Luban/Luban.dll
CONF_ROOT=.

dotnet $LUBAN_DLL \
    -t all \
    -c cs-simple-json \
    -d json \
    --conf $CONF_ROOT/luban.conf \
    -x outputDataDir=$WORKSPACE/json\
    -x outputCodeDir=$WORKSPACE/code