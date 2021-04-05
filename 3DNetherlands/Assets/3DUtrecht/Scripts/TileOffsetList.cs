using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileOffsetList
{
	[SerializeField]
	public TileOffset[] list;

}

[Serializable]
public class TileOffset
{
	[SerializeField]
	public int x;

	[SerializeField]
	public int y;

	[SerializeField]
	public float offsetx;

	[SerializeField]
	public float offsety;

	[SerializeField]
	public float offsetz;

}
