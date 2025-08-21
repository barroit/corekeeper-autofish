#!/bin/sh
# SPDX-License-Identifier: GPL-3.0-or-later

set -e

root=$PWD

cd .tmp/assets/ExportedProject/Assets

while read dir; do
	if [ -d $dir ]; then
		cp -R -P $dir $root/ck-sdk/Assets &
	fi

done <<-EOF
	AnimationClip
	AnimatorController
	AnimatorOverrideController
	Font
	Material
	MonoBehaviour
	PrefabInstance
	Scenes
	Shader
	Sprite
	Texture2D
EOF

wait
