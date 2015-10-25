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

	// private variables
	private bool isEmpty = true;
	private bool ready = false;

	void Awake () {
		bColl = GetComponent<BoxCollider2D> ();
		SpawnItem ();
	}

	void SpawnItem () {
		Random
	}
}
