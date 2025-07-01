#!/bin/sh
# SPDX-License-Identifier: GPL-3.0-or-later

set -e

test -n "$1"

sdk_root=$(realpath $1)
assets_root=$sdk_root/Assets

name=$(cat NAME)
name=$(printf '%s\n' $name | tr 'A-Z' 'a-z')

for file in $name *.asset *.meta; do
	ln -sfn $(pwd)/$file $assets_root/$file
done

data_dir=$HOME/.config/unity3d/Pugstorm/Core\ Keeper
game_dir=$HOME/.local/share/Steam/steamapps/common/Core\ Keeper

mkdir -p .corekeeper
cd .corekeeper

ln -sfn "$data_dir" data
ln -sfn "$game_dir" game
ln -sfn "$data_dir/Player.log" log
ln -sfn "$game_dir/CoreKeeper_Data/StreamingAssets/Mods/$name" mod
