using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour {

	public GameObject stream;
	public GameObject explosion;
	[HideInInspector]
	public int ownerID = -1;
	private int speed = 50;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate()
	{
		GameObject.Destroy((GameObject)GameObject.Instantiate(stream, this.transform.position, this.transform.rotation), 3f);
	}

	void OnCollisionEnter(Collision c)
	{
		if(c.gameObject.GetComponent<GravityFPSWalker>() != null)
		{

		}
		else
		{
			if(c.gameObject.GetComponent<DestructablePiece>() != null)
			{
				Destructable d = c.gameObject.transform.root.GetComponent<Destructable>();
				d.Activate(10000, this.transform.position, 100);
			}
			GameObject.Destroy(this.gameObject);
			GameObject.Destroy((GameObject)GameObject.Instantiate(explosion, this.transform.position, this.transform.rotation), 5f);
		}
	}

	public void Init(Vector3 velocity, int playerID)
	{
		ownerID = playerID;
		GetComponent<Rigidbody> ().velocity = velocity * speed;
	}
}
