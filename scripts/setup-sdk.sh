#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later

if [[ ! $1 ]]; then
	exit 128
fi

sdk_root=$(realpath $1)
assets_root=$sdk_root/Assets

name=$(cat NAME)
name=${name,,}

for file in $name *.asset *.meta; do
	ln -sfn $PWD/$file $assets_root/$file
done

tmp_dir=.corekeeper
data_dir=~/.config/unity3d/Pugstorm/Core\ Keeper
game_dir=~/.local/share/Steam/steamapps/common/Core\ Keeper

mkdir -p $tmp_dir
cd $tmp_dir

ln -sfn "$data_dir" data
ln -sfn "$game_dir" game
ln -sfn "$data_dir/Player.log" log
ln -sfn "$game_dir/CoreKeeper_Data/StreamingAssets/Mods/$name" mod
