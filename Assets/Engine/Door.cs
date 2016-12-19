
using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

	Transform r;
	Transform l;
	Vector3 curR;
	Vector3 curL;
	Vector3 desR;
	Vector3 desL;
	bool inRange = false;

	public AudioClip close;
	public AudioClip open;
	public int axis = 0;
	// Use this for initialization
	void Start () {
		r = transform.FindChild ("R");
		l = transform.FindChild ("L");
		curR = r.transform.position;
		curL = l.transform.position;
		Vector3 dir = axis == 0 ? transform.forward : transform.right;
		desR = curR + dir * 2;
		desL = curL + dir * -2;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if(inRange)
		{
			r.transform.position = Vector3.Lerp(r.transform.position, desR, .1f);
			l.transform.position = Vector3.Lerp(l.transform.position, desL, .1f);
		}
		else
		{
			r.transform.position = Vector3.Lerp(r.transform.position, curR, .1f);
			l.transform.position = Vector3.Lerp(l.transform.position, curL, .1f);
		}
	}

	void OnTriggerEnter(Collider c)
	{
		if(c.gameObject.GetComponent<GravityFPSWalker>() != null)
		{
			inRange = true;
			AudioSource.PlayClipAtPoint(open, this.transform.position);
		}
	}

	void OnTriggerExit(Collider c)
	{
		if(c.gameObject.GetComponent<GravityFPSWalker>() != null)
		{
			inRange = false;
			AudioSource.PlayClipAtPoint(close, this.transform.position);
		}
	}
}
