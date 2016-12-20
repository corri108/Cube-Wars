using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Pickable))]
[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour {

	//weapon zooming props
	public GameObject weaponADS;

	//individual weapon properties
    public WeaponType weaponType;
    public string Name;
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
    public bool BoltAction = false;
    public AudioClip BoltNoise;

	//displaying clip info on GUI
	private int _currentClip;
	private int _ammoStockpile;
	[HideInInspector]
	public int maxAmmo;
	private Text ammoStockpileText;
	private Text currentClipText;	
	[HideInInspector]
	public float FireRateReset;
    private Pickable thisP;
    [HideInInspector]
    public WeaponShoot whichWeapon;
    [SerializeField]
    private Vector3 translateOffset;
    [SerializeField]
    private Vector3 rotationOffset;
    [SerializeField]
    private Vector3 scaleOffset;

    // Corrseponds to weapon index and is used to determine what animations and other things are tied to this weapon.
    public enum WeaponType
    {
        Sniper,
        Assault,
        RocketLauncher,
        MachineGun
    }
	
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

        thisP = GetComponent<Pickable>();
    }

    public void OnHover()
    {
        thisP.pickText = "Press E to pickup " + this.Name;
    }

    public void OnPickUp()
    {
        List<Weapon> weaponList = whichWeapon.weaponList;
        if (weaponList.Count < whichWeapon.maxPrimaryWeapons)
        {
            whichWeapon.EquipWeapon(this, translateOffset, rotationOffset, scaleOffset);
        }
        GetComponent<Pickable>().enabled = false;
    }

    public void SetWeaponShootOwner(WeaponShoot newWeaponShoot)
    {
        whichWeapon = newWeaponShoot;
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
