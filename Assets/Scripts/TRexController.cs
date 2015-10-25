using UnityEngine;
using System.Collections;

public class TRexController : MonoBehaviour {

	// public variables
	public Transform target;
	public float speed;
	public static float spawnDistance = 8f;

	// components
	Rigidbody2D rb;

	// private variables
	private Vector2 direction;
	private Vector2 directionNorm;
	private bool facingRight = true;
		// for OnSerializeNetworkView
		private float lastSynchronizationTime = 0f;
		private float syncDelay = 0f;
		private float syncTime = 0f;
		private Vector3 syncStartPosition = Vector3.zero;
		private Vector3 syncEndPosition = Vector3.zero;
	
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
	}

	void FixedUpdate () {
		if (GetComponent<NetworkView> ().isMine) {
			direction = target.position - transform.position;
			directionNorm = direction / direction.magnitude;
			rb.velocity = new Vector2 (directionNorm.x * speed, directionNorm.y * speed);

			if (facingRight && rb.velocity.x < 0)
				Flip ();
			else if (!facingRight && rb.velocity.x > 0)
				Flip ();
		} else {
			SyncedMovement ();
		}
	}
	
	private void SyncedMovement () {
		syncTime += Time.deltaTime;
		rb.position = Vector3.Lerp (syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.CompareTag("HostPlayer") || other.gameObject.CompareTag ("ClientPlayer")) {
			Network.Destroy (GetComponent<NetworkView>().gameObject);
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncScale = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		if (stream.isWriting) {
			syncPosition = rb.position;
			stream.Serialize (ref syncPosition);
			
			syncScale = transform.localScale;
			stream.Serialize (ref syncScale);
			syncVelocity = rb.velocity;
			stream.Serialize (ref syncVelocity);
		} else {
			stream.Serialize (ref syncPosition);
			stream.Serialize (ref syncScale);
			stream.Serialize (ref syncVelocity);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = rb.position;
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
