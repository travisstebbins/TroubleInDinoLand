using UnityEngine;
using System.Collections;

public class TRexController : MonoBehaviour {

	// public variables
	public Transform target;
	public float speed;

	// components
	Rigidbody2D rb;

	// private variables
	private Vector2 direction;
	private Vector2 directionNorm;
	private bool facingRight = true;

	void Start () {
		rb = GetComponent<Rigidbody2D> ();
	}

	void FixedUpdate () {
		direction = target.position - transform.position;
		directionNorm = direction / direction.magnitude;
		rb.velocity = new Vector2 (directionNorm.x * speed, directionNorm.y * speed);

		if (facingRight && rb.velocity.x < 0)
			Flip ();
		else if (!facingRight && rb.velocity.x > 0)
			Flip ();
	}

	void Flip () {
		Vector2 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
		facingRight = !facingRight;
	}
}
