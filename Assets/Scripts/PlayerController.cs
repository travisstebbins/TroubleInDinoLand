﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	// public variables
	public float maxSpeed = 10f;
	public float jumpHeight = 10f;
	public Transform groundCheck;
	public float groundRadius = 0.2f;
	public float glitchDuration = 5f;
	public LayerMask groundLayerMask;
	public GameObject tRexPrefab;	
	public GameObject otherDinosaur; 
	
	// components
	private Rigidbody2D rb;
	
	// private variables
	private bool isGrounded = false;
	private bool doubleJump = false;
	private bool facingRight = false;
	private int count;
	private bool glitchActive = false;
	private bool moveThroughWallsGlitch = false;
	private bool hasBeenFound = false;
	private string glitchType = "trex";
	private NetworkView networkView;
	private GameObject tRex;
		// for OnSerializeNetworkView
		private float lastSynchronizationTime = 0f;
		private float syncDelay = 0f;
		private float syncTime = 0f;
		private Vector3 syncStartPosition = Vector3.zero;
		private Vector3 syncEndPosition = Vector3.zero;

	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		count = 0;
		networkView = GetComponent<NetworkView> ();
	}
	
	void FixedUpdate () {
		if (GetComponent<NetworkView>().isMine) {
			// check if character is grounded
			isGrounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, groundLayerMask);
			if (isGrounded)
				doubleJump = false;
		
			float move = Input.GetAxis ("Horizontal");
			rb.velocity = new Vector2 ((isGrounded ? move * maxSpeed : move * maxSpeed * 0.8f), rb.velocity.y);

			if (facingRight && move > 0)
				Flip ();
			else if (!facingRight && move < 0)
				Flip ();
		}
	}
	
	void Update () {
		if (GetComponent<NetworkView> ().isMine) {
			if (moveThroughWallsGlitch) {
				float move = Input.GetAxis ("Vertical");
				rb.velocity = new Vector2 (rb.velocity.x, move * maxSpeed);
			} else if ((isGrounded || !doubleJump) && Input.GetKeyDown (KeyCode.UpArrow)) {
				if (!isGrounded && !doubleJump)
					doubleJump = true;
				rb.velocity = new Vector2 (rb.velocity.x, Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y));			
			}
		} else {
			SyncedMovement ();
		}
	}
	
	private void SyncedMovement () {
		syncTime += Time.deltaTime;
		rb.position = Vector3.Lerp (syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncScale = Vector3.zero;
		//Vector3 syncVelocity = Vector3.zero;
		if (stream.isWriting) {
			syncPosition = rb.position;
			stream.Serialize (ref syncPosition);

			syncScale = transform.localScale;
			stream.Serialize (ref syncScale);
			//syncVelocity = rb.velocity;
			//stream.Serialize (ref syncVelocity);
		} else {
			stream.Serialize (ref syncPosition);
			stream.Serialize (ref syncScale);
			//stream.Serialize (ref syncVelocity);

			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;

			syncStartPosition = rb.position;
			syncEndPosition = syncPosition;
			transform.localScale = syncScale;
		}
	}

	[RPC]
	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "Pickup") {
			other.gameObject.SetActive (false);
		}
		if (other.gameObject.CompareTag ("GlitchEgg")) {
			other.gameObject.SetActive (false);
			glitchType = "trex";
			StartCoroutine(Glitch ());
		}
		if (other.gameObject.CompareTag ("TRex")) {
			Debug.Log ("TRex attack!");
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (tRex);
		}
	}

	IEnumerator Glitch () {
		glitchActive = true;
		if (glitchType == "moveThroughWalls") {
			moveThroughWallsGlitch = true;
			Physics2D.IgnoreLayerCollision (LayerMask.NameToLayer ("Player"), LayerMask.NameToLayer ("BoardIgnoreCollisions"), true);
		}
		if (glitchType == "trex") {
			Debug.Log ("TRex glitch triggered");
			if (otherDinosaur != null)
				Debug.Log ("Other Dinosaur exists");
			glitchDuration = 10f;
			PlayerController otherPlayer = otherDinosaur.GetComponent<PlayerController> ();
			tRex = (GameObject) Network.Instantiate (tRexPrefab, new Vector3 (otherPlayer.isFacingRight () ? otherPlayer.transform.position.x + 4 : otherPlayer.transform.position.x - 4, otherPlayer.transform.position.y, otherPlayer.transform.position.z), Quaternion.identity, 1);
			tRex.GetComponent<TRexController>().target = otherPlayer.transform;
		}
		Debug.Log ("glitch active");
		yield return new WaitForSeconds (glitchDuration);
		if (glitchType == "moveThroughWalls") {			
			moveThroughWallsGlitch = false;
			Physics2D.IgnoreLayerCollision (LayerMask.NameToLayer ("Player"), LayerMask.NameToLayer ("BoardIgnoreCollisions"), false);
		}
		if (glitchType == "trex") {
			glitchDuration = 5f;
			Network.RemoveRPCs (networkView.viewID);
			Network.Destroy (tRex);
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
}