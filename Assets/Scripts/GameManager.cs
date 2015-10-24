using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// public variables

	// private variables
	private NetworkManagerScript networkManager = NetworkManagerScript.instance;
	private GameObject hostDinosaur;
	private GameObject clientDinosaur;
	private bool dinosaurSetupComplete = false;

	void OnLevelWasLoaded () {
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag ("SpawnPoint");
		networkManager.hostSpawnPoint = (spawnPoints [0].name == "HostSpawnPoint") ? spawnPoints [0].transform : spawnPoints [1].transform;
		networkManager.clientSpawnPoint = (spawnPoints [0].name == "ClientSpawnPoint") ? spawnPoints [0].transform : spawnPoints [1].transform;
	}

	void Update () {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		if (players.Length == 1) {
			hostDinosaur = players [0];
			hostDinosaur.GetComponent<PlayerController>().setHasBeenFound(true);
		}
		if (players.Length == 2) {
			if (!players[0].GetComponent<PlayerController>().getHasBeenFound())
			{
				clientDinosaur = players[0];
				clientDinosaur.GetComponent<PlayerController>().setHasBeenFound(true);
				dinosaurSetupComplete = true;
			}
			else {
				clientDinosaur = players[1];
				clientDinosaur.GetComponent<PlayerController>().setHasBeenFound(true);
				dinosaurSetupComplete = true;
			}
		}

		if (dinosaurSetupComplete) {
			hostDinosaur.GetComponent<PlayerController>().otherDinosaur = clientDinosaur;
			clientDinosaur.GetComponent<PlayerController>().otherDinosaur = hostDinosaur;
			dinosaurSetupComplete = false;
		}
	}

}
