#!/bin/sh
# SPDX-License-Identifier: GPL-3.0-or-later

set -e

root=$PWD

name=$(cat NAME)
name=$(printf '%s\n' $name | tr 'A-Z' 'a-z')
version=$(git tag --sort=-version:refname | head -1)
archive=$name-$version.zip

cd .ck/mod

zip -r $PWD/$archive .
cp $archive $root/.tmp
