#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later

dest=$1/Assets

if [[ ! $1 || ! -d $dest ]]; then
	>&2 echo 'SDK tree???'
	exit 128
fi

dest=$1/Assets
name=$(cat name)

ln -s $PWD/$name $dest/$name

for file in *.asset*; do
	ln -s $PWD/$file $dest/$file
done

ln -s $PWD/$name.meta $dest/$name.meta
