using UnityEngine;
using System.Collections;

public class LeafScript : MonoBehaviour {

	public float rotateSpeed = 3f;
	public Sprite[] sprites;

	void Awake () {
		int i = Random.Range (0, sprites.Length);
		GetComponent<SpriteRenderer> ().sprite = sprites [i];
	}

	void FixedUpdate () {
		transform.Rotate (new Vector3 (0, rotateSpeed, 0));
	}

}
