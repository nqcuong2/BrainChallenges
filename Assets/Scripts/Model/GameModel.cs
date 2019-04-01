using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel {

	public int heart;
	public bool end;
	public int level;
	public enum Gamestate { START, MENU, PLAY, PAUSE, SETTINGS, TUT, OVER, QUIT };
	public enum Gamemode { CLASSIC, ROTATE, ORDER, COLOR, LPREV, SYMMETRY, WHONEW };

	Gamestate gameState;

	public Gamestate GameState {
		get {
			return gameState;
		}
		set {
			gameState = value;
		}
	}

	Gamemode gameMode;

	public Gamemode GameMode {
		get {
			return gameMode;
		}
		set {
			gameMode = value;
		}
	}


	public GameModel() {
		level = 1;
		heart = 3;
		end = false;
		gameState = Gamestate.START;
	}

}
