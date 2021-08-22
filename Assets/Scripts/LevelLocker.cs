using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class LevelLocker : MonoBehaviour {
    public int unlockLevel;
    public bool active; 
	
	void Start () {
        active = false; 
        var xp = PlayerPrefs.GetInt ("xp");
        int level = 1;
        for (int i = 1; xp >= 25 * i + 75; i++) {
            level++;
            xp -= 25 * i + 75;
        }
        if (level < unlockLevel) {
            GetComponent<Button> ().enabled = false; 
            if (PlayerPrefs.GetString ("language") == "chinese") {
                transform.GetChild (0).GetComponent<Text> ().text = "未解锁 (等级 " + unlockLevel.ToString () + ")";
            } else
                transform.GetChild (0).GetComponent<Text> ().text = "Locked (lv. " + unlockLevel.ToString () + ")";
            active = true; 
        }
	}
}
