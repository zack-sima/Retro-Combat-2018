using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Flagpole : MonoBehaviour {
    //-40 to 40, <0 is red & >0 is blue
    Controller controllerScript; 
    public float captureStatus; 
    public float maxCaptureStatus;
    public Material red;
    public Material green; 
    Vector3 originalFlagPos;
    float subtractPointsCooldown = 6f; 
    float subtractPointsTimer; 
	void Start () {
        controllerScript = GameObject.Find("Controller").GetComponent<Controller>(); 
        subtractPointsTimer = subtractPointsCooldown; 
        if (!controllerScript.isPointCapture)
            Destroy(transform.parent.gameObject); 
        originalFlagPos = transform.parent.GetChild(0).position;
	}
    void Update() {
        if (!controllerScript.paused) {
            if (subtractPointsTimer <= 0) {
                if (captureStatus > 0) {
                    if (controllerScript.axisPoints > 0 && controllerScript.alliedPoints > 0)
                        controllerScript.axisPoints -= 3;
                    else if (controllerScript.axisPoints < 0)
                        controllerScript.axisPoints = 0; 
                } else {
                    if (controllerScript.alliedPoints > 0 && controllerScript.axisPoints > 0)
                        controllerScript.alliedPoints -= 3;
                    else if (controllerScript.alliedPoints < 0)
                        controllerScript.alliedPoints = 0; 
                }
                subtractPointsTimer = subtractPointsCooldown;
            } else
                subtractPointsTimer -= Time.deltaTime;
            //add for ally, subtract for enemy as flag raise
            int pointsToAdd = 0;
            foreach (Collider coll in Physics.OverlapBox(transform.position, new Vector3(15, 40, 15))) {
                if (coll.GetComponent<SoldierController>() != null) {
                    if (coll.GetComponent<SoldierController>().isAlly)
                        pointsToAdd++;
                    else
                        pointsToAdd--;
                }
            }
            captureStatus += pointsToAdd * Time.deltaTime * 1.7f;
            if (pointsToAdd == 0) {
                if (captureStatus > 0)
                    captureStatus += Time.deltaTime * 1.7f;
                else if (captureStatus < 0)
                    captureStatus -= Time.deltaTime * 1.7f;
            }
            if (captureStatus > maxCaptureStatus)
                captureStatus = maxCaptureStatus;
            if (captureStatus < -maxCaptureStatus)
                captureStatus = -maxCaptureStatus;
            if (captureStatus > 0)
                transform.parent.GetChild(0).GetComponent<MeshRenderer>().material = green;
            else
                transform.parent.GetChild(0).GetComponent<MeshRenderer>().material = red;
            transform.parent.GetChild(0).position = new Vector3(originalFlagPos.x, originalFlagPos.y + Mathf.Abs(captureStatus * 0.8f) - maxCaptureStatus * 0.8f, originalFlagPos.z);
        }
    }
}
