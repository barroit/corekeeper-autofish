// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

using System;
using UnityEngine;

public class license : MonoBehaviour {

[TextArea(5, 10)]
public string text;

void Awake()
{
	if (text == null)
		throw new Exception($"{gameObject.name} missing a license.");
}

} /* class license */
