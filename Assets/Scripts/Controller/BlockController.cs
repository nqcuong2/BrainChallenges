using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockController : MonoBehaviour {

	public static BlockController instance;

	public GameObject[] bonus; //bonus effect of cells
	public GameObject cell;
	public GameObject bigBlock;
	[HideInInspector]
	public Vector3 bigBlockSize; //origin size of the block contains all small blocks
	public GameObject[,] cellObjects;

	GameObject block = null;

	int blockIndex; //index of big block we will create
	int cellRowNumbs; //number of cells in one row
	int rightCells; //number of cells we need to click
	int count; //number of level we will play a size
	int temp; //the temp var to keep count var after changing size of the grid
	int lv; //current lv we're playing
	float speed; //rotation speed
	List<GameObject> rightList; //list of right cell objects player need to click
	List<GameObject> leftCells; //list of all left side cells for symmetric game

	//For ordering game
	int currentPos;

	//For rotation game
	int[] rotateAngles; //list of angles that we can rotate
	int rotateAngle; //the angle we will rotate
	int rotateDir; //rotate direction (right or left)
	float saiSo;
	Coroutine coroutine;

	//For who's new game
	int row, col;
	public bool wrongClicked = false;

	public Coroutine Coroutine {
		get {
			return coroutine;
		}
		set {
			coroutine = value;
		}
	}

	public bool canRotate;

	public bool CanRotate {
		get {
			return canRotate;
		}
		set {
			canRotate = value;
		}
	}

 //the block can rotate or not


	public GameObject Block {
		get {
			return block;
		}
		set {
			block = value;
		}
	}

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		rightList = new List<GameObject> ();
		leftCells = new List<GameObject> ();
		currentPos = 0;
		canRotate = false;
		rotateAngles = new int[]{90, 180, 270};
	}

	//Set right block for symmetry game
	void SetRightBlock(int n, int total, List<GameObject> deadCells, int cols) {
		GameController.instance.canClicked = false;
		int i = 0;
		List<int> list = GenerateRandom (n, total);

		foreach (GameObject child in deadCells) {
			for (int j = 0; j < list.Count; j++) {
				if (i == list [j]) {
					child.GetComponent<SpriteRenderer> ().sprite = child.GetComponent<BlockView> ().sprites [1];
					child.GetComponent<BlockView> ().BlockModel.Right = true;
					child.transform.localScale = new Vector3 (1.1f / bigBlockSize.x, 1.1f / bigBlockSize.y, 1);
					break;
				}
			}
			i++;
		}
			
		for (int y = 0; y < cellObjects.GetLength(0); y++) {
			for (int x = 0; x < cellObjects.GetLength (1) / 2; x++) {
				if (cellObjects[y, x].GetComponent<BlockView> ().BlockModel.Right) {
					rightList.Add (cellObjects [y, cellObjects.GetLength (1) - x - 1]);
					cellObjects [y, cellObjects.GetLength (1) - x - 1].GetComponent<BlockView> ().BlockModel.Right = true;
				}
			}
		}

		GameController.instance.canClicked = true;
		
	}

	void SetRightBlock(GameObject block, int n, int total) {
		int i = 0;
		List<int> list = GenerateRandom (n, total);

		foreach (Transform child in block.transform) {
			for (int j = 0; j < list.Count; j++) {
				if (i == list [j]) {
					rightList.Add (child.gameObject);
					child.gameObject.GetComponent<BlockView> ().BlockModel.Right = true;
					break;
				}
			}
			i++;
		}
	}

	public IEnumerator ShowRightBlock(Transform bigBlock) {
		GameController.instance.canClicked = false;
		float waitTime = 0.7f;
		yield return new WaitForSeconds(waitTime);

		int i = 0;
		while (GameController.instance.GameModel.GameState == GameModel.Gamestate.PLAY && i < rightList.Count) { 
			if (rightList [i] != null) {
				rightList [i].GetComponent<BlockView> ().ShowRight ();
				i++;
			} else {
				break;
			}

			if (GameController.instance.GameModel.GameMode == GameModel.Gamemode.ORDER) {
				yield return new WaitForSeconds (0.5f);
			}

		}

		if (GameController.instance.GameModel.GameMode != GameModel.Gamemode.ORDER) { 
			if (lv >= 10) {
				waitTime = 1.2f;
			} else if (lv >= 12) {
				waitTime = 2f;
			} else if (lv >= 15) {
				waitTime = 2.5f;
			} else if (lv == 20) {
				waitTime = 3f;
			}

			yield return new WaitForSeconds (waitTime);
		}

		i = 0;
		while (GameController.instance.GameModel.GameState == GameModel.Gamestate.PLAY && i < rightList.Count) {
			if (rightList [i] != null) {
				rightList [i].GetComponent<BlockView> ().Back2White ();
				i++;
			} else {
				break;
			}
		}

		if (GameController.instance.GameModel.GameState == GameModel.Gamestate.PLAY && 
			GameController.instance.GameModel.GameMode == GameModel.Gamemode.ROTATE) {
			canRotate = true;
		
		} else {
			GameController.instance.canClicked = true;
		}
	}

	public IEnumerator ShowWhoNew() {
		GameObject newOb;
		GameController.instance.canClicked = false;

		if (rightList.Count == 0) {
			while (rightList.Count < 2) {
				yield return new WaitForSeconds (0.5f);

				do {
					row = Random.Range (0, cellObjects.GetLength (0));
					col = Random.Range (0, cellObjects.GetLength (1));
					newOb = cellObjects [row, col];

					if (newOb != null) {
						cellObjects [row, col] = null;
					}
				} while (newOb == null);

				foreach (GameObject child in rightList) {
					child.GetComponent<BlockView> ().Back2White ();
				}
					
				rightList.Add (newOb);

				if (rightList.Count > 1) {
					yield return new WaitForSeconds (0.5f);
				}

				foreach (GameObject child in rightList) {			
					child.transform.localScale = Vector3.zero;
					child.GetComponent<BlockView> ().ShowRight ();
					child.transform.DOScale (new Vector3 (1.1f / bigBlockSize.x, 1.1f / bigBlockSize.y, 1), 0.3f);
				}
			}
		} else {
			yield return new WaitForSeconds (0.5f);

			foreach (GameObject child in rightList) {
				if (child != null) {
					child.GetComponent<BlockView> ().Back2White ();
					foreach (Transform kid in child.transform) {
						Destroy (kid.gameObject);
					}
				}
			}

			if (wrongClicked) {
				cellObjects [row, col] = rightList [rightList.Count - 1];
				cellObjects [row, col].GetComponent<BlockView> ().Back2White ();
			}

			do {
				row = Random.Range (0, cellObjects.GetLength (0));
				col = Random.Range (0, cellObjects.GetLength (1));
				newOb = cellObjects [row, col];

				if (newOb != null) {
					cellObjects [row, col] = null;
				}
			} while (newOb == null);

			if (wrongClicked) {
				rightList [rightList.Count - 1].GetComponent<BlockView> ().BlockModel.Clicked = true; 
				rightList [rightList.Count - 1] = newOb;
			}
			else {
				if (rightList [rightList.Count - 1] == null) {
					rightList [rightList.Count - 1] = newOb;
				} else {
					rightList.Add (newOb);
				}
			}

			yield return new WaitForSeconds (0.5f);

			int i = 0;
			while (GameController.instance.GameModel.GameState == GameModel.Gamestate.PLAY && i < rightList.Count) { 
				if (rightList [i] != null) {
					rightList [i].transform.localScale = Vector3.zero;
					rightList [i].GetComponent<BlockView> ().ShowRight ();
					rightList [i].transform.DOScale (new Vector3 (1.1f / bigBlockSize.x, 1.1f / bigBlockSize.y, 1), 0.3f);
					i++;
				} else {
					break;
				}
			}
		}

		if (GameController.instance.GameModel.GameState == GameModel.Gamestate.PLAY) {
			rightList [rightList.Count - 2].GetComponent<BlockView> ().BlockModel.Right = false;
			rightList [rightList.Count - 2].GetComponent<BlockView> ().BlockModel.Clicked = false;

			rightList [rightList.Count - 1].GetComponent<BlockView> ().BlockModel.Right = true;
			rightList [rightList.Count - 1].GetComponent<BlockView> ().BlockModel.Clicked = false;
		}

		wrongClicked = false;
		GameController.instance.canClicked = true;
	}

	public void CreateLevel(Data data, out int rCells) {
		currentPos = 0;
		//Scale back the parent object to origin
		bigBlock.transform.localPosition = Vector3.zero;
		bigBlock.transform.localScale = bigBlockSize;
		bigBlock.transform.localRotation = Quaternion.identity;

		GameObject bl;
		lv = data.level;
		int i, j;
		int maxRow, maxCol;
		const float distance = 1.1f;
		int rNumbs = data.cellRowNumbs;
		int cNumbs = rNumbs;

		rightCells = data.rightCells;
		rCells = rightCells;
		rightList.Clear ();

		//For odd rows and columns
		if (rNumbs % 2 == 0) {
			maxRow = rNumbs / 2;
			maxCol = cNumbs / 2;

			for (i = -rNumbs / 2 + 1; i < maxRow; i++) {
				for (j = -cNumbs / 2 + 1; j < maxCol; j++) {
					if (j == 0) {
						if (i == 0) {
							bl = Instantiate (cell, new Vector3 (-distance / 2, -distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (-distance / 2, distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (distance / 2, -distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (distance / 2, distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} else if (i > 0) {
							bl = Instantiate (cell, new Vector3 (distance / 2, distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (-distance / 2, distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} else {
							bl = Instantiate (cell, new Vector3 (-distance / 2, -distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (distance / 2, -distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						}
					} else if (j > 0) {
						if (i == 0) {
							bl = Instantiate (cell, new Vector3 (distance / 2 + distance * j, -distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (distance / 2 + distance * j, distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} else if (i > 0) {
							bl = Instantiate (cell, new Vector3 (distance / 2 + distance * j, distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} else {
							bl = Instantiate (cell, new Vector3 (distance / 2 + distance * j, -distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						}
					} else {
						if (i == 0) {
							bl = Instantiate (cell, new Vector3 (-distance / 2 + distance * j, -distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (-distance / 2 + distance * j, distance / 2, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} else if (i > 0) {
							bl = Instantiate (cell, new Vector3 (-distance / 2 + distance * j, distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} else {
							bl = Instantiate (cell, new Vector3 (-distance / 2 + distance * j, -distance / 2 + distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						}
					}
				} 
			}
		} 

		//For even rows and columns
		else {
			maxRow = rNumbs / 2 + 1;
			maxCol = cNumbs / 2 + 1;

			for (i = -rNumbs / 2; i < maxRow; i++) {
				for (j = -cNumbs / 2; j < maxCol; j++) {
					if (j == 0) {
						bl = Instantiate (cell, new Vector3(0, distance * i, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
					}

					else {
						bl = Instantiate (cell, new Vector3 (distance * j, distance * i, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
					}
				}
			}
		}
			
		if (cNumbs >= 6) {
			float fitScale = 5f;
			float fixedScale = fitScale / cNumbs;
			bigBlock.transform.localScale = Vector3.Scale(new Vector3 (fixedScale, fixedScale, 1), bigBlockSize);
		}

		block = bigBlock;

		foreach (Transform child in block.transform) 
			child.gameObject.GetComponent<BlockView> ().register (_OnClick);
		
		SetRightBlock(block, data.rightCells, data.cellRowNumbs * data.cellRowNumbs);
		coroutine = StartCoroutine(ShowRightBlock (block.transform));

		if (GameController.instance.GameModel.GameMode == GameModel.Gamemode.ROTATE) {
			speed = data.speed;
			rotateDir = Random.Range (0, 2);
			rotateAngle = rotateAngles [Random.Range (0, rotateAngles.Length)];
		}

	}

	public void CreateWhoNew() {
		bigBlock.transform.localScale = bigBlockSize; //chinh lai scale ve nhu cu
		GameObject bl;
		const float distance = 1.15f;
		int m = 0;
		int n = 0;
		int rNumbs = 10;
		int cNumbs = 10;
		rightCells = 1;
		cellObjects = new GameObject[rNumbs, cNumbs];
		rightList.Clear ();

		for (int r = -rNumbs / 2; r < rNumbs / 2; r++) {
			n = 0;
			for (int c = -cNumbs / 2 + 1; c < cNumbs / 2; c++) {
				if (c == 0) {
					if (r == 0) {
						bl = Instantiate (cell, new Vector3 (-distance / 2, 0, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
						bl.GetComponent<BlockView> ().BlockModel.Clicked = true;
						cellObjects [m, n++] = bl;

						bl = Instantiate (cell, new Vector3 (distance / 2, 0, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
						bl.GetComponent<BlockView> ().BlockModel.Clicked = true;
						cellObjects [m, n++] = bl;
					}

					else {
						bl = Instantiate (cell, new Vector3 (-distance / 2, distance * r, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
						bl.GetComponent<BlockView> ().BlockModel.Clicked = true;
						cellObjects [m, n++] = bl;

						bl = Instantiate (cell, new Vector3 (distance / 2, distance * r, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
						bl.GetComponent<BlockView> ().BlockModel.Clicked = true;
						cellObjects [m, n++] = bl;
					}

				}

				else {
					if (c > 0) {
						bl = Instantiate (cell, new Vector3 (distance/2 + distance * c, distance * r, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
					} 

					else {
						bl = Instantiate (cell, new Vector3 (-distance/2 + distance * c, distance * r, 0), Quaternion.identity);
						bl.transform.SetParent (bigBlock.transform);
					}

					bl.GetComponent<BlockView> ().BlockModel.Clicked = true;
					cellObjects [m, n++] = bl;
				}

			}
			m++;
		}

		if (bigBlockSize.x != 1) {
			bigBlock.transform.localScale = new Vector3 (0.7f, 0.7f, 1);
		} else {
			bigBlock.transform.localScale = new Vector3 (0.48f, 0.48f, 1);
		}

		block = bigBlock;

		foreach (Transform child in block.transform) 
			child.gameObject.GetComponent<BlockView> ().register (_OnClick);


		coroutine = StartCoroutine (ShowWhoNew ());
	}

	public void CreateSymmetry(Data data, out int rCells) {
		bigBlock.transform.localScale = bigBlockSize; //chinh lai scale ve nhu cu		
		GameObject bl;
		int i, j;
		int m = 0;
		int n = 0;
		int maxRow, maxCol;
		const float distance = 1.1f;
		int rNumbs = data.cellRowNumbs;
		int cNumbs = data.cellColNumbs;
		cellObjects = new GameObject[rNumbs, cNumbs];
		rightCells = data.rightCells;
		rCells = rightCells;
		leftCells.Clear ();
		rightList.Clear ();

		//Start max bound column
		maxCol = cNumbs / 2;

		//Start i index and max bound row
		if (rNumbs % 2 == 0) {
			maxRow = rNumbs / 2;
			for (i = -rNumbs / 2; i < maxRow; i++) {
				n = 0;
				for (j = -cNumbs / 2 + 1; j < maxCol; j++) {
					if (j == 0) {
						if (i == 0) {
							bl = Instantiate (cell, new Vector3 (-distance / 2, 0, 0), Quaternion.identity);
							leftCells.Add (bl);
							cellObjects [m, n++] = bl;
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (distance / 2, 0, 0), Quaternion.identity);
							cellObjects [m, n++] = bl;
							bl.transform.SetParent (bigBlock.transform);
						}

						else {
							bl = Instantiate (cell, new Vector3 (-distance / 2, distance * i, 0), Quaternion.identity);
							leftCells.Add (bl);
							cellObjects [m, n++] = bl;
							bl.transform.SetParent (bigBlock.transform);

							bl = Instantiate (cell, new Vector3 (distance / 2, distance * i, 0), Quaternion.identity);
							cellObjects [m, n++] = bl;
							bl.transform.SetParent (bigBlock.transform);
						}

					}

					else {
						if (j > 0) {
							bl = Instantiate (cell, new Vector3 (distance/2 + distance * j, distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} 

						else {
							bl = Instantiate (cell, new Vector3 (-distance/2 + distance * j, distance * i, 0), Quaternion.identity);
							leftCells.Add (bl);
							bl.transform.SetParent (bigBlock.transform);
						}

						cellObjects [m, n++] = bl;
					}
				}
				m++;
			}

			bigBlock.transform.localPosition = new Vector3 (0, 0.55f * rNumbs / 2 - 1, 0);
		} 

		else {
			maxRow = rNumbs / 2 + 1;
			for (i = -rNumbs / 2; i < maxRow; i++) {
				n = 0;
				for (j = -cNumbs / 2 + 1; j < maxCol; j++) {
					if (j == 0) {
						bl = Instantiate (cell, new Vector3 (-distance / 2, distance * i, 0), Quaternion.identity);
						leftCells.Add (bl);
						cellObjects [m, n++] = bl;
						bl.transform.SetParent (bigBlock.transform);

						bl = Instantiate (cell, new Vector3 (distance / 2, distance * i, 0), Quaternion.identity);
						cellObjects [m, n++] = bl;
						bl.transform.SetParent (bigBlock.transform);
					}

					else {
						if (j > 0) {
							bl = Instantiate (cell, new Vector3 (distance/2 + distance * j, distance * i, 0), Quaternion.identity);
							bl.transform.SetParent (bigBlock.transform);
						} 

						else {
							bl = Instantiate (cell, new Vector3 (-distance/2 + distance * j, distance * i, 0), Quaternion.identity);
							leftCells.Add (bl);
							bl.transform.SetParent (bigBlock.transform);
						}
							
						cellObjects [m, n++] = bl;
					}
				}
				m++;
			}
		}

		if (cNumbs >= 6) {
			float fitScale = 5f;
			float fixedScale = fitScale / cNumbs;
			bigBlock.transform.localScale = new Vector3 (fixedScale, fixedScale, 1);
		}

		foreach (GameObject child in leftCells) {
			child.GetComponent<SpriteRenderer> ().color = Color.black;
			child.GetComponent<SpriteRenderer> ().sprite = child.GetComponent<BlockView> ().sprites [0];
			child.transform.localScale = new Vector3 (0.25f, 0.25f, 1);
			child.GetComponent<BlockView> ().BlockModel.Clicked = true;
		}

		block = bigBlock;

		foreach (Transform child in block.transform) 
			child.gameObject.GetComponent<BlockView> ().register (_OnClick);
		
		SetRightBlock (rightCells, leftCells.Count, leftCells, cNumbs);
	}

	List<int> GenerateRandom(int count, int total)
	{
		// generate count random values.
		HashSet<int> candidates = new HashSet<int>();

		while (candidates.Count < count)
		{
			// May strike a duplicate.
			candidates.Add(Random.Range(0, total));
		}

		// load them in to a list.
		List<int> result = new List<int>();
		result.AddRange(candidates);

		// shuffle the results:
		int i = result.Count;
		while (i > 1)
		{
			i--;
			int k = Random.Range(0, i + 1);
			int value = result[k];
			result[k] = result[i];
			result[i] = value;
		}
		return result;
	}

	void _OnClick( BlockView blockView )
	{
		BlockModel blockModel = blockView.BlockModel;
		if (GameController.instance.GameModel.GameState == GameModel.Gamestate.PLAY && !blockModel.Clicked && GameController.instance.canClicked  ) {
			if (GameController.instance.GameModel.GameMode != GameModel.Gamemode.WHONEW) {
				blockModel.Clicked = true;
			}

			if (blockModel.Right) {
				if (GameController.instance.rightClicked < rightCells) {
					AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [0]);
				}

				GameController.instance.rightClicked++;
				blockView.gameObject.GetComponent<SpriteRenderer> ().color = new Color (0, 0.5376711f, 1, 1);

				//For ordering game, if player click wrong order, show all left over right cells
				if (GameController.instance.GameModel.GameMode == GameModel.Gamemode.ORDER && !CheckOrder (blockView.gameObject)) {
					foreach (GameObject child in rightList) {
						if (child != blockView.gameObject && !child.GetComponent<BlockView> ().BlockModel.Clicked) {
							child.gameObject.GetComponent<SpriteRenderer> ().color = Color.gray;
						}
					}
				}

				if (currentPos == rightCells || GameController.instance.rightClicked == rightCells) {
					GameObject check = Instantiate (bonus [1], blockView.transform, false);
					check.transform.rotation = Quaternion.identity;
				}

			} else {
				GameController.instance.wrongChoice = true;
				GameController.instance.canClicked = false;
				blockView.gameObject.GetComponent<SpriteRenderer> ().color = Color.red;

				if (GameController.instance.GameModel.GameMode != GameModel.Gamemode.WHONEW) {
					//Show all unclicked right cells left over
					foreach (GameObject child in rightList) {
						if (!child.GetComponent<BlockView> ().BlockModel.Clicked) {
							child.GetComponent<SpriteRenderer> ().color = Color.gray;
						}
					}
				} else {
					rightList [rightList.Count - 1].GetComponent<SpriteRenderer> ().color = Color.gray;
				}

				Instantiate (bonus [0], blockView.transform, false);
			}
		}
	}

	//Checking if player click in a right order (For ordering game)
	bool CheckOrder(GameObject check) {
		int count = 0;
		foreach(GameObject child in rightList) {
			if (child == check) {
				if (count == currentPos) {
					currentPos++;
					return true;
				} else {
					GameController.instance.wrongChoice = true;			
				}
			}
			count++;
		}
		return false;
	}

	//Rotate the block (for rotation game)
	public void RotateBlock() {
		if (GameController.instance.GameModel.GameMode == GameModel.Gamemode.ROTATE && block != null && canRotate) {

			//Rotate to the left
			if (rotateDir == 0) {
				block.transform.Rotate (0, 0, Time.deltaTime * speed);

				if (Mathf.Abs (block.transform.localRotation.eulerAngles.z) > (rotateAngle)) {
					block.transform.localRotation = Quaternion.Euler(0, 0, rotateAngle);
					canRotate = false;
					GameController.instance.canClicked = true;
				}
			}

			//Rotate to the right
			else { 
				block.transform.Rotate (0, 0, -Time.deltaTime * speed);

				if (Mathf.Abs (block.transform.localRotation.eulerAngles.z) < (360 - rotateAngle)) {
					block.transform.localRotation = Quaternion.Euler(0, 0, 360 - rotateAngle);
					canRotate = false;
					GameController.instance.canClicked = true;
				}
			}

		}
	}

}
