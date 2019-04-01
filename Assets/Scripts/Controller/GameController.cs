using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameController : MonoBehaviour
{

	public static GameController instance;

	public GameObject mainCam;
	public int rightClicked = 0;
	//number of right clicks
	public List<GameObject> listLife;
	public GameObject[] shapes;
	public GameObject colorBlock;
	public GameObject lprevBlock;

	private int rightCells;
	//number of right cells need to click
	private LevelData lvData;
	private int bestLv;
	public List<GameObject> trash;

	public List<GameObject> Trash {
		get {
			return trash;
		}
		set {
			trash = value;
		}
	}

	public int BestLv {
		get {
			return bestLv;
		}
		set {
			bestLv = value;
		}
	}

	//Index of color of each block for color game
	private int clIndex1;
	private int clIndex2;
	private int clIndex3;
	//Text index of each block for color game
	private int txIndex1;
	private int txIndex2;
	private int txIndex3;
	//Shape index of each block for color game
	private int shIndex1;
	private int shIndex2;
	private int shIndex3;

	GameObject[] colorBlocks;

	public GameObject[] ColorBlocks {
		get {
			return colorBlocks;
		}
		set {
			colorBlocks = value;
		}
	}

	public bool wrongChoice = false;

	public bool canClicked = false;

	GameModel gameModel;

	public GameModel GameModel {
		get {
			return gameModel;
		}
		set {
			gameModel = value;
		}
	}

	ColorModel colorModel;

	public ColorModel ColorModel {
		get {
			return colorModel;
		}
	}

	void Awake ()
	{
		instance = this;
		gameModel = new GameModel ();
	}

	void Start ()
	{
		bestLv = 0;
		colorBlocks = new GameObject[3];
		colorModel = new ColorModel ();
		trash = new List<GameObject> ();
	}

	void Update ()
	{
		if (Input.touchCount > 0) {
			Debug.Log ("Touch");
		}

		if (gameModel.GameState == GameModel.Gamestate.PLAY) {
			if (gameModel.GameMode != GameModel.Gamemode.COLOR && gameModel.GameMode != GameModel.Gamemode.LPREV) { 
				if (gameModel.GameMode == GameModel.Gamemode.SYMMETRY) {
					if (UIController.Instance.slider.value > 0) {
						UIController.Instance.slider.value -= (0.02f * Time.deltaTime);
					} else {
						if (gameModel.level > bestLv) {
							bestLv = gameModel.level;
							PlayerPrefs.SetInt ("BestSymmetry", gameModel.level);
						}
							
						UIController.Instance.UIGameOver ();
					}
				} else if (gameModel.GameMode != GameModel.Gamemode.WHONEW) {
					BlockController.instance.RotateBlock ();
				}
				Playing ();
			} else {
				if (UIController.Instance.slider.value > 0) {
					UIController.Instance.slider.value -= (0.035f * Time.deltaTime);
				} else {
					if (gameModel.level > bestLv) {
						bestLv = gameModel.level;
						if (gameModel.GameMode == GameModel.Gamemode.COLOR) {
							PlayerPrefs.SetInt ("BestColor", gameModel.level);
						} else if (gameModel.GameMode == GameModel.Gamemode.LPREV) {
							PlayerPrefs.SetInt ("BestLPrev", gameModel.level);
						}
					}

					UIController.Instance.UIGameOver ();
				}

				if (gameModel.heart == 0) {
					AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [1]);
					if (gameModel.level > bestLv) {
						bestLv = gameModel.level;
						if (gameModel.GameMode == GameModel.Gamemode.COLOR) {
							PlayerPrefs.SetInt ("BestColor", gameModel.level);
						} else if (gameModel.GameMode == GameModel.Gamemode.LPREV) {
							PlayerPrefs.SetInt ("BestLPrev", gameModel.level);
						}
					}
					gameModel.GameState = GameModel.Gamestate.OVER;
					UIController.Instance.UIGameOver ();
				}
			}
		}
	}

	public void UpdateLife ()
	{
		AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [2]);
		gameModel.heart -= 1;
		UIController.Instance.hearts [gameModel.heart].gameObject.SetActive (false);
	}

	//Playing method to play classic, rotate, ordering games
	void Playing ()
	{
		if (gameModel.heart == 0) {
			if (gameModel.level > bestLv) {
				bestLv = gameModel.level;
				if (gameModel.GameMode == GameModel.Gamemode.CLASSIC) {
					PlayerPrefs.SetInt ("BestClassic", gameModel.level);
				} else if (gameModel.GameMode == GameModel.Gamemode.ROTATE) {
					PlayerPrefs.SetInt ("BestRotate", gameModel.level);
				} else if (gameModel.GameMode == GameModel.Gamemode.ORDER) {
					PlayerPrefs.SetInt ("BestOrder", gameModel.level);
				} else if (gameModel.GameMode == GameModel.Gamemode.SYMMETRY) {
					PlayerPrefs.SetInt ("BestSymmetry", gameModel.level);
				} else {
					PlayerPrefs.SetInt ("BestWhonew", gameModel.level);
				}
			}

			if (BlockController.instance.Coroutine != null) {
				StopCoroutine (BlockController.instance.Coroutine);
			}
			UIController.Instance.UIGameOver ();

		} else if (wrongChoice) {
			UpdateLife ();
			if (gameModel.GameMode == GameModel.Gamemode.WHONEW) {
				BlockController.instance.wrongClicked = wrongChoice;
				BlockController.instance.Coroutine = StartCoroutine (BlockController.instance.ShowWhoNew ());
				wrongChoice = false;
			} else {
				StartCoroutine (SpawnBlock ());
			}
		} else if (rightClicked == rightCells) {
			AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [5]);
			canClicked = false;
			gameModel.level++;
			UIController.Instance.textNumberLv.text = "" + gameModel.level;
			if (gameModel.GameMode == GameModel.Gamemode.WHONEW) {
				BlockController.instance.Coroutine = StartCoroutine (BlockController.instance.ShowWhoNew ());
				rightClicked = 0;
			} else {
				StartCoroutine (SpawnBlock ());
			}
		}
	}

	public void ColorPlay (bool isRight)
	{
		if (gameModel.GameState == GameModel.Gamestate.PLAY && canClicked) {
			canClicked = false;
			if (gameModel.GameMode == GameModel.Gamemode.COLOR) {
				if (isRight) {
					if (clIndex3 == txIndex3) {
						colorBlocks [2].transform.DOMove (new Vector3 (11, 9, 0), 0.4f);
						AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [4]);
						gameModel.level++;
					} else {
						colorBlocks [2].transform.DOMove (new Vector3 (11, -9, 0), 0.4f);
						UpdateLife ();
					}
				} else {
					if (clIndex3 == txIndex3) {
						colorBlocks [2].transform.DOMove (new Vector3 (-11, -9, 0), 0.4f);
						UpdateLife ();
					} else {
						colorBlocks [2].transform.DOMove (new Vector3 (-11, 9, 0), 0.4f);
						AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [4]);
						gameModel.level++;
					}
				}
				BlockController.instance.Coroutine = StartCoroutine (ShiftingWhenClickColor ());

			} else {
				if (isRight) {
					if (clIndex3 == InitController.instance.tempClIndex) {
						colorBlocks [2].transform.DOMove (new Vector3 (11, 9, 0), 0.4f);
						AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [4]);
						gameModel.level++;
					} else {
						colorBlocks [2].transform.DOMove (new Vector3 (11, -9, 0), 0.4f);
						UpdateLife ();
					}
				} else {
					if (clIndex3 == InitController.instance.tempClIndex) {
						colorBlocks [2].transform.DOMove (new Vector3 (-11, -9, 0), 0.4f);
						UpdateLife ();
					} else {
						colorBlocks [2].transform.DOMove (new Vector3 (-11, 9, 0), 0.4f);
						AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [4]);
						gameModel.level++;
					}
				}
				InitController.instance.tempClIndex = clIndex3;
				BlockController.instance.Coroutine = StartCoroutine (ShiftingWhenClickLPrev ());
			}
		}

		UIController.Instance.textNumberLv.text = "" + gameModel.level;

	}


	//Shifting two below pictures up and create new picture for color game
	IEnumerator ShiftingWhenClickColor ()
	{
		yield return new WaitForSeconds (0.2f);
		clIndex3 = clIndex2;
		txIndex3 = txIndex2;
		clIndex2 = clIndex1;
		txIndex2 = txIndex1;
		colorBlocks [2] = colorBlocks [1];
		colorBlocks [1] = colorBlocks [0];
		//Scale back the top picture
		colorBlocks [2].transform.DOMove (new Vector3 (0, 0.5f, -4), 0.15f, false);
		colorBlocks [2].transform.localScale = new Vector3 (1, 1, 1);

		//Scale back the second picture
		colorBlocks [1].transform.DOMove (new Vector3 (0, 0.35f, -2), 0.3f, false);
		colorBlocks [1].transform.localScale = new Vector3 (0.95f, 1, 1);

		int i = Random.Range (0, 2);
		//Create block 1
		GameObject clone;
		colorBlocks [0] = Instantiate (colorBlock, new Vector3 (0, 0.2f, 0), Quaternion.identity);
		trash.Add (colorBlocks [0]);
		colorBlocks [0].transform.localScale = new Vector3 (0.9f, 1, 1);
		//Text1
		txIndex1 = Random.Range (0, colorModel.clNames.Length);
		colorBlocks [0].GetComponentInChildren<TextMesh> ().text = colorModel.clNames [txIndex1];
		colorBlocks [0].GetComponentInChildren<TextMesh> ().color = colorModel.colors [Random.Range (0, colorModel.colors.Length)];
		//Shape1
		clone = Instantiate (shapes [Random.Range (0, shapes.Length)], colorBlocks [0].transform, false);
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

		yield return new WaitForSeconds (0.2f);
		canClicked = true;
	}

	public IEnumerator ShiftingWhenClickLPrev ()
	{
		yield return new WaitForSeconds (0.2f);
		clIndex3 = clIndex2;
		clIndex2 = clIndex1;
		shIndex3 = shIndex2;
		shIndex2 = shIndex1;
		colorBlocks [2] = colorBlocks [1];
		colorBlocks [1] = colorBlocks [0];

		//Scale back the top picture
		colorBlocks [2].transform.DOMove (new Vector3 (0, 0.5f, -4), 0.15f, false);
		colorBlocks [2].transform.localScale = new Vector3 (1, 1, 1);

		//Scale back the second picture
		colorBlocks [1].transform.DOMove (new Vector3 (0, 0.35f, -2), 0.3f, false);
		colorBlocks [1].transform.localScale = new Vector3 (0.95f, 1, 1);

		GameObject clone;
		colorBlocks [0] = Instantiate (lprevBlock, new Vector3 (0, 0.2f, 0), Quaternion.identity);
		trash.Add (colorBlocks [0]);
		colorBlocks [0].transform.localScale = new Vector3 (0.9f, 1, 1);

		int i = Random.Range (0, 2);
		if (i == 0) {
			clone = Instantiate (shapes [shIndex1], colorBlocks [0].transform, false);
		} else {
			shIndex1 = Random.Range (0, shapes.Length);
			clone = Instantiate (shapes [shIndex1], colorBlocks [0].transform, false);
			clIndex1 = Random.Range (0, colorModel.colors.Length);
			while (clIndex1 == clIndex2) {
				clIndex1 = Random.Range (0, colorModel.colors.Length);
			}

		}
		clone.GetComponent<SpriteRenderer> ().color = colorModel.colors [clIndex1];
		clone.transform.localPosition = new Vector3 (0, 0, -1);
		clone.transform.localScale = new Vector3 (2.5f, 2.5f, 1);

		yield return new WaitForSeconds (0.2f);
		canClicked = true;
	}

	IEnumerator SpawnBlock ()
	{
		rightClicked = 0;
		wrongChoice = false;
		BlockController.instance.CanRotate = false;

		if (BlockController.instance.Block != null) {
			foreach (Transform child in BlockController.instance.bigBlock.transform) {
				Destroy (child.gameObject, 0.25f);
			}
				
			BlockController.instance.Block = null;
			yield return new WaitForSeconds (1f);
		}

		if (gameModel.GameState == GameModel.Gamestate.PLAY && BlockController.instance.Block == null) {
			if ((gameModel.level - 1) <= lvData.data [lvData.data.Length - 1].level) {
				if (gameModel.GameMode == GameModel.Gamemode.SYMMETRY) {
					BlockController.instance.CreateSymmetry (lvData.data [gameModel.level - 1], out rightCells);
				} else {
					BlockController.instance.CreateLevel (lvData.data [gameModel.level - 1], out rightCells);
				}
			} else {

			}
		}
	}

	public void OnClassicStart ()
	{
		Input.multiTouchEnabled = true;
		InitController.instance.InitLife ();

		gameModel.GameMode = GameModel.Gamemode.CLASSIC;
		lvData = LayDuLieu ();
		gameModel.GameState = GameModel.Gamestate.PLAY;
		StartCoroutine (SpawnBlock ());

		UIController.Instance.textNumberLv.text = "" + gameModel.level;
	}

	public void OnRotateStart ()
	{
		Input.multiTouchEnabled = true;
		InitController.instance.InitLife ();
		BlockController.instance.CanRotate = false;

		gameModel.GameMode = GameModel.Gamemode.ROTATE;
		lvData = LayDuLieu ();
		gameModel.GameState = GameModel.Gamestate.PLAY;
		StartCoroutine (SpawnBlock ());

		UIController.Instance.textNumberLv.text = "" + gameModel.level;
	}

	public void OnOrderStart ()
	{
		Input.multiTouchEnabled = true;
		InitController.instance.InitLife ();

		gameModel.GameMode = GameModel.Gamemode.ORDER;
		lvData = LayDuLieu ();
		gameModel.GameState = GameModel.Gamestate.PLAY;
		StartCoroutine (SpawnBlock ());

		UIController.Instance.textNumberLv.text = "" + gameModel.level;
	}

	public void OnSymmetryStart ()
	{
		Input.multiTouchEnabled = true;
		InitController.instance.InitLife ();

		gameModel.GameMode = GameModel.Gamemode.SYMMETRY;
		lvData = LayDuLieu ();
		gameModel.GameState = GameModel.Gamestate.PLAY;
		StartCoroutine (SpawnBlock ());

		UIController.Instance.slider.gameObject.SetActive (true);
		UIController.Instance.slider.value = 1;
		UIController.Instance.textNumberLv.text = "" + gameModel.level;
	}

	public void OnWhonewStart ()
	{
		Input.multiTouchEnabled = true;
		InitController.instance.InitLife ();
		rightCells = 1;

		gameModel.GameMode = GameModel.Gamemode.WHONEW;
		gameModel.GameState = GameModel.Gamestate.PLAY;
		BlockController.instance.CreateWhoNew ();

		UIController.Instance.textNumberLv.text = "" + gameModel.level;
	}

	public void OnLPrevStart ()
	{
		Input.multiTouchEnabled = true;
		InitController.instance.InitLife ();
		gameModel.GameMode = GameModel.Gamemode.LPREV;
		gameModel.GameState = GameModel.Gamestate.PLAY;

		UIController.Instance.slider.gameObject.SetActive (true);
		UIController.Instance.slider.value = 1;
		UIController.Instance.textNumberLv.text = "" + gameModel.level;

		InitController.instance.InitLPrev (shapes.Length, out clIndex1, out clIndex2, out clIndex3, out shIndex1, out shIndex2, out shIndex3);
	}

	public void OnColorStart ()
	{
		Input.multiTouchEnabled = true;
		if (colorBlocks != null) {
			for (int i = 0; i < colorBlocks.Length; i++) {
				Destroy (colorBlocks [i]);
			}
		}

		InitController.instance.InitLife ();
		gameModel.GameMode = GameModel.Gamemode.COLOR;
		gameModel.GameState = GameModel.Gamestate.PLAY;

		canClicked = true;

		UIController.Instance.slider.gameObject.SetActive (true);
		UIController.Instance.slider.value = 1;
		UIController.Instance.textNumberLv.text = "" + gameModel.level;

		InitController.instance.InitColorBlocks (shapes.Length, out clIndex1, out clIndex2, out clIndex3,
			out txIndex1, out txIndex2, out txIndex3);
	}

	public void OnReset ()
	{
		Input.multiTouchEnabled = true;

		gameModel.heart = 3;
		gameModel.level = 1;

		InitController.instance.InitLife ();

		if (gameModel.GameMode != GameModel.Gamemode.COLOR && gameModel.GameMode != GameModel.Gamemode.LPREV) {
			if (BlockController.instance.Coroutine != null) {
				StopCoroutine (BlockController.instance.Coroutine);
			}

			if (BlockController.instance.Block != null) {
				foreach (Transform child in BlockController.instance.Block.transform) {
					Destroy (child.gameObject);
				}
			}
				
			gameModel.GameState = GameModel.Gamestate.PLAY;

			if (gameModel.GameMode == GameModel.Gamemode.SYMMETRY) {
				UIController.Instance.slider.value = 1;

				BlockController.instance.Coroutine = StartCoroutine (SpawnBlock ());
			} else if (gameModel.GameMode == GameModel.Gamemode.WHONEW) {
				BlockController.instance.CreateWhoNew ();
			} else {
				BlockController.instance.CanRotate = false;
				BlockController.instance.Coroutine = StartCoroutine (SpawnBlock ());
			}

		} else {
			if (BlockController.instance.Coroutine != null) {
				StopCoroutine (BlockController.instance.Coroutine);
			}

			foreach (GameObject child in trash) {
				Destroy (child);
			}
			trash.Clear ();

			gameModel.GameState = GameModel.Gamestate.PLAY;

			if (gameModel.GameMode == GameModel.Gamemode.COLOR) {
				canClicked = true;
				InitController.instance.InitColorBlocks (shapes.Length, out clIndex1, out clIndex2, out clIndex3,
					out txIndex1, out txIndex2, out txIndex3);
			} else {
				canClicked = false;
				InitController.instance.InitLPrev (shapes.Length, out clIndex1, out clIndex2, out clIndex3,
					out shIndex1, out shIndex2, out shIndex3);
			}
				
			UIController.Instance.slider.value = 1;

		}

		UIController.Instance.textNumberLv.text = "" + gameModel.level;
	}

	public void OnB2Menu ()
	{
		Input.multiTouchEnabled = false;
		gameModel.GameState = GameModel.Gamestate.MENU;

		BlockController.instance.CanRotate = false;

		if (gameModel.GameMode == GameModel.Gamemode.COLOR || gameModel.GameMode == GameModel.Gamemode.LPREV) {
			for (int i = 0; i < colorBlocks.Length; i++) {
				Destroy (colorBlocks [i]);
			}
			UIController.Instance.left.gameObject.SetActive (false);
			UIController.Instance.right.gameObject.SetActive (false);
			canClicked = false;
		} else {
			if (BlockController.instance.Coroutine != null) {
				StopCoroutine (BlockController.instance.Coroutine);
			}

			if (BlockController.instance.Block != null) {
				foreach (Transform child in BlockController.instance.Block.transform) {
					Destroy (child.gameObject);
				}
			}

			BlockController.instance.Block = null;
			UIController.Instance.symmetryBG.gameObject.SetActive (false);
		}
			
		gameModel.heart = 3;
		gameModel.level = 1;
	}

	public void OnGameOver ()
	{
		gameModel.GameState = GameModel.Gamestate.OVER;
	}

	public void OnGamePause ()
	{
		Input.multiTouchEnabled = false;
		gameModel.GameState = GameModel.Gamestate.PAUSE;
		Time.timeScale = 0;
	}

	LevelData LayDuLieu ()
	{
		string json;
		if (gameModel.GameMode == GameModel.Gamemode.CLASSIC) {
			json = "{\"data\":[\n  {\n    \"level\": 1,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 2,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 2,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 3,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 3,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 3,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 4,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 4,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 5,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 5,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 6,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 5,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 7,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 6,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 8,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 7,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 9,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 8,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 10,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 8,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 11,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 9,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 12,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 10,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 13,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 11,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 14,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 12,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 15,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 12,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 16,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 13,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 17,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 14,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 18,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 15,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 19,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 16,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 20,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 17,\n    \"speed\": \"\"\n  }\n]}";
		} else if (gameModel.GameMode == GameModel.Gamemode.ROTATE) {
			json = "{\"data\":[\n  {\n    \"level\": 1,\n    \"cellRowNumbs\": 2,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 0,\n    \"rightCells\": 1,\n    \"speed\": \"70f\"\n  },\n  {\n    \"level\": 2,\n    \"cellRowNumbs\": 2,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 0,\n    \"rightCells\": 2,\n    \"speed\": \"70f\"\n  },\n  {\n    \"level\": 3,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 1,\n    \"speed\": \"80f\"\n  },\n  {\n    \"level\": 4,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 2,\n    \"speed\": \"80f\"\n  },\n  {\n    \"level\": 5,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 3,\n    \"speed\": \"80f\"\n  },\n  {\n    \"level\": 6,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 3,\n    \"speed\": \"100f\"\n  },\n  {\n    \"level\": 7,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 4,\n    \"speed\": \"100f\"\n  },\n  {\n    \"level\": 8,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 5,\n    \"speed\": \"120f\"\n  },\n  {\n    \"level\": 9,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 3,\n    \"speed\": \"140f\"\n  },\n  {\n    \"level\": 10,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 4,\n    \"speed\": \"160f\"\n  },\n  {\n    \"level\": 11,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 5,\n    \"speed\": \"160f\"\n  },\n  {\n    \"level\": 12,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 7,\n    \"speed\": \"180f\"\n  },\n  {\n    \"level\": 13,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 8,\n    \"speed\": \"180f\"\n  },\n  {\n    \"level\": 14,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 9,\n    \"speed\": \"200f\"\n  },\n  {\n    \"level\": 15,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 10,\n    \"speed\": \"200f\"\n  }\n]}";
		} else if (gameModel.GameMode == GameModel.Gamemode.ORDER) {
			json = "{\"data\":[\n  {\n    \"level\": 1,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 2,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 2,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 3,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 3,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 1,\n    \"rightCells\": 4,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 4,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 3,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 5,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 2,\n    \"rightCells\": 4,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 6,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 4,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 7,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 6,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 8,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 3,\n    \"rightCells\": 8,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 9,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 6,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 10,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 8,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 11,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 10,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 12,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 4,\n    \"rightCells\": 11,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 13,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 12,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 14,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 13,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 15,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": \"\",\n    \"blockIndex\": 5,\n    \"rightCells\": 14,\n    \"speed\": \"\"\n  }\n]}";
		} else {
			json = "{\"data\":[\n  {\n    \"level\": 1,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": 4,\n    \"blockIndex\": \"\",\n    \"rightCells\": 2,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 2,\n    \"cellRowNumbs\": 3,\n    \"cellColNumbs\": 4,\n    \"blockIndex\": \"\",\n    \"rightCells\": 3,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 3,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": 4,\n    \"blockIndex\": \"\",\n    \"rightCells\": 4,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 4,\n    \"cellRowNumbs\": 4,\n    \"cellColNumbs\": 4,\n    \"blockIndex\": \"\",\n    \"rightCells\": 5,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 5,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": 6,\n    \"blockIndex\": \"\",\n    \"rightCells\": 5,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 6,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": 6,\n    \"blockIndex\": \"\",\n    \"rightCells\": 6,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 7,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": 6,\n    \"blockIndex\": \"\",\n    \"rightCells\": 7,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 8,\n    \"cellRowNumbs\": 5,\n    \"cellColNumbs\": 6,\n    \"blockIndex\": \"\",\n    \"rightCells\": 8,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 9,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": 6,\n    \"blockIndex\": \"\",\n    \"rightCells\": 8,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 10,\n    \"cellRowNumbs\": 6,\n    \"cellColNumbs\": 6,\n    \"blockIndex\": \"\",\n    \"rightCells\": 9,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 11,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 9,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 12,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 10,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 13,\n    \"cellRowNumbs\": 7,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 11,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 14,\n    \"cellRowNumbs\": 8,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 11,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 15,\n    \"cellRowNumbs\": 8,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 12,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 16,\n    \"cellRowNumbs\": 8,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 13,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 17,\n    \"cellRowNumbs\": 8,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 14,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 18,\n    \"cellRowNumbs\": 9,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 14,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 19,\n    \"cellRowNumbs\": 9,\n    \"cellColNumbs\": 8,\n    \"blockIndex\": \"\",\n    \"rightCells\": 15,\n    \"speed\": \"\"\n  },\n  {\n    \"level\": 20,\n    \"cellRowNumbs\": 10,\n    \"cellColNumbs\": 10,\n    \"blockIndex\": \"\",\n    \"rightCells\": 20,\n    \"speed\": \"\"\n  }\n]}";
		}
		LevelData levelData = JsonUtility.FromJson<LevelData> (json);
		return levelData;
	}

	void OnTriggerExit2D (Collider2D other)
	{
		Destroy (other.gameObject);
	}
		
}
