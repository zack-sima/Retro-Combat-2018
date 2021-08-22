using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnConnect : MonoBehaviour {
    Controller controller;
    void Start () {
        controller = GameObject.Find ("Controller").GetComponent<Controller> ();
    }
	void Update () {
        if (controller.id != 0)
            Destroy (gameObject);
	}
}
