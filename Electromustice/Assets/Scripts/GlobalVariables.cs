using UnityEngine;
using System.Collections;

public class GlobalVariables : MonoBehaviour {

	public static GameObject GO_PLAYER_EMPTY; 
	public GameObject go_playerEmpty;

	public static GameObject GO_PLAYER_COMPLETE;
	public GameObject go_playerComplete;

	// Use this for initialization
	void Awake () {
		GO_PLAYER_EMPTY = go_playerEmpty;
		GO_PLAYER_COMPLETE = go_playerComplete;
	}
}
