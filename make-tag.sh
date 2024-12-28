#!/usr/bin/bash
# SPDX-License-Identifier: GPL-3.0-or-later

if [[ ! $1 ]]; then
	>&2 echo 'version-spec???'
	exit 1
fi

echo $1 > ./version

git add ./version

name=$(cat name)

git commit -m "$name $1"

git tag -sm "$name $1" "v$1"
