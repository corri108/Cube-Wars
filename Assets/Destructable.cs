using UnityEngine;
using System.Collections;

public class Destructable : MonoBehaviour {

	Rigidbody[] pieces;
	// Use this for initialization
	void Start () 
	{
		pieces = transform.GetComponentsInChildren<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void FixedUpdate()
	{

	}

	public void Activate(float explosionForce, Vector3 centerExplosion, float explosionRadius)
	{
		foreach(var p in pieces)
		{
			p.isKinematic = false;
			p.useGravity = true;
			p.AddExplosionForce(explosionForce, centerExplosion, explosionRadius);
			GameObject.Destroy(p.gameObject, 25f);
		}

		GameObject.Destroy (this.gameObject, 25f);
	}
}
