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
	public LayerMask groundLayerMask;
	public GameObject tRexPrefab;	
	public GameObject otherDinosaur; 
	
	// components
	private Rigidbody2D rb;
	
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

	void Awake () {
		gameManager = GameManager.instance;
	}

	void OnLevelWasLoaded () {
		gameManager = GameManager.instance;
	}

	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		leafCount = 0;
	}
	
	void FixedUpdate () {
		if (GetComponent<NetworkView>().isMine) {
			// check if character is grounded
			isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, groundLayerMask);
			if (isGrounded)
				doubleJump = false;
			
			float move = Input.GetAxis ("Horizontal");
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
				}
			} else if (controlsFlipped) {
				if ((isGrounded || !doubleJump) && Input.GetKeyDown (KeyCode.DownArrow)) {
					if (!isGrounded && !doubleJump)
						doubleJump = true;
					rb.velocity = new Vector2 (rb.velocity.x, !gravityFlipped ? Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y) : -Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y));
				}
			}
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

			//syncVelocity = rb.velocity;
			//stream.Serialize (ref syncVelocity);
		} else {
			stream.Serialize (ref syncPosition);
			stream.Serialize (ref syncScale);
			stream.Serialize (ref syncRotation);
			stream.Serialize (ref syncLeafCount);
			stream.Serialize (ref syncMoveThroughWallsGlitch);
			stream.Serialize (ref syncGravityFlipped);
			stream.Serialize (ref syncControlsFlipped);
			stream.Serialize (ref syncGlitchActive);
			//stream.Serialize (ref syncVelocity);

			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;

			syncStartPosition = rb.position;
			syncEndPosition = syncPosition;

			transform.localScale = syncScale;
			transform.rotation = syncRotation;
			leafCount = syncLeafCount;
			moveThroughWallsGlitch = syncMoveThroughWallsGlitch;
			gravityFlipped = syncGravityFlipped;
			controlsFlipped = syncControlsFlipped;
			glitchActive = syncGlitchActive;
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "Leaf") {
			Debug.Log ("leaf triggered");
			//other.gameObject.SetActive (false);
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
			Network.RemoveRPCs (GetComponent<NetworkView>().viewID);
			Network.Destroy (tRex);
		}
	}

	// glitchIDs: 0 = moveThroughWalls, 1 = trex, 2 = cameraRotate, 3 = flipGravity, 4 = flipControls
	IEnumerator Glitch (int glitchID) {
		glitchActive = true;
		if (glitchID == 0) {
			moveThroughWallsGlitch = true;
			Physics2D.IgnoreLayerCollision (LayerMask.NameToLayer ("Player"), LayerMask.NameToLayer ("BoardIgnoreCollisions"), true);
		} else if (glitchID == 1) {
			Debug.Log ("TRex glitch triggered");
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur exists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			tRex = (GameObject)Network.Instantiate (tRexPrefab, new Vector3 (!otherPlayer.isFacingRight () ? otherPlayer.transform.position.x - TRexController.spawnDistance : otherPlayer.transform.position.x + TRexController.spawnDistance, otherPlayer.transform.position.y, otherPlayer.transform.position.z), Quaternion.identity, 1);
			tRex.GetComponent<TRexController> ().target = otherPlayer.transform;
		} else if (glitchID == 2) {
			Debug.Log ("glitchType == cameraRotate");
			GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraRotate> ().Rotate ();
		} else if (glitchID == 3) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur exists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.gravityFlipped = true;
			otherPlayer.GetComponent<Rigidbody2D>().gravityScale = -1;
			otherPlayer.Rotate ();
		} else if (glitchID == 4) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur esists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController>();
			otherPlayer.controlsFlipped = true;
		}
		Debug.Log ("glitch active");
		yield return new WaitForSeconds (glitchDuration);
		if (glitchID == 0) {			
			moveThroughWallsGlitch = false;
			Physics2D.IgnoreLayerCollision (LayerMask.NameToLayer ("Player"), LayerMask.NameToLayer ("BoardIgnoreCollisions"), false);
		} else if (glitchID == 1) {
			Network.RemoveRPCs (GetComponent<NetworkView>().viewID);
			Network.Destroy (tRex);
		} else if (glitchID == 2) {
			GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<CameraRotate> ().Rotate ();
		} else if (glitchID == 3) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur exists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			otherPlayer.gravityFlipped = false;
			otherPlayer.GetComponent<Rigidbody2D>().gravityScale = 1;
			otherPlayer.Rotate ();
		} else if (glitchID == 4) {
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur esists");
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController>();
			otherPlayer.controlsFlipped = false;
		}
		glitchActive = false;
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
}