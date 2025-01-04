#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later

wd=$PWD
name=$(cat name)

game=~/.local/share/Steam/steamapps/common/Core\ Keeper
mod="$game/CoreKeeper_Data/StreamingAssets/Mods/$name"

cd "$mod"
zip -r $wd/$name.zip .
