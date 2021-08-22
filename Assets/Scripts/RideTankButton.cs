using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RideTankButton : MonoBehaviour {
    SoldierController player; 

	void Start () {
        player = GameObject.Find ("Player").GetComponent<SoldierController> ();
	}
	
	void Update () {
        if (player.tankIsNearby || player.isTank) {
            GetComponent<Image> ().enabled = true;
            transform.GetChild (0).GetComponent<Text> ().enabled = true; 

            if (!player.isTank) {
                if (PlayerPrefs.GetString ("language") == "chinese")
                    transform.GetChild (0).GetComponent<Text> ().text = "驾驶坦克";
                else
                    transform.GetChild (0).GetComponent<Text> ().text = "Drive Tank";
            } else {
                if (PlayerPrefs.GetString ("language") == "chinese")
                    transform.GetChild (0).GetComponent<Text> ().text = "离开坦克";
                else
                    transform.GetChild (0).GetComponent<Text> ().text = "Exit Tank";
            }
        } else {
            GetComponent<Image> ().enabled = false;
            transform.GetChild (0).GetComponent<Text> ().enabled = false; 
        }
	}
    public void rideTank () {
        player.rideTank = true; 
    }
}
