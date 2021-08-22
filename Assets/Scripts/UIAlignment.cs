using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIAlignment : MonoBehaviour {
	//1 is default text
	[HideInInspector]
	public float scale; 
	public bool dontChangeRect;
    public bool onlyYAxis;
    public bool onlyXAxis; 
	void Awake () {
		//align scale and anchored position to fit device measurements
		scale = Screen.width / 1500f;

		if (GetComponent<Text> () != null) {
			GetComponent<Text> ().fontSize = (int)(GetComponent<Text> ().fontSize * scale);
		}
        if (!dontChangeRect) {
            if (onlyYAxis) {
                GetComponent<RectTransform> ().anchoredPosition = new Vector2 (GetComponent<RectTransform> ().anchoredPosition.x, GetComponent<RectTransform> ().anchoredPosition.y * scale);
                GetComponent<RectTransform> ().sizeDelta = new Vector2 (GetComponent<RectTransform> ().rect.width, GetComponent<RectTransform> ().rect.height * scale);
            } else if (onlyXAxis) {
                GetComponent<RectTransform> ().anchoredPosition = new Vector2 (GetComponent<RectTransform> ().anchoredPosition.x * scale, GetComponent<RectTransform> ().anchoredPosition.y);
                GetComponent<RectTransform> ().sizeDelta = new Vector2 (GetComponent<RectTransform> ().rect.width * scale, GetComponent<RectTransform> ().rect.height);
            } else {
                GetComponent<RectTransform> ().anchoredPosition = new Vector2 (GetComponent<RectTransform> ().anchoredPosition.x * scale, GetComponent<RectTransform> ().anchoredPosition.y * scale);
                GetComponent<RectTransform> ().sizeDelta = new Vector2 (GetComponent<RectTransform> ().rect.width * scale, GetComponent<RectTransform> ().rect.height * scale);
            }
        }
	}
}


