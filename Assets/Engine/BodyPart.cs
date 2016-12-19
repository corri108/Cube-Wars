using UnityEngine;
using System.Collections;

public class BodyPart : MonoBehaviour {

	[HideInInspector]
	public float DamageMultiplyer = 1f;

	public BodyType bt = BodyType.Soldier;
	public BodyPartType bpt = BodyPartType.Chest;
	// Use this for initialization
	void Start () 
	{
		switch(bpt)
		{
		case BodyPartType.Arm:
			DamageMultiplyer = .8f;
			break;
		case BodyPartType.Leg:
			DamageMultiplyer = 1f;
			break;
		case BodyPartType.Chest:
			DamageMultiplyer = 1.5f;
			break;
		case BodyPartType.Torso:
			DamageMultiplyer = 1.25f;
			break;
		case BodyPartType.Foot:
			DamageMultiplyer = .5f;
			break;
		case BodyPartType.Hand:
			DamageMultiplyer = .5f;
			break;
		case BodyPartType.Head:
			DamageMultiplyer = 2f;
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DoDamage(Vector3 point, Quaternion rot, float damage, string bodyPartName)
	{
		GameObject correctPS = null;
		GlobalLoader GLo = GameObject.FindObjectOfType<GlobalLoader> ();

		switch(bt)
		{
		case BodyType.Soldier:
			correctPS = GLo.bloodParticleSystem;
			break;
		}

		GameObject ps = (GameObject)Instantiate(correctPS, point, rot);
		float intDamage = damage * DamageMultiplyer;

		if(this.transform.root.GetComponent<ObjectHealth>() != null)
		{
			if(this.transform.root.GetComponent<ObjectHealth>().alive)
			{
				this.transform.root.GetComponent<ObjectHealth> ().TakeDamage ((int)intDamage, bodyPartName, point);
			}
			else
			{
				this.transform.root.GetComponent<ObjectHealth> ().MoveOnly (bodyPartName, point);
			}
		}
	}
}

public enum BodyType
{
	Soldier,
	Robot
}

public enum BodyPartType
{
	Head,
	Chest,
	Arm,
	Hand,
	Torso,
	Leg,
	Foot
}