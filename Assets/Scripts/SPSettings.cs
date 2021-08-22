using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SPSettings : MonoBehaviour {
	public InputField allyCountInput;
	public InputField enemyCountInput;
    public string[] mapNames; 
    public Sprite[] maps;
    public Image map;
    public Toggle spectatorToggle;
    public Toggle vehiclesToggle;
    public Toggle pointCaptureToggle; 

	void Start () {
		Cursor.visible = true;
		if (PlayerPrefs.GetInt ("allyAmount") != 0) 
			allyCountInput.text = PlayerPrefs.GetInt ("allyAmount").ToString ();
		if (PlayerPrefs.GetInt ("enemyAmount") != 0) 
			enemyCountInput.text = PlayerPrefs.GetInt ("enemyAmount").ToString ();
        if (PlayerPrefs.GetInt("vehicles") == 0)
            vehiclesToggle.isOn = true;
		if (PlayerPrefs.GetString ("map") == "") 
            PlayerPrefs.SetString ("map", "mountains");
        if (PlayerPrefs.GetInt("pointCapture") == 1) 
            pointCaptureToggle.isOn = true; 
        if (PlayerPrefs.GetInt("spectator") == 1) 
            spectatorToggle.isOn = true;
        for (int i = 0; i < mapNames.Length; i++) {
            if (mapNames[i] == PlayerPrefs.GetString ("map")) {
                map.sprite = maps[i];
            }
        }
	}
	void Update () {
        
	} 
    //load scene and saves selections
	public void loadScene (int sceneCount) {
        if (pointCaptureToggle.isOn && int.Parse(allyCountInput.text) > int.Parse(enemyCountInput.text)) {
            allyCountInput.text = enemyCountInput.text;
        }
		if (allyCountInput.text == "" || allyCountInput.text == "-" || int.Parse (allyCountInput.text) <= 0) {
			allyCountInput.text = "1";
		}
		if (enemyCountInput.text == "" || enemyCountInput.text == "-" || int.Parse (enemyCountInput.text) <= 0) {
			enemyCountInput.text = "1";
		}
		PlayerPrefs.SetInt ("allyAmount", int.Parse(allyCountInput.text));
		PlayerPrefs.SetInt ("enemyAmount", int.Parse(enemyCountInput.text));

        if (spectatorToggle.isOn)
            PlayerPrefs.SetInt("spectator", 1);
        else
            PlayerPrefs.SetInt("spectator", 0);
        //vehicles refers to remove tanks and airplanes
        if (vehiclesToggle.isOn)
            PlayerPrefs.SetInt("vehicles", 0);
        else
            PlayerPrefs.SetInt("vehicles", 1);
        if (pointCaptureToggle.isOn) {
            PlayerPrefs.SetInt("pointCapture", 1);
        } else
            PlayerPrefs.SetInt("pointCapture", 0);
        SceneManager.LoadScene (sceneCount);
	}
    public void changeLanguage (string language) {
        if (language == "chinese") {
            PlayerPrefs.SetString ("language", "chinese");
        } else
            PlayerPrefs.SetString ("language", "english");
    }
}