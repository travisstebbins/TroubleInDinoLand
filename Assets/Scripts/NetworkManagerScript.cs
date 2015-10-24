using UnityEngine;
using System.Collections;

public class NetworkManagerScript : MonoBehaviour {

	// public variables
	public static NetworkManagerScript instance = null;
	public GameObject playerPrefab;
	public Transform hostSpawnPoint;
	public Transform clientSpawnPoint;

	// private variables
	private const string typeName = "Chillennium.NetflixAndChillennium.MultiplayerTest";
	private string gameName = "MultiplayerTestGame";
	private HostData[] hostList;
	private bool refreshing = false;
	private bool hostPlayerSpawned = false;
	private bool clientPlayerSpawned = false;

	// unity functions
	
	void Awake () {
		DontDestroyOnLoad (this);
	}

	void Start () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
	}

	// public functions

	public void CreateGame (string newGameName) {
		if (!Network.isClient && !Network.isServer) {
			gameName = newGameName;
			StartServer ();
		}
	}

	public HostData[] getHostList () {
		//RefreshHostList ();
		return hostList;
	}

	// server initialization

	private void StartServer () {
		Network.InitializeServer (2, 25000, !Network.HavePublicAddress ());
		MasterServer.RegisterHost (typeName, gameName);
	}

	//void OnServerInitialized () {
	//}

	// host list refreshing/joining server

	private void RefreshHostList () {
		MasterServer.RequestHostList (typeName);
		refreshing = true;
	}

	void OnMasterServerEvent (MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList ();
	}

	public void JoinServer (HostData hostData) {
		Debug.Log ("join server called");
		Network.Connect (hostData);
	}

	private void SpawnPlayer (int spawnPointID) {
		if (spawnPointID == 0) {
			Network.Instantiate (playerPrefab, hostSpawnPoint.position, Quaternion.identity, 0);
		} else if (spawnPointID == 1) {
			Network.Instantiate (playerPrefab, clientSpawnPoint.position, Quaternion.identity, 0);
		}
	}

	void Update () {
		RefreshHostList ();
		if (refreshing) {
			if (MasterServer.PollHostList ().Length > 0) {
				refreshing = false;
			}
		}
		if (!hostPlayerSpawned && Network.isServer) {
			if (hostSpawnPoint != null) {
				SpawnPlayer (0);
				hostPlayerSpawned = true;
			}
		}
		if (!clientPlayerSpawned && Network.isClient) {
			if (clientSpawnPoint != null) {
				SpawnPlayer (1);
				clientPlayerSpawned = true;
			}
		}
	}

	/*void OnGUI () {
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
	}*/

}
