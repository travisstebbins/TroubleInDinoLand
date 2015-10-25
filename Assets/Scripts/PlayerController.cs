using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	
	// public variables
	public float maxSpeed = 10f;
	public float jumpHeight = 10f;
	public Transform groundCheck;
	public float groundRadius = 0.2f;
	public float glitchDuration = 10f;
	public float rotateSpeed = 7f;
	public int tRexAttack = 5;
	public LayerMask groundLayerMask;
	public GameObject tRexPrefab;
	public GameObject otherDinosaur;
	public Camera camera;
	public AudioClip[] eatSounds;
	
	// components
	private Rigidbody2D rb;
	private Animator anim;
	private AudioSource source;
	
	// private variables
	private GameManager gameManager;
	private bool isGrounded = false;
	private bool doubleJump = false;
	private bool facingRight = false;
	private int leafCount;
	private bool glitchActive = false;
	private bool moveThroughWallsGlitch = false;
	private bool hasBeenFound = false;
	private bool gravityFlipped = false;
	private Quaternion newRotation;
	private bool rotating = false;
	private bool controlsFlipped = false;
	private GameObject tRex;
	// for OnSerializeNetworkView
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private float syncAnimationSpeed = 0;
	private bool syncAnimationJump = false;
	public bool syncAnimationIsGlitched = false;
	private bool syncOtherAnimationIsGlitched;
	
	void Awake () {
		gameManager = GameManager.instance;
	}
	
	void OnLevelWasLoaded () {
		gameManager = GameManager.instance;
	}
	
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		source = GetComponent<AudioSource> ();
		leafCount = 0;
	}
	
	void FixedUpdate () {
		if (GetComponent<NetworkView>().isMine) {
			// check if character is grounded
			isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, groundLayerMask);	
			if (isGrounded) {
				doubleJump = false;
			}
			
			float move = Input.GetAxis ("Horizontal");
			anim.SetFloat("speed", Mathf.Abs (move));
			syncAnimationSpeed = move;
			if (!controlsFlipped)
				rb.velocity = !gravityFlipped ? new Vector2 ((isGrounded ? move * maxSpeed : move * maxSpeed * 0.8f), rb.velocity.y) : new Vector2 ((isGrounded ? -move * maxSpeed : -move * maxSpeed * 0.8f), rb.velocity.y);
			else
				rb.velocity = new Vector2 ((isGrounded ? -move * maxSpeed : -move * maxSpeed * 0.8f), rb.velocity.y);
			
			if (!controlsFlipped) {
				if (facingRight && move > 0)
					Flip ();
				else if (!facingRight && move < 0)
					Flip ();
			}
			else {
				if (facingRight && move < 0)
					Flip ();
				else if (!facingRight && move > 0)
					Flip ();
			}
			
		}
	}
	
	void Update () {
		if (rotating)
			rotation ();
		if (GetComponent<NetworkView> ().isMine) {
			if (moveThroughWallsGlitch) {
				float move = Input.GetAxis ("Vertical");
				rb.velocity = new Vector2 (rb.velocity.x, move * maxSpeed);
			}
			if (!controlsFlipped) {
				if ((isGrounded || !doubleJump) && Input.GetKeyDown (KeyCode.UpArrow)) {
					if (!isGrounded && !doubleJump)
						doubleJump = true;
					rb.velocity = new Vector2 (rb.velocity.x, !gravityFlipped ? Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y) : -Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y));
					anim.SetTrigger ("jump");
					syncAnimationJump = true;
				}
			} else if (controlsFlipped) {
				if ((isGrounded || !doubleJump) && Input.GetKeyDown (KeyCode.DownArrow)) {
					if (!isGrounded && !doubleJump)
						doubleJump = true;
					rb.velocity = new Vector2 (rb.velocity.x, !gravityFlipped ? Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y) : -Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y));					
					anim.SetTrigger ("jump");
					syncAnimationJump = true;
				}
			}
			camera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
			camera.transform.position = new Vector3 (transform.position.x, transform.position.y, -10);
		} else {
			SyncedMovement ();
		}
	}
	
	public void Rotate () {
		Debug.Log ("player Rotate called");
		newRotation = !gravityFlipped ? Quaternion.Euler (new Vector3 (0, 0, 0)) : Quaternion.Euler (new Vector3 (0, 0, 180));
		rotating = true;
	}
	
	void rotation() {
		if (Mathf.Abs (transform.rotation.eulerAngles.z - newRotation.eulerAngles.z) < 10 * float.Epsilon) {
			transform.rotation = newRotation;
			rotating = false;
		}
		else
			transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.deltaTime * rotateSpeed);		
	}
	
	private void SyncedMovement () {
		syncTime += Time.deltaTime;
		rb.position = Vector3.Lerp (syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncScale = Vector3.zero;
		Quaternion syncRotation = Quaternion.identity;
		Quaternion syncLocalRotation = Quaternion.identity;
		float syncGravityScale = 1f;
		int syncLeafCount = 0;
		bool syncMoveThroughWallsGlitch = false;
		bool syncGravityFlipped = false;
		bool syncControlsFlipped = false;
		bool syncGlitchActive = false;
		
		//Vector3 syncVelocity = Vector3.zero;
		if (stream.isWriting) {
			syncPosition = rb.position;
			stream.Serialize (ref syncPosition);
			
			syncScale = transform.localScale;
			stream.Serialize (ref syncScale);
			
			syncRotation = transform.rotation;
			stream.Serialize (ref syncRotation);
			
			syncLocalRotation = transform.localRotation;
			stream.Serialize (ref syncLocalRotation);
			
			syncGravityScale = rb.gravityScale;
			stream.Serialize (ref syncGravityScale);
			
			syncLeafCount = leafCount;
			stream.Serialize (ref syncLeafCount);
			
			syncMoveThroughWallsGlitch = moveThroughWallsGlitch;
			stream.Serialize (ref syncMoveThroughWallsGlitch);
			
			syncGravityFlipped = gravityFlipped;
			stream.Serialize (ref syncGravityFlipped);
			
			syncControlsFlipped = controlsFlipped;
			stream.Serialize (ref syncControlsFlipped);
			
			syncGlitchActive = glitchActive;
			stream.Serialize (ref syncGlitchActive);

			stream.Serialize (ref syncAnimationSpeed);
			stream.Serialize (ref syncAnimationJump);
			stream.Serialize (ref syncAnimationIsGlitched);
			stream.Serialize (ref syncOtherAnimationIsGlitched);
			
			//syncVelocity = rb.velocity;
			//stream.Serialize (ref syncVelocity);
		} else {
			stream.Serialize (ref syncPosition);
			stream.Serialize (ref syncScale);
			stream.Serialize (ref syncRotation);
			stream.Serialize (ref syncLocalRotation);
			stream.Serialize (ref syncGravityScale);
			stream.Serialize (ref syncLeafCount);
			stream.Serialize (ref syncMoveThroughWallsGlitch);
			stream.Serialize (ref syncGravityFlipped);
			stream.Serialize (ref syncControlsFlipped);
			stream.Serialize (ref syncGlitchActive);
			stream.Serialize (ref syncAnimationSpeed);
			stream.Serialize (ref syncAnimationJump);
			stream.Serialize (ref syncAnimationIsGlitched);
			stream.Serialize (ref syncOtherAnimationIsGlitched);
			//stream.Serialize (ref syncVelocity);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			syncStartPosition = rb.position;
			syncEndPosition = syncPosition;
			
			transform.localScale = syncScale;
			transform.rotation = syncRotation;
			transform.localRotation = syncLocalRotation;
			rb.gravityScale = syncGravityScale;
			leafCount = syncLeafCount;
			moveThroughWallsGlitch = syncMoveThroughWallsGlitch;
			gravityFlipped = syncGravityFlipped;
			controlsFlipped = syncControlsFlipped;
			glitchActive = syncGlitchActive;
			anim.SetFloat ("speed", Mathf.Abs (syncAnimationSpeed));
			if (syncAnimationJump)
				anim.SetTrigger ("jump");
			anim.SetBool ("isGlitched", syncAnimationIsGlitched);
			otherDinosaur.GetComponent<PlayerController>().getAnimator().SetBool ("isGlitched", syncOtherAnimationIsGlitched);
		}
	}
	
	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "Leaf") {
			PlayEatSound ();
			Debug.Log ("leaf triggered");
			leafCount++;
			gameManager.SetScore();
		}
		if (other.gameObject.CompareTag ("GlitchEgg")) {
			Debug.Log ("glitch egg triggered");
			//other.gameObject.SetActive (false);
			StartCoroutine(Glitch (other.gameObject.GetComponent<GlitchEggController>().glitchID));
		}
		if (other.gameObject.CompareTag ("TRex")) {
			Debug.Log ("TRex attack!");
			leafCount -= tRexAttack;
			if (leafCount < 0)
				leafCount = 0;
			Network.RemoveRPCs (GetComponent<NetworkView>().viewID);
			Network.Destroy (tRex);
		}
	}

	public void PlayEatSound () {
		int i = Random.Range (0, eatSounds.Length);
		source.PlayOneShot (eatSounds[i]);
	}
	
	// glitchIDs: 0 = moveThroughWalls, 1 = trex, 2 = cameraRotate, 3 = flipGravity, 4 = flipControls
	IEnumerator Glitch (int glitchID) {
		if (glitchID == 0) {
			glitchActive = true;
			anim.SetBool ("isGlitched", true);
			syncAnimationIsGlitched = true;
			moveThroughWallsGlitch = true;
			Physics2D.IgnoreLayerCollision (LayerMask.NameToLayer ("Player"), LayerMask.NameToLayer ("BoardIgnoreCollisions"), true);
		} else if (glitchID == 1) {
			Debug.Log ("TRex glitch triggered");
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur exists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.glitchActive = true;
			otherPlayer.getAnimator().SetBool ("isGlitched", true);
			otherPlayer.syncAnimationIsGlitched = true;
			syncOtherAnimationIsGlitched = true;
			tRex = (GameObject)Network.Instantiate (tRexPrefab, new Vector3 (!otherPlayer.isFacingRight () ? otherPlayer.transform.position.x - TRexController.spawnDistance : otherPlayer.transform.position.x + TRexController.spawnDistance, otherPlayer.transform.position.y, otherPlayer.transform.position.z), Quaternion.identity, 1);
			tRex.GetComponent<TRexController> ().target = otherPlayer.transform;
		} else if (glitchID == 2) {
			Debug.Log ("glitchType == cameraRotate");
			//GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.glitchActive = true;
			otherPlayer.getAnimator().SetBool ("isGlitched", true);
			otherPlayer.syncAnimationIsGlitched = true;
			syncOtherAnimationIsGlitched = true;
			otherDinosaur.GetComponent<PlayerController>().camera.GetComponent<CameraRotate>().Rotate ();
		} else if (glitchID == 3) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur exists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.glitchActive = true;
			otherPlayer.getAnimator().SetBool ("isGlitched", true);
			otherPlayer.syncAnimationIsGlitched = true;
			otherPlayer.gravityFlipped = true;
			otherPlayer.GetComponent<Rigidbody2D>().gravityScale = -1;
			syncOtherAnimationIsGlitched = true;
			otherPlayer.Rotate ();
		} else if (glitchID == 4) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur esists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController>();
			otherPlayer.glitchActive = true;
			otherPlayer.getAnimator().SetBool ("isGlitched", true);
			otherPlayer.syncAnimationIsGlitched = true;
			otherPlayer.controlsFlipped = true;
			syncOtherAnimationIsGlitched = true;
		}
		Debug.Log ("glitch active");
		yield return new WaitForSeconds (glitchDuration);
		if (glitchID == 0) {
			glitchActive = false;
			anim.SetBool ("isGlitched", false);
			syncAnimationIsGlitched = false;
			moveThroughWallsGlitch = false;
			Physics2D.IgnoreLayerCollision (LayerMask.NameToLayer ("Player"), LayerMask.NameToLayer ("BoardIgnoreCollisions"), false);
		} else if (glitchID == 1) {
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.glitchActive = false;
			otherPlayer.getAnimator().SetBool ("isGlitched", false);
			otherPlayer.syncAnimationIsGlitched = false;
			syncOtherAnimationIsGlitched = false;
			Network.RemoveRPCs (GetComponent<NetworkView>().viewID);
			Network.Destroy (tRex);
		} else if (glitchID == 2) {
			//GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraRotate> ().Rotate ();
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.glitchActive = false;
			otherPlayer.getAnimator().SetBool ("isGlitched", false);
			otherPlayer.syncAnimationIsGlitched = false;
			syncOtherAnimationIsGlitched = false;
			otherDinosaur.GetComponent<PlayerController>().camera.GetComponent<CameraRotate>().Rotate ();
		} else if (glitchID == 3) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur exists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.glitchActive = false;
			otherPlayer.getAnimator().SetBool ("isGlitched", false);
			otherPlayer.syncAnimationIsGlitched = false;
			otherPlayer.gravityFlipped = false;
			otherPlayer.GetComponent<Rigidbody2D>().gravityScale = 1;
			syncOtherAnimationIsGlitched = false;
			otherPlayer.Rotate ();
		} else if (glitchID == 4) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur esists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController>();
			otherPlayer.glitchActive = false;
			otherPlayer.getAnimator().SetBool ("isGlitched", false);
			otherPlayer.syncAnimationIsGlitched = false;
			syncOtherAnimationIsGlitched = false;
			otherPlayer.controlsFlipped = false;
		}
		Debug.Log ("glitch deactivated");
	}
	
	void Flip () {
		Vector2 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
		facingRight = !facingRight;
	}
	
	public bool isFacingRight () {
		return facingRight;
	}
	
	public void setHasBeenFound (bool b) {
		hasBeenFound = b;
	}
	
	public bool getHasBeenFound () {
		return hasBeenFound;
	}
	
	public int getLeafCount () {
		return leafCount;
	}

	public void AddLeaf () {
		leafCount++;
	}

	public Animator getAnimator () {
		return anim;
	}
}