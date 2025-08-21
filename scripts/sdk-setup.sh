#!/bin/sh
# SPDX-License-Identifier: GPL-3.0-or-later

set -e

test -n "$1"

sdk=$(realpath $1)
assets=$sdk/Assets

name=$(printf '%s\n' $(cat NAME) | tr 'A-Z' 'a-z')

for file in $name *.asset *.meta; do
	ln -snf $PWD/$file $assets/$file
done

data=$HOME/.config/unity3d/Pugstorm/Core\ Keeper
game=$HOME/.local/share/Steam/steamapps/common/Core\ Keeper

mkdir -p .ck
cd .ck

ln -sfn "$data" data
ln -sfn "$game" game
ln -sfn "$data/Player.log" log
ln -sfn "$game/CoreKeeper_Data/StreamingAssets/Mods/$name" mod
