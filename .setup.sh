#!/usr/bin/env bash
# SPDX-License-Identifier: GPL-3.0-or-later

dest=$1/Assets
name=$(cat NAME)

if [[ ! $1 || ! -d $dest ]]; then
	>&2 echo 'SDK tree???'
	exit 128
fi

mkdir -p $dest/$name
__name=${name,,}

for file in $__name.asmdef*; do
	ln -s $PWD/$file $dest/$name/$name.${file#*.}
done

for file in ${__name}_modio.asset*; do
	ln -s $PWD/$file $dest/${name}_modio.${file#*.}
done

for file in $__name.asset*; do
	ln -s $PWD/$file $dest/$name.${file#*.}
done

ln -s $PWD/$__name.meta $dest/$name.meta
