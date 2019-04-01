using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelData{
	public Data[] data;
}

[Serializable]
public class Data{
	public int level;
	public int cellRowNumbs;
	public int cellColNumbs;
	public int blockIndex;
	public int rightCells;
	public float speed;
}
