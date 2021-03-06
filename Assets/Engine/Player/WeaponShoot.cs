﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WeaponShoot : MonoBehaviour {

	public GameObject weaponStart;
	public GameObject weaponSprint;
	public Weapon weapon;
	Vector3 initPos;

	bool isFiring = false;
	private AudioSource myAudioSource;
	//because we cant have two noises playing at once
	private AudioSource myCameraAudioSource;
	public Image reticle;
	GravityFPSWalker gfps;

	bool zooming = false;
    bool wasZooming = false;
	public Camera gunCam;
	public Camera worldCam;
	private float FOVinit;
	private float FOVsprint;
	private Animator animator;

	public AudioClip[] reloadingPhrases;
	public AudioClip[] outOfAmmoPhrases;
	int outOfAmmoTimer = 60 * 6;
	int outOfAmmoReset = 60 * 6;
	bool outOfAmmoPlayed = false;
    
	public List<Weapon> weaponList;
    public uint maxPrimaryWeapons = 4;
	private int weaponIndex = 0;
	public Transform handHoldingWeapons;

    [HideInInspector]
    public bool reloading = false;
    [HideInInspector]
    public bool bolting = false;

    bool boltActionFinish = false;
    //returns true if we have just finished bolting, and we want to continue zooming in

    public bool sprinting
	{
		get { return gfps.sprinting;}
	}

	private Text interaction;
	// Use this for initialization
	void Start () 
	{
		gfps = this.transform.root.GetComponent<GravityFPSWalker> ();
		FOVinit = worldCam.fieldOfView;
		FOVsprint = FOVinit + 5;
		animator = transform.GetChild (0).GetComponent<Animator> ();

		interaction = GameObject.Find ("Canvas").transform.FindChild ("Interaction").GetComponent<Text> ();
		myAudioSource = GetComponent<AudioSource> ();
		myCameraAudioSource = transform.parent.GetComponent<AudioSource> ();

        //Hide all weapons that are not the starting one.
        if(weaponList == null)
        {
            weaponList = new List<Weapon>();
        }
        foreach (var w in weaponList)
        {
            if (w != null)
            {
                w.gameObject.SetActive(false);
            }
        }
        weaponIndex = 0;
        //weaponList[weaponIndex].gameObject.SetActive(true);
		ChangeWeapon ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKey(KeyCode.R) && !animator.GetBool("Reloading") && !animator.GetBool("BoltAction"))
		{
			if(weapon.ammoStockpile > 0 && weapon.currentClip != weapon.ClipSize)
				StartReloading();
		}
		if(Input.GetMouseButtonDown(1) || (boltActionFinish && wasZooming && !zooming))
		{
            //if youre not currently in the bolt action or reload animation
            if (!animator.GetBool("Reloading") && !animator.GetBool("BoltAction"))
            {
                zooming = !zooming;
                reticle.gameObject.active = !zooming;
                gfps.SetZooming(zooming);

                //for snipers / texture-based zoomed in weapons
                if (!zooming && weapon.GUIZoom)
                {
                    TurnSniperGUI(false);
                }
                else boltActionFinish = false;
            }
		}

		CheckChangeWeapons ();

        if(weapon != null)
        {
            if (weapon.Automatic)
            {
                if (Input.GetMouseButton(0))
                {
                    isFiring = true;
                    weapon.FireRate -= Time.deltaTime;

                    if (weapon.FireRate < 0)
                    {
                        if (weapon.currentClip > 0 && !reloading)
                            FireBullet();
                        else if (weapon.ammoStockpile > 0)
                            StartReloading();
                        else if (!outOfAmmoPlayed)
                        {
                            outOfAmmoPlayed = true;
                            myCameraAudioSource.clip = outOfAmmoPhrases[Random.Range(0, outOfAmmoPhrases.Length)];
                            myCameraAudioSource.Play();
                        }
                    }
                }
                else
                {
                    isFiring = false;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (weapon.currentClip > 0 && !reloading && !bolting)
                        FireBullet();
                    else if (weapon.ammoStockpile > 0 && !bolting)
                        StartReloading();
                    else if (!outOfAmmoPlayed && !bolting)
                    {
                        outOfAmmoPlayed = true;
                        myCameraAudioSource.clip = outOfAmmoPhrases[Random.Range(0, outOfAmmoPhrases.Length)];
                        myCameraAudioSource.Play();
                    }
                }
                else
                {
                    isFiring = false;
                }
            }
        }
		if(Input.GetMouseButtonUp(0))
		{
			weapon.FireRate = 0;
		}
		if(sprinting)
		{
			this.transform.position = Vector3.Lerp(this.transform.position, weaponSprint.transform.position, .1f);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, weaponSprint.transform.rotation, .1f);
			
			gunCam.fieldOfView = Mathf.Lerp(gunCam.fieldOfView, FOVsprint, .1f);
			worldCam.fieldOfView = Mathf.Lerp(worldCam.fieldOfView, FOVsprint, .1f);
		}
		else if(zooming)
		{
			this.transform.position = Vector3.Lerp(this.transform.position, weapon.weaponADS.transform.position, .1f);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, weapon.weaponADS.transform.rotation, .1f);

			gunCam.fieldOfView = Mathf.Lerp(gunCam.fieldOfView, weapon.FOVzoom, .1f);
			worldCam.fieldOfView = Mathf.Lerp(worldCam.fieldOfView, weapon.FOVzoom, .1f);

			//for snipers
			if(weapon.GUIZoom && gunCam.fieldOfView <= weapon.FOVzoom + 2.5f)
			{
                TurnSniperGUI(true);
			}
		}
		else
		{
			this.transform.position = Vector3.Lerp(this.transform.position, weaponStart.transform.position, .12f);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, weaponStart.transform.rotation, .1f);

			gunCam.fieldOfView = Mathf.Lerp(gunCam.fieldOfView, FOVinit, .1f);
			worldCam.fieldOfView = Mathf.Lerp(worldCam.fieldOfView, FOVinit, .1f);
		}

		Pick ();
	}

	public void EndReloading()
	{
		reloading = false;
		animator.SetBool ("Reloading", false);

		int amtRefill = weapon.ClipSize - weapon.currentClip;

		if (weapon.ammoStockpile >= amtRefill)
		{
			weapon.currentClip += amtRefill;
			weapon.ammoStockpile -= amtRefill;
		}
		else
		{
			weapon.currentClip = weapon.ammoStockpile;
			weapon.ammoStockpile = 0;
		}

        if(weapon.BoltAction)
        {
            StartBoltAction();
        }
        else
        {
            //if we were zooming in, lets zoom back in
            if(wasZooming && !zooming)
            {
                zooming = true;
                reticle.gameObject.active = !zooming;
                gfps.SetZooming(zooming);

                if (weapon.GUIZoom)
                    TurnSniperGUI(true);
            }
        }
	}

    void AddWeapon(Weapon weapon)
    {
        Debug.Log("added weapon");
    }
	
	void Pick()
	{
		Ray ray = new Ray (this.transform.parent.position, this.transform.parent.forward);
		RaycastHit rh;
		bool isHit = Physics.Raycast (ray, out rh, 5.5f);

		interaction.text = "";
		if(isHit)
		{
			Pickable ri = rh.collider.gameObject.GetComponent<Pickable>();
			if(ri != null)
			{
                if (ri.GetComponent<AmmoBox>() != null)
                {
                    ri.GetComponent<AmmoBox>().SetHowMuch(weapon.maxAmmo - weapon.ammoStockpile, this);
                }
                else if (ri.GetComponent<Weapon>() != null)
                {
                    if (ri.GetComponent<Weapon>() == this.weapon)
                    {
                        return;
                    }
                    ri.GetComponent<Weapon>().SetWeaponShootOwner(this);
                }

                ri.OnHover.Invoke();
				interaction.text = ri.pickText;

				if(Input.GetKeyDown(KeyCode.E))
				{
					ri.OnPick.Invoke();
				}
			}
		}
	}

    public void StartBoltAction()
    {
        if(!bolting)
        {
            animator.SetBool("BoltAction", true);
            bolting = true;
            wasZooming = zooming;
            if(wasZooming)
            {
                zooming = false;

                if (weapon.GUIZoom)
                    TurnSniperGUI(false);
            }
        }
    }

	public void StartReloading()
	{
		if(!reloading)
		{
			reloading = true;
			animator.SetBool ("Reloading", true);
			myCameraAudioSource.clip = reloadingPhrases [Random.Range (0, reloadingPhrases.Length)];
			myCameraAudioSource.Play ();
            wasZooming = zooming;
            if (wasZooming)
            {
                zooming = false;

                if(weapon.GUIZoom)
                    TurnSniperGUI(false);
            }
        }
	}

	public void PlayClipOut()
	{
		myAudioSource.clip = weapon.ClipOut;
		myAudioSource.Play ();
	}

	public void PlayClipIn()
	{
		myAudioSource.clip = weapon.ClipIn;
		myAudioSource.Play ();
	}

    public void PlayBoltAction()
    {
        if (weapon.BoltAction)
        {
            myAudioSource.clip = weapon.BoltNoise;
            myAudioSource.Play();
        }
    }

    public void EndBoltAction()
    {
        if (weapon.BoltAction)
        {
            bolting = false;
            animator.SetBool("BoltAction", false);
            boltActionFinish = true;
        }
    }

    void TurnSniperGUI(bool on)
    {
        weapon.GUIZoomImage.gameObject.active = on;
        weapon.SetVisible(!on);
        this.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = !on;
    }

    public void EquipWeapon(Weapon weapon, Vector3 translateOffset, Vector3 rotOffset, Vector3 scaleOffset)
    {
        weaponList.Add(weapon);
        weapon.transform.SetParent(handHoldingWeapons.transform, true);
        weapon.transform.localPosition = translateOffset;
        weapon.transform.localRotation = Quaternion.Euler(rotOffset.x, rotOffset.y, rotOffset.z);
        weapon.transform.localScale = scaleOffset;
        weapon.gameObject.SetActive(false);
    }

    void CheckChangeWeapons()
	{
		float d = Input.GetAxis("Mouse ScrollWheel");
		if (d > 0f)
		{
			// scroll up
			if(weaponIndex == weaponList.Count - 1)
				weaponIndex = 0;
			else weaponIndex++;
		}
		else if (d < 0f)
		{
			// scroll down
			if(weaponIndex == 0)
				weaponIndex = (int)weaponList.Count - 1;
			else 	weaponIndex--;
		}

		ChangeWeapon ();
	}

	void ChangeWeapon ()
	{
        // don't allow weapon changes while reloading.
        if(reloading || bolting || weaponList.Count <= 0)
        {
            return;
        }

		//turn shit off of old one
        if(weapon != null)
        {
            this.weapon.gameObject.active = false;
            this.weapon.GetComponent<AudioSource>().enabled = false;
            this.weapon.enabled = false;
        }
		//change the weapons now
		this.weapon = weaponList [weaponIndex];
        if(weapon != null)
        {
            //tell animator to switch
            animator.SetInteger("WeaponIndex", (int)weapon.weaponType);
            //turn stuff on of new one
            this.weapon.gameObject.active = true;
            this.weapon.GetComponent<AudioSource>().enabled = true;
            this.weapon.enabled = true;
            //reset the GUI so its for this weapon now
            this.weapon.SetGUI();
        }
	}

	void FireBullet ()
	{
		GameObject muzF = (GameObject)GameObject.Instantiate (weapon.MuzzlePrefab, weapon.MuzzleTransform.position, weapon.MuzzleTransform.rotation);
		GameObject.Destroy (muzF, .2f);
		muzF.transform.SetParent (weapon.MuzzleTransform);
		weapon.FireRate = weapon.FireRateReset;
		this.transform.position -= this.transform.forward * weapon.RecoilTranslation;
		this.transform.Rotate (new Vector3 (-1, 0, 0) * weapon.RecoilRotation);
		Vector3 direction = this.transform.parent.transform.forward;
		direction.Normalize ();
		myCameraAudioSource.clip = weapon.FireNoise;
		myAudioSource.PlayOneShot (myCameraAudioSource.clip);
		weapon.currentClip--;
		//raycasting

		//only raycast if we arent firing a rigidbody
		if(!weapon.FiresRigidbody)
		{
			//make our aesthetic-only bullet 
			GameObject bul = (GameObject)GameObject.Instantiate (weapon.BulletPrefab, weapon.MuzzleTransform.position, weapon.MuzzleTransform.rotation);
			bul.GetComponent<Bullet> ().Set (weapon.MuzzleTransform.position, weapon.MuzzleTransform.position + direction, direction * 1.6f);
			//create the actual ray - fires from center of camera instead of gun like the bullet does
			Ray ray = new Ray (this.transform.parent.position, this.transform.parent.forward);
			RaycastHit rh;
			bool isHit = Physics.Raycast (ray, out rh, 100000);

			if(isHit)
			{
				RaycastInfo ri = rh.collider.gameObject.GetComponent<RaycastInfo>();
				BodyPart bp = rh.collider.gameObject.GetComponent<BodyPart>();
				if(ri != null)
				{
					Vector3 point = rh.point;
					Quaternion rot = Quaternion.LookRotation(rh.normal);
					GameObject ps = (GameObject)Instantiate(ri.onHitParticleSystem, point, rot);

					Vector3 newBulletDir = rh.point - weapon.MuzzleTransform.position;
					newBulletDir.Normalize();
					bul.GetComponent<Bullet> ().Set (weapon.MuzzleTransform.position, weapon.MuzzleTransform.position + newBulletDir, newBulletDir * 1.6f);

					DestructablePiece dp = rh.collider.gameObject.GetComponent<DestructablePiece>();
					//see if it is descturctable
					if(dp != null)
					{
						Destructable d = rh.collider.gameObject.transform.root.GetComponent<Destructable>();
						d.Activate(400, point, 80);
					}
				}
				else if(bp != null)
				{
					bp.DoDamage(rh.point, Quaternion.LookRotation(rh.normal), weapon.Damage, rh.collider.gameObject.name);

					Vector3 newBulletDir = rh.point - weapon.MuzzleTransform.position;
					newBulletDir.Normalize();
					bul.GetComponent<Bullet> ().Set (weapon.MuzzleTransform.position, weapon.MuzzleTransform.position + newBulletDir, newBulletDir * 1.6f);
				}
			}
		}
		else
		{
			//we are using a rocket or something.
			GameObject projectile = (GameObject)GameObject.Instantiate (weapon.BulletPrefab, weapon.MuzzleTransform.position, weapon.MuzzleTransform.rotation);
			projectile.GetComponent<Rocket> ().Init (direction, 1);

			//tell the "Rocket" part on the gun to become inactive, since we just shot it
			weapon.MuzzleTransform.gameObject.active = false;
			weapon.MuzzleTransform.gameObject.GetComponent<MeshRenderer>().enabled = false;

			//now we need to raycast to see if the user is pointing at a wall - in which case it should fire towards the wall
			Ray ray = new Ray (this.transform.parent.position, this.transform.parent.forward);
			RaycastHit rh;
			bool isHit = Physics.Raycast (ray, out rh, 100000);
			//if we have hit something, adjust the trajectory of the projectile
			if(isHit)
			{
				RaycastInfo ri = rh.collider.gameObject.GetComponent<RaycastInfo>();
				if(ri != null)
				{
					Vector3 newBulletDir = rh.point - weapon.MuzzleTransform.position;
					newBulletDir.Normalize();
					projectile.GetComponent<Rocket> ().Init (newBulletDir, 1);
				}
			}
		}

        //check if we should / can reload
        if (weapon.currentClip == 0 && weapon.ammoStockpile > 0)
            StartReloading();
        else if (weapon.currentClip > 0 && weapon.BoltAction)
            StartBoltAction();
	}

	void FixedUpdate()
	{
		if(outOfAmmoPlayed)
		{
			outOfAmmoTimer--;

			if(outOfAmmoTimer == 0)
			{
				outOfAmmoTimer = outOfAmmoReset;
				outOfAmmoPlayed = false;
			}
		}
	}
	
}