using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBullet : MonoBehaviour {
    public bool explode;
    public float explosionRadius;
    public float detectionRadius; 
    public float damage; 
    public float travelSpeed;
    public GameObject explosionPrefab;
    public float immuneCountDown;

    void Start () {
        transform.Translate (Vector3.forward * 6);
    }
    void Update () {
        transform.Translate (Vector3.forward * Time.deltaTime * travelSpeed);
        if (immuneCountDown > 0)
            immuneCountDown -= Time.deltaTime;
        else if (Physics.CheckBox (transform.position, new Vector3 (detectionRadius, detectionRadius, detectionRadius))) {
            checkColls ();
        }
	}
    void checkColls () {
        foreach (Collider coll in Physics.OverlapBox (transform.position, new Vector3(explosionRadius, explosionRadius, explosionRadius))) {
            if (coll.GetComponent<Tank> () != null && coll.GetComponent<Tank> ().user != null) {
                coll.GetComponent<Tank> ().user.GetComponent<SoldierController> ().health -= Random.Range (damage * 1.5f, damage * 2.5f);
            } else if (coll.GetComponent<SoldierController>()) {
                coll.GetComponent<SoldierController>().health -= Random.Range (damage * 0.8f, damage * 1.2f);
            }
        }
        if (explode) {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Controller controllerScript = GameObject.Find ("Controller").GetComponent<Controller> ();
            controllerScript.audioManager.Play ("explosion", 1f - (Vector3.Distance (transform.position, controllerScript.player.transform.position) / 200));
        }
        Destroy (gameObject);
    }
}
