using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour {

	public float destroyTimer;

	void Update () {
		destroyTimer -= Time.deltaTime;
		if (destroyTimer <= 0)
			Destroy (gameObject);
	}
}
