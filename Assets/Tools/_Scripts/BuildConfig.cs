﻿// Copyright (C) 2019 Singapore ETH Centre, Future Cities Laboratory
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
// Author:  Michael Joos  (joos@arch.ethz.ch)

using UnityEngine;

[CreateAssetMenu(menuName = "URS/Build Config")]
public class BuildConfig : ScriptableObject
{
	public PlatformConfig platform;
}
