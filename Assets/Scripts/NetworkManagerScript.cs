using UnityEngine;
using System.Collections;

public class NetworkManagerScript : MonoBehaviour {

	// public variables
	public GameObject playerPrefab;
	public Transform hostSpawnPoint;
	public Transform clientSpawnPoint;

	// private variables
	private const string typeName = "Chillennium.NetflixAndChillennium.MultiplayerTest";
	private const string gameName = "MultiplayerTestGame";
	private HostData[] hostList;
	
	void Start () {
	
	}

	void Update () {
	
	}

	private void StartServer () {
		Network.InitializeServer (2, 25000, !Network.HavePublicAddress ());
		MasterServer.RegisterHost (typeName, gameName);
	}

	void OnServerInitialized () {
		SpawnPlayer (0);
	}

	private void RefreshHostList () {
		MasterServer.RequestHostList (typeName);
	}

	void OnMasterServerEvent (MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList ();
	}

	private void JoinServer (HostData hostData) {
		Network.Connect (hostData);
	}

	void OnConnectedToServer () {
		SpawnPlayer (1);
	}

	private void SpawnPlayer (int spawnPointID) {
		if (spawnPointID == 0) {
			Network.Instantiate (playerPrefab, hostSpawnPoint.position, Quaternion.identity, 0);
		} else if (spawnPointID == 1) {
			Network.Instantiate (playerPrefab, clientSpawnPoint.position, Quaternion.identity, 0);
		}
	}

	void OnGUI () {
		if (!Network.isClient && !Network.isServer) {
			if (GUI.Button (new Rect (100, 100, 250, 100), "Start Server"))
				StartServer ();
			if (GUI.Button (new Rect (100, 250, 250, 100), "Refresh Hosts"))
				RefreshHostList();
			if (hostList != null) {
				for (int i = 0; i < hostList.Length; ++i) {
					if (GUI.Button (new Rect (400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer (hostList[i]);
				}
			}
		}
	}

}
