using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour {
    private void Start () {
         
    }
    void Update () {
        transform.Rotate (0, Time.deltaTime * 16, 0);
	}
}
