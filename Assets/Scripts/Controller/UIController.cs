using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

	public static UIController Instance;

	public Canvas masterCanvas;

	public Slider slider;
	public Button left;
	public Button right;
	public Image symmetryBG;
	public Image[] hearts;
	public ScrollRect scRect;
	public ScrollRect menuScRect;
	public Button soundToggle;
	public Button soundIcon;
	public Sprite[] soundOnOff;
	public RectTransform[] tutContents;
	public Dropdown settingsDropdown;
	public RectTransform menuContents;
	public RectTransform startBG;
	public RectTransform goverBG;
	public Button[] tutBtns;

	public Text textNumberLv;
	public Text bestLvText;
	public Text bestText;
	public Text finishText;

	private GameModel.Gamestate gState;
	private GameModel.Gamemode gMode;
	private Vector2 origin;
	private int gameCount;

	public GameModel.Gamemode GMode {
		get {
			return gMode;
		}
	}

	void Awake ()
	{
		Instance = this;
	}

	void Start ()
	{
		gameCount = 0;
		SetScreen ();
		Input.multiTouchEnabled = false;
		origin = menuScRect.content.anchoredPosition;

		if (!PlayerPrefs.HasKey ("Sound")) {
			PlayerPrefs.SetInt ("Sound", 1);
		}

		AudioController.instance.AdSource.volume = PlayerPrefs.GetInt ("Sound");

		soundToggle.GetComponentInChildren<Image> ().sprite = soundOnOff [PlayerPrefs.GetInt ("Sound")];
		soundIcon.GetComponentInChildren<Image> ().sprite = soundOnOff [PlayerPrefs.GetInt ("Sound") + 2];
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			switch (GameController.instance.GameModel.GameState) {
			case GameModel.Gamestate.START:
				//Quit
				DoozyUI.UIManager.ShowUiElement ("OverlayPanel");
				DoozyUI.UIManager.ShowUiElement ("QuitPanel");
				GameController.instance.GameModel.GameState = GameModel.Gamestate.QUIT;
				break;
			case GameModel.Gamestate.MENU:
				DoozyUI.UIManager.ShowUiElement ("StartPanel");
				GameController.instance.GameModel.GameState = GameModel.Gamestate.START;
				break;
			case GameModel.Gamestate.SETTINGS:
				DoozyUI.UIManager.HideUiElement ("SettingsPanel");
				DoozyUI.UIManager.HideUiElement ("OverlayPanel");
				DoozyUI.UIManager.SendGameEvent ("BackFromSettings");
				break;
			case GameModel.Gamestate.PLAY:
				//Pause
				Time.timeScale = 0;
				DoozyUI.UIManager.ShowUiElement ("OverlayPanel");
				DoozyUI.UIManager.ShowUiElement ("PausePanel");
				GameController.instance.GameModel.GameState = GameModel.Gamestate.PAUSE;
				break;
			case GameModel.Gamestate.OVER:
				//Back2Menu
				onclickBtnMenu ();
				break;
			case GameModel.Gamestate.PAUSE: 
				//Playing
				Time.timeScale = 1;
				DoozyUI.UIManager.HideUiElement ("PausePanel");
				DoozyUI.UIManager.HideUiElement ("OverlayPanel");
				GameController.instance.GameModel.GameState = GameModel.Gamestate.PLAY;
				break;
			case GameModel.Gamestate.TUT:
				DoozyUI.UIManager.HideUiElement ("TutPanel");
				onclickPlay ();
				break;
			case GameModel.Gamestate.QUIT:
				DoozyUI.UIManager.HideUiElement ("QuitPanel");
				DoozyUI.UIManager.HideUiElement ("OverlayPanel");
				GameController.instance.GameModel.GameState = GameModel.Gamestate.START;
				break;
			}
		}	
	}

	//Set camera following screen resolution
	public void SetScreen() {
		Vector2 targetAspect1 = new Vector2(9, 16);
		Vector2 targetAspect2 = new Vector2 (6, 8);
 
		// Determine ratios of screen/window & target, respectively.
		float screenRatio = Screen.width / (float)Screen.height;
		float targetRatio1 = targetAspect1.x / targetAspect1.y;
		float targetRatio2 = targetAspect2.x / targetAspect2.y;

		if (!Mathf.Approximately (screenRatio, targetRatio2)) {
			masterCanvas.renderMode = RenderMode.WorldSpace;
			BlockController.instance.bigBlockSize = Vector3.one;

			if (Mathf.Approximately (screenRatio, targetRatio1)) {
				// Screen or window is the target aspect ratio: use the whole area.
				GameController.instance.mainCam.GetComponent<Camera> ().rect = new Rect (0, 0, 1, 1);
			} else if (screenRatio > targetRatio1 || screenRatio > targetRatio2) {
				// Screen or window is wider than the target: pillarbox.
				float normalizedWidth = targetRatio1 / screenRatio;
				float barThickness = (1f - normalizedWidth) / 2f;
				GameController.instance.mainCam.GetComponent<Camera> ().rect = new Rect (barThickness, 0, normalizedWidth, 1);
			} else {
				// Screen or window is narrower than the target: letterbox.
				float normalizedHeight = screenRatio / targetRatio1;
				float barThickness = (1f - normalizedHeight) / 2f;
				GameController.instance.mainCam.GetComponent<Camera> ().rect = new Rect (0, barThickness, 1, normalizedHeight);
			}
		} else {
			masterCanvas.renderMode = RenderMode.ScreenSpaceCamera;
			BlockController.instance.bigBlockSize = new Vector3(1.2f, 1.2f, 1);

			startBG.offsetMin = new Vector2 (startBG.offsetMin.x, -50);
			startBG.offsetMax = new Vector2 (startBG.offsetMax.x, 25);

			goverBG.offsetMin = new Vector2 (goverBG.offsetMin.x, -50);
			goverBG.offsetMax = new Vector2 (goverBG.offsetMax.x, 50);

			menuContents.offsetMin = new Vector2 (menuContents.offsetMin.x, -1418.6f);
			menuContents.offsetMax = new Vector2 (menuContents.offsetMax.x, 0);

			left.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().sizeDelta.x / 2f, left.gameObject.GetComponent<RectTransform> ().sizeDelta.y);
			right.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().sizeDelta.x / 2f, right.gameObject.GetComponent<RectTransform> ().sizeDelta.y);
		}
	}

	public void UITutorial ()
	{	
		gState = GameController.instance.GameModel.GameState;

		if (gState == GameModel.Gamestate.PAUSE) {
			tutBtns [1].gameObject.SetActive (true);
		} else {
			tutBtns [0].gameObject.SetActive (true);
		}

		foreach (Transform child in tutContents) {
			child.gameObject.SetActive (false);
		}

		switch (GameController.instance.GameModel.GameMode) {
		case GameModel.Gamemode.CLASSIC:
			tutContents [0].gameObject.SetActive (true);
			tutContents [7].gameObject.SetActive (true);

			scRect.content = tutContents [0];
			scRect.gameObject.GetComponent<ScrollSnapRect> ().pageSelectionIcons = tutContents [7];
			scRect.GetComponent<ScrollSnapRect> ().ChangeTut ();
			break;
		case GameModel.Gamemode.ROTATE:
			tutContents [1].gameObject.SetActive (true);
			tutContents [7].gameObject.SetActive (true);

			scRect.content = tutContents [1];
			scRect.gameObject.GetComponent<ScrollSnapRect> ().pageSelectionIcons = tutContents [7];
			scRect.GetComponent<ScrollSnapRect> ().ChangeTut ();
			break;
		case GameModel.Gamemode.ORDER:
			tutContents [2].gameObject.SetActive (true);
			tutContents [7].gameObject.SetActive (true);

			scRect.content = tutContents [2];
			scRect.gameObject.GetComponent<ScrollSnapRect> ().pageSelectionIcons = tutContents [7];
			scRect.GetComponent<ScrollSnapRect> ().ChangeTut ();
			break;
		case GameModel.Gamemode.COLOR:
			tutContents [3].gameObject.SetActive (true);
			tutContents [8].gameObject.SetActive (true);

			scRect.content = tutContents [3];
			scRect.gameObject.GetComponent<ScrollSnapRect> ().pageSelectionIcons = tutContents [8];
			scRect.GetComponent<ScrollSnapRect> ().ChangeTut ();
			break;
		case GameModel.Gamemode.LPREV:
			tutContents [4].gameObject.SetActive (true);
			tutContents [7].gameObject.SetActive (true);

			scRect.content = tutContents [4];
			scRect.gameObject.GetComponent<ScrollSnapRect> ().pageSelectionIcons = tutContents [7];
			scRect.GetComponent<ScrollSnapRect> ().ChangeTut ();
			break;
		case GameModel.Gamemode.SYMMETRY:			
			tutContents [5].gameObject.SetActive (true);
			tutContents [8].gameObject.SetActive (true);

			scRect.content = tutContents [5];
			scRect.gameObject.GetComponent<ScrollSnapRect> ().pageSelectionIcons = tutContents [8];
			scRect.GetComponent<ScrollSnapRect> ().ChangeTut ();
			break;
		case GameModel.Gamemode.WHONEW:
			tutContents [6].gameObject.SetActive (true);
			tutContents [7].gameObject.SetActive (true);

			scRect.content = tutContents [6];
			scRect.gameObject.GetComponent<ScrollSnapRect> ().pageSelectionIcons = tutContents [7];
			scRect.GetComponent<ScrollSnapRect> ().ChangeTut ();
			break;
		}

		DoozyUI.UIManager.ShowUiElement ("TutPanel");
		GameController.instance.GameModel.GameState = GameModel.Gamestate.TUT;
	}

	public void UIGameOver ()
	{
		Input.multiTouchEnabled = false;
		AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [1]);
		DoozyUI.UIManager.ShowUiElement ("GameOverPanel");
		GameController.instance.GameModel.GameState = GameModel.Gamestate.OVER;
		bestLvText.gameObject.SetActive (true);
		bestText.gameObject.SetActive (true);
		finishText.gameObject.SetActive (false);
		bestLvText.text = "" + GameController.instance.BestLv;
		gameCount++;
	}

	public void UIFinishGame ()
	{
		DoozyUI.UIManager.ShowUiElement ("GameOverPanel");
		GameController.instance.GameModel.GameState = GameModel.Gamestate.OVER;
		bestLvText.gameObject.SetActive (false);
		bestText.gameObject.SetActive (false);
		finishText.gameObject.SetActive (true);
	}

	public void onclickBtnYes ()
	{
		Application.Quit ();
	}

	public void onclickPlay ()
	{
		if (gState == GameModel.Gamestate.PAUSE) {
			tutBtns [1].gameObject.SetActive (false);
			DoozyUI.UIManager.HideUiElement ("TutPanel");
			GameController.instance.GameModel.GameState = gState;
		} else {
			tutBtns [0].gameObject.SetActive (false);
			GameController.instance.GameModel.GameState = gState;
			gMode = GameController.instance.GameModel.GameMode;
			scRect.horizontalNormalizedPosition = 0;

			switch (GameController.instance.GameModel.GameMode) {
			case GameModel.Gamemode.CLASSIC:
				GameController.instance.OnClassicStart ();
				break;
			case GameModel.Gamemode.ROTATE:
				GameController.instance.OnRotateStart ();
				break;
			case GameModel.Gamemode.ORDER:
				GameController.instance.OnOrderStart ();
				break;		
			case GameModel.Gamemode.COLOR:
				GameController.instance.OnColorStart ();
				break;
			case GameModel.Gamemode.LPREV:
				GameController.instance.OnLPrevStart ();
				break;
			case GameModel.Gamemode.SYMMETRY:
				GameController.instance.OnSymmetryStart ();
				break;
			case GameModel.Gamemode.WHONEW:
				GameController.instance.OnWhonewStart ();
				break;
			}

			GameController.instance.BestLv = 1;
		}
	}

	public void onclickSound ()
	{
		if (PlayerPrefs.GetInt ("Sound") == 1) {
			soundToggle.GetComponentInChildren<Image> ().sprite = soundOnOff [0];
			soundIcon.GetComponentInChildren<Image> ().sprite = soundOnOff [2];
			AudioController.instance.AdSource.volume = 0;
			PlayerPrefs.SetInt ("Sound", 0);
		} else {
			soundToggle.GetComponentInChildren<Image> ().sprite = soundOnOff [1];
			soundIcon.GetComponentInChildren<Image> ().sprite = soundOnOff [3];
			AudioController.instance.AdSource.volume = 1;
			PlayerPrefs.SetInt ("Sound", 1);
		}
	}

	public void onclickBtnMenu ()
	{
		Time.timeScale = 1;
		DoozyUI.UIManager.ShowUiElement ("MenuPanel");

		if (GameController.instance.GameModel.GameState == GameModel.Gamestate.OVER) {
			DoozyUI.UIManager.HideUiElement ("GameOverPanel");
		} else {
			DoozyUI.UIManager.HideUiElement ("PausePanel");
			DoozyUI.UIManager.HideUiElement ("OverlayPanel");
		}

		GameController.instance.OnB2Menu ();
		slider.gameObject.SetActive (false);
	}

	public void onclickBtnAgain ()
	{
		DoozyUI.UIManager.SendGameEvent ("Reset");
	}

	public void onclickBtnPause ()
	{
		DoozyUI.UIManager.SendGameEvent ("GamePause");
	}

	public void onclickBtnContinue ()
	{
		Input.multiTouchEnabled = true;
		DoozyUI.UIManager.HideUiElement ("PausePanel");
		DoozyUI.UIManager.HideUiElement ("OverlayPanel");
		Time.timeScale = 1;
		GameController.instance.GameModel.GameState = GameModel.Gamestate.PLAY;
	}

	public void OnGameEvent (string gameEvent)
	{
		switch (gameEvent) {
		case "NoButton":
			DoozyUI.UIManager.HideUiElement ("QuitPanel");
			DoozyUI.UIManager.HideUiElement ("OverlayPanel");
			GameController.instance.GameModel.GameState = GameModel.Gamestate.START;
			break;
		case "ButtonClick":
			AudioController.instance.AdSource.PlayOneShot (AudioController.instance.audioClips [3]);
			break;
		case "B2Start":
			GameController.instance.GameModel.GameState = GameModel.Gamestate.START;
			DoozyUI.UIManager.ShowUiElement ("StartPanel");
			break;
		case "ToQuit":
			GameController.instance.GameModel.GameState = GameModel.Gamestate.QUIT;
			DoozyUI.UIManager.ShowUiElement ("OverlayPanel");
			DoozyUI.UIManager.ShowUiElement ("QuitPanel");
			break;
		case "ToTut":
			gState = GameController.instance.GameModel.GameState;
			UITutorial ();
			break;
		case "BackFromSettings":
			GameController.instance.GameModel.GameState = gState;
			break;
		case "ToSettings":
			gState = GameController.instance.GameModel.GameState;
			GameController.instance.GameModel.GameState = GameModel.Gamestate.SETTINGS;
			DoozyUI.UIManager.ShowUiElement ("OverlayPanel");
			DoozyUI.UIManager.ShowUiElement ("SettingsPanel");
			break;
		case "ToStart":
			GameController.instance.GameModel.GameState = GameModel.Gamestate.START;
			break;
		case "ClassicStart":
			GameController.instance.BestLv = PlayerPrefs.GetInt ("BestClassic", 0);

			if (GameController.instance.BestLv == 0) {
				GameController.instance.GameModel.GameMode = GameModel.Gamemode.CLASSIC;
				UITutorial ();			
			} else {
				GameController.instance.OnClassicStart ();
			}
			break;
		case "RotateStart":
			GameController.instance.BestLv = PlayerPrefs.GetInt ("BestRotate", 0);

			if (GameController.instance.BestLv == 0) {
				GameController.instance.GameModel.GameMode = GameModel.Gamemode.ROTATE;
				UITutorial ();			
			} else {
				GameController.instance.OnRotateStart ();
			}
			break;
		case "OrderStart":
			GameController.instance.BestLv = PlayerPrefs.GetInt ("BestOrder", 0);

			if (GameController.instance.BestLv == 0) {
				GameController.instance.GameModel.GameMode = GameModel.Gamemode.ORDER;
				UITutorial ();			
			} else {
				GameController.instance.OnOrderStart ();
			}
			break;
		case "ColorStart":
			GameController.instance.BestLv = PlayerPrefs.GetInt ("BestColor", 0);
			left.gameObject.SetActive (true);
			right.gameObject.SetActive (true);

			if (GameController.instance.BestLv == 0) {
				GameController.instance.GameModel.GameMode = GameModel.Gamemode.COLOR;
				UITutorial ();			
			} else {
				GameController.instance.OnColorStart ();
			}
			break;
		case "LPrevStart":
			GameController.instance.BestLv = PlayerPrefs.GetInt ("BestLPrev", 0);
			left.gameObject.SetActive (true);
			right.gameObject.SetActive (true);

			if (GameController.instance.BestLv == 0) {
				GameController.instance.GameModel.GameMode = GameModel.Gamemode.LPREV;
				UITutorial ();			
			} else {
				GameController.instance.OnLPrevStart ();
			}
			break;
		case "SymmetryStart":
			GameController.instance.BestLv = PlayerPrefs.GetInt ("BestSymmetry", 0);

			if (GameController.instance.BestLv == 0) {
				GameController.instance.GameModel.GameMode = GameModel.Gamemode.SYMMETRY;
				UITutorial ();			
			} else {
				GameController.instance.OnSymmetryStart ();
			}

			symmetryBG.gameObject.SetActive (true);
			break;
		case "WhonewStart":
			GameController.instance.BestLv = PlayerPrefs.GetInt ("BestWhonew", 0);

			if (GameController.instance.BestLv == 0) {
				GameController.instance.GameModel.GameMode = GameModel.Gamemode.WHONEW;
				UITutorial ();			
			} else {
				GameController.instance.OnWhonewStart ();
			}		
			break;
		case "StartPress":
			DoozyUI.UIManager.HideUiElement ("StartPanel");
			menuScRect.content.anchoredPosition = origin;
			GameController.instance.GameModel.GameState = GameModel.Gamestate.MENU;
			ScrollSnapRect.instance.DecelerateTime = Time.deltaTime;
			break;
		case "Rate":
			break;
		case "LDBoard":
			break;
		case "GamePause":
			GameController.instance.OnGamePause ();
			break;
		case "Reset":
			Time.timeScale = 1;
			DoozyUI.UIManager.HideUiElement ("GameOverPanel");
			DoozyUI.UIManager.HideUiElement ("PausePanel");
			DoozyUI.UIManager.HideUiElement ("OverlayPanel");
			GameController.instance.OnReset ();
			break;
		}
	}
		
}
