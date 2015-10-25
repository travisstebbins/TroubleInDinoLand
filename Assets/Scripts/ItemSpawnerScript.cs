using UnityEngine;
using System.Collections;

public class ItemSpawnerScript : MonoBehaviour {

	// public variables
	public float itemRespawnTime = 10f;
	public GameObject leafPrefab;
	public GameObject glitchEggPrefab;

	// components
	private Collider2D coll;
	private NetworkManagerScript networkManager = NetworkManagerScript.instance;
	private GameObject item;

	// private variables

	void Awake () {
		coll = GetComponent<Collider2D> ();
		SpawnItem ();
	}

	void Update () {
		coll.transform.rotation = item.GetComponent<Collider2D>().transform.rotation;
		coll.transform.position = item.GetComponent<Collider2D>().transform.position;
	}

	void SpawnItem () {
		int i = Random.Range (0, 10);
		if (i <= 6) {
			Debug.Log ("leaf created");
			item = (GameObject) Network.Instantiate (leafPrefab, transform.position, Quaternion.identity, 0);
		} else {
			Debug.Log ("egg created");
			item = (GameObject) Network.Instantiate (glitchEggPrefab, transform.position, Quaternion.identity, 0);
		}
		coll = item.GetComponent<Collider2D> ();
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.CompareTag ("HostPlayer") || other.gameObject.CompareTag ("ClientPlayer")) {
			Debug.Log ("item collision");
			/*if (isLeaf) {
				Debug.Log ("leaf triggered");
				other.gameObject.GetComponent<PlayerController> ().AddLeaf ();
				Debug.Log (other.gameObject.GetComponent<PlayerController> ().getLeafCount ());
				other.gameObject.GetComponent<PlayerController> ().PlayEatSound ();
				isLeaf = false;
			}
			Network.RemoveRPCs (item.GetComponent<NetworkViewID>());*/
			Network.Destroy (item.GetComponent<NetworkView> ().gameObject);
			coll = null;
			StartCoroutine (SpawnItemCoroutine ());
		}
	}

	IEnumerator SpawnItemCoroutine () {
		yield return null;
		yield return new WaitForSeconds (itemRespawnTime);
		SpawnItem ();
	}

	/*void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		bool syncIsLeaf = false;
		if (stream.isWriting) {
			syncIsLeaf = isLeaf;
			stream.Serialize (ref syncIsLeaf);
		} else {
			stream.Serialize (ref syncIsLeaf);
			
			isLeaf = syncIsLeaf;
		}
	}*/
}
