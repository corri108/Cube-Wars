using UnityEngine;
using System.Collections;

public class ObjectHealth : MonoBehaviour {

	private int _health = 100;
	private string partName;
	private Vector3 contactPoint;
	public bool alive = true;
	public AudioClip death;

	public int Health
	{
		get {return _health;}
		set 
		{
			_health = value;
			if(_health <= 0)
			{
				GlobalLoader gLo = GameObject.FindObjectOfType<GlobalLoader>();
				GameObject ragdoll = (GameObject)GameObject.Instantiate(gLo.marineRagdoll, this.transform.position, this.transform.rotation);
				ragdoll.transform.localScale = this.transform.localScale;
				Rigidbody rb = ragdoll.transform.FindDeepChild(partName).GetComponent<Rigidbody>();

				if(rb != null)
				{
					if(partName.Contains("Head") || partName.Contains("Arm"))
					{
						rb.AddExplosionForce(7500, contactPoint, 1000);
					}
					else
					{
						rb.AddExplosionForce(4000, contactPoint, 1000);
					}
				}

				GameObject.Destroy(this.gameObject);
				AudioSource.PlayClipAtPoint(death, this.transform.position);
				GameObject.Destroy(ragdoll, 15f);
			}
		}
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider c)
	{
		if(c.gameObject.GetComponent<VehicleColliders>() != null)
		{
			Vehicle v = c.gameObject.transform.root.GetComponent<Vehicle>() ;

			if(v.GetComponent<Rigidbody>().velocity.magnitude > 4.5f)
			{
				this.Health -= 100;
				this.contactPoint = this.transform.position;
				partName = "b_Hips";

				AudioSource.PlayClipAtPoint(v.bump, this.transform.position);
				
				GlobalLoader GLo = GameObject.FindObjectOfType<GlobalLoader> ();
				GameObject correctPS = GLo.bloodParticleSystem;
				GameObject ps = (GameObject)Instantiate(correctPS, contactPoint, this.transform.rotation);
			}
		}
	}

	public void MoveOnly (string nameOfPartHit, Vector3 contactPoint)
	{
		this.contactPoint = contactPoint;
		partName = nameOfPartHit;

		Rigidbody rb = this.transform.FindDeepChild(partName).GetComponent<Rigidbody>();

		if(rb != null)
		{
			if(partName.Contains("Head") || partName.Contains("Arm"))
			{
				rb.AddExplosionForce(7500, contactPoint, 1000);
			}
			else
			{
				rb.AddExplosionForce(1000, contactPoint, 1000);
			}
		}
	}

	public void TakeDamage (int d, string nameOfPartHit, Vector3 contactPoint)
	{
		Health -= d;
		this.contactPoint = contactPoint;
		partName = nameOfPartHit;
		Debug.Log ("OBject has : " + Health.ToString ());
	}
}
