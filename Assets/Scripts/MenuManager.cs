using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	// public variables
	public GameObject content;
	public GameObject buttonPrefab;
	public NetworkManagerScript networkManager;
	public GameObject hostListGroup;
	public GameObject gameNameField;

	// private variables
	private HostData[] hostList;

	void Start () {
	}

	public void CreateGame () {
		string gameName = gameNameField.GetComponent<InputField> ().text;
		if (gameName == null)
			gameName = "Trouble in Dinoland" + System.DateTime.UtcNow;
		networkManager.CreateGame (gameName);
		LoadLevel ("Main");
	}

	public void LoadLevel (string levelName) {
		Application.LoadLevel (levelName);
	}

	public void ShowHostsList () {
		hostListGroup.gameObject.SetActive (true);
		hostList = networkManager.getHostList ();
		Debug.Log (hostList.Length);
		for (int i = 0; i < hostList.Length; ++i) {
			GameObject button = (GameObject)Instantiate (buttonPrefab);
			button.GetComponentInChildren<Text>().text = hostList[i].gameName;
			int index = i;
			button.GetComponent<Button>().onClick.AddListener (
				() => {Debug.Log("button clicked");networkManager.JoinServer (hostList[index]);LoadLevel ("Main");}
			);
			button.transform.parent = content.transform;
			button.transform.position = new Vector2(content.transform.position.x + 100, content.transform.position.y - 20 - (40 * i));
			//button.transform.localScale = new Vector2 (button.transform.localScale.x * 2, button.transform.localScale.y);
		}
	}
}
