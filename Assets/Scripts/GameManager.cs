using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	// public variables
	public static GameManager instance = null;	
	public Text player1ScoreText;
	public Text player2ScoreText;
	public Text endText;
	public Timer timer;

	// private variables
	private NetworkManagerScript networkManager = NetworkManagerScript.instance;
	private GameObject hostDinosaur;
	private GameObject clientDinosaur;
	private bool foundHost = false;
	private bool foundClient = false;
	private bool dinosaurSetupComplete = false;
	private bool levelSetupComplete = false;
	private int player1Score;
	private int player2Score;
	private bool endGame = false;

	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void OnLevelWasLoaded () {
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag ("SpawnPoint");
		networkManager.hostSpawnPoint = (spawnPoints [0].name == "HostSpawnPoint") ? spawnPoints [0].transform : spawnPoints [1].transform;
		networkManager.clientSpawnPoint = (spawnPoints [0].name == "ClientSpawnPoint") ? spawnPoints [0].transform : spawnPoints [1].transform;
	}

	void Update () {
		if (!foundHost) {
			GameObject host = GameObject.FindGameObjectWithTag ("HostPlayer");
			if (host != null) {
				hostDinosaur = host;
				hostDinosaur.GetComponent<PlayerController> ().setHasBeenFound (true);
				foundHost = true;
			}
		}
		if (!foundClient) {
			GameObject client = GameObject.FindGameObjectWithTag("ClientPlayer");
			if (client != null) {
				clientDinosaur = client;
				clientDinosaur.GetComponent<PlayerController>().setHasBeenFound(true);
				foundClient = true;
			}
		}

		if (foundHost && foundClient && !dinosaurSetupComplete) {
			Debug.Log ("found host and client");
			dinosaurSetupComplete = true;
		}
		
		if (dinosaurSetupComplete && !levelSetupComplete) {
			Debug.Log ("attempting to assign references to other dinosaur");
			if (hostDinosaur != null)
				Debug.Log("host dinosaur found");
			if (clientDinosaur != null)
				Debug.Log ("client dinosaur found");
			hostDinosaur.GetComponent<PlayerController>().otherDinosaur = clientDinosaur;
			clientDinosaur.GetComponent<PlayerController>().otherDinosaur = hostDinosaur;
			timer.StartTimer ();
			Debug.Log ("start timer");			
			levelSetupComplete = true;
			Debug.Log ("levelSetupComplete = true");
		}

		SetScore ();

		if (endGame && Input.GetKeyDown (KeyCode.Return)) {
			Time.timeScale = 1;
			endGame = false;
			Application.LoadLevel ("MainMenu");
		}
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		int syncPlayer1Score = 0;
		int syncPlayer2Score = 0;
		if (stream.isWriting) {
			syncPlayer1Score = player1Score;
			stream.Serialize (ref syncPlayer1Score);
			syncPlayer2Score = player2Score;
			stream.Serialize (ref syncPlayer2Score);
		} else {
			stream.Serialize (ref syncPlayer1Score);
			stream.Serialize (ref syncPlayer2Score);
			player1Score = syncPlayer1Score;
			player2Score = syncPlayer2Score;
		}
	}

	public void SetScore () {
		player1Score = hostDinosaur.GetComponent<PlayerController> ().getLeafCount ();
		player2Score = clientDinosaur.GetComponent<PlayerController> ().getLeafCount ();
		player1ScoreText.text = "Player 1: " + player1Score;
		player2ScoreText.text = "Player 2: " + player2Score;
	}

	public void EndGame () {
		Debug.Log ("EndGame");
		Time.timeScale = 0;		
		endGame = true;
		if (player1Score > player2Score)
			endText.text = "Player 1 Wins!\nPress Enter to Quit";
		else if (player2Score > player1Score)
			endText.text = "Player 2 Wins!\nPress Enter to Quit";
		else
			endText.text = "It's a Tie!\nPress Enter to Quit";
	}

}
