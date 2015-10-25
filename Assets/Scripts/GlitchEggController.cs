using UnityEngine;
using System.Collections;

public class GlitchEggController : MonoBehaviour {

	public int glitchID;
	public RuntimeAnimatorController[] animators;
	public Sprite[] yellowSprites;
	public Sprite[] pinkSprites;
	
	void Awake () {
		glitchID = Random.Range (0, 5);
		int i = Random.Range (0, animators.Length);
		GetComponent<Animator> ().runtimeAnimatorController = animators [i];
		if (i == 0)
			GetComponent<SpriteRenderer> ().sprite = yellowSprites [Random.Range (0, yellowSprites.Length)];
		else
			GetComponent<SpriteRenderer> ().sprite = pinkSprites[Random.Range (0, pinkSprites.Length)];
	}

}
