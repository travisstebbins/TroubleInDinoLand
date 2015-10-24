using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	// public variables
	public float maxSpeed = 10f;
	public float jumpHeight = 10f;
	public Transform groundCheck;
	public float groundRadius = 0.2f;
	public LayerMask groundLayerMask;
	
	// components
	private Rigidbody2D rb;
	//private NetworkView networkView;
	
	// private variables
	private bool isGrounded = false;
	private bool doubleJump = false;
	private bool facingRight = false;
		// for OnSerializeNetworkView
		private float lastSynchronizationTime = 0f;
		private float syncDelay = 0f;
		private float syncTime = 0f;
		private Vector3 syncStartPosition = Vector3.zero;
		private Vector3 syncEndPosition = Vector3.zero;

	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		//networkView = GetComponent<NetworkView> ();
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
			if ((isGrounded || !doubleJump) && Input.GetKeyDown (KeyCode.UpArrow)) {
				if (!isGrounded && !doubleJump)
					doubleJump = true;
				rb.velocity = new Vector2 (rb.velocity.x, Mathf.Sqrt (2f * jumpHeight * -Physics2D.gravity.y));			
			}
		} else {
			SyncedMovement();
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

	void Flip () {
		Vector2 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
		facingRight = !facingRight;
	}
}