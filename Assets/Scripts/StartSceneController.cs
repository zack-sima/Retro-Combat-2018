using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour {
    public Image gun1;
    public Image gun2; 
	public InputField playerName;
    public string[] gunNames; 
    public Sprite[] guns; 

	void Start () {
		Cursor.visible = true;
        Time.timeScale = 1; 
		if (PlayerPrefs.GetString ("playerName") != "") {
			playerName.text = PlayerPrefs.GetString ("playerName");
		}
		if (PlayerPrefs.GetString ("gun1") == "") {
            PlayerPrefs.SetString ("gun1", "ak47");
		} 	
		if (PlayerPrefs.GetString ("gun2") == "") {
            PlayerPrefs.SetString ("gun2", "s12k");
		}	
        if (PlayerPrefs.GetString("gun1") == PlayerPrefs.GetString ("gun2")) {
            PlayerPrefs.SetString ("gun2", "ak47");
            if (PlayerPrefs.GetString ("gun1") == PlayerPrefs.GetString ("gun2"))
                PlayerPrefs.SetString ("gun2", "s12k");
        }
        for (int i = 0; i < gunNames.Length; i++) {
            if (gunNames[i] == PlayerPrefs.GetString ("gun1")) {
                gun1.sprite = guns[i];
            }
            if (gunNames[i] == PlayerPrefs.GetString ("gun2")) {
                gun2.sprite = guns[i];
            }
        }
	}
	void Update () {
		PlayerPrefs.SetString ("playerName", playerName.text);
		if (PlayerPrefs.GetInt("loggedIn") == 1) {
			playerName.text = PlayerPrefs.GetString ("accountName");
		}
	} 
	public void loadScene (int sceneCount) {
		PlayerPrefs.SetString ("playerName", playerName.text);
		SceneManager.LoadSceneAsync (sceneCount);
	}
    public void changeLanguage (string language) {
        if (language == "chinese") {
            PlayerPrefs.SetString ("language", "chinese");
        } else
            PlayerPrefs.SetString ("language", "english");
    }
}






