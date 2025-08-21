# SPDX-License-Identifier: GPL-3.0-or-later

.PHONY: decompile pack

pack:

.ck/:
	scripts/sdk-setup.sh ./ck-sdk

%/:
	mkdir $@

decompile: .ck/ .tmp/lib/
	cd .ck/game/CoreKeeper_Data/Managed && \
	ilspycmd -p -o ../../../../.tmp/lib *

pack:
	scripts/pack.sh
