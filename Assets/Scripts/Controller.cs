using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class Custom2D {
    public static Rect generatePointDetectionRect(Vector2 position, Rect rect) {
        Rect newRect = new Rect(new Vector2(position.x - rect.width / 2, position.y - rect.height / 2), new Vector2(rect.width, rect.height));
        return newRect;
    }
}
public class VarFinder {
    public static float findFloatInString(string text, int index) {
        char[] charText = text.ToCharArray();
        bool haveStartIndex = false;
        int myStartIndex = 0;
        for (int i = 0; i < text.Length; i++) {
            float f;
            if (float.TryParse(charText[i].ToString(), out f) || charText[i].ToString() == "." || charText[i].ToString() == "-") {
                if (i != 0) {
                    if (!float.TryParse(charText[i - 1].ToString(), out f) && charText[i - 1].ToString() != "." && charText[i - 1].ToString() != "-") {
                        myStartIndex = i;
                        haveStartIndex = true;
                    }
                } else {
                    myStartIndex = i;
                    haveStartIndex = true;
                }
                if (i == text.Length - 1 && charText[text.Length - 1].ToString() != "." && charText[text.Length - 1].ToString() != "-") {
                    if (index > 0) {
                        return 0;
                    } else {
                        return float.Parse(text.Substring(myStartIndex, i - myStartIndex + 1));
                    }
                }
            } else if (i != 0) {
                if ((float.TryParse(charText[i - 1].ToString(), out f) || charText[i - 1].ToString() == ".") && charText[i - 1].ToString() != "-") {
                    if (haveStartIndex) {
                        if (index > 0) {
                            index--;
                            haveStartIndex = false;
                        } else {
                            if (text.Substring(myStartIndex, i - myStartIndex) != "." && text.Substring(myStartIndex, i - myStartIndex) != "-")
                                return float.Parse(text.Substring(myStartIndex, i - myStartIndex));
                        }
                    }
                }
            }
        }
        return 0;
    }
}
public class Controller : MonoBehaviour {
    public float version;

    public GameObject currentMap;
    public GameObject[] maps;
    public InputField chatText;
    public Text idText;
    public Text[] messagesDisplay;
    public GameObject mapCam;

    bool gameFinished = false;

    public bool isSinglePlayer;
    //manually change for publishing purposes
    public bool isTablet;
    //immune but cannot shoot, cannot die, and can fly
    public bool isSpectator;
    //point capture mode
    public bool isPointCapture;
    //delete on demand
    public GameObject victoryBanner;
    public GameObject[] UIComponents;
    public GameObject[] spectatorRemoveUIComponents;
    public GameObject ballRoller;
    public GameObject fireRight;
    public GameObject fireLeft;
    [HideInInspector]
    public Rect ballRollerRect;
    [HideInInspector]
    public Rect fireRightRect;
    [HideInInspector]
    public Rect fireLeftRect;

    public Image blood;

    //for team deathmatch
    public float alliesScore;
    public float axisScore;

    //for point capture
    public float alliedPoints;
    public float axisPoints;

    public Text displayKD;
    public Image displayKDBackground;
    public Text scoreText1;
    public Text scoreText2;
    public GameObject alliedScoreBar;
    public Image crosshair;
    public Image scope;
    public GameObject pauseUI;

    [HideInInspector]
    public Transform[] allySpawnPoints;
    [HideInInspector]
    public Transform[] enemySpawnPoints;
    [HideInInspector]
    public GameObject[] flagpoles;
    [HideInInspector]
    public GameObject[] tanks;

    //singlePlayer for findtargeting
    [HideInInspector]
    public GameObject[] allyArray;
    [HideInInspector]
    public GameObject[] enemyArray;

    //multiplayer, does not include this client's player
    public SoldierController[] players;

    public SoldierController player;
    public AudioManager audioManager;
    public GameObject soldierPrefab;
    public Text healthDisplay;
    public Text ammoDisplay;
    public Image hitDisplay;

    //guns
    public GameObject ak47;
    public GameObject mosinNagant;
    public GameObject mg42;
    public GameObject mp40;
    public GameObject s1897;
    public GameObject sks;
    public GameObject svd; 
    public GameObject ppsh41;
    public GameObject m3;

    public bool showKD;

    [HideInInspector]
    public GameObject[] gunPrefabs;
    public GameObject kickButton;

    public bool displayMap;
    public RawImage map;

    public float hitDisplayTimer;
    public float recentDeathTimer;
    public bool paused;
    public bool gameQuit;
    string controllerURL = "http://47.97.222.112:8080/";
    public int id;

    [HideInInspector]
    public int maxPlayers;

    IEnumerator getMaxPlayers() {
        WWW variables = new WWW(controllerURL + "getMaxPlayers");
        yield return variables;

        maxPlayers = int.Parse(variables.text);
    }

    //current vars only for storage transportation
    Vector3 currentGunRotation;
    Vector3 currentRotation;
    Vector3 currentPosition;
    int currentId;
    IEnumerator getId(bool setInitialValues) {
        WWW variables = new WWW(controllerURL + "getId?name=" + PlayerPrefs.GetString("playerName"));
        yield return variables;

        id = int.Parse(variables.text);
        print(variables.text);
        if (id == 0 && !gameQuit) {
            //room full
            kickButton.transform.Translate(0, -3000, 0);
            kickButton.transform.GetChild(0).GetComponent<Text>().text = "The room is full.\nClick to exit to menu";
            Time.timeScale = 0;
            if (id != 0)
                StartCoroutine(quitMultiplayer(false));
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseGame();
            gameQuit = true;
        }
        player.id = id;
        if (setInitialValues) {
            respawnPlayer();
            StartCoroutine(setPlayerInitialValues());
            StartCoroutine(updatePlayerInfo());
        }
    }
    IEnumerator getScores() {
        WWW variables = new WWW(controllerURL + "getScore");
        yield return variables;

        float deltaAlliesScore = alliesScore;
        float deltaAxisScore = axisScore;

        alliesScore = (int)VarFinder.findFloatInString(variables.text, 0);
        axisScore = (int)VarFinder.findFloatInString(variables.text, 1);

        if ((int)deltaAxisScore != (int)axisScore || (int)deltaAlliesScore != (int)alliesScore) {
            player.killedPersonId = 0;
        }
    }
    IEnumerator getVersion() {
        WWW variables = new WWW(controllerURL + "getVersion");
        yield return variables;
        print(variables.text);
        if (float.Parse(variables.text) > version && !gameQuit) {
            kickButton.transform.Translate(0, -3000, 0);
            kickButton.transform.GetChild(0).GetComponent<Text>().text = "Version outdated.\nClick to exit to menu";
            Time.timeScale = 0;
            if (id != 0)
                StartCoroutine(quitMultiplayer(false));
            print("Version Mismatch");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseGame();
            gameQuit = true;
        }
    }
    IEnumerator setPlayerInitialValues() {
        string urlString = controllerURL + "postInitialPlayerInfo" + "?id=" + id.ToString() + "&maxHealth=" + player.maxHealth + "&isAlly=" + player.isAlly.ToString();
        WWW variables = new WWW(urlString);
        yield return variables;
    }
    IEnumerator updatePlayerInfo() {
        string urlString = controllerURL + "postPlayerInfo" + "?id=" + id.ToString() + "&posX=" + player.transform.position.x.ToString() + "&posY=" + player.transform.position.y.ToString() + "&posZ=" + player.transform.position.z.ToString() + "&rotX=" + player.transform.eulerAngles.x.ToString() + "&rotY=" + player.transform.eulerAngles.y.ToString() + "&rotZ=" + player.transform.eulerAngles.z.ToString() + "&gunRotX=" + player.gunHold.transform.eulerAngles.x.ToString() + "&gunRotY=" + player.gunHold.transform.eulerAngles.y.ToString() + "&gunRotZ=" + player.gunHold.transform.eulerAngles.z.ToString() + "&moving=" + player.moving.ToString() + "&dead=" + player.dead.ToString() + "&shooting=" + player.recentShooting.ToString() + "&gunName=" + player.currentGunName;
        WWW variables = new WWW(urlString);
        yield return variables;
    }
    IEnumerator spawnPlayers(int id) {
        WWW variables = new WWW(controllerURL + "getAllPositionsAndRotationsById?id=" + (id + 1).ToString());
        yield return variables;
        if (players[id] == null) {
            PosAndRot data = JsonUtility.FromJson<PosAndRot>(variables.text);
            currentPosition = new Vector3(data.position[0], data.position[1], data.position[2]);
            currentRotation = new Vector3(data.rotation[0], data.rotation[1], data.rotation[2]);
            currentGunRotation = new Vector3(data.gunRotation[0], data.gunRotation[1], data.gunRotation[2]);
            //change gun to custom later
            GameObject insItem = Instantiate(soldierPrefab, currentPosition, Quaternion.Euler(currentRotation));
            insItem.GetComponent<SoldierController>().currentGunName = "ak47";
            insItem.GetComponent<SoldierController>().id = id + 1;
            players[id] = insItem.GetComponent<SoldierController>();
        }
    }
    IEnumerator getPlayers() {
        WWW variables = new WWW(controllerURL + "getAllIds");
        yield return variables;
        for (int i = 0; i < maxPlayers; i++) {
            if (VarFinder.findFloatInString(variables.text, i) == 0 && i + 1 == id) {
                //disconnected
                if (!gameQuit) {
                    kickButton.transform.Translate(0, -3000, 0);
                    kickButton.transform.GetChild(0).GetComponent<Text>().text = "You have been disconnected.\nClick to exit to menu";
                }
                Time.timeScale = 0;
                if (id != 0)
                    StartCoroutine(quitMultiplayer(false));
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                pauseGame();
                gameQuit = true;
            } else if (VarFinder.findFloatInString(variables.text, i) != 0 && players[i] == null && i + 1 != id) {
                StartCoroutine(spawnPlayers(i));
            } else if (VarFinder.findFloatInString(variables.text, i) == 0 && players[i] != null) {
                Destroy(players[i].gameObject);
            }
        }
    }
    public void togglePause() {
        if (player.isTank) {
            player.transform.position = player.currentTank.GetComponent<Tank>().turret.transform.position;
        }
        paused = !paused;
        if (isSinglePlayer) {
            foreach (GameObject i in allyArray) {
                if (i != null) {
                    i.GetComponent<SoldierController>().enabled = !paused;
                    i.GetComponent<Rigidbody>().freezeRotation = !paused;
                }
            }
            foreach (GameObject i in enemyArray) {
                if (i != null) {
                    i.GetComponent<SoldierController>().enabled = !paused;
                    i.GetComponent<Rigidbody>().freezeRotation = paused;
                }
            }
        }
        if (paused && isSinglePlayer) {
            Time.timeScale = 0;
        } else
            Time.timeScale = 1;

    }
    void Awake() {
        if (isSinglePlayer) {
            if (PlayerPrefs.GetInt("spectator") == 1) {
                isSpectator = true;
            }
            if (PlayerPrefs.GetInt("pointCapture") == 1)
                isPointCapture = true;
        }
        //no more than 60 tanks per map
        tanks = new GameObject[60];
        if (isSpectator) {
            player.health = 1000000f;
            player.maxHealth = 1000000f;
            player.GetComponent<Rigidbody>().useGravity = false;
            player.GetComponent<Rigidbody>().freezeRotation = false;
            foreach (BoxCollider i in player.GetComponentsInChildren<BoxCollider>()) {
                i.enabled = false;
            }
            foreach (GameObject i in spectatorRemoveUIComponents) {
                if (i.name == "Crosshair")
                    i.transform.Translate(0, 8000, 0);
                foreach (Renderer j in i.GetComponentsInChildren<Renderer>()) {
                    j.enabled = false;
                }
                foreach (Text j in i.GetComponentsInChildren<Text>()) {
                    j.enabled = false;
                }
                foreach (Image j in i.GetComponentsInChildren<Image>()) {
                    j.enabled = false;
                }
            }
        }
    }
    void Start() {
        controllerURL = PlayerPrefs.GetString("mpUrl");
        if (!isSinglePlayer) {
            foreach (GameObject i in maps) {
                if (i.GetComponent<Map>().name == PlayerPrefs.GetString("map")) {
                    Destroy(currentMap);
                    GameObject insItem = Instantiate(i, new Vector3(-i.GetComponent<Map>().size / 2, 0, -i.GetComponent<Map>().size / 2), Quaternion.identity);
                    allySpawnPoints = insItem.GetComponent<Map>().allySpawnPoints;
                    enemySpawnPoints = insItem.GetComponent<Map>().enemySpawnPoints;
                    mapCam.GetComponent<Camera>().orthographicSize = i.GetComponent<Map>().size / 2;
                    break;
                }
            }
        } else {
            foreach (GameObject i in maps) {
                if (i.GetComponent<Map>().name != PlayerPrefs.GetString("map")) {
                    Destroy(i);
                } else {
                    allySpawnPoints = i.GetComponent<Map>().allySpawnPoints;
                    enemySpawnPoints = i.GetComponent<Map>().enemySpawnPoints;
                    alliedPoints = i.GetComponent<Map>().startingVictoryPoints;
                    axisPoints = i.GetComponent<Map>().startingVictoryPoints;
                    flagpoles = i.GetComponent<Map>().flagpoles;
                    mapCam.transform.position = new Vector3(i.transform.position.x + (i.GetComponent<Map>().size / 2), 300, i.transform.position.z + (i.GetComponent<Map>().size / 2));
                    mapCam.GetComponent<Camera>().orthographicSize = i.GetComponent<Map>().size / 2;
                }
            }
        }
        ballRollerRect = Custom2D.generatePointDetectionRect(new Vector2(ballRoller.transform.position.x, ballRoller.transform.position.y), ballRoller.GetComponent<RectTransform>().rect);
        fireRightRect = Custom2D.generatePointDetectionRect(new Vector2(fireRight.transform.position.x, fireRight.transform.position.y), fireRight.GetComponent<RectTransform>().rect);
        fireLeftRect = Custom2D.generatePointDetectionRect(new Vector2(fireLeft.transform.position.x, fireLeft.transform.position.y), fireLeft.GetComponent<RectTransform>().rect);
        gunPrefabs = new GameObject[] {ak47, mosinNagant, mg42, mp40, s1897, sks, ppsh41, m3, svd};
        togglePause();
        togglePause();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!isTablet) {
            foreach (GameObject i in UIComponents) {
                Destroy(i.gameObject);
            }
        }
        if (!isSinglePlayer) {
            StartCoroutine(getId(true));
            StartCoroutine(getVersion());
            StartCoroutine(getMaxPlayers());
            StartCoroutine(setPlayerInitialValues());
            chatText.enabled = false;
        }
        player.gun1 = findGun(PlayerPrefs.GetString("gun1"));
        player.gun2 = findGun(PlayerPrefs.GetString("gun2"));
        if (isSinglePlayer) {
            enemyArray = new GameObject[600];
            allyArray = new GameObject[600];
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (!isSpectator)
                allyArray[0] = player.gameObject;

            //Spawn ally
            for (int j = 0; j < PlayerPrefs.GetInt("allyAmount"); j++) {
                spawnSoldier(true, player.randomGun());
            }

            //Spawn enemies
            for (int j = 0; j < PlayerPrefs.GetInt("enemyAmount"); j++) {
                spawnSoldier(false, player.randomGun());
            }

            respawnPlayer();
        }
    }
    public void respawnPlayer() {
        if (player.aiming) {
            player.toggleAim();
            player.aimClamper = 1;
        }
        player.reloadDelay = 0;
        player.hoverArrow.GetComponent<SpriteRenderer>().enabled = true;
        Vector3 spawnPos = Vector3.zero;
        if (!isSinglePlayer) {
            if (id % 2 != 0) {
                player.isAlly = true;
                int chosenIndex = 0;
                int currentIndex = 0;

                //selects longest distance index from all enemy players to find best spawn point
                float longestDistance = 0;
                foreach (Transform h in allySpawnPoints) {
                    //choose shortest distance to any player
                    float currentShortestDistance = 100000;
                    foreach (SoldierController i in players) {
                        if (i != null && !i.isAlly) {
                            if (Vector3.Distance(h.position, i.transform.position) < currentShortestDistance) {
                                currentShortestDistance = Vector3.Distance(h.position, i.transform.position);
                            }
                        }
                    }
                    if (currentShortestDistance > longestDistance) {
                        longestDistance = currentShortestDistance;
                        chosenIndex = currentIndex;
                    }
                    currentIndex++;
                }
                spawnPos = allySpawnPoints[chosenIndex].position;
                spawnPos.x += Random.Range(-4f, 4f);
                spawnPos.z += Random.Range(-4f, 4f);
            } else {
                player.isAlly = false;
                int chosenIndex = 0;
                int currentIndex = 0;
                float longestDistance = 0;
                foreach (Transform h in enemySpawnPoints) {
                    float currentShortestDistance = 100000;
                    foreach (SoldierController i in players) {
                        if (i != null && i.isAlly) {
                            if (Vector3.Distance(h.position, i.transform.position) < currentShortestDistance) {
                                currentShortestDistance = Vector3.Distance(h.position, i.transform.position);
                            }
                        }
                    }
                    if (currentShortestDistance > longestDistance) {
                        longestDistance = currentShortestDistance;
                        chosenIndex = currentIndex;
                    }
                    currentIndex++;
                }
                spawnPos = enemySpawnPoints[chosenIndex].position;
                spawnPos.x += Random.Range(-4f, 4f);
                spawnPos.z += Random.Range(-4f, 4f);
            }
        } else {
            player.isAlly = true;
            int rand = Random.Range(0, allySpawnPoints.Length);
            spawnPos = allySpawnPoints[rand].position;
            spawnPos.x += Random.Range(-4f, 4f);
            spawnPos.z += Random.Range(-4f, 4f);
            recentDeathTimer = 5;
        }
        player.changeUniform();
        player.resetAmmo();
        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.identity;
        player.health = player.maxHealth;

        if (isSpectator) {
            player.transform.Translate(0, 60, 0);
            player.transform.Rotate(-30, 180, 0);
            player.moveSpeed *= 10;
        }
    }
    public GameObject findGun(string name) {
        for (int i = 0; i < gunPrefabs.Length; i++) {
            if (gunPrefabs[i] != null && gunPrefabs[i].GetComponent<Gun>().name == name) {
                return gunPrefabs[i].gameObject;
            }
        }
        return ak47;
    }
    public void spawnSoldier(bool isAlly, string gunName) {
        //only use this function when in singleplayer
        if (!isAlly) {
            int rand = Random.Range(0, enemySpawnPoints.Length);
            Vector3 spawnPos = enemySpawnPoints[rand].position;
            spawnPos.x += Random.Range(-6f, 6f);
            spawnPos.z += Random.Range(-6f, 6f);

            GameObject insItem = Instantiate(soldierPrefab, spawnPos, Quaternion.identity);
            insItem.GetComponent<SoldierController>().currentGunName = gunName;
            for (int i = 0; i < enemyArray.Length; i++) {
                if (enemyArray[i] == null) {
                    enemyArray[i] = insItem;
                    break;
                }
            }
        } else {
            int rand = Random.Range(0, allySpawnPoints.Length);
            Vector3 spawnPos = allySpawnPoints[rand].position;
            spawnPos.x += Random.Range(-6f, 6f);
            spawnPos.z += Random.Range(-6f, 6f);

            GameObject insItem = Instantiate(soldierPrefab, spawnPos, Quaternion.identity);
            insItem.GetComponent<SoldierController>().currentGunName = gunName;
            insItem.GetComponent<SoldierController>().isAlly = true;
            for (int i = 0; i < allyArray.Length; i++) {
                if (allyArray[i] == null) {
                    allyArray[i] = insItem;
                    break;
                }
            }
        }
    }
    float updateCooldown = 0.37f;
    float updateTimer = 0.37f;
    float slowUpdateCooldown = 4f;
    float slowUpdateTimer = 3f;
    int kills = 0;
    IEnumerator updateKillDeath() {
        string urlString = controllerURL + "getKills";
        WWW variables1 = new WWW(urlString);
        yield return variables1;

        urlString = controllerURL + "getDeaths";
        WWW variables2 = new WWW(urlString);
        yield return variables2;

        urlString = controllerURL + "getPlayerNames";
        WWW variables3 = new WWW(urlString);
        yield return variables3;
        Strings s = JsonUtility.FromJson<Strings>(variables3.text);
        displayKD.text = "";
        for (int i = 0; i < s.text.Length; i++) {
            if (s.text[i] != "") {
                if (s.text[i] == PlayerPrefs.GetString("accountName")) {
                    int killsDelta = kills;
                    kills = (int)VarFinder.findFloatInString(variables1.text, i);
                    if (killsDelta > 0) {
                        PlayerPrefs.SetInt("xp", PlayerPrefs.GetInt("xp") + 10 * (kills - killsDelta));
                        PlayerPrefs.SetInt("cash", PlayerPrefs.GetInt("cash") + 1 * (kills - killsDelta));
                    }
                }
                displayKD.text += s.text[i] + ": " + (int)VarFinder.findFloatInString(variables1.text, i) + " kills, " + (int)VarFinder.findFloatInString(variables2.text, i) + " deaths\n";
            }
        }
    }
    IEnumerator getMessages() {
        string urlString = controllerURL + "getMessages";
        WWW variables = new WWW(urlString);
        yield return variables;

        Strings s = JsonUtility.FromJson<Strings>(variables.text);

        for (int i = 0; i < messagesDisplay.Length; i++) {
            if (i < s.text.Length) {
                messagesDisplay[i].text = s.text[s.text.Length - i - 1];
            }
        }
    }
    IEnumerator pushMessage(string message) {
        string urlString = controllerURL + "postMessage?message=" + message + "&name=" + PlayerPrefs.GetString("playerName");
        WWW variables = new WWW(urlString);
        yield return variables;
        print(variables.url);
    }
    public void enableChat() {
        chatText.enabled = true;
    }
    public void sendMessage() {
        if (chatText.text != "") {
            StartCoroutine(pushMessage(chatText.text));
            chatText.text = "";
            chatText.enabled = false;
        }
    }
    //add in mobile
    public void toggleKD() {
        showKD = !showKD;
    }
    //point capture
    void winGame() {
        gameFinished = true;
        victoryBanner.GetComponent<Image>().enabled = true;
        victoryBanner.transform.GetChild(0).GetComponent<Text>().enabled = true;
        victoryBanner.GetComponent<Image>().color = new Color(0, 1, 0, 0.7f);

        //change to first time reward
        if (!isSpectator)
            PlayerPrefs.SetInt("xp", PlayerPrefs.GetInt("xp") + 100);
    }
    void loseGame() {
        gameFinished = true;
        victoryBanner.GetComponent<Image>().enabled = true;
        victoryBanner.transform.GetChild(0).GetComponent<Text>().enabled = true;
        victoryBanner.GetComponent<Image>().color = new Color(1, 0, 0, 0.7f);
        victoryBanner.transform.GetChild(0).GetComponent<Text>().text = "Axis Victorious!";
    }
    void Update() {
        
        if (isTablet) {
            ballRollerRect = Custom2D.generatePointDetectionRect(new Vector2(ballRoller.transform.position.x, ballRoller.transform.position.y), ballRoller.GetComponent<RectTransform>().rect);
            fireRightRect = Custom2D.generatePointDetectionRect(new Vector2(fireRight.transform.position.x, fireRight.transform.position.y), fireRight.GetComponent<RectTransform>().rect);
            fireLeftRect = Custom2D.generatePointDetectionRect(new Vector2(fireLeft.transform.position.x, fireLeft.transform.position.y), fireLeft.GetComponent<RectTransform>().rect);
        }
           if (recentDeathTimer > 0) {
            recentDeathTimer -= Time.deltaTime;
        }
        if (gameQuit && Input.GetMouseButtonUp(0)) {
            kickExit();
        }
        if (player.health < player.maxHealth && !player.isTank) {
            if (isSinglePlayer)
                blood.color = new Color(1, 1, 1, 1 - player.health / player.maxHealth);
            else
                blood.color = new Color(1, 1, 1, (1 - player.health / player.maxHealth) / 1.5f);
        } else
            blood.color = new Color(1, 1, 1, 0);
        if (!isSinglePlayer) {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                showKD = !showKD;
            }
            if (showKD) {
                displayKD.enabled = true;
                displayKDBackground.enabled = true;
            } else {
                displayKD.enabled = false;
                displayKDBackground.enabled = false;
            }
            if (Input.GetKeyDown(KeyCode.T)) {
                chatText.enabled = true;
            }
            if (chatText.enabled)
                chatText.Select();
            idText.text = id.ToString();
            if (updateTimer > 0) {
                updateTimer -= Time.deltaTime;
            } else {
                StartCoroutine(updatePlayerInfo());
                updateTimer = updateCooldown;
            }
            if (slowUpdateTimer > 0) {
                slowUpdateTimer -= Time.deltaTime;
            } else {
                StartCoroutine(getMessages());
                StartCoroutine(getPlayers());
                StartCoroutine(getScores());
                StartCoroutine(updateKillDeath());
                slowUpdateTimer = slowUpdateCooldown;
            }
            if (Input.GetKeyDown(KeyCode.Return) && chatText.text != "") {
                StartCoroutine(pushMessage(chatText.text));
                chatText.text = "";
                chatText.enabled = false;
            }
        }
        if (isPointCapture) {
            if (alliedPoints == 0 && axisPoints == 0) {
                alliedScoreBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -80) * alliedScoreBar.GetComponent<UIAlignment>().scale;
            } else {
                alliedScoreBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(alliedPoints / (alliedPoints + axisPoints) * 200f - 200f, -80) * alliedScoreBar.GetComponent<UIAlignment>().scale;
                alliedScoreBar.GetComponent<RectTransform>().sizeDelta = new Vector2(alliedPoints / (alliedPoints + axisPoints) * 400, 40) * alliedScoreBar.GetComponent<UIAlignment>().scale;
            }
        } else {
            if (alliesScore == 0 && axisScore == 0) {
                alliedScoreBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -80) * alliedScoreBar.GetComponent<UIAlignment>().scale;
            } else {
                if (player.isAlly) {
                    alliedScoreBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(alliesScore / (alliesScore + axisScore) * 200f - 200f, -80) * alliedScoreBar.GetComponent<UIAlignment>().scale;
                    alliedScoreBar.GetComponent<RectTransform>().sizeDelta = new Vector2(alliesScore / (alliesScore + axisScore) * 400, 40) * alliedScoreBar.GetComponent<UIAlignment>().scale;
                } else {
                    alliedScoreBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(axisScore / (axisScore + alliesScore) * 200f - 200f, -80) * alliedScoreBar.GetComponent<UIAlignment>().scale;
                    alliedScoreBar.GetComponent<RectTransform>().sizeDelta = new Vector2(axisScore / (axisScore + alliesScore) * 400, 40) * alliedScoreBar.GetComponent<UIAlignment>().scale;
                }
            }
        }

        ammoDisplay.text = player.currentGunClipBullets.ToString() + " / " + player.currentGunTotalBullets.ToString();

        if (player.aiming) {
            if (player.hasScope && player.aimClamper >= 1) {
                player.cam.GetComponent<Camera>().nearClipPlane = 0.06f;
                scope.enabled = true;
            } else {
                player.cam.GetComponent<Camera>().nearClipPlane = 0.01f;
                scope.enabled = false;
            }
            if (player.aimClamper >= 1)
                crosshair.enabled = false;
        } else {
            player.cam.GetComponent<Camera>().nearClipPlane = 0.01f;
            crosshair.enabled = true;
            scope.enabled = false;
        }
        if (player.isAlly) {
            if (isPointCapture) {
                if (!gameFinished) {
                    if (axisPoints <= 0) {
                        winGame();
                    }
                    if (alliedPoints <= 0) {
                        loseGame();
                    }
                }
                int allyVP = 0;
                int axisVP = 0;
                foreach (GameObject i in flagpoles) {
                    if (i.GetComponent<Flagpole>() != null) {
                        if (i.GetComponent<Flagpole>().captureStatus > 0) {
                            allyVP++;
                        } else {
                            axisVP++;
                        }
                    }
                }
                scoreText1.text = alliedPoints.ToString() + " (" + allyVP.ToString() + ")";
                scoreText2.text = axisPoints.ToString() + " (" + axisVP.ToString() + ")";
            } else {
                scoreText1.text = alliesScore.ToString();
                scoreText2.text = axisScore.ToString();
            }
        } else {
            scoreText2.text = alliesScore.ToString();
            scoreText1.text = axisScore.ToString();
        }
        if (displayMap) {
            map.enabled = true;
        } else
            map.enabled = false;
        if (player.health < 0) {
            player.health = 0;
        }
        healthDisplay.text = ((int)player.health).ToString();

        if (Input.GetKeyDown(KeyCode.M) && (isSinglePlayer || !chatText.isFocused)) {
            displayMap = !displayMap;
        }
        if (hitDisplayTimer > 0) {
            hitDisplayTimer -= Time.deltaTime;
            hitDisplay.enabled = true;
        } else
            hitDisplay.enabled = false;

        if (Input.GetKeyDown(KeyCode.Escape) && !gameQuit || (Input.GetKeyDown(KeyCode.Q) && (isSinglePlayer || !chatText.isFocused)) && !gameQuit) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (!paused) {
                pauseGame();
            } else {
                resumeGame();
            }
        }
    }
    //tablet
    public void pauseGame() {
        if (!paused) {
            togglePause();
            pauseUI.transform.Translate(0, -4000, 0);
        }
    }
    public void resumeGame() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (paused) {
            togglePause();
            pauseUI.transform.Translate(0, 4000, 0);
        }
    }
    IEnumerator quitMultiplayer(bool changeScene) {
        WWW variables = new WWW(controllerURL + "removeId?id=" + id.ToString());
        yield return variables;
        if (changeScene) {
            SceneManager.LoadSceneAsync(0);
        } else
            player.gameObject.GetComponent<SoldierController>().enabled = false;
    }
    public void kickExit() {
        SceneManager.LoadSceneAsync(0);
    }
    public void exitToMenu() {
        PlayerPrefs.SetInt("inGame", 1);
        if (isSinglePlayer) {
            SceneManager.LoadSceneAsync(0);
        } else {
            SceneManager.LoadSceneAsync(0);
            StartCoroutine(quitMultiplayer(true));
        }
    }
}
