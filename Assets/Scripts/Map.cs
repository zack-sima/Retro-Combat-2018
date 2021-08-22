using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {
	public float size;
	public string name;
	public Transform[] allySpawnPoints;
	public Transform[] enemySpawnPoints;
    public GameObject[] flagpoles;
    //initial points on both sides for point capture; modify for difficulty
    public float startingVictoryPoints; 
}
