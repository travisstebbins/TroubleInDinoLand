using UnityEngine;
using System.Collections;

public class ItemSpawnerScript : MonoBehaviour {

	// public variables
	public float itemRespawnTime = 10f;
	public GameObject leafPrefab;
	public GameObject glitchEggPrefab;

	// components
	private BoxCollider2D bColl;
	private NetworkManagerScript networkManager = NetworkManagerScript.instance;
	private GameObject item;

	// private variables

	void Awake () {
		bColl = GetComponent<BoxCollider2D> ();
		SpawnItem ();
	}

	void SpawnItem () {
		int i = Random.Range (0, 10);
		if (i <= 6) {
			item = (GameObject) Network.Instantiate (leafPrefab, transform.position, Quaternion.identity, 1);
		} else {
			item = (GameObject) Network.Instantiate (glitchEggPrefab, transform.position, Quaternion.identity, 1);
		}
		bColl = item.GetComponent<BoxCollider2D> ();
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.CompareTag ("Player")) {
			Network.Destroy (item.GetComponent<NetworkView>().gameObject);
			StartCoroutine(SpawnItemCoroutine());
		}
	}

	IEnumerator SpawnItemCoroutine () {
		yield return null;
		yield return new WaitForSeconds (itemRespawnTime);
		SpawnItem ();
	}
}
