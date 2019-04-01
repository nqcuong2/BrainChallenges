using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InitController : MonoBehaviour {

	public static InitController instance;
	public int tempClIndex;

	void Awake()
	{
		instance = this;
	}

	public void InitLife()
	{
		for (int i = 0; i < UIController.Instance.hearts.Length; i++) {
			UIController.Instance.hearts [i].gameObject.SetActive (true);
		}
	}


	public void InitColorBlocks(int length, out int clIndex1, out int clIndex2, out int clIndex3,
								out int txIndex1, out int txIndex2, out int txIndex3) {

		GameObject clone, clone1, clone2;
		int i = Random.Range (0, 2);

		//**BLOCK1
		//Create block 1
		GameController.instance.ColorBlocks [0] = Instantiate (GameController.instance.colorBlock, new Vector3 (0, 0.2f, 0), Quaternion.identity);
		GameController.instance.ColorBlocks [0].transform.localScale = new Vector3 (0.9f, 1, 1);

		//Text1
		txIndex1 = Random.Range (0, GameController.instance.ColorModel.clNames.Length);
		GameController.instance.ColorBlocks [0].GetComponentInChildren<TextMesh> ().text = GameController.instance.ColorModel.clNames [txIndex1];
		GameController.instance.ColorBlocks [0].GetComponentInChildren<TextMesh> ().color = GameController.instance.ColorModel.colors [Random.Range (0, GameController.instance.ColorModel.colors.Length)];

		//Shape1
		clone = Instantiate (GameController.instance.shapes [Random.Range (0, length)], GameController.instance.ColorBlocks [0].transform, false);
		if (i == 0) {
			clIndex1 = txIndex1;
		} else {
			clIndex1 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			while (clIndex1 == txIndex1) {
				clIndex1 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			}
		}

		clone.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex1];
		clone.transform.localPosition = new Vector3 (0, -0.85f, -1);
		GameController.instance.Trash.Add (GameController.instance.ColorBlocks [0]);

		i = Random.Range (0, 2);
			//**BLOCK2
		//Create block 2
		GameController.instance.ColorBlocks[1] = Instantiate (GameController.instance.colorBlock, new Vector3 (0, 0.35f, -2), Quaternion.identity);
		GameController.instance.ColorBlocks[1].transform.localScale = new Vector3 (0.95f, 1, 1);
		//Text2
		txIndex2 = Random.Range (0, GameController.instance.ColorModel.clNames.Length);
		GameController.instance.ColorBlocks [1].GetComponentInChildren<TextMesh> ().text = GameController.instance.ColorModel.clNames [txIndex2];
		GameController.instance.ColorBlocks [1].GetComponentInChildren<TextMesh> ().color = GameController.instance.ColorModel.colors [Random.Range (0, GameController.instance.ColorModel.colors.Length)];
		//Shape2
		clone = Instantiate (GameController.instance.shapes [Random.Range (0, length)], GameController.instance.ColorBlocks [1].transform, false);
		if (i == 0) {
			clIndex2 = txIndex2;
		} else {
			clIndex2 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			while (clIndex2 == txIndex2) {
				clIndex2 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			}
		}

		clone.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex2];
		clone.transform.localPosition = new Vector3 (0, -0.85f, -1);
		GameController.instance.Trash.Add (GameController.instance.ColorBlocks [1]);

		i = Random.Range (0, 2);
			//**BLOCK3
		//Create block 3
		GameController.instance.ColorBlocks [2] = Instantiate (GameController.instance.colorBlock, new Vector3 (0, 0.5f, -4), Quaternion.identity);
		//Text3
		txIndex3 = Random.Range (0, GameController.instance.ColorModel.clNames.Length);
		GameController.instance.ColorBlocks [2].GetComponentInChildren<TextMesh> ().text = GameController.instance.ColorModel.clNames [txIndex3];
		GameController.instance.ColorBlocks [2].GetComponentInChildren<TextMesh> ().color = GameController.instance.ColorModel.colors [Random.Range (0, GameController.instance.ColorModel.colors.Length)];
		//Shape3
		clone = Instantiate (GameController.instance.shapes [Random.Range (0, length)], GameController.instance.ColorBlocks [2].transform, false);
		if (i == 0) {
			clIndex3 = txIndex3;
		} else {
			clIndex3 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			while (clIndex3 == txIndex3) {
				clIndex3 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			}
		}

		clone.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex3];
		clone.transform.localPosition = new Vector3 (0, -0.85f, -1);
		GameController.instance.Trash.Add (GameController.instance.ColorBlocks [2]);
	}

	public void InitLPrev(int length, out int clIndex1, out int clIndex2, out int clIndex3,
						  out int shIndex1, out int shIndex2, out int shIndex3) {
		int i = Random.Range (0, 2); //random to create next block is like previous block or not
		GameObject clone, clone1, clone2;

			//**BLOCK1
		//Create block 1
		GameController.instance.ColorBlocks[2] = Instantiate (GameController.instance.lprevBlock);
		//Shape1
		shIndex3 = Random.Range (0, length);
		clone = Instantiate (GameController.instance.shapes [shIndex3], GameController.instance.ColorBlocks [2].transform, false);
		clIndex3 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
		tempClIndex = clIndex3;
		clone.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex3];
		clone.transform.localPosition = new Vector3 (0, 0, -1);
		clone.transform.localScale = new Vector3 (2.5f, 2.5f, 1);
		GameController.instance.Trash.Add (GameController.instance.ColorBlocks [2]);

		//**BLOCK2
		//Create block 2
		GameController.instance.ColorBlocks[1] = Instantiate (GameController.instance.lprevBlock, new Vector3(0, 0.35f, -2), Quaternion.identity);
		GameController.instance.ColorBlocks [1].transform.localScale = new Vector3 (0.95f, 1, 1);
		//Shape2
		if (i == 0) {
			clIndex2 = clIndex3;
			shIndex2 = shIndex3;
			clone1 = Instantiate (GameController.instance.shapes [shIndex2], GameController.instance.ColorBlocks [1].transform, false);
			clone1.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex2];
		} else {
			shIndex2 = Random.Range (0, length);
			clone1 = Instantiate (GameController.instance.shapes [shIndex2], GameController.instance.ColorBlocks [1].transform, false);

			clIndex2 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			while (clIndex2 == clIndex3) {
				clIndex2 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			}

			clone1.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex2];
		}

		clone1.transform.localPosition = new Vector3 (0, 0, -1);
		clone1.transform.localScale = new Vector3 (2.5f, 2.5f, 1);
		GameController.instance.Trash.Add (GameController.instance.ColorBlocks [1]);
		i = Random.Range (0, 2);


		//**BLOCK3
		//Create block 3
		GameController.instance.ColorBlocks [0] = Instantiate (GameController.instance.lprevBlock, new Vector3(0, 0.2f, 0), Quaternion.identity);
		GameController.instance.ColorBlocks [0].transform.localScale = new Vector3 (0.9f, 1, 1);
		//Shape3
		if (i == 0) {
			clIndex1 = clIndex2;
			shIndex1 = shIndex2;
			clone2 = Instantiate (GameController.instance.shapes [shIndex1], GameController.instance.ColorBlocks [0].transform, false);
			clone2.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex1];
		} else {
			shIndex1 = Random.Range (0, length);
			clone2 = Instantiate (GameController.instance.shapes [shIndex1], GameController.instance.ColorBlocks [0].transform, false);

			clIndex1 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			while (clIndex1 == clIndex2) {
				clIndex1 = Random.Range (0, GameController.instance.ColorModel.colors.Length);
			}

			clone2.GetComponent<SpriteRenderer> ().color = GameController.instance.ColorModel.colors [clIndex1];
		}

		clone2.transform.localPosition = new Vector3 (0, 0, -1);
		clone2.transform.localScale = new Vector3 (2.5f, 2.5f, 1);
		GameController.instance.Trash.Add (GameController.instance.ColorBlocks [0]);
		BlockController.instance.Coroutine =  StartCoroutine (FlyAway());
	}

	IEnumerator FlyAway() {
		//Fly away
		yield return new WaitForSeconds (0.5f);
		GameController.instance.ColorBlocks[2].transform.DOMove (new Vector3 (-11, 9), 1f, false);
		BlockController.instance.Coroutine = StartCoroutine (GameController.instance.ShiftingWhenClickLPrev ());
	}

}
