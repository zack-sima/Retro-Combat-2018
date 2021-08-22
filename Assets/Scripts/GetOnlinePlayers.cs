using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GetOnlinePlayers : MonoBehaviour {
	//only use for mp buttons
	//use format "api[num]/"
	public string serverName;
    public string chineseServerName; 
	public string serverDirectory;
	public string mapName;
	string url = "http://47.97.222.112:8080/";

	float timerDelay = 0;
	void Awake () {
		if (serverDirectory != "") {
			url += serverDirectory;
		} else {
			url += "api/";
		}
	}
	int deltaPing; 
	IEnumerator getOnlinePlayers () {
		WWW variables = new WWW (url + "getOnlinePlayers"); 
		yield return variables;
        if (PlayerPrefs.GetString ("language") == "chinese") {
            if (variables.text.Length <= 3 && variables.text.Length > 0) {
                transform.GetChild (0).GetComponent<Text> ().text = "进入 " + chineseServerName + " (" + variables.text + " 在线)";
            } else
                transform.GetChild (0).GetComponent<Text> ().text = "进入 " + chineseServerName + " (0 在线)";
        } else {
            if (variables.text.Length <= 3 && variables.text.Length > 0) {
                transform.GetChild (0).GetComponent<Text> ().text = "Join " + serverName + " (" + variables.text + " online)";
            } else
                transform.GetChild (0).GetComponent<Text> ().text = "Join " + serverName + " (0 online)";
        }
	}
	void Update () {
		if (timerDelay > 0) {
			timerDelay -= Time.deltaTime;
		} else {
			StartCoroutine (getOnlinePlayers ());
			timerDelay = 1;
		}
	}
	public void loadGame (int sceneCount) {
		PlayerPrefs.SetString ("map", mapName);
		PlayerPrefs.SetString ("mpUrl", url);
        SceneManager.LoadScene (sceneCount);
	}
}
