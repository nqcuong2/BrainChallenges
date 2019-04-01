using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

	public static AudioController instance;
	public AudioClip[] audioClips;
	private AudioSource adSource;

	public AudioSource AdSource {
		get {
			return adSource;
		}
	}

	// Use this for initialization
	void Awake () {
		instance = this;
		adSource = GetComponent<AudioSource> ();
	}

}
