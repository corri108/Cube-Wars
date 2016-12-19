using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Weapon : MonoBehaviour {

	//weapon zooming props
	public GameObject weaponADS;

	//individual weapon properties
	public int ClipSize = 8;
	public bool Automatic = false;
	public float FireRate = .1f;
	public float RecoilTranslation = .1f;
	public float RecoilRotation = .5f;
	public AudioClip FireNoise;
	public AudioClip ClipIn;
	public AudioClip ClipOut;
	public Transform MuzzleTransform;
	public GameObject MuzzlePrefab;
	public GameObject BulletPrefab;
	public int Damage = 25;
	public float FOVzoom = 10;
	public bool FiresRigidbody = false;
	public bool GUIZoom = false;
	public Image GUIZoomImage;

	//displaying clip info on GUI
	private int _currentClip;
	private int _ammoStockpile;
	[HideInInspector]
	public int maxAmmo;
	private Text ammoStockpileText;
	private Text currentClipText;	
	[HideInInspector]
	public float FireRateReset;
	
	public int ammoStockpile
	{
		get { return _ammoStockpile;}
		set
		{ 
			_ammoStockpile = value;

			if(ammoStockpileText == null)
				ammoStockpileText = GameObject.Find ("Canvas").transform.FindChild ("AmmoStockpile").GetComponent<Text>();

			ammoStockpileText.text = _ammoStockpile.ToString();
		}
	}
	
	public int currentClip
	{
		get { return _currentClip;}
		set
		{ 
			_currentClip = value;

			if(currentClipText == null)
				currentClipText = GameObject.Find ("Canvas").transform.FindChild ("AmmoClip").GetComponent<Text>();

			currentClipText.text = _currentClip.ToString();
		}
	}
	private AudioSource myAudioSource;
	// Use this for initialization
	void Start () 
	{
		ammoStockpileText = GameObject.Find ("Canvas").transform.FindChild ("AmmoStockpile").GetComponent<Text>();
		currentClipText = GameObject.Find ("Canvas").transform.FindChild ("AmmoClip").GetComponent<Text>();
		FireRateReset = FireRate;
		FireRate = 0;

		//for ammo
		currentClip = ClipSize;
		maxAmmo = ClipSize * 6;
		ammoStockpile = ClipSize * 3;
		FOVzoom = FOVzoom;
		myAudioSource = GetComponent<AudioSource> ();
	}

	public void SetGUI()
	{
		currentClip = currentClip;
		ammoStockpile = ammoStockpile;
	}

	public void PlayFireNoise()
	{
		myAudioSource.clip = FireNoise;
		myAudioSource.PlayOneShot (myAudioSource.clip);
	}

	public void SetVisible (bool visible)
	{
		MeshRenderer[] rends = this.gameObject.GetComponentsInChildren<MeshRenderer> ();

		foreach(var mr in rends)
		{
			mr.enabled = visible;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
