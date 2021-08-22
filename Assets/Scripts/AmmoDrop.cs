using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoDrop : MonoBehaviour {
    SoldierController player; 
	void Start () {
        player = GameObject.Find("Player").GetComponent<SoldierController>(); 
	}
	void Update () {
        if (Vector3.Distance(player.transform.position, transform.position) < 3f) {
            if (player.currentGunClipSize > 30) {
                player.currentGunTotalBullets += 30; 
            } else 
                player.currentGunTotalBullets += player.currentGunClipSize; 
            Destroy(gameObject);
        }
	}
}
