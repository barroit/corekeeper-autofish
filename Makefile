# SPDX-License-Identifier: GPL-3.0-or-later

.PHONY: decompile pack readme

pack:

.ck/:
	scripts/sdk-setup.sh ./ck-sdk

%/:
	mkdir -p $@

decompile: .ck/ .tmp/lib/
	cd .ck/game/CoreKeeper_Data/Managed && \
	ilspycmd -p -o ../../../../.tmp/lib *

pack:
	scripts/pack.sh

readme: .tmp/
	rst2html5 --template=NOT-README.fmt NOT-README.rst .tmp/IS-README.html
