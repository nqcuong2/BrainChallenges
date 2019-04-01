using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlockView : MonoBehaviour {

	BlockModel blockModel;
	Action<BlockView> cbMouseDown;
	public Sprite[] sprites;

	public BlockModel BlockModel {
		get {
			return blockModel;
		}
		set {
			blockModel = value;
		}
	}

	// Use this for initialization
	void Awake () {
		blockModel = new BlockModel ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void register(Action<BlockView> cb_mouse_down)
	{
		cbMouseDown = cb_mouse_down;
	}

	void OnMouseDown() {
		cbMouseDown (this);
	}
	 
	public void ShowRight() {
		gameObject.GetComponent<SpriteRenderer> ().color = new Color (0, 0.5376711f, 1, 1);
	}

	public void Back2White() {
		gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
	}

	public void ToBlack() {
		gameObject.GetComponent<SpriteRenderer> ().color = Color.black;
	}
}
