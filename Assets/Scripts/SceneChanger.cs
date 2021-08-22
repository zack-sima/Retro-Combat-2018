using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {
	public void changeScene (int index) {
        PlayerPrefs.SetInt ("inGame", 1);
		SceneManager.LoadSceneAsync (index);
	}
	public void changeGunName (string name) {
		PlayerPrefs.SetString (PlayerPrefs.GetString("gunName"), name);
	}
	public void changeMapName (string name) {
		PlayerPrefs.SetString ("map", name);
	}
	public void changeSelectedGun (string name) {
		//should be either gun1 or gun2
		PlayerPrefs.SetString ("gunName", name);
	}
}
