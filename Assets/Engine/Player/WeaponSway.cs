using UnityEngine;
using System.Collections;

public class WeaponSway : MonoBehaviour {

	public float MoveAmount = 1;
	public float MoveSpeed = 2;
	public float LateralAmount = .5f;
	public GameObject GUN;
	[HideInInspector]
	public float MoveOnX;
	[HideInInspector]
	public float MoveOnY;
	[HideInInspector]
	public Vector3 DefaultPos;
	[HideInInspector]
	public Vector3 NewGunPos;
	[HideInInspector]
	public Quaternion DefaultRot;
	[HideInInspector]
	public Vector3 NewGunRot;

	private float eulerScale = 10f;
	GravityFPSWalker gfp;

	void Start()
	{	
		gfp = GameObject.FindObjectOfType<GravityFPSWalker> ();
		DefaultPos = this.transform.localPosition;    	
		DefaultRot = this.transform.localRotation;
	}            
	void Update () 
	{
		MoveOnX = Input.GetAxis("Mouse X") * Time.deltaTime * MoveAmount;	
		MoveOnY = Input.GetAxis("Mouse Y") * Time.deltaTime * MoveAmount;
		NewGunPos = new Vector3 (DefaultPos.x+MoveOnX, DefaultPos.y+MoveOnY, DefaultPos.z);
		//NewGunRot = new Vector3 (DefaultRot.x + (MoveOnX * eulerScale), DefaultRot.y + (MoveOnY * eulerScale), DefaultRot.z);
		GUN.transform.Rotate (new Vector3 (0, -1, 0) * Input.GetAxis("Horizontal") * LateralAmount);
		GUN.transform.Rotate (new Vector3 (-1, 0, 0) * Input.GetAxis("Vertical") * LateralAmount);
		GUN.transform.localRotation = Quaternion.Lerp (GUN.transform.localRotation, DefaultRot, .15f);
		GUN.transform.localPosition = Vector3.Lerp(GUN.transform.localPosition, 
		                                           NewGunPos + gfp.animationIdlePosition + gfp.animationWalkingPosition + gfp.animationSprintingPosition, 
		                                           MoveSpeed*Time.deltaTime);
		//GUN.transform.localEulerAngles = Vector3.Lerp (GUN.transform.localEulerAngles, NewGunRot, MoveSpeed * Time.deltaTime);
	}

	public void PlayClipIn()
	{
		transform.parent.GetComponent<WeaponShoot> ().PlayClipIn ();
	}

	public void PlayClipOut()
	{
		transform.parent.GetComponent<WeaponShoot> ().PlayClipOut ();
	}

	public void StopReloading()
	{
		transform.parent.GetComponent<WeaponShoot> ().EndReloading ();
	}

	public void SetProjectileActive()
	{
		if(transform.parent.GetComponent<WeaponShoot>().weapon.FiresRigidbody)
		{
			transform.parent.GetComponent<WeaponShoot> ().weapon.MuzzleTransform.gameObject.active = true;
			transform.parent.GetComponent<WeaponShoot> ().weapon.MuzzleTransform.gameObject.GetComponent<MeshRenderer> ().enabled = true;
		}
		else Debug.LogError("The current weapon does not shoot rigidbodies, so the rigidbody part of the gun cannot be turned back on.");
	}
}
