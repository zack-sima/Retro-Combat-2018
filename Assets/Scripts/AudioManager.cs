using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Sound {
	public string name;
	public AudioClip clip;
	public float volume; 
	[HideInInspector]
	public AudioSource source;
}
public class AudioManager : MonoBehaviour {
	public Sound[] sounds;
	void Awake () {
		foreach (Sound s in sounds) {
			s.source = gameObject.AddComponent<AudioSource>();
		}
	}
	public void Play (string name, float volume) {
		foreach (Sound s in sounds) {
			if (s.name == name) {
				GetComponent<AudioSource>().PlayOneShot(s.clip, s.volume * volume / 10f);
				break;
			}
		}
	}
}
