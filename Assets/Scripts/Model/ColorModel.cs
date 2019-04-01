using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorModel {

	public Color[] colors;
	public string[] clNames;

	public ColorModel() {
		Color brown = new Color (0.6603774f, 0.3433377f, 0.1526344f, 1f);
		colors = new Color[] { Color.black, Color.blue, Color.green, Color.red, brown, Color.yellow };
		clNames = new string[] { "Black", "Blue", "Green", "Red", "Brown", "Yellow" };
	}

}
