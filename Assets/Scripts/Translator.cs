using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Translator : MonoBehaviour {
    public string chineseTranslateText;
    string englishOriginalText;
    string deltaLang; 

    void Start () {
        englishOriginalText = GetComponent<Text> ().text;
        deltaLang = PlayerPrefs.GetString ("language");
        updateLanguage ();
    }
    private void Update () {
        if (PlayerPrefs.GetString ("language") != deltaLang) {
            deltaLang = PlayerPrefs.GetString ("language");
            updateLanguage ();
        }
    }
    void updateLanguage () {
        if (transform.parent == null || transform.parent.GetComponent<LevelLocker> () == null || !transform.parent.GetComponent<LevelLocker> ().active) {
            if (PlayerPrefs.GetString ("language") == "chinese") {
                GetComponent<Text> ().text = chineseTranslateText;
            } else
                GetComponent<Text> ().text = englishOriginalText;
        }
	}
}
