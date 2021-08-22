using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
	public string name;
	public GameObject gunClip;
	public GameObject attachedSoldier;
	public float damage;
	public float bulletsPerFire;
	public float shootCooldown;
	public float shootingDistance;
	public float maxShootingRange;
	public float recoil;
	public float sideRecoil;
    [Range(0f, 59f)]
	public float zoom;
    public float aimSpeed; 
	public float camUpAim;
	public float length;
    public float muzzleLength; 
	public float aiMistakeRotation;
	//player shoot spread without aiming 
	public float mistakeRotation;
	public float clipSize;
	public float initialBullets;
	public float reloadCooldown;
	public float currentGunClipPosX;
	public float currentGunClipPosY;
	public float currentGunClipPosZ;
	public bool hasScope; 
	public float gunReloadUp;
    public bool semiAuto; 
}