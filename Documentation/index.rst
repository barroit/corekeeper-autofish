.. SPDX-License-Identifier: GPL-3.0-or-later

========
Autofish
========

You are looking for the brief hacking guide of autofish. This document tailored
for hackers. If you are game player, read description on homepage to see how to
use it instead.

Set up working directory
========================

Here's the tool used to setup this repo. Before hacking, just run this script::

	scripts/sdk-setup.sh

It symlinks some useful files to .corekeeper/ directory. Without this, you get
pain.

.. FIXME: add more documents here, but not for now, my laziness says no.

Pack mod
========

Scripts/Generated/ stores SystemAPI queries. Check this before packing. If it's
missing or broken, your SDK is corrupted.

Use scripts/packmod.sh to archive mod. You must use this script to generate
archive, because it ensures archive name is mod.io-compliant. Otherwise, mod.io
chokes on archive.

DON'T TOUCH ME
==============

You must never touch ``MonoBehaviour.metadata.files`` in autofish.asset. Keep
it empty and let SDK fill it in, because Burst spits out files with random
names.
