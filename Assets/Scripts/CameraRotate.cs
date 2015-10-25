using UnityEngine;
using System.Collections;

public class CameraRotate : MonoBehaviour {

	public float rotateSpeed = 3f;
	
	private float angle;
	private Quaternion newRotation;
	private bool rotating = false;
	private bool flipped = false;

	void Update () {
		if (rotating) {
			rotation ();
		}
	}

	public void Rotate () {
		newRotation = flipped ? Quaternion.Euler (new Vector3 (0, 0, 0)) : Quaternion.Euler (new Vector3 (0, 0, 180));
		rotating = true;
		flipped = !flipped;
	}
	
	void rotation() {
		if (Mathf.Abs (transform.rotation.eulerAngles.z - newRotation.eulerAngles.z) < 10 * float.Epsilon) {
			transform.rotation = newRotation;
			rotating = false;
		}
		else
			transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.deltaTime * rotateSpeed);		
	}
}
