using UnityEngine;
using System.Collections;

public class Vehicle : MonoBehaviour {

	public Transform mount;
	public AudioClip bump;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log (GetComponent<Rigidbody> ().velocity.magnitude);
	}

	public void Deactivate()
	{
		//GetComponent<Engine> ().enabled = false;
		//GetComponent<Steering> ().enabled = false;
	}

	public void Activate()
	{
		//GetComponent<Engine> ().enabled = true;
		//GetComponent<Steering> ().enabled = true;
	}
}
