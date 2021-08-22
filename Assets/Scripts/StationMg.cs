using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationMg : MonoBehaviour {
    SoldierController player;
    bool changedPlayer; 
	void Start () {
        player = GameObject.Find ("Player").GetComponent<SoldierController> ();
        if (player.controllerScript.isPointCapture)
            Destroy(gameObject);
	}
	void Update () {
        if (Vector3.Distance (player.transform.position, transform.position) < 2.5f) {
            if (!changedPlayer) {
                player.changeWeapon (3);
                changedPlayer = true;
                foreach (Renderer i in GetComponentsInChildren<Renderer> ()) {
                    i.enabled = false; 
                }
                //transform.GetChild (1).GetComponent<Renderer> ().enabled = false;
            }
        } else if (changedPlayer) {
            player.changeWeapon (1);
            changedPlayer = false; 
            foreach (Renderer i in GetComponentsInChildren<Renderer> ()) {
                i.enabled = true;
            }
        }
	}
}



