using UnityEngine;
using System.Collections;

public class NetworkManagerScript : MonoBehaviour {

	// public variables
	public static NetworkManagerScript instance = null;
	public GameObject hostPlayerPrefab;
	public GameObject clientPlayerPrefab;
	public Transform hostSpawnPoint;
	public Transform clientSpawnPoint;

	// private variables
	private const string typeName = "Chillennium.NetflixAndChillennium.MultiplayerTest";
	private string gameName = "MultiplayerTestGame";
	private HostData[] hostList;
	private bool refreshing = false;
	private bool hostPlayerSpawned = false;
	private bool clientPlayerSpawned = false;
	private bool networkSetupComplete = false;
	private GameObject hostDinosaur;
	private GameObject clientDinosaur;

	// unity functions
	
	void Awake () {
		DontDestroyOnLoad (this);
		MasterServer.ipAddress = "192.168.5.2";
		/*MasterServer.ipAddress = "your ip";
		MasterServer.port = port masterserver;
		Network.natFacilitatorIP = "your ip";
		Network.natFacilitatorPort = port facilitator;*/
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
		Network.Connect (hostData);
	}

	private void SpawnPlayer (int spawnPointID) {
		if (spawnPointID == 0) {
			hostDinosaur = (GameObject) Network.Instantiate (hostPlayerPrefab, hostSpawnPoint.position, Quaternion.identity, 0);
		} else if (spawnPointID == 1) {
			//networkSetupComplete = true;
			clientDinosaur = (GameObject) Network.Instantiate (clientPlayerPrefab, clientSpawnPoint.position, Quaternion.identity, 0);
			//hostDinosaur.GetComponent<PlayerController>().otherDinosaur = clientDinosaur;
			//clientDinosaur.GetComponent<PlayerController>().otherDinosaur = hostDinosaur;
		}
	}

	void Update () {
		RefreshHostList ();
		if (refreshing) {
			if (MasterServer.PollHostList ().Length > 0) {
				refreshing = false;
			}
		}
		if (!hostPlayerSpawned && Network.isServer && !networkSetupComplete) {
			if (hostSpawnPoint != null) {				
				hostPlayerSpawned = true;
				SpawnPlayer (0);
			}
		}
		if (!clientPlayerSpawned && Network.isClient && !networkSetupComplete) {
			if (clientSpawnPoint != null) {
				clientPlayerSpawned = true;
				SpawnPlayer (1);
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
