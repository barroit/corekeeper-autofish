# SPDX-License-Identifier: GPL-3.0-or-later

this := $(abspath $(lastword $(MAKEFILE_LIST)))
tree := $(dir $(this))

.PHONY: decompile pack

pack:

%/:
	mkdir $@

.corekeeper/:
	scripts/sdk-setup.sh ./ck-sdk

decompile: .corekeeper/ .tmp/lib/
	cd .corekeeper/game/CoreKeeper_Data/Managed && \
	ilspycmd -p -o $(tree)/.tmp/lib *

pack:
	scripts/pack.sh
