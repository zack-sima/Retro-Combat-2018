using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

[System.Serializable]
public class PosAndRot {
    public float[] position;
    public float[] rotation;
    public float[] gunRotation;
}
[System.Serializable]
public class Strings {
    public string[] text;
}

public class SoldierController : MonoBehaviour {
    public string currentGunName;
    public Sprite redHoverArrow;
    GameObject crosshair;

    //for player
    public GameObject gun1;
    public GameObject gun2;
    public GameObject stationMg; 

    //ai
    public GameObject target;
    public float changeTargetCooldown;
    float changeTargetDelay;

    public Material redMaterial;
    public Material blueMaterial; 
    public Material greyMaterial;
    public Material greyGreenMaterial;

    public bool cannotAim;

    public bool isPlayer;
    public bool isAlly;
    public bool isTank;
    public bool tankIsNearby;
    public bool rideTank; 
    public GameObject currentTank;
    public GameObject nearbyTank;
    public GameObject tankWreckPrefab;
    public GameObject tankBulletPrefab; 
    public GameObject explosionPrefab; 
    public GameObject ammoDropPrefab; 

    public float scrollerSpeed;
    public float moveSpeed;

    public float maxHealth;
    float originalMaxHealth; 
    public float health;
    public bool recentDeath; 

    float recentShootingTimer;
    public bool recentShooting; 

    //gun properties
    float damage;
    float bulletsPerFire;
    public float shootCooldown;
    public float shootDelay;
    float recoil;
    float shootingDistance;
    float zoom;
    float aimSpeed; 
    float camUpAim;
    GameObject gunClip;
    public GameObject gun;
    float length;
    float muzzleLength;
    float aiMistakeRotation;
    float mistakeRotation;
    float reloadCooldown;
    public float reloadDelay;
    float currentGunClipPosX;
    float currentGunClipPosY;
    float currentGunClipPosZ;
    [HideInInspector]
    public bool hasScope;
    float sideRecoil;
    public float gunReloadUp;
    public bool semiAuto;

    public float gun1ClipBullets;
    public float gun2ClipBullets;
    public float gun1TotalBullets;
    public float gun2TotalBullets;
    public float currentGunClipBullets;
    public float currentGunTotalBullets;
    public float currentGunClipSize;

    Vector3 targetPosition;
    Quaternion targetRotation;
    Quaternion prevRotation;

    float recoilRotation;

    public GameObject gunHold;

    float sprint = 1;

    public GameObject head;
    public GameObject leftLeg;
    public GameObject rightLeg;
    public GameObject leftArm;
    public GameObject rightArm;
    public GameObject gunFlashPrefab;
    public GameObject hoverArrow;
    public GameObject hoverParent; 
    public GameObject leftFist; 

    float maxShootingRange;

    [HideInInspector]
    public float totalRotationX;
    float totalLegRotationX;

    bool rightLegForward;
    public bool moving;
    public bool aiming;

    float deadDelay = 1f;
    public bool dead;
    public bool isGrounded;
    bool reloading;

    float jumpTimer;
    bool jumping;

    [HideInInspector]
    public Transform cam;

    //mobile movement
    public bool isRolling;
    public int rollerIndex;
    public float rollSpeed;
    public bool isSliding;
    public int sliderIndex;
    public Vector2 slidingAxis;
    public bool firing;
    bool slideExit;
    public bool changeGun1;
    public bool changeGun2;
    public bool changeGun3; 
    ArrayList touchesStartInShoot;
    ArrayList touchesStartOutShoot;

    public float aimClamper;
    Vector3 gunHoldForwardPos; 

    public Controller controllerScript;

    string controllerURL = "http://47.97.222.112:8080/";
    public int id;
    int sensitivity;
    public bool shooting;
    public int killedPersonId = 0;

    void OnCollisionStay(Collision coll) {
        if (coll.transform.position.y + (coll.transform.lossyScale.y / 2f) - 0.02f < transform.position.y)
            isGrounded = true;
    }
    void OnCollisionExit(Collision coll) {
        isGrounded = false;
    }
    void Awake() {
        originalMaxHealth = maxHealth; 
        controllerScript = GameObject.Find("Controller").GetComponent<Controller>();
        if (PlayerPrefs.GetInt("useRed") == 1) {
            greyMaterial = redMaterial;
            greyGreenMaterial = redMaterial;
        } else if (PlayerPrefs.GetInt("useBlue") == 1) {
            greyMaterial = blueMaterial;
            greyGreenMaterial = blueMaterial;
        }
        touchesStartInShoot = new ArrayList();
        touchesStartOutShoot = new ArrayList();
        if (controllerScript.isSinglePlayer && !isPlayer) {
            GetComponent<BoxCollider>().isTrigger = true;
            GetComponent<NavMeshAgent>().enabled = true;
        }
        controllerURL = PlayerPrefs.GetString("mpUrl");
        if (PlayerPrefs.GetInt("sensitivity") == 0)
            PlayerPrefs.SetInt("sensitivity", 2);  
        sensitivity = PlayerPrefs.GetInt("sensitivity");
        crosshair = GameObject.Find("Crosshair");
    }
    IEnumerator getMoving() {
        WWW variables = new WWW(controllerURL + "getMovingById?id=" + id);
        yield return variables;
        if (variables.text == "true") {
            moving = true;
        } else
            moving = false;
    }
    IEnumerator getShooting() {
        //if not player
        WWW variables = new WWW(controllerURL + "getShootingById?id=" + id);
        yield return variables;
        if (variables.text == "true") {
            shooting = true;
            updateGun();
        } else
            shooting = false;
    }
    IEnumerator getPositionAndRotationWithId () {
        WWW variables = new WWW(controllerURL + "getAllPositionsAndRotationsById?id=" + id);
        yield return variables;

        PosAndRot data = JsonUtility.FromJson<PosAndRot>(variables.text);

        if (targetPosition == Vector3.zero || Vector3.Distance(targetPosition, transform.position) > 10f) {
            transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
        }

        targetPosition = new Vector3(data.position[0], data.position[1], data.position[2]);
        targetRotation = Quaternion.Euler(data.rotation[0], data.rotation[1], data.rotation[2]);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 200 * Time.deltaTime);
        gunHold.transform.rotation = Quaternion.Euler(data.gunRotation[0], data.gunRotation[1], data.gunRotation[2]);
    }
    IEnumerator updateScore() {
        WWW variables = new WWW(controllerURL + "changeScore?isAlly=" + (!isAlly).ToString());
        yield return variables;
    }
    public void changeWeapon(int gunNum) {
        if (gunNum == 1)
            changeGun1 = true;
        if (gunNum == 2)
            changeGun2 = true;
        if (gunNum == 3)
            changeGun3 = true;
    }
    public void updateGun() {
        gunClip = gun.GetComponent<Gun>().gunClip;
        damage = gun.GetComponent<Gun>().damage;
        shootCooldown = gun.GetComponent<Gun>().shootCooldown;
        shootingDistance = gun.GetComponent<Gun>().shootingDistance;
        maxShootingRange = gun.GetComponent<Gun>().maxShootingRange;
        recoil = gun.GetComponent<Gun>().recoil;
        zoom = gun.GetComponent<Gun>().zoom;
        aimSpeed = gun.GetComponent<Gun> ().aimSpeed; 
        camUpAim = gun.GetComponent<Gun>().camUpAim;
        length = gun.GetComponent<Gun>().length;
        muzzleLength = gun.GetComponent<Gun>().muzzleLength;
        aiMistakeRotation = gun.GetComponent<Gun>().aiMistakeRotation;
        mistakeRotation = gun.GetComponent<Gun>().mistakeRotation;
        reloadCooldown = gun.GetComponent<Gun>().reloadCooldown;
        currentGunClipSize = gun.GetComponent<Gun>().clipSize;
        currentGunClipPosY = gun.GetComponent<Gun>().currentGunClipPosY;
        currentGunClipPosX = gun.GetComponent<Gun>().currentGunClipPosX;
        currentGunClipPosZ = gun.GetComponent<Gun>().currentGunClipPosZ;
        hasScope = gun.GetComponent<Gun>().hasScope;
        sideRecoil = gun.GetComponent<Gun>().sideRecoil;
        gunReloadUp = gun.GetComponent<Gun>().gunReloadUp;
        bulletsPerFire = gun.GetComponent<Gun>().bulletsPerFire;
        semiAuto = gun.GetComponent<Gun>().semiAuto;

        reloadDelay = 0;
        reloading = false;
    }
    public void setGun (bool mg) {
        if (aiming) {
            toggleAim ();
            aimClamper = 1; 
        }
        if (gunHold.transform.childCount > 0) {
            for (int i = 0; i < gunHold.transform.childCount; i++)
                Destroy(gunHold.transform.GetChild(i).gameObject);
        }
        GameObject insItem = null;

        //determine gun
        if (mg) {
            insItem = Instantiate (stationMg, gunHold.transform.position, gunHold.transform.rotation);
            currentGunClipBullets = 9999;
            currentGunTotalBullets = 0;
        } else if (currentGunName == gun1.GetComponent<Gun>().name) {
            insItem = Instantiate(gun1, gunHold.transform.position, gunHold.transform.rotation);
            currentGunClipBullets = gun1ClipBullets;
            currentGunTotalBullets = gun1TotalBullets;
        } else {
            insItem = Instantiate(gun2, gunHold.transform.position, gunHold.transform.rotation);
            currentGunClipBullets = gun2ClipBullets;
            currentGunTotalBullets = gun2TotalBullets;
        }
        gun = insItem;
        insItem.GetComponent<Gun>().attachedSoldier = gameObject;
        insItem.transform.SetParent(gunHold.transform);
        updateGun();
        insItem.transform.Translate(new Vector3(0, 0, -length / 4));
    }
    public void resetAmmo() {
        gun1ClipBullets = gun1.GetComponent<Gun>().clipSize;
        gun1TotalBullets = gun1.GetComponent<Gun>().initialBullets;
        gun2ClipBullets = gun2.GetComponent<Gun>().clipSize;
        gun2TotalBullets = gun2.GetComponent<Gun>().initialBullets;
        if (currentGunName == gun1.GetComponent<Gun>().name) {
            currentGunClipBullets = gun1.GetComponent<Gun>().clipSize;
            currentGunTotalBullets = gun1.GetComponent<Gun>().initialBullets;
        } else {
            currentGunClipBullets = gun2.GetComponent<Gun>().clipSize;
            currentGunTotalBullets = gun2.GetComponent<Gun>().initialBullets;
        }
    }
    public void toggleMap() {
        if (controllerScript.isSinglePlayer || !controllerScript.chatText.isFocused) {
            controllerScript.displayMap = !controllerScript.displayMap;
        }
    }
    public void changeUniform() {
        if (!isAlly) {
            //change enemy color
            transform.GetChild(1).GetComponent<Renderer>().material = greyGreenMaterial;
            transform.GetChild(2).GetChild(1).GetComponent<Renderer>().material = greyGreenMaterial;
            transform.GetChild(3).GetChild(1).GetComponent<Renderer>().material = greyGreenMaterial;
            transform.GetChild(4).GetChild(0).GetChild(0).GetComponent<Renderer>().material = greyGreenMaterial;
            transform.GetChild(4).GetChild(1).GetComponent<Renderer>().material = greyGreenMaterial;
            transform.GetChild(6).GetChild(0).GetChild(0).GetComponent<Renderer>().material = greyGreenMaterial;
            transform.GetChild(6).GetChild(1).GetComponent<Renderer>().material = greyGreenMaterial;
            if (!isPlayer) {
                head.transform.GetChild(0).GetComponent<Renderer>().material = greyMaterial;
            } else {
                head.transform.GetChild(1).GetComponent<Renderer>().material = greyMaterial;
            }
        }
    }
    void Start() {
        cam = head.transform.GetChild(0);
        if (PlayerPrefs.GetInt("renderDistance") != 0 && isPlayer) {
            cam.GetComponent<Camera>().farClipPlane = (float)PlayerPrefs.GetInt("renderDistance");
        }

        if (!isPlayer && controllerScript.isSinglePlayer) {
            gun1 = controllerScript.findGun(currentGunName);
        }

        hoverArrow.GetComponent<SpriteRenderer>().enabled = true;
        if (controllerScript.isSinglePlayer) {
            id = 1;
            if (!isAlly) {
                hoverArrow.GetComponent<SpriteRenderer>().enabled = false;
                hoverArrow.GetComponent<SpriteRenderer>().sprite = redHoverArrow;
            }
        }
        if (!isPlayer && !controllerScript.isSinglePlayer) {
            GetComponent<Rigidbody>().useGravity = false;
            if (id % 2 != 0) {
                isAlly = true;
            } else
                isAlly = false;
            if (isAlly != controllerScript.player.isAlly) {
                hoverArrow.GetComponent<SpriteRenderer>().enabled = false;
                hoverArrow.GetComponent<SpriteRenderer>().sprite = redHoverArrow;
            }
        }

        if (isPlayer)
            currentGunName = gun1.GetComponent<Gun>().name;

        resetAmmo();
        setGun(false);
        changeUniform();
        if (controllerScript.isSpectator && isPlayer) {
            foreach (Renderer i in GetComponentsInChildren<Renderer>()) {
                i.enabled = false;
            }
        }
    }
    //externally used
    public void jump() {
        if (isGrounded && !jumping) {
            jumping = true;
            jumpTimer = 0.2f;
        }
    }
    public void aim() {
        if (!reloading) {
            toggleAim();
        }
    }
    void playerMovement() {
        Quaternion prevRot = transform.rotation; 
        transform.rotation = cam.transform.rotation;
        transform.Rotate(0, 180, 0);
        moving = false;
        if (!isTank) {
            if (jumping) {
                transform.position = new Vector3 (transform.position.x, transform.position.y + Time.deltaTime * 150 * jumpTimer, transform.position.z);
                jumpTimer -= Time.deltaTime;
                if (jumpTimer <= 0) {
                    jumping = false;
                }
            }
            if (Input.GetKey (KeyCode.Space) && isGrounded && !jumping) {
                if (!controllerScript.isSinglePlayer) {
                    if (!controllerScript.chatText.isFocused) {
                        jumping = true;
                        jumpTimer = 0.2f;
                    }
                } else {
                    jumping = true;
                    jumpTimer = 0.2f;
                }
            }
        }
        if (controllerScript.isTablet) {
            int t = Input.touchCount;

            if (!isSliding) {
                for (int i = 0; i < t; i++) {
                    if (Input.GetTouch(i).position.x > crosshair.transform.position.x) {
                        isSliding = true;
                        sliderIndex = i;
                        break;
                    }
                }
            } else {
                if (Input.touchCount <= sliderIndex || Input.GetTouch(sliderIndex).phase == TouchPhase.Ended || Input.GetTouch (sliderIndex).position.x < crosshair.transform.position.x * 0.5f) {
                    isSliding = false;
                    slideExit = true;
                } else {
                    if (slideExit) {
                        slidingAxis = Vector2.zero;
                        slideExit = false;
                    } else {
                        slidingAxis = Input.GetTouch(sliderIndex).deltaPosition;
                    }
                }
            }
            if (Input.touchCount == 0) {
                isRolling = false;
                isSliding = false;
                slideExit = true;
            }
            if (!isRolling) {
                controllerScript.ballRoller.GetComponent<Image>().color = new Color(1, 1, 1, 0.7f);

                for (int i = 0; i < t; i++) {
                    if (Input.GetTouch(i).position.x < crosshair.transform.position.x && !controllerScript.fireLeftRect.Contains(Input.GetTouch(i).position)) {
                        if (PlayerPrefs.GetInt("movingJoystick") == 1)
                            controllerScript.ballRoller.transform.position = Input.GetTouch(i).position + new Vector2(0, Random.Range(1f, 3f));
                        isRolling = true;
                        rollerIndex = i;
                        break; 
                    }
                    //if (controllerScript.ballRollerRect.Contains(Input.GetTouch(i).position)) {
                    //    isRolling = true;
                    //    rollerIndex = i;
                    //    break;
                    //}
                }
            } else {
                controllerScript.ballRoller.GetComponent<Image>().color = new Color(1f, 0.92f, 0.016f, 0.7f);
                if (Input.touchCount <= rollerIndex || Input.GetTouch(rollerIndex).phase == TouchPhase.Ended || Input.GetTouch(rollerIndex).position.x > crosshair.transform.position.x) {
                    isRolling = false;
                } else if (Input.GetTouch(rollerIndex).position.x < crosshair.transform.position.x) {
                    moving = true;
                    controllerScript.ballRoller.transform.LookAt(new Vector3(Input.GetTouch(rollerIndex).position.x, Input.GetTouch(rollerIndex).position.y, 0));
                    controllerScript.ballRoller.transform.Rotate(0, 90, 90);
                    rollSpeed = Vector2.Distance(Input.GetTouch(rollerIndex).position, new Vector2(controllerScript.ballRoller.transform.position.x, controllerScript.ballRoller.transform.position.y)) / 50;
                    if (rollSpeed > 3)
                        rollSpeed = 3;
                    if (controllerScript.isSpectator)
                        rollSpeed *= 8f; 
                    Quaternion prevRotation = transform.rotation;
                    float y = controllerScript.ballRoller.transform.eulerAngles.z;
                    if (Input.GetTouch(rollerIndex).position.x < controllerScript.ballRoller.transform.position.x)
                        y = 360 - y;
                    if (isTank) {
                        y -= 180;
                        if (y > 0) {
                            if (y > 90) {
                                if (currentTank.GetComponent<Tank>().airplaneAcceleration < currentTank.GetComponent<Tank>().airplaneMaxAcceleration)
                                    currentTank.GetComponent<Tank>().airplaneAcceleration += Time.deltaTime * currentTank.GetComponent<Tank>().moveSpeed / 18f; 
                                currentTank.transform.rotation = Quaternion.Euler (currentTank.transform.eulerAngles.x, currentTank.transform.eulerAngles.y - (180 - y) / 40, currentTank.transform.eulerAngles.z);
                                if (currentTank.GetComponent<Tank>().isAirplane)
                                    currentTank.transform.Translate(Vector3.forward * Time.deltaTime * rollSpeed * currentTank.GetComponent<Tank>().airplaneAcceleration / 2);
                                else
                                    currentTank.transform.Translate(Vector3.forward * Time.deltaTime * rollSpeed * 1.6f);
                            } else if (!currentTank.GetComponent<Tank>().isAirplane) {
                                //don't move back if on airplane
                                currentTank.transform.rotation = Quaternion.Euler (currentTank.transform.eulerAngles.x, currentTank.transform.eulerAngles.y + y / 40, currentTank.transform.eulerAngles.z);
                                currentTank.transform.Translate (-Vector3.forward * Time.deltaTime * rollSpeed * 1.6f);
                            }
                        } else {
                            if (y < -90) {
                                if (currentTank.GetComponent<Tank>().airplaneAcceleration < currentTank.GetComponent<Tank>().airplaneMaxAcceleration)
                                    currentTank.GetComponent<Tank>().airplaneAcceleration += Time.deltaTime * currentTank.GetComponent<Tank>().moveSpeed / 18f; 
                                currentTank.transform.rotation = Quaternion.Euler (currentTank.transform.eulerAngles.x, currentTank.transform.eulerAngles.y + (180 + y) / 40, currentTank.transform.eulerAngles.z);
                                if (currentTank.GetComponent<Tank>().isAirplane)
                                    currentTank.transform.Translate(Vector3.forward * Time.deltaTime * rollSpeed * currentTank.GetComponent<Tank>().airplaneAcceleration / 2);
                                else
                                    currentTank.transform.Translate(Vector3.forward * Time.deltaTime * rollSpeed * 1.6f);
                                
                            } else if (!currentTank.GetComponent<Tank>().isAirplane) {
                                currentTank.transform.rotation = Quaternion.Euler (currentTank.transform.eulerAngles.x, currentTank.transform.eulerAngles.y + y / 40, currentTank.transform.eulerAngles.z);
                                currentTank.transform.Translate (-Vector3.forward * Time.deltaTime * rollSpeed * 1.6f);
                            }
                        }
                    } else {
                        float y1 = y - 180;
                        float prevPosY = transform.position.y; 
                        if (controllerScript.isSpectator) {
                            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + y - 180, transform.eulerAngles.z);
                        } else 
                            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + y - 180, 0);
                        transform.Translate(Vector3.forward * Time.deltaTime * rollSpeed * sprint * 3);
                        transform.rotation = prevRotation;
                        if ((y1 > 90 || y1 < -90) && controllerScript.isSpectator) {
                            transform.position = new Vector3(transform.position.x, transform.position.y - (transform.position.y - prevPosY) * 2, transform.position.z);
                        }
                    }
                } else
                    isRolling = false;
            }
        }

        if (controllerScript.isSinglePlayer || !controllerScript.chatText.isFocused) {
            if (isTank) {
                if (Input.GetKey (KeyCode.S) && !currentTank.GetComponent<Tank>().isAirplane) {
                    currentTank.transform.Translate (Vector3.back * Time.deltaTime * currentTank.GetComponent<Tank> ().moveSpeed);
                }
                if (!currentTank.GetComponent<Tank>().isAirplane) {
                    if (Input.GetKey(KeyCode.D)) {
                        currentTank.transform.Rotate(new Vector3(0, 100, 0) * Time.deltaTime * currentTank.GetComponent<Tank>().rotateSpeed);
                    }
                    if (Input.GetKey(KeyCode.A)) {
                        currentTank.transform.Rotate(new Vector3(0, -100, 0) * Time.deltaTime * currentTank.GetComponent<Tank>().rotateSpeed);
                    }
                }
                if (Input.GetKey(KeyCode.W)) {
                    if (currentTank.GetComponent<Tank>().isAirplane) {
                        if (currentTank.GetComponent<Tank>().airplaneAcceleration < currentTank.GetComponent<Tank>().airplaneMaxAcceleration)
                            currentTank.GetComponent<Tank>().airplaneAcceleration += Time.deltaTime * currentTank.GetComponent<Tank>().moveSpeed / 15f;
                        currentTank.transform.Translate(Vector3.forward * Time.deltaTime * currentTank.GetComponent<Tank>().airplaneAcceleration);
                        currentTank.GetComponent<Tank>().turret.transform.rotation = currentTank.transform.rotation; 
                    } else
                        currentTank.transform.Translate(Vector3.forward * Time.deltaTime * currentTank.GetComponent<Tank>().moveSpeed);
                }
                transform.position = currentTank.GetComponent<Tank>().turret.transform.position;
            } else {
                Quaternion prevRot1 = transform.rotation;
                if (!controllerScript.isSpectator)
                    transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
                if (Input.GetKey (KeyCode.W)) {
                    transform.Translate (Vector3.back * Time.deltaTime * moveSpeed * sprint);
                    moving = true;
                }
                if (Input.GetKey (KeyCode.S)) {
                    transform.Translate (Vector3.forward * Time.deltaTime * moveSpeed * sprint);
                    moving = true;
                }
                if (Input.GetKey (KeyCode.D)) {
                    transform.Translate (Vector3.left * Time.deltaTime * moveSpeed * sprint);
                    moving = true;
                }
                if (Input.GetKey (KeyCode.A)) {
                    transform.Translate (Vector3.right * Time.deltaTime * moveSpeed * sprint);
                    moving = true;
                }
                transform.rotation = prevRot1; 
            }
        }
       
        if (Input.GetKey(KeyCode.LeftShift)) {
            sprint = 1.5f;
        } else
            sprint = 1;
        if (shooting) {
            sprint = 0.7f;
        }
        transform.rotation = prevRot;
    }
    public void toggleAim() {
        if (!isTank && !controllerScript.isSpectator) {
            aiming = !aiming;
            aimClamper = 0; 
        }
    }
    public void reloadGun() {
        if (!reloading) {
            reloadDelay = reloadCooldown;
            reloading = true;
        }
    }

    //only for server nonplayers
    public float updatePositionCooldown = 0.36f;
    float updatePositionTimer;
    float updateHealthCooldown = 0.12f;
    float updateHealthTimer;
    bool scoreUpdated;
    bool aimAgain = false;
    bool canReload = false;

    public void tabletReload() {
        if (canReload) {
            reloadGun ();
            canReload = false;
        }
    }
    void updateGunHoldForward () {
        gunHold.transform.Translate(0, 0, -0.85f);
        gunHoldForwardPos = gunHold.transform.position;
        gunHold.transform.Translate(0, 0, 0.85f);
    }
    void Update() {
        updateGunHoldForward(); 
        if (isPlayer) {
            if (controllerScript.recentDeathTimer > 0) {
                health = maxHealth;
            }
        }
        if (isTank) {
            hoverParent.transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
            if (!isPlayer)
                hoverParent.transform.Rotate (0, 180, 0);
        }
        if (recentShootingTimer > 0) {
            recentShootingTimer -= Time.deltaTime;
            recentShooting = true;
        } else
            recentShooting = false;
        
        if (!controllerScript.isSinglePlayer && !isPlayer) {
            if (Vector3.Distance(transform.position, targetPosition) > 0.2f) {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * 2 * Time.deltaTime);
            }
            if (transform.rotation != targetRotation) {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 200 * Time.deltaTime);
            }
        }
        if (updateHealthTimer > 0) {
            updateHealthTimer -= Time.deltaTime;
        } else {
            if (!controllerScript.isSinglePlayer && !dead && isPlayer) {
                StartCoroutine(getHealth());
            }
            if (recentDeath && !controllerScript.isSinglePlayer) {
                health = maxHealth; 
            }
            updateHealthTimer = updateHealthCooldown;
        }
        if (shootDelay > 0) {
            shootDelay -= Time.deltaTime;
        }
        if (!isPlayer && !controllerScript.isSinglePlayer) {
            if (shooting) {
                if (shootDelay <= 0) {
                    shootBullet();
                    shootDelay = shootCooldown;
                }
                //auto subtracts delay
            }
            if (updatePositionTimer > 0) {
                updatePositionTimer -= Time.deltaTime;
            } else {
                updatePositionTimer = updatePositionCooldown;
                StartCoroutine(getPositionAndRotationWithId());
                StartCoroutine(getShooting());
                StartCoroutine(getMoving());
                StartCoroutine(getGunName());
            }
            if (isAlly != controllerScript.player.isAlly) {
                if (shooting) {
                    hoverArrow.GetComponent<SpriteRenderer>().enabled = true;
                } else
                    hoverArrow.GetComponent<SpriteRenderer>().enabled = false;
            }
        } else if (controllerScript.isSinglePlayer && !isPlayer) {
            if (!isAlly) {
                if (shootDelay > 0) {
                    hoverArrow.GetComponent<SpriteRenderer>().enabled = true;
                } else
                    hoverArrow.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        if (reloading) {
            gunClip.transform.position = gunClip.transform.parent.position;
            gunClip.transform.Translate(currentGunClipPosX, gunReloadUp, currentGunClipPosZ);

            gunClip.transform.Translate(0, 0, -0.3f);
            if (reloadDelay < reloadCooldown / 2) {
                gunClip.transform.Translate(0, currentGunClipPosY - reloadDelay / reloadCooldown * 1.5f, 0);
                gunClip.transform.position = new Vector3(gunClip.transform.position.x, gunClip.transform.position.y, gunClip.transform.position.z);
            } else {
                gunClip.transform.Translate(0, currentGunClipPosY + (reloadDelay - reloadCooldown / 2) / reloadCooldown * 1.5f - 0.75f, 0);
                gunClip.transform.position = new Vector3(gunClip.transform.position.x, gunClip.transform.position.y, gunClip.transform.position.z);
            }
            gunClip.transform.Translate(0, 0, 0.3f);
        }
        if (currentGunName == gun1.GetComponent<Gun>().name) {
            gun1ClipBullets = currentGunClipBullets;
            gun1TotalBullets = currentGunTotalBullets;
        } else if (currentGunName == gun2.GetComponent<Gun> ().name) {
            gun2ClipBullets = currentGunClipBullets;
            gun2TotalBullets = currentGunTotalBullets;
        } 
        if (isPlayer && !isTank) {
            if (reloading && aiming) {
                toggleAim();
                aimAgain = true;
            }
            if (!reloading && aimAgain && !aiming) {
                toggleAim();
                aimAgain = false;
            }

            if (recoilRotation > 0) {
                gunHold.transform.Rotate(-Time.deltaTime * 10f, 0, 0);
                recoilRotation -= Time.deltaTime * 10f;
            }
            if (shootDelay <= 0 && !controllerScript.isSpectator) {
                if (controllerScript.isSinglePlayer || !controllerScript.chatText.isFocused) {
                    if (Input.GetKeyDown(KeyCode.Alpha1) || changeGun1) {
                        currentGunName = gun1.GetComponent<Gun> ().name;
                        setGun (false);
                        changeGun1 = false;
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha2) || changeGun2) {
                        currentGunName = gun2.GetComponent<Gun> ().name;
                        setGun (false);
                        changeGun2 = false;
                    }
                    if (changeGun3) {
                        currentGunName = stationMg.GetComponent<Gun> ().name;
                        setGun (true);
                        changeGun3 = false;
                    }
                }
            }
        }

        if (changeTargetDelay > 0) {
            changeTargetDelay -= Time.deltaTime;
        } else {
            changeTargetDelay = changeTargetCooldown + Random.Range(0f, 10f);
            if (target == null || target.GetComponent<Tank>() == null)
                target = null;
        }
        if (!dead) {
            if (isPlayer) {
                if (aimClamper < 1) {
                    if (aiming) {
                        cam.position = Vector3.Lerp (head.transform.position, gunHoldForwardPos, aimClamper); 
                        cam.rotation = gunHold.transform.rotation;
                        cam.Translate (Vector3.up * camUpAim * (aimClamper));
                        cam.Translate (Vector3.forward * -0.37f * (1 - aimClamper));
                        cam.Rotate (0, 180, 0);
                    } else {
                        cam.position = Vector3.Lerp (gunHoldForwardPos, head.transform.position, aimClamper);
                        cam.rotation = gunHold.transform.rotation;
                        cam.Rotate (0, 180, 0);
                        cam.Translate (Vector3.up * camUpAim * (1 - aimClamper));
                        cam.Translate (Vector3.forward * 0.37f * (aimClamper));
                    }
                    aimClamper += Time.deltaTime * aimSpeed; 
                }
                //add controls for mobile 
                if (isTank && (Input.GetKeyDown (KeyCode.G) || rideTank)) {
                    shootDelay = 0;
                    shootCooldown = 0; 
                    rideTank = false; 
                    isTank = false;
                    transform.position = new Vector3 (transform.position.x, transform.position.y + 3, transform.position.z);
                    foreach (Renderer i in GetComponentsInChildren<Renderer>()) {
                        i.enabled = true; 
                    }
                    foreach (BoxCollider i in GetComponentsInChildren<BoxCollider> ()) {
                        i.enabled = true;
                    }
                    transform.rotation = Quaternion.identity;
                    gunHold.transform.rotation = Quaternion.identity;
                    totalRotationX = 0;
                    aim ();
                    aim ();
                    currentTank.GetComponent<Tank> ().user = null;
                    maxHealth = originalMaxHealth; 
                    health = maxHealth;
                    changeWeapon (1);
                } else if (tankIsNearby && !isTank && (Input.GetKeyDown(KeyCode.G) || rideTank)) {
                    if (aiming) {
                        aim (); 
                    }
                    rideTank = false; 
                    currentGunName = stationMg.GetComponent<Gun> ().name;
                    setGun (true);
                    recoil = 0f;
                    sideRecoil = 0f;
                    isTank = true; 
                    foreach (Renderer i in GetComponentsInChildren<Renderer> ()) {
                        if (i.gameObject.name != "HoverArrow")
                            i.enabled = false;
                    }
                    foreach (BoxCollider i in GetComponentsInChildren<BoxCollider> ()) {
                        i.enabled = false; 
                    }
                    currentTank = nearbyTank;
                    totalRotationX = 0;
                    currentTank.GetComponent<Tank> ().turret.transform.rotation = currentTank.transform.rotation;
                    currentTank.GetComponent<Tank> ().turret.transform.Rotate (0, 180, 0);
                    currentTank.GetComponent<Tank> ().user = gameObject;
                    shootCooldown = currentTank.GetComponent<Tank>().shootSpeed; 
                    semiAuto = currentTank.GetComponent<Tank>().semiAuto;
                    maxHealth = currentTank.GetComponent<Tank> ().maxHealth;
                    health = currentTank.GetComponent<Tank> ().health;
                }
                if (isTank) {
                    transform.position = currentTank.GetComponent<Tank> ().turret.transform.position;
                    transform.rotation = currentTank.GetComponent<Tank> ().turret.transform.rotation;
                    currentTank.GetComponent<Tank> ().health = health;
                    gunHold.transform.rotation = transform.rotation;
                    cam.rotation = transform.rotation;
                    cam.Rotate (0, 180, 0);
                    cam.position = transform.position;
                    cam.Translate (0, 0.5f, 4f);
                } 

                if (shooting) {
                    if (moving) {
                        crosshair.GetComponent<RectTransform>().localScale = new Vector3(1.6f, 1.8f, 1.8f);
                    } else
                        crosshair.GetComponent<RectTransform>().localScale = new Vector3(1.3f, 1.5f, 1.5f);
                } else if (moving) {
                    crosshair.GetComponent<RectTransform>().localScale = new Vector3(1.3f, 1.3f, 1.3f);
                } else {
                    crosshair.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                }
            }
            if (health < maxHealth && health > 0.5f && !isTank) {
                if (controllerScript.isSinglePlayer) {
                    health += 12 * Time.deltaTime;
                } else {
                    //add health and update health ?
                }
            }
            if ((controllerScript.isSinglePlayer || !controllerScript.chatText.isFocused) && currentGunTotalBullets > 0 && currentGunClipBullets < currentGunClipSize) {
                if (Input.GetKeyDown(KeyCode.R))
                    reloadGun();
                canReload = true;
            } else
                canReload = false;
            //force reload
            if (currentGunClipBullets <= 0 && !reloading && currentGunTotalBullets > 0) {
                reloadDelay = reloadCooldown;
                reloading = true;
            }
            if (reloading) {
                reloadDelay -= Time.deltaTime;
                if (reloadDelay <= 0) {
                    gunClip.transform.position = gunClip.transform.parent.position;
                    gunClip.transform.Translate(currentGunClipPosX, currentGunClipPosY + gunReloadUp, currentGunClipPosZ);
                    reloading = false;
                    if (currentGunTotalBullets > 0) {
                        if (currentGunTotalBullets >= currentGunClipSize - currentGunClipBullets) {
                            currentGunTotalBullets -= currentGunClipSize - currentGunClipBullets;
                            currentGunClipBullets = currentGunClipSize;
                        } else {
                            currentGunClipBullets += currentGunTotalBullets;
                            currentGunTotalBullets = 0;
                        }
                    }
                }
            }
            if (health < 1 && health > 0)
                health = 0; 
            if (health <= 0) {
                if (!isPlayer && controllerScript.isSinglePlayer)
                    die (); 
                if (isTank) {
                    shootDelay = 0;
                    shootCooldown = 0; 
                    isTank = false;
                    transform.position = new Vector3 (transform.position.x, transform.position.y + 3, transform.position.z);
                    foreach (Renderer i in GetComponentsInChildren<Renderer>()) {
                        i.enabled = true; 
                    }
                    foreach (BoxCollider i in GetComponentsInChildren<BoxCollider> ()) {
                        i.enabled = true;
                    }
                    if (isPlayer) {
                        transform.rotation = Quaternion.identity;
                        gunHold.transform.rotation = Quaternion.identity;
                        totalRotationX = 0;
                        aim ();
                        aim ();
                    }
                    currentTank.GetComponent<Tank> ().user = null;

                    maxHealth = originalMaxHealth;
                    health = maxHealth;
                    if (!currentTank.GetComponent<Tank>().isAirplane)
                        Instantiate(tankWreckPrefab, currentTank.transform.position, currentTank.transform.rotation);
                    else {
                        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                    }
                    controllerScript.audioManager.Play ("explosion", 1);
                    currentTank.GetComponent<Tank> ().resetTank ();
                    changeWeapon (1);
                } else 
                    die ();
            }
            leftArm.transform.LookAt(gunClip.transform.position);
            leftArm.transform.Rotate(-90, 0, -5);
            rightArm.transform.LookAt(gunClip.transform.position);
            rightArm.transform.Rotate(143, 0, 180);
            if (isPlayer && !controllerScript.paused) {
                playerMovement();
                if ((Input.GetMouseButtonDown(1) && !controllerScript.isTablet) && !reloading) {
                    toggleAim();
                }
                float gunRotation = 0f;
                float playerRotationY = 0f;

                if (!controllerScript.isTablet) {
                    //fix
                    gunRotation = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity * 3.4f;
                    playerRotationY = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity * 3.4f;
                }
                if (controllerScript.isTablet && isSliding) {
                    gunRotation = slidingAxis.y * Time.deltaTime * sensitivity * 0.26f;
                    playerRotationY = slidingAxis.x * Time.deltaTime * sensitivity * 0.25f;
                }
                if (aiming) {
                    gunRotation /= 0.7f + zoom / 30;
                    playerRotationY /= 0.7f + zoom / 30;
                }

                if (gunRotation > 2)
                    gunRotation = 2;
                if (gunRotation < -2)
                    gunRotation = -2;
                if (!isTank) {
                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + playerRotationY * scrollerSpeed, transform.eulerAngles.z);
                } else if (!currentTank.GetComponent<Tank>().isAirplane) {
                    Vector3 selfEuler = currentTank.GetComponent<Tank>().turret.transform.localEulerAngles;
                    currentTank.GetComponent<Tank>().turret.transform.localRotation = Quaternion.Euler(0, selfEuler.y, 0);
                    currentTank.GetComponent<Tank>().turret.transform.Rotate(0, playerRotationY, 0);
                    currentTank.GetComponent<Tank>().turret.transform.Rotate(totalRotationX, 0, 0);
                    transform.rotation = currentTank.GetComponent<Tank>().turret.transform.rotation;
                    cam.rotation = transform.rotation;
                    cam.Rotate(0, 180, 0);
                } else {
                    //airplane sensitivity
                    gunRotation *= -2f; 
                    playerRotationY *= 3f; 
                    if (playerRotationY > 13)
                        playerRotationY = 13;
                    if (playerRotationY < -13)
                        playerRotationY = -13;
                    currentTank.transform.eulerAngles = new Vector3(currentTank.transform.eulerAngles.x + gunRotation, currentTank.transform.eulerAngles.y + playerRotationY, currentTank.transform.eulerAngles.z);
                    transform.eulerAngles = currentTank.transform.eulerAngles;
                    transform.Rotate(0, 180, 0);
                    cam.eulerAngles = currentTank.transform.eulerAngles;
                }

                if (!isTank && (totalRotationX < 50 && gunRotation > 0 || totalRotationX > -50 && gunRotation < 0) || isTank && (totalRotationX < 12 && gunRotation > 0 || totalRotationX > -12 && gunRotation < 0)) {
                    if (isTank) {
                        totalRotationX += gunRotation * scrollerSpeed / 7.2f;
                    } else {
                        gunHold.transform.Rotate (gunRotation * scrollerSpeed, 0, 0);
                        totalRotationX += gunRotation * scrollerSpeed;
                    }
                }
                firing = false;
                if (controllerScript.isTablet) {
                    for (int i = 0; i < Input.touchCount; i++) {
                        if (Input.GetTouch(i).phase == TouchPhase.Began && (controllerScript.fireRightRect.Contains(Input.GetTouch(i).position) || controllerScript.fireLeftRect.Contains(Input.GetTouch(i).position))) {
                            touchesStartInShoot.Add(Input.GetTouch(i).fingerId);
                            if (touchesStartOutShoot.Contains(Input.GetTouch(i).fingerId))
                                touchesStartOutShoot.Remove(Input.GetTouch(i).fingerId);
                        } else if (Input.GetTouch(i).phase == TouchPhase.Began) {
                            touchesStartOutShoot.Add(Input.GetTouch(i).fingerId);
                            if (touchesStartInShoot.Contains(Input.GetTouch(i).fingerId))
                                touchesStartInShoot.Remove(Input.GetTouch(i).fingerId);
                        }

                        if ((Input.GetTouch(i).phase == TouchPhase.Began || !semiAuto) && touchesStartInShoot.Contains(Input.GetTouch(i).fingerId)) {
                            if (controllerScript.fireRightRect.Contains(Input.GetTouch(i).position) || controllerScript.fireLeftRect.Contains(Input.GetTouch(i).position)) {
                                firing = true;
                                if (controllerScript.fireRightRect.Contains(Input.GetTouch(i).position)) {
                                    controllerScript.fireRight.GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);

                                } else {
                                    controllerScript.fireLeft.GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
                                }
                                break;
                            }
                        }
                    }
                }
                bool mouseDown = false;
                if (!semiAuto && (Input.GetMouseButton(0) || Input.GetKey(KeyCode.F)) || (semiAuto && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)))) {
                    mouseDown = true;
                }
                if ((mouseDown && !controllerScript.isTablet || firing) && currentGunClipBullets > 0 && !reloading) {
                    shooting = true;
                    recentShootingTimer = 0.5f;
                } else {
                    shooting = false;
                }
                if (controllerScript.isTablet && !firing) {
                    controllerScript.fireRight.GetComponent<RectTransform> ().localScale = new Vector2 (1f, 1f);
                    controllerScript.fireLeft.GetComponent<RectTransform> ().localScale = new Vector2 (1f, 1f);
                }
                if ((mouseDown && !controllerScript.isTablet || firing) && shootDelay <= 0 && currentGunClipBullets > 0 && !reloading && !controllerScript.isSpectator) {
                    if (!aiming) {
                        Quaternion originalRot = transform.rotation;
                        transform.Rotate(Random.Range(-mistakeRotation, mistakeRotation), Random.Range(-mistakeRotation, mistakeRotation), 0);
                        shootBullet();
                        transform.rotation = originalRot;
                        transform.Rotate(0, Random.Range(-sideRecoil, sideRecoil), 0);
                    } else {
                        shootBullet();
                    }
                    shootDelay = shootCooldown;
                }
                leftArm.transform.LookAt(gunClip.transform.position);
                leftArm.transform.Rotate(-90, 0, -5);
                rightArm.transform.LookAt(gunClip.transform.position);
                rightArm.transform.Rotate(143, 0, 180);
            } else if (controllerScript.isSinglePlayer) {
                //find target
                if (target == null) {
                    float ttlMinDist = 100000;
                    GameObject currentTarget = null; 
                    if (isAlly) {
                        float minDist = 100000;
                        foreach (GameObject i in controllerScript.enemyArray) {
                            if (i != null) {
                                if (Vector3.Distance(transform.position, i.transform.position) < minDist && i.transform.position.y < transform.position.y + 30 && (i.GetComponent<SoldierController>().isPlayer || !i.GetComponent<SoldierController>().isTank || !i.GetComponent<SoldierController>().currentTank.GetComponent<Tank>().isAirplane)) {
                                    currentTarget = i;
                                    minDist = Vector3.Distance(transform.position, i.transform.position);
                                    ttlMinDist = minDist;
                                }
                            }
                        }
                        target = currentTarget;
                    } else {
                        float minDist = 100000;
                        foreach (GameObject i in controllerScript.allyArray) {
                            if (i != null) {
                                if (Vector3.Distance(transform.position, i.transform.position) < minDist && i.transform.position.y < transform.position.y + 30 && (i.GetComponent<SoldierController>().isPlayer || !i.GetComponent<SoldierController>().isTank || !i.GetComponent<SoldierController>().currentTank.GetComponent<Tank>().isAirplane)) {
                                    currentTarget = i;
                                    minDist = Vector3.Distance(transform.position, i.transform.position);
                                    ttlMinDist = minDist; 
                                }
                            }
                        }
                        target = currentTarget;
                    }
                    //go to point in point capture mode
                    if (controllerScript.isPointCapture) {
                        foreach (GameObject i in controllerScript.flagpoles) {
                            if (i != null && Vector3.Distance(transform.position, i.transform.position) + 15f < ttlMinDist && (isAlly && i.GetComponent<Flagpole>().captureStatus < i.GetComponent<Flagpole>().maxCaptureStatus || !isAlly && i.GetComponent<Flagpole>().captureStatus > -i.GetComponent<Flagpole>().maxCaptureStatus)) {
                                ttlMinDist = Vector3.Distance(transform.position, i.transform.position) + 15f;
                                currentTarget = i; 
                            }
                        }
                    }
                    //soldier to find tank/airplane
                    if (!isTank) {
                        foreach (GameObject i in controllerScript.tanks) {
                            if (i != null && i.GetComponent<Tank>().user == null) {
                                if (Vector3.Distance (transform.position, i.transform.position) < ttlMinDist) {
                                    ttlMinDist = Vector3.Distance (transform.position, i.transform.position);
                                    currentTarget = i;
                                }
                            }
                        }
                        if (currentTarget != null) {
                            target = currentTarget;
                            if (currentTarget.GetComponent<Tank> () != null) {
                                currentTarget.GetComponent<Tank> ().user = gameObject;
                            }
                        }
                    }
                } else if ((!isTank || !currentTank.GetComponent<Tank>().isAirplane) && !controllerScript.paused) {
                    Vector3 originalRotation = transform.eulerAngles;
                    head.transform.LookAt(target.transform);
                    detectEnemyPathBlock();
                    if (target.GetComponent<Flagpole>() != null) {
                        if (Vector3.Distance(transform.position, target.transform.position) > 7) {
                            GetComponent<NavMeshAgent>().enabled = true;
                            GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
                            GetComponent<NavMeshAgent>().isStopped = false;
                            moving = true;
                        } else {
                            GetComponent<NavMeshAgent>().isStopped = true;
                            moving = false; 
                        }
                        if (isAlly && target.GetComponent<Flagpole>().captureStatus >= target.GetComponent<Flagpole>().maxCaptureStatus || !isAlly && target.GetComponent<Flagpole>().captureStatus <= -target.GetComponent<Flagpole>().maxCaptureStatus) {
                            target = null; 
                        }
                    } else if (target.GetComponent<Tank> () == null) {
                        if (isTank) {
                            if (transform.GetChild(0).GetComponent<Renderer>().enabled) {
                                foreach (Renderer i in GetComponentsInChildren<Renderer>()) {
                                    i.enabled = false;
                                }
                            }
                            if (!currentTank.GetComponent<Tank>().isAirplane) {
                                currentTank.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                                currentTank.transform.rotation = Quaternion.Euler(currentTank.transform.eulerAngles.x, transform.eulerAngles.y, currentTank.transform.eulerAngles.z);
                                if (target != null && Vector3.Distance(transform.position, target.transform.position) < shootingDistance * 2.2f) {
                                    Transform turret = currentTank.GetComponent<Tank>().turret.transform;
                                    turret.LookAt(target.transform);
                                    turret.transform.Rotate(0, 180, 0);
                                    turret.localEulerAngles = new Vector3(turret.localEulerAngles.x, turret.localEulerAngles.y, 0);
                                }
                                if (currentTank.transform.eulerAngles.z > 50 || currentTank.transform.eulerAngles.z < -50) {
                                    currentTank.transform.rotation = Quaternion.Euler(currentTank.transform.eulerAngles.x, transform.eulerAngles.y, 0);
                                }
                            } else {
                                if (!transform.GetChild(0).GetComponent<Renderer>().enabled) {
                                    foreach (Renderer i in GetComponentsInChildren<Renderer>()) {
                                        i.enabled = false;
                                    }
                                }
                                transform.position = currentTank.transform.position; 
                            }
                            currentTank.GetComponent<Tank> ().health = health;
                        }
                        if (Vector3.Distance (transform.position, target.transform.position) > shootingDistance || cannotAim && Vector3.Distance (transform.position, target.transform.position) > 1.5f) {
                            moving = true;
                            if (GetComponent<NavMeshAgent>().enabled) {
                                GetComponent<NavMeshAgent>().isStopped = false;
                                GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
                            }
                        } else {
                            if (GetComponent<NavMeshAgent>().enabled)
                                GetComponent<NavMeshAgent> ().isStopped = true;
                            moving = false;
                        }
                        if ((Vector3.Distance (transform.position, target.transform.position) < shootingDistance * 1.5f || isTank && Vector3.Distance (transform.position, target.transform.position) < shootingDistance * 2.2f) && !cannotAim && !reloading) {
                            if (shootDelay <= 0 && currentGunClipBullets > 0) {
                                Quaternion originalRot = transform.rotation;
                                Quaternion originalHeadRot = head.transform.rotation;
                                transform.Rotate (0, Random.Range (-aiMistakeRotation, aiMistakeRotation), 0);
                                head.transform.Rotate (Random.Range (-aiMistakeRotation, aiMistakeRotation), 0, 0);
                                if (isTank) {
                                    head.transform.Rotate (-1.5f, 0, 0);
                                }
                                shootBullet ();
                                transform.rotation = originalRot;
                                head.transform.rotation = originalHeadRot;
                                shootDelay = shootCooldown;
                            }
                        }
                    } else {
                        //tank
                        GetComponent<NavMeshAgent>().radius = 8f;
                        GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
                        GetComponent<NavMeshAgent>().isStopped = false;
                        
                        moving = true; 
                        if (Vector3.Distance(transform.position, target.transform.position) < 7.1f) {
                            currentGunName = stationMg.GetComponent<Gun> ().name;
                            setGun (true);
                            recoil = 0f;
                            sideRecoil = 0f;
                            isTank = true;
                            shootingDistance = 40f; 
                            foreach (Renderer i in GetComponentsInChildren<Renderer> ()) {
                                if (i.gameObject.name != "HoverArrow")
                                    i.enabled = false;
                            }
                            foreach (BoxCollider i in GetComponentsInChildren<BoxCollider> ()) {
                                i.enabled = false;
                            }
                            currentTank = target;
                            totalRotationX = 0;
                            currentTank.GetComponent<Tank> ().turret.transform.rotation = currentTank.transform.rotation;
                            currentTank.GetComponent<Tank> ().turret.transform.Rotate (0, 180, 0);
                            currentTank.GetComponent<Tank> ().user = gameObject;
                            semiAuto = currentTank.GetComponent<Tank>().semiAuto;
                            shootCooldown = currentTank.GetComponent<Tank>().shootSpeed; 
                            maxHealth = currentTank.GetComponent<Tank> ().maxHealth;
                            health = currentTank.GetComponent<Tank> ().health;
                            target = null; 
                        } else if (target.GetComponent<Tank> ().user != null && target.GetComponent<Tank>().user != gameObject) {
                            target = null;
                        }
                    }
                    head.transform.Rotate (0, 180, 0);
                }
            }
            if (!isTank) {
                if (!aiming) {
                    if (isPlayer && aimClamper >= 1) {
                        head.transform.rotation = gunHold.transform.rotation;
                        cam.position = head.transform.position;
                        cam.localRotation = Quaternion.identity;
                        cam.transform.Rotate (0, 180, 0);
                        cam.Translate (Vector3.forward * 0.37f);
                    } else if (controllerScript.isSinglePlayer && target != null) {
                        transform.LookAt (target.transform);
                        transform.Rotate (0, 180, 0);
                        gunHold.transform.rotation = head.transform.rotation;
                    }
                } else if (aimClamper >= 1) {
                    updateGunHoldForward(); 
                    head.transform.rotation = gunHold.transform.rotation; 
                    cam.position = gunHoldForwardPos;
                    cam.rotation = gunHold.transform.rotation;
                    cam.Translate (Vector3.up * camUpAim);
                    cam.Rotate (0, 180, 0);
                }
            } else {
                cam.rotation = gunHold.transform.rotation;
                cam.Rotate (0, 180, 0);
            }
        } else {
            recentDeath = true; 
            GetComponent<BoxCollider>().isTrigger = false;
            GetComponent<NavMeshAgent>().enabled = false;
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x + 100 * Time.deltaTime, transform.eulerAngles.y, transform.eulerAngles.z);
            transform.Translate(Vector3.forward * Time.deltaTime);
            deadDelay -= Time.deltaTime;
            hoverArrow.GetComponent<SpriteRenderer>().enabled = false;
            if (deadDelay <= 0) {
                float rand = Random.Range(0f, 100f);
                if (rand > 75)
                    Instantiate(ammoDropPrefab, transform.position, transform.rotation);
                if (!isPlayer) {
                    string gun = randomGun (); 
                    if (isAlly) {
                        controllerScript.axisScore++;
                        controllerScript.spawnSoldier(true, gun);
                        Destroy(gameObject);
                    } else {
                        controllerScript.alliesScore++;
                        controllerScript.spawnSoldier(false, gun);
                        Destroy(gameObject);
                    }
                } else {
                    if (!controllerScript.isSinglePlayer) {
                        if (!scoreUpdated) {
                            StartCoroutine(changeHealth(id, maxHealth * 10, true));
                            scoreUpdated = true;
                        }
                    } else {
                        controllerScript.axisScore++;
                        controllerScript.respawnPlayer();
                        deadDelay = 1f;
                        dead = false;
                    }
                }
            }
        }
        if (moving) {
            if (totalLegRotationX > -30 && !rightLegForward) {
                rightLeg.transform.Rotate(new Vector3(moveSpeed * Time.deltaTime * 35, 0, 0));
                leftLeg.transform.Rotate(new Vector3(-moveSpeed * Time.deltaTime * 35, 0, 0));
                totalLegRotationX += -moveSpeed * Time.deltaTime * 35;
            }
            if (totalLegRotationX < -30 && !rightLegForward) {
                rightLegForward = true;
            }
            if (totalLegRotationX < 30 && rightLegForward) {
                rightLeg.transform.Rotate(new Vector3(-moveSpeed * Time.deltaTime * 35, 0, 0));
                leftLeg.transform.Rotate(new Vector3(moveSpeed * Time.deltaTime * 35, 0, 0));
                totalLegRotationX += moveSpeed * Time.deltaTime * 35;
            }
            if (totalLegRotationX > 30 && rightLegForward) {
                rightLegForward = false;
            }
        } else if (totalLegRotationX > 1.5f || totalLegRotationX < -1.5f) {
            if (totalLegRotationX > -1.5f) {
                rightLeg.transform.Rotate(new Vector3(moveSpeed * Time.deltaTime * 20, 0, 0));
                leftLeg.transform.Rotate(new Vector3(-moveSpeed * Time.deltaTime * 20, 0, 0));
                totalLegRotationX += -moveSpeed * Time.deltaTime * 20;
            }
            if (totalLegRotationX < 1.5f) {
                rightLeg.transform.Rotate(new Vector3(-moveSpeed * Time.deltaTime * 20, 0, 0));
                leftLeg.transform.Rotate(new Vector3(moveSpeed * Time.deltaTime * 20, 0, 0));
                totalLegRotationX += moveSpeed * Time.deltaTime * 20;
            }
        }
        if (isPlayer && !isTank) {
            if (aiming && aimClamper >= 1) {
                cam.GetComponent<Camera>().fieldOfView = 60 - zoom;
            } else {
                cam.GetComponent<Camera>().fieldOfView = 60;
            }
        }

    }
    void die() {
        dead = true;
    }
    //gun determinance here
    public string randomGun() {
        string gun = "";
        float rand = Random.Range (0, 100);
        if (rand > 85) {
            gun = "mosinNagant";
        } else if (rand > 70) {
            gun = "mg42";
        } else if (rand > 55) {
            gun = "mp40";
        } else if (rand > 40) {
            gun = "ak47";
        } else if (rand > 25) {
            gun = "ppsh41";
        } else
            gun = "m3";
        return gun; 
    }
    void shootBullet() {
        if (!isPlayer || !controllerScript.isSpectator) {
            currentGunClipBullets--;
            if (!controllerScript.isSinglePlayer && !isPlayer)
                currentGunClipBullets++;
            for (int h = 0; h < bulletsPerFire; h++) {
                if (!isTank) {
                    GameObject insItem = Instantiate(gunFlashPrefab, gunHold.transform.position, gunHold.transform.rotation);
                    insItem.transform.Translate(-Vector3.forward * muzzleLength * 1.8f);
                } else {
                    currentTank.GetComponent<BoxCollider>().enabled = false;
                }

                //if not singleplayer, only player should be able to call this function
                if (controllerScript.isSinglePlayer) {
                    if (isTank && !currentTank.GetComponent<Tank>().isAirplane) {
                        Transform turret = currentTank.GetComponent<Tank>().turret.transform;
                        GameObject insItem = Instantiate(tankBulletPrefab, turret.position, turret.rotation);
                        insItem.transform.Rotate(0, 180, 0);
                        controllerScript.audioManager.Play("tankFiring", 1 - (Vector3.Distance(transform.position, controllerScript.audioManager.gameObject.transform.position) / 280));
                    } else if (isTank && currentTank.GetComponent<Tank>().isAirplane) {
                        currentTank.GetComponent<Tank>().shootBullet();
                    } else if (isPlayer) {
                        controllerScript.audioManager.Play(currentGunName, 1);
                        if (totalRotationX < 50 && recoilRotation <= recoil) {
                            gunHold.transform.Rotate(recoil - recoilRotation, 0, 0);
                            recoilRotation = recoil;
                        }
                        RaycastHit hit;
                        if (Physics.Raycast(cam.position, cam.forward, out hit, maxShootingRange)) {
                            if (hit.collider.gameObject.GetComponent<SoldierController>() != null || hit.collider.gameObject.GetComponent<Tank>() != null) {
                                SoldierController hitTarget;
                                if (hit.collider.gameObject.GetComponent<Tank>() != null && hit.collider.gameObject.GetComponent<Tank>().user != null) {
                                    hitTarget = hit.collider.gameObject.GetComponent<Tank>().user.GetComponent<SoldierController>();
                                } else if (hit.collider.gameObject.GetComponent<SoldierController>() != null) {
                                    hitTarget = hit.collider.gameObject.GetComponent<SoldierController>();
                                } else {
                                    break;
                                }
                                if (!hitTarget.isAlly) {
                                    float headShotMultiplier = 1;
                                    if (hit.point.y > hit.collider.transform.position.y + 0.7f && !hitTarget.dead) {
                                        headShotMultiplier = 1.7f;
                                        PlayerPrefs.SetInt("xp", PlayerPrefs.GetInt("xp") + 1);
                                    }
                                    hitTarget.health -= Random.Range(damage * 0.8f * headShotMultiplier, damage * 1.2f * headShotMultiplier);
                                    if (hitTarget.health <= 0 && !hitTarget.dead) {
                                        PlayerPrefs.SetInt("xp", PlayerPrefs.GetInt("xp") + 2);
                                        float rand = Random.Range(0f, 100f);
                                        if (rand > 80f)
                                            PlayerPrefs.SetInt("cash", PlayerPrefs.GetInt("cash") + 1);
                                    }
                                    controllerScript.hitDisplayTimer = 0.1f;
                                }
                            }
                        }
                    } else {
                        if (Vector3.Distance(transform.position, controllerScript.audioManager.gameObject.transform.position) < 280) {
                            controllerScript.audioManager.Play(currentGunName, 1 - (Vector3.Distance(transform.position, controllerScript.audioManager.gameObject.transform.position) / 280));
                        }
                        RaycastHit hit;
                        if (Physics.Raycast(head.transform.position, head.transform.forward, out hit, maxShootingRange)) {
                            if (hit.collider.gameObject.GetComponent<SoldierController>() != null || hit.collider.gameObject.GetComponent<Tank>() != null) {
                                SoldierController hitTarget;
                                if (hit.collider.gameObject.GetComponent<Tank>() != null && hit.collider.gameObject.GetComponent<Tank>().user != null) {
                                    hitTarget = hit.collider.gameObject.GetComponent<Tank>().user.GetComponent<SoldierController>();
                                } else if (hit.collider.gameObject.GetComponent<SoldierController>() != null) {
                                    hitTarget = hit.collider.gameObject.GetComponent<SoldierController>();
                                } else {
                                    break;
                                }
                                if (hitTarget.isAlly == !isAlly) {
                                    float headShotMultiplier = 1;
                                    if (hit.point.y > hit.collider.transform.position.y + 0.7f)
                                        headShotMultiplier = 1.7f;
                                    if (!hitTarget.isPlayer || controllerScript.recentDeathTimer <= 0)
                                        hitTarget.health -= Random.Range(damage * 0.8f * headShotMultiplier, damage * 1.2f * headShotMultiplier);
                                }
                            }
                        }
                    }
                } else {
                    controllerScript.audioManager.Play(currentGunName, 1 - (Vector3.Distance(transform.position, controllerScript.audioManager.gameObject.transform.position) / 280));
                    if (isPlayer) {
                        if (totalRotationX < 50 && recoilRotation <= recoil) {
                            gunHold.transform.Rotate(recoil - recoilRotation, 0, 0);
                            recoilRotation = recoil;
                        }
                        RaycastHit hit;
                        if (Physics.Raycast(cam.position, cam.forward, out hit, maxShootingRange)) {
                            if (hit.collider.gameObject.GetComponent<SoldierController>() != null) {
                                if (hit.collider.gameObject.GetComponent<SoldierController>().isAlly == !isAlly) {
                                    float headShotMultiplier = 1;
                                    if (hit.point.y > hit.collider.transform.position.y + 0.7f)
                                        headShotMultiplier = 1.7f;
                                    StartCoroutine(changeHealth(hit.collider.gameObject.GetComponent<SoldierController>().id, -Random.Range(damage * 0.8f * headShotMultiplier, damage * 1.2f * headShotMultiplier), false));

                                    controllerScript.hitDisplayTimer = 0.1f;
                                }
                            }
                        }
                    }
                }
                transform.Rotate(0, Random.Range(-sideRecoil, sideRecoil), 0);
            }
            if (isTank)
                currentTank.GetComponent<BoxCollider>().enabled = true;
        }
    }
    IEnumerator getGunName() {
        string urlString = controllerURL + "getGunNameById?id=" + id.ToString();
        WWW variables = new WWW(urlString);
        yield return variables;
        gun1 = controllerScript.findGun(variables.text);
        currentGunName = variables.text;
        setGun (false);
    }
    IEnumerator getHealth() {
        string urlString = controllerURL + "getHealthById?id=" + id.ToString();
        WWW variables = new WWW(urlString);
        yield return variables;
        float a;
        if (float.TryParse(variables.text, out a)) {
            health = float.Parse(variables.text);
            if (health > 0) {
                recentDeath = false; 
                scoreUpdated = false;
            }
        }
    }
    IEnumerator changeHealth(int targetId, float amount, bool respawnPlayer) {
        if (targetId != killedPersonId) {
            string urlString = controllerURL + "changeHealthById?id=" + targetId.ToString() + "&change=" + amount.ToString();
            WWW variables = new WWW(urlString);
            yield return variables;
            recentDeath = false; 
            if (respawnPlayer) {
                health = maxHealth;
                controllerScript.respawnPlayer();
                StartCoroutine(updateScore());
                deadDelay = 1f;
                dead = false;
            } else {
                if (killedPersonId != targetId && float.Parse(variables.text) <= 0) {
                    StartCoroutine(addKill(id, targetId));
                    killedPersonId = targetId;
                }
            }
        }
    }

    public IEnumerator addKill(int senderId, int victimId) {
        string urlString = controllerURL + "addKill?id=" + id.ToString() + "&victimId=" + victimId.ToString();
        WWW variables = new WWW(urlString);
        yield return variables;
    }
    void detectEnemyPathBlock() {
        RaycastHit hit;
        if (Physics.Raycast(head.transform.position, head.transform.forward, out hit, maxShootingRange)) {
            if (hit.collider.gameObject.GetComponent<SoldierController>() == null && hit.collider.gameObject.GetComponent<Tank> () == null) {
                cannotAim = true;
            } else
                cannotAim = false;

        }
    }
}
