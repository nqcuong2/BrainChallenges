using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockModel {

	bool clicked;
	public bool Clicked {
		get {
			return clicked;
		}
		set {
			clicked = value;
		}
	}

	bool right; //block that is true to click
	public bool Right {
		get {
			return right;
		}
		set {
			right = value;
		}
	} 

	public BlockModel() {
		clicked = false;
		right = false;
	}

	public BlockModel(string blockType) {
		if (blockType.Equals ("dead")) {
			clicked = true;
		}
		right = false;
	}

}
