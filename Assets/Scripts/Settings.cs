using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
    public Slider sensitivitySlider;
    public Slider renderDistanceSlider;
    public Toggle useJoystickToggle; 

	void Start () {
		if (PlayerPrefs.GetInt ("sensitivity") != 0) {
            sensitivitySlider.value = (float)PlayerPrefs.GetInt ("sensitivity"); 
		}
        if (PlayerPrefs.GetInt("renderDistance") != 0) {
            renderDistanceSlider.value = (float)PlayerPrefs.GetInt("renderDistance");
        }
        if (PlayerPrefs.GetInt("movingJoystick") == 0)
            useJoystickToggle.isOn = false;
        else
            useJoystickToggle.isOn = true; 
	}
	void Update () {
        PlayerPrefs.SetInt("sensitivity", (int)sensitivitySlider.value);
        PlayerPrefs.SetInt("renderDistance", (int)renderDistanceSlider.value);
        if (useJoystickToggle.isOn)
            PlayerPrefs.SetInt("movingJoystick", 1);
        else
            PlayerPrefs.SetInt("movingJoystick", 0);
	} 
	public void loadScene (int sceneCount) {
		SceneManager.LoadSceneAsync (sceneCount);
	}
    public void changeLanguage (string language) {
        if (language == "chinese") {
            PlayerPrefs.SetString ("language", "chinese");
        } else
            PlayerPrefs.SetString ("language", "english");
    }
    public void changeEnemyColor (string color) {
        if (color == "red") {
            PlayerPrefs.SetInt("useRed", 1);
            PlayerPrefs.SetInt("useBlue", 0);
        }
        if (color == "grey") {
            PlayerPrefs.SetInt("useRed", 0);
            PlayerPrefs.SetInt("useBlue", 0);
        }
        if (color == "blue") {
            PlayerPrefs.SetInt("useRed", 0);
            PlayerPrefs.SetInt("useBlue", 1);
        }
    }
}