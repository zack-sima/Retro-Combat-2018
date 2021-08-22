using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AccountManagement : MonoBehaviour {
    public InputField nameField;
    public InputField passwordField;
    public InputField emailField;
    public Text signUpButtonText;
    public Text emailText;
    public Text emailPlaceholder;
    public Text emailReminder;
    public Text levelText;
    public Transform accountBackground;
    string controllerURL = "http://47.97.222.112:8080/accountManagement/";

    void Start() {
        if (PlayerPrefs.GetInt("loggedIn") == 1) {
            nameField.text = PlayerPrefs.GetString("accountName");
            passwordField.text = PlayerPrefs.GetString("accountPassword");
        }
        if (PlayerPrefs.GetInt("loggedIn") == 1 && PlayerPrefs.GetInt("inGame") == 1) {
            if (PlayerPrefs.GetInt("xp") >= 1)
                StartCoroutine(updateAccountInfo(false));
        } else if (PlayerPrefs.GetInt("loggedIn") == 1) {
            PlayerPrefs.SetInt("loggedIn", 0);
            resetStats();
            loginToAccount();
        }
        PlayerPrefs.SetInt("inGame", 0);
    }
    void Update() {
        var xp = PlayerPrefs.GetInt("xp");
        int level = 1;
        for (int i = 1; xp >= 25 * i + 75; i++) {
            level++;
            xp -= 25 * i + 75;
        }
        int xpForLevelUp = 25 * level + 75;
        if (PlayerPrefs.GetInt("loggedIn") == 1) {
            loggedIn();
            if (PlayerPrefs.GetString("language") == "chinese") {
                levelText.text = PlayerPrefs.GetString("accountName") + ": 等级 " + level + " (" + xp + "/" + xpForLevelUp + ")";
            } else {
                levelText.text = PlayerPrefs.GetString("accountName") + ": Level " + level + " (" + xp + "/" + xpForLevelUp + ")";
            }
        } else if (PlayerPrefs.GetString("language") == "chinese") {
            levelText.text = "游客: 等级 " + level + " (" + xp + "/" + xpForLevelUp + ")";
            PlayerPrefs.SetInt("guestCash", PlayerPrefs.GetInt("cash"));
            PlayerPrefs.SetInt("guestXp", PlayerPrefs.GetInt("xp"));
        } else {
            levelText.text = "Guest: Level " + level + " (" + xp + "/" + xpForLevelUp + ")";
            PlayerPrefs.SetInt("guestCash", PlayerPrefs.GetInt("cash"));
            PlayerPrefs.SetInt("guestXp", PlayerPrefs.GetInt("xp"));
        }
        if (emailField.GetComponent<Image>().enabled) {
            if (PlayerPrefs.GetString("language") == "chinese") {
                signUpButtonText.text = "发送邮箱验证";
            } else
                signUpButtonText.text = "Send Verification";
        } else if (PlayerPrefs.GetString("language") == "chinese") {
            signUpButtonText.text = "注册账号";
        } else
            signUpButtonText.text = "Sign Up";
    }
    public void loginToAccount() {
        if (PlayerPrefs.GetInt("loggedIn") == 1)
            logoutOfAccount();
        else if (nameField.text != "" && passwordField.text != "") {
            StartCoroutine(login());
        }
    }
    public void signUpToAccount() {
        if (!emailField.GetComponent<Image>().enabled && PlayerPrefs.GetInt("loggedIn") == 0) {
            accountBackground.Translate(Vector3.up * 62 * GetComponent<UIAlignment>().scale);
            emailField.GetComponent<Image>().enabled = true;
            emailField.enabled = true;
            if (PlayerPrefs.GetString("language") == "chinese") {
                signUpButtonText.text = "发送邮箱验证";
            } else
                signUpButtonText.text = "Send Verification";
            emailText.enabled = true;
            emailPlaceholder.enabled = true;
            emailReminder.enabled = true;
        } else if (PlayerPrefs.GetInt("loggedIn") == 0 && nameField.text != "" && passwordField.text != "" && emailField.text != "") {
            accountBackground.Translate(Vector3.down * 62 * GetComponent<UIAlignment>().scale);
            StartCoroutine(signUp());
            emailField.GetComponent<Image>().enabled = false;
            emailField.enabled = false;
            if (PlayerPrefs.GetString("language") == "chinese") {
                signUpButtonText.text = "注册账号";
            } else
                signUpButtonText.text = "Sign Up";
            emailText.enabled = false;
            emailPlaceholder.enabled = false;
            emailReminder.enabled = false;
        }
    }
    IEnumerator getAccountInfo() {
        WWW variables = new WWW(controllerURL + "getAccountInformation?name=" + PlayerPrefs.GetString("accountName") + "&password=" + PlayerPrefs.GetString("accountPassword"));
        yield return variables;

        //add more attributes (perks, achievements) here
        PlayerPrefs.SetInt("cash", (int)VarFinder.findFloatInString(variables.text, 0));
        PlayerPrefs.SetInt("xp", (int)VarFinder.findFloatInString(variables.text, 1));
    }
    IEnumerator updateAccountInfo(bool reset) {
        WWW variables = new WWW(controllerURL + "updateAccountInformation?name=" + PlayerPrefs.GetString("accountName") + "&password=" + PlayerPrefs.GetString("accountPassword") + "&cash=" + PlayerPrefs.GetInt("cash").ToString() + "&experience=" + PlayerPrefs.GetInt("xp").ToString());
        yield return variables;
        if (reset) {
            resetStats();
        }
    }
    void resetStats() {
        //add more attributes (perks, achievements) here
        PlayerPrefs.SetInt("cash", PlayerPrefs.GetInt("guestCash"));
        PlayerPrefs.SetInt("xp", PlayerPrefs.GetInt("guestXp"));
    }

    public void logoutOfAccount() {
        //add more attributes (perks, achievements) here
        PlayerPrefs.SetInt("cash", PlayerPrefs.GetInt("guestCash"));
        PlayerPrefs.SetInt("xp", PlayerPrefs.GetInt("guestXp"));
        PlayerPrefs.SetInt("loggedIn", 0);
        if (PlayerPrefs.GetString("language") == "chinese") {
            transform.GetChild(0).GetComponent<Text>().text = "登陆账号";
        } else
            transform.GetChild(0).GetComponent<Text>().text = "Login";
    }
    void loggedIn() {
        if (PlayerPrefs.GetString("language") == "chinese") {
            transform.GetChild(0).GetComponent<Text>().text = "退出登录";
        } else
            transform.GetChild(0).GetComponent<Text>().text = "Logout";
    }
    IEnumerator login() {
        WWW variables = new WWW(controllerURL + "loginAccount?name=" + nameField.text + "&password=" + passwordField.text);
        yield return variables;

        if (variables.text == "true") {
            PlayerPrefs.SetInt("loggedIn", 1);
            PlayerPrefs.SetString("accountName", nameField.text);
            PlayerPrefs.SetString("accountPassword", passwordField.text);
            StartCoroutine(getAccountInfo());
        }
    }
    IEnumerator signUp() {
        WWW variables = new WWW(controllerURL + "createAccount?name=" + nameField.text + "&password=" + passwordField.text + "&email=" + emailField.text);
        yield return variables;
    }
}
