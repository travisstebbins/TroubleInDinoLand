using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// public variables

	// private variables
	private NetworkManagerScript networkManager = NetworkManagerScript.instance;

	void OnLevelWasLoaded () {
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag ("SpawnPoint");
		networkManager.hostSpawnPoint = (spawnPoints [0].name == "HostSpawnPoint") ? spawnPoints [0].transform : spawnPoints [1].transform;
		networkManager.clientSpawnPoint = (spawnPoints [0].name == "ClientSpawnPoint") ? spawnPoints [0].transform : spawnPoints [1].transform;
	}

}
