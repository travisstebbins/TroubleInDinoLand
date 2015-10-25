using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

	public GameManager gameManager;
	public Text timerText;
	public float timeRemaining = 120f;

	private bool start = false;

	public void StartTimer () {
		start = true;
	}
	
	void Update () {
		if (start && timeRemaining >= 0) {
			timeRemaining -= Time.deltaTime;
			
			timerText.text = Mathf.RoundToInt (timeRemaining).ToString ();
		} else if (start) {
			gameManager.EndGame ();
			Debug.Log ("EndGame called");
		}
	}
}
