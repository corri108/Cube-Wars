using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (CharacterController))]
public class GravityFPSWalker: MonoBehaviour {
	
	public float walkSpeed = 6.0f;
	
	public float runSpeed = 11.0f;
	
	// If true, diagonal speed (when strafing + moving forward or back) can't exceed normal move speed; otherwise it's about 1.4 times faster
	public bool limitDiagonalSpeed = true;
	
	// If checked, the run key toggles between running and walking. Otherwise player runs if the key is held down and walks otherwise
	// There must be a button set up in the Input Manager called "Run"
	public bool toggleRun = false;
	
	public float jumpSpeed = 8.0f;
	public float gravity = 20.0f;
	
	// Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector
	public float fallingDamageThreshold = 10.0f;
	
	// If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down
	public bool slideWhenOverSlopeLimit = false;
	
	// If checked and the player is on an object tagged "Slide", he will slide down it regardless of the slope limit
	public bool slideOnTaggedObjects = false;
	
	public float slideSpeed = 12.0f;
	
	// If checked, then the player can change direction while in the air
	public bool airControl = false;
	
	// Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast
	public float antiBumpFactor = .75f;
	
	// Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
	public int antiBunnyHopFactor = 1;

	public Camera camera;
	
	private Vector3 moveDirection = Vector3.zero;
	private bool grounded = false;
	private bool prevGrounded = false;
	private CharacterController controller;
	private Transform myTransform;
	private float speed;
	private RaycastHit hit;
	private float fallStartLevel;
	private bool falling;
	private float slideLimit;
	private float rayDistance;
	private Vector3 contactPoint;
	private bool playerControl = false;
	private int jumpTimer;
	private Vector3 eulerAngles;
	private Vector3 initEulerAngles;
	private Vector3 sprintEulerAngles;
	public GameObject weapon;
	public float tiltMax = 1f;
	public float tiltAmt = .1f;

	private int IdleTimer = 60 * 2;
	private int IdleTimerReset = 60 * 2;
	private float idleAmount = .001f;

	private float WalkTimer = 0;
	//private int WalkTimerReset = 60 * 2;
	private float walkAmount = .035f;
	private bool walking = false;
	private float B = .002f;
	private float period = 0;
	private float inc = .1f;

	private float SprintTimer = 0;
	//private int WalkTimerReset = 60 * 2;
	private float sprintAmount = .2f;
	[HideInInspector]
	public bool sprinting = false;
	private float sprintB = .0012f;
	private float sprintPeriod = 0;
	private float sprintInc = 0;
	private int sprintJuice = 6 * 60;
	private int sprintJuiceMax;
	private bool exhausted = false;
	private int exhaustTimer = 6 * 60;
	private int exhaustTimerReset;

	public AudioClip jumpNoise;
	public AudioClip landNoise;
	public AudioClip exhaustedNoise;
	public AudioClip[] footNoises;
	public AudioClip[] sprintNoises;

	private bool isZooming = false;
	private float zoomWalkAmount = .2f;
	private float zoomIdleAmount = .1f;
	private Image sprintUI;
	private AudioSource myAudioSource;
	//used for noises cause you cant play more than one at a time
	private AudioSource myCameraAudioSource;
	
	void Start() {
		controller = GetComponent<CharacterController>();
		myTransform = transform;
		speed = walkSpeed;
		rayDistance = controller.height * .5f + controller.radius;
		slideLimit = controller.slopeLimit - .1f;
		jumpTimer = antiBunnyHopFactor;
		//weapon = GetComponentInChildren<CurrentWeapon> ().GUN;
		initEulerAngles = weapon.transform.eulerAngles;
		sprintEulerAngles = initEulerAngles - this.transform.right * 50;
		period = Mathf.PI * 2 / B;
		inc = period * .04f;

		sprintPeriod = Mathf.PI * 2 / sprintB;
		sprintInc = sprintPeriod * .04f;
		sprintJuiceMax = sprintJuice;
		exhaustTimerReset = exhaustTimer;
		sprintUI = GameObject.Find ("Canvas").transform.FindChild ("Sprintbar").GetComponent<Image> ();
		myAudioSource = GetComponent<AudioSource> ();
		myCameraAudioSource = this.transform.GetChild (0).GetComponent<AudioSource> ();
	}

	void SprintLogic ()
	{
		if (!sprinting) 
		{
			eulerAngles = Vector3.Lerp (eulerAngles, initEulerAngles, .1f);

			if (!exhausted) 
			{
				sprintJuice++;
				if (sprintJuice > sprintJuiceMax)
					sprintJuice = sprintJuiceMax;
			}
		}
		else 
		{
			sprintJuice--;

			if(sprintJuice % 30 == 0)
			{
				myCameraAudioSource.clip = sprintNoises[Random.Range(0, sprintNoises.Length)];
				myCameraAudioSource.Play();
			}

			if (sprintJuice == 0) 
			{
				if(!exhausted)
				{
					//play exhausted noise
					myCameraAudioSource.clip = exhaustedNoise;
					myCameraAudioSource.Play();
				}
				exhausted = true;
				sprinting = false;
			}
			//eulerAngles = Vector3.Lerp (eulerAngles, sprintEulerAngles, .1f);
		}
		if (exhausted) 
		{
			exhaustTimer--;

			if (exhaustTimer == 0) 
			{
				exhausted = false;
				exhaustTimer = exhaustTimerReset;
			}
		}

		float rat = (float)sprintJuice / (float)sprintJuiceMax;
		sprintUI.fillAmount = rat;
	}
	
	void FixedUpdate() 
	{
		if (currentVehicle != null)
			return;

		prevGrounded = grounded;

		SprintLogic ();

		float inputX = Input.GetAxis("Horizontal");
		float inputY = Input.GetAxis("Vertical");

		if(inputX == 0 && inputY == 0)
		{
			walking = false;
		}
		else walking = true;

		eulerAngles += Vector3.right * inputY * tiltAmt;
		eulerAngles += Vector3.up * inputX * tiltAmt;
		//eulerAngles += Vector3.right * inputX * tiltAmt;

		float euX = (Mathf.Clamp(eulerAngles.x ,-tiltMax, tiltMax));
		float euY = (Mathf.Clamp(eulerAngles.y ,-tiltMax, tiltMax));
		float euZ = (Mathf.Clamp(eulerAngles.z ,-tiltMax, tiltMax));

		eulerAngles = new Vector3 (euX, euY, euZ);
		//weapon.transform.eulerAngles = eulerAngles;
		// If both horizontal and vertical are used simultaneously, limit speed (if allowed), so the total doesn't exceed normal move speed
		float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed)? .7071f : 1.0f;
		
		if (grounded) 
		{
			bool sliding = false;
			// See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
			// because that interferes with step climbing amongst other annoyances
			if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance)) 
			{
				if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
					sliding = true;
			}
			// However, just raycasting straight down from the center can fail when on steep slopes
			// So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
			else 
			{
				Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out hit);

				if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
					sliding = true;
			}
			
			// If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
			if (falling) 
			{
				falling = false;

				if (myTransform.position.y < fallStartLevel - fallingDamageThreshold)
					FallingDamageAlert (fallStartLevel - myTransform.position.y);
			}
			
			// If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down
			if (!toggleRun)
			{
				speed = Input.GetButton("Run")? runSpeed : walkSpeed;
				sprinting = speed == runSpeed;

				if(exhausted)
				{
					sprinting = false;
					speed = walkSpeed;
				}
			}
			
			// If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
			if ((sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && hit.collider.tag == "Slide")) 
			{
				Vector3 hitNormal = hit.normal;
				moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
				Vector3.OrthoNormalize (ref hitNormal, ref moveDirection);
				moveDirection *= slideSpeed;
				playerControl = false;
			}
			// Otherwise recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
			else 
			{
				moveDirection = new Vector3(inputX * inputModifyFactor , -antiBumpFactor, inputY * inputModifyFactor);
				moveDirection = myTransform.TransformDirection(moveDirection) * speed;
				playerControl = true;
			}
			
			// Jump! But only if the jump button has been released and player has been grounded for a given number of frames
			if (!Input.GetButton("Jump"))
				jumpTimer++;

			else if (jumpTimer >= antiBunnyHopFactor) 
			{
				moveDirection.y = jumpSpeed;
				jumpTimer = 0;
			}
		}
		else 
		{
			// If we stepped over a cliff or something, set the height at which we started falling
			if (!falling) 
			{
				falling = true;
				fallStartLevel = myTransform.position.y;
			}
			
			// If air control is allowed, check movement but don't touch the y component
			if (airControl && playerControl) 
			{
				moveDirection.x = inputX * speed * inputModifyFactor;
				moveDirection.z = inputY * speed * inputModifyFactor;
				moveDirection = myTransform.TransformDirection(moveDirection);
			}
		}

		Animation ();
		// Apply gravity
		moveDirection.y -= gravity * Time.deltaTime;

		// Move the controller, and set grounded true or false depending on whether we're standing on something
		grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

		if(grounded && !prevGrounded)
		{
			myAudioSource.clip = landNoise;
			myAudioSource.Play();//.PlayClipAtPoint(landNoise, transform.position);
		}
	}

	public void SetZooming(bool isZooming)
	{
		if(sprinting)
		{
			this.isZooming = false;
			return;
		}

		this.isZooming = isZooming;
		animationIdlePosition = Vector3.zero;
		animationWalkingPosition = Vector3.zero;
	}

	[HideInInspector]
	public Vector3 animationIdlePosition;
	[HideInInspector]
	public Vector3 animationWalkingPosition;
	[HideInInspector]
	public Vector3 animationSprintingPosition;
	float lastPos;
	float lastSprintPos;
	void Animation()
	{
		if (currentVehicle != null)
		{
			animationIdlePosition = Vector3.zero;
			animationWalkingPosition = Vector3.zero;
			animationSprintingPosition = Vector3.zero;
			IdleTimer = 0;
			WalkTimer = 0;
			walkCount = 1;
			SprintTimer = 0;
			sprintCount = 1;
			return;
		}
		/*if(isZooming)
		{
			animationPosition = Vector3.Lerp(animationPosition, Vector3.zero, .1f);
			animationWalkingPosition = Vector3.Lerp(animationWalkingPosition, Vector3.zero, .1f);
			IdleTimer = 0;
			WalkTimer = 0;
			walkCount = 1;
		}*/
		if(!walking && !sprinting)
		{
			IdleTimer--;

			if (IdleTimer >= IdleTimerReset / 2) 
			{
				if(isZooming)
				{
					animationIdlePosition += Vector3.up * idleAmount * zoomIdleAmount;
				}
				else
				{
					animationIdlePosition += Vector3.up * idleAmount;
				}
			}
			else
			{
				if(isZooming)
				{
					animationIdlePosition -= Vector3.up * idleAmount * zoomIdleAmount;
				}
				else
				{
					animationIdlePosition -= Vector3.up * idleAmount;
				}
			}

			if(IdleTimer == 0)
			{
				IdleTimer = IdleTimerReset;
			}

			animationWalkingPosition = Vector3.zero;
			WalkTimer = 0;
			walkCount = 1;
			animationSprintingPosition = Vector3.zero;
			SprintTimer = 0;
			sprintCount = 1;
		}
		else if(!sprinting)
		{
			if(grounded)
			{
				WalkTimer += inc;
				float check = period * walkCount;
				if(isZooming)
				{
					animationWalkingPosition += Mathf.Sin(B * WalkTimer / 2) * walkAmount * zoomWalkAmount * Vector3.right;
					animationWalkingPosition -= Mathf.Sin(B * WalkTimer) * walkAmount * zoomWalkAmount * Vector3.up;
				}
				else
				{
					animationWalkingPosition += Mathf.Sin(B * WalkTimer / 2) * walkAmount * Vector3.right;
					animationWalkingPosition -= Mathf.Sin(B * WalkTimer) * walkAmount * Vector3.up;
				}

				if(WalkTimer > check)
				{
					int r = Random.Range(0, footNoises.Length);
					myAudioSource.clip = footNoises[r];
					myAudioSource.Play();
					walkCount++;
				}
				animationIdlePosition = Vector3.zero;
				animationSprintingPosition = Vector3.zero;
			}

			lastPos = WalkTimer;
		}
		else
		{
			if(grounded)
			{
				SprintTimer += sprintInc;
				float check = sprintPeriod * sprintCount;
				if(isZooming)
				{
					animationSprintingPosition += Mathf.Sin(B * SprintTimer / 2) * sprintAmount * Vector3.right;
					animationSprintingPosition -= Mathf.Sin(B * SprintTimer) * sprintAmount * Vector3.up;
				}
				else
				{
					animationSprintingPosition += Mathf.Sin(B * SprintTimer / 2) * sprintAmount * Vector3.right;
					animationSprintingPosition -= Mathf.Sin(B * SprintTimer) * sprintAmount * Vector3.up;
				}
				
				if(SprintTimer > check)
				{
					int r = Random.Range(0, footNoises.Length);
					myAudioSource.clip = footNoises[r];
					myAudioSource.Play();
					sprintCount++;
				}
				animationIdlePosition = Vector3.zero;
				animationWalkingPosition = Vector3.zero;
			}
			
			lastSprintPos = SprintTimer;
		}
	}

	int walkCount = 1;
	int sprintCount = 1;
	
	void Update () 
	{	
		if(Input.GetKeyDown(KeyCode.E))
		{
			if(touchingVehicle != null && currentVehicle == null)
				GetInVehicle(touchingVehicle);
			else if(currentVehicle != null)
				GetOutVehicle(currentVehicle);
		}

		if (currentVehicle != null)
		{
			this.transform.position = currentVehicle.mount.position;
			this.transform.rotation = currentVehicle.mount.rotation;
			return;
		}
		// If the run button is set to toggle, then switch between walk/run speed. (We use Update for this...
		// FixedUpdate is a poor place to use GetButtonDown, since it doesn't necessarily run every frame and can miss the event)
		if (toggleRun && grounded && Input.GetButtonDown("Run"))
			speed = (speed == walkSpeed? runSpeed : walkSpeed);

		if(prevGrounded )
		{
			if(Input.GetButtonDown("Jump"))
			{
				myAudioSource.clip = jumpNoise;
				myAudioSource.Play();
			}
		}
	}

	void GetInVehicle(Vehicle v)
	{
		currentVehicle = v;
		currentVehicle.Activate ();
		GetComponent<CapsuleCollider> ().enabled = false;
		GetComponent<CharacterController> ().enabled = false;
		GetComponentInChildren<MouseCameraControl> ().enabled = false;
		weapon.gameObject.active = false;
		weapon.transform.parent.GetComponent<WeaponShoot> ().enabled = false;
	}

	void GetOutVehicle(Vehicle v)
	{
		currentVehicle.Deactivate ();
		currentVehicle = null;
		GetComponent<CapsuleCollider> ().enabled = true;
		GetComponent<CharacterController> ().enabled = true;
		GetComponentInChildren<MouseCameraControl> ().enabled = true;
		weapon.gameObject.active = true;
		weapon.transform.parent.GetComponent<WeaponShoot> ().enabled = true;
	}

	Vehicle touchingVehicle;
	Vehicle currentVehicle;

	void OnTriggerEnter(Collider c)
	{
		Vehicle v = c.gameObject.GetComponent<Vehicle>();
		if(v != null)
		{
			touchingVehicle = v;
		}
	}

	void OnTriggerExit(Collider c)
	{
		Vehicle v = c.gameObject.GetComponent<Vehicle>();
		if(v != null)
		{
			touchingVehicle = null;
		}
	}
	
	// Store point that we're in contact with for use in FixedUpdate if needed
	void OnControllerColliderHit (ControllerColliderHit hit) 
	{
		contactPoint = hit.point;
	}
	
	// If falling damage occured, this is the place to do something about it. You can make the player
	// have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
	void FallingDamageAlert (float fallDistance) 
	{
		print ("Ouch! Fell " + fallDistance + " units!");   
	}
}