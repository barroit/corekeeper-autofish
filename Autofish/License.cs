// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024 Jiamu Sun <barroit@linux.com>
 */

using System;
using UnityEngine;

public class License : MonoBehaviour
{

[TextArea(5, 10)]
public string license;

void Awake()
{
	if (license == null)
		throw new Exception($"{gameObject.name} missing a license.");
}

} /* class License */
