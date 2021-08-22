using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {
    Controller controllerScript; 
    SoldierController player;
    bool nearPlayer;
    public bool isAirplane;
    public GameObject airplaneBulletPrefab; 
    public float shootSpeed;
    float shootDelay;
    public float strafeTimer; 
    float strafeCooldown = 6; 
    public bool semiAuto; 
    public int bulletType; //1 = bomb, 2 = bullet, not implemented yet
    public float airplaneAcceleration;
    public float airplaneMaxAcceleration;
    public Transform propellers; 
    public GameObject user;
    public float health;
    public float maxHealth;
    public float moveSpeed;
    public float rotateSpeed;
    //change on airplane according to map
    public float maxFlyHeight;
    public float lowStrafeHeight; 
    public GameObject turret;
    Vector3 originalPos;
    Quaternion originalRot;
    bool updated;

	private void OnTriggerEnter(Collider other) {
        if (other.name != "Soldier(Clone)" && other.name != "Border") {
            if (user != null && user.GetComponent<SoldierController>().isTank) {
                user.GetComponent<SoldierController>().health = 0;
                user = null; 
            }
        }
	}
	void Start () {
        if (PlayerPrefs.GetInt("vehicles") == 1) {
            Destroy(gameObject);
        }
        controllerScript = GameObject.Find("Controller").GetComponent<Controller>(); 
        originalPos = transform.position;
        originalRot = transform.rotation;
        health = maxHealth;
        turret = transform.GetChild (0).gameObject; 
        player = GameObject.Find ("Player").GetComponent<SoldierController> ();
	}
    public void resetTank () {
        transform.position = originalPos;
        transform.rotation = originalRot;
        health = maxHealth;
        turret.transform.rotation = transform.rotation;
        turret.transform.Rotate (0, 180, 0);
        airplaneAcceleration = 0; 
    }
    public void shootBullet () {
        GameObject insItem = Instantiate(airplaneBulletPrefab, turret.transform.position, turret.transform.rotation);
        insItem.transform.Translate(-6, -2f, -2f);
        insItem.transform.Rotate(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f));
        insItem = Instantiate(airplaneBulletPrefab, turret.transform.position, turret.transform.rotation);
        insItem.transform.Translate(6, -2f, -2f);
        insItem.transform.Rotate(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f));
        controllerScript.audioManager.Play("mg42", 1 - (Vector3.Distance(transform.position, controllerScript.audioManager.gameObject.transform.position) / 280));
	}
	void Update () {
        if (isAirplane) {
            if (airplaneAcceleration >= airplaneMaxAcceleration * 0.5f) {
                transform.Translate(0, Time.deltaTime, 0);
                GetComponent<Rigidbody>().useGravity = false;
            } else {
                GetComponent<Rigidbody>().useGravity = true;
            }
            if (strafeTimer > 0)
                strafeTimer -= Time.deltaTime; 
            if (user == player.gameObject) {
                player.cam.transform.eulerAngles = new Vector3(player.cam.transform.eulerAngles.x, transform.eulerAngles.y, player.cam.transform.eulerAngles.z);
                if (airplaneAcceleration > 0)
                    airplaneAcceleration -= Time.deltaTime * 0.9f;
                if (airplaneAcceleration < 0)
                    airplaneAcceleration = 0;
            } else if (user != null && user.GetComponent<SoldierController>().isTank) {
                //AI airplane mechanics
                user.transform.position = transform.position;
                user.transform.rotation = transform.rotation;
                if (transform.position.y > maxFlyHeight)
                    transform.position = new Vector3(transform.position.x, maxFlyHeight, transform.position.z);
                else {
                    if (airplaneAcceleration < airplaneMaxAcceleration)
                        airplaneAcceleration += Time.deltaTime * moveSpeed / 15f; 
                }
                if (user.GetComponent<SoldierController>().target != null) {
                    if (strafeTimer > 0)
                        strafeTimer -= Time.deltaTime; 
                    Vector3 prevRot = transform.eulerAngles;
                    transform.LookAt(user.GetComponent<SoldierController>().target.transform);
                    Vector3 newRot = transform.eulerAngles;
                    transform.eulerAngles = prevRot;
                    if (transform.position.y < lowStrafeHeight && airplaneAcceleration > airplaneMaxAcceleration * 0.9f)
                        strafeTimer = strafeCooldown; 
                    if (strafeTimer > 0) {
                        if (Mathf.DeltaAngle(transform.eulerAngles.x, 0) < 30) {
                            transform.Rotate(Time.deltaTime * -40, 0, 0);
                        }

                    } else if (Mathf.Abs(Mathf.DeltaAngle(prevRot.y, newRot.y)) > 10f) {
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + Time.deltaTime * 150, transform.eulerAngles.z);
                        if (Mathf.DeltaAngle(newRot.y, transform.eulerAngles.y) > Mathf.DeltaAngle(newRot.y, prevRot.y)) {
                            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + Time.deltaTime * -300, transform.eulerAngles.z);
                        }
                    } else if ((Vector3.Distance(transform.position, user.GetComponent<SoldierController>().target.transform.position) > 240 || strafeTimer > 0) && transform.position.y < 80f) {
                        transform.Rotate(Time.deltaTime * -5, 0, 0);
                    } else if (strafeTimer <= 0){
                        if (Mathf.Abs(Mathf.DeltaAngle(prevRot.x, newRot.x)) > 8f && Mathf.DeltaAngle(transform.eulerAngles.x, 0) > -53) {
                            transform.Rotate(Time.deltaTime * 20, 0, 0);
                        } else {
                            if (strafeTimer <= 0) {
                                shootBullet();
                                strafeTimer = shootSpeed; 
                            }
                        }
                    }
                }
            } else {
                if (airplaneAcceleration > 0)
                    airplaneAcceleration -= Time.deltaTime * 0.9f;
                if (airplaneAcceleration < 0)
                    airplaneAcceleration = 0;
            }
            turret.transform.rotation = transform.rotation;
            propellers.Rotate (0, 0, Time.deltaTime * airplaneAcceleration * 100);
            transform.Translate(Vector3.forward * Time.deltaTime * airplaneAcceleration * 4.8f);
        }
        //late start
        if (!updated) {
            updated = true;
            for (int i = 0; i < player.controllerScript.tanks.Length; i++) {
                if (player.controllerScript.tanks[i] == null) {
                    player.controllerScript.tanks[i] = gameObject;
                    break;
                }
            }
        }
        if (user != null && user.GetComponent<SoldierController> ().target != gameObject && !user.GetComponent<SoldierController>().isTank)
            user = null;

        //detect nearby tank for player; cannot ride if spectating
        if (Vector3.Distance(player.transform.position, transform.position) < 8f && (user == null || !user.GetComponent<SoldierController>().isTank) && !controllerScript.isSpectator) {
            player.tankIsNearby = true;
            player.nearbyTank = gameObject; 
            nearPlayer = true; 
        } else if (nearPlayer) {
            player.tankIsNearby = false;
            nearPlayer = false; 
        }
	}
}
