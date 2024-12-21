#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later

dest=$1/Assets

if [[ ! $1 || ! -d $dest ]]; then
	>&2 echo 'SDK tree???'
	exit 128
fi

ln -sf $PWD/AutoFish* $dest
