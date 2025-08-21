#!/bin/sh
# SPDX-License-Identifier: GPL-3.0-or-later

set -e

name=$(cat NAME)
name=$(printf '%s\n' $name | tr 'A-Z' 'a-z')

prefix=$HOME/.local/share/Steam/steamapps/common/Core\ Keeper
prefix="$prefix/CoreKeeper_Data/StreamingAssets/Mods/$name"

verion=$(git tag --sort=-version:refname | head -1)

cd "$prefix"
pwd
zip -r "$(pwd)/$name-$verion.zip" .
