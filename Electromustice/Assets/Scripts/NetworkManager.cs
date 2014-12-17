using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable] // serializable for transfering on network
public class PlayerInfo{
	// Attention: if not set to empty string, the playerID will not be instantiated
	// even in case of playerList[i] = new PlayerInfo();
	public string playerID = "";
	public bool active = false;
	//public Vector3 pos;
	//public Vector3 rot;
	public float x;
	public float y;
	public float z;
	public float rx;
	public float ry;
	public float rz;
};

public class NetworkManager : MonoBehaviour {

	public GameObject ServerCamera;

	private System.Int32 dwFlag = new int();
	private const int INTERNET_CONNECTION_MODEM = 1; 
	private const int INTERNET_CONNECTION_LAN = 2;
	[DllImport("wininet.dll")]
	private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);


	private int i_maxNumOfClients = 2;   // maximum 4 clients in the game
	private const string s_typeName = "MyUniqueElectromusiticeGame";
	private string s_gameName = "DefaultRoomName";
	private bool b_serverStarted = false;
	private bool b_isServer = false;
	private int i_indexMyPlayer = -1;    //just for the clients : the index of the value of the array iArray_idPlayer of my own player
	private int i_indexPlayerControlled = -1; //the index of the value of the array iArray_idPlayer of the player controlled by the server

	/*
	 * Just used by server, its length is i_maxNumOfClients 
	 * The index of the array is i_idPlayer
	 * The values of the array is the networkView ID
	 * 
	 */
	//private string[] sArray_viewIDPlayer;

	//private Vector3[] v3Array_playerPosition;
	//private Vector3[] v3Array_playerRotation;
	private GameObject[] goArray_playerEmpty;
	private PlayerInfo[] playersInfo;
	private GameObject go_player;

	public Vector3 v3_speed = new Vector3(1, 1, 1);

	void Awake()
	{
		goArray_playerEmpty = new GameObject[i_maxNumOfClients];

		playersInfo = new PlayerInfo[i_maxNumOfClients];

		for (int i=0; i<playersInfo.Length; i++) {
			playersInfo[i] = new PlayerInfo();
		}

		for(int i = 0; i < i_maxNumOfClients; ++i)
		{
			goArray_playerEmpty[i] = null;
		}

		
		for(int i = 0; i < i_maxNumOfClients; ++i)
		{
			playersInfo[i].x = 0f;
			playersInfo[i].y = 2f;
			playersInfo[i].z = 0f;
			playersInfo[i].rx = 0f;
			playersInfo[i].ry = 0f;
			playersInfo[i].rz = 0f;
		}

		#if UNITY_EDITOR
		Application.runInBackground = true;

		GameObject go_menuClient = GameObject.Find("MenuClient");
		go_menuClient.SetActive(false);


		ServerCamera.SetActive(true);
		b_isServer = true;
		b_serverStarted = false;

		#elif UNITY_ANDROID
		b_isServer = false;
		ServerCamera.SetActive (false);

		go_player = GameObject.Instantiate (
			GlobalVariables.GO_PLAYER_COMPLETE, new Vector3(0f, 2f, 0f), Quaternion.identity) as GameObject;
		#elif UNITY_IPHONE
		#endif
	}

	// Use this for initialization
	void Start () {

	}

	private void StartServer()
	{
		/* Launch MasterServer */
		//			string path = Application.dataPath;
		//			Debug.Log(path);
		//			System.Diagnostics.Process.Start(path + @"\MasterServer\MasterServer.exe");
		
		/* Bind to it */
		MasterServer.ipAddress = "192.168.1.45";
		MasterServer.port = 23466;
		Network.InitializeServer (i_maxNumOfClients, 25000, false);
		//Network.InitializeServer (i_maxNumOfClients, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost (s_typeName, s_gameName);

		Debug.Log ("server initialised");
	}

	// client side code
	void OnConnectedToServer(){
		// Send client's player info to server
		networkView.RPC("serverInsertIntoPlayerList", RPCMode.Server, Network.player.ToString());
	}
	
	// server side code. rpc called by the client
	[RPC]
	void serverInsertIntoPlayerList(string np){
		int maxPlayer = i_maxNumOfClients;
		for (int i=0; i<maxPlayer; i++) {
			if(!playersInfo[i].active){
				playersInfo[i].playerID = np;
				playersInfo[i].active = true;
				goArray_playerEmpty[i] = GameObject.Instantiate(GlobalVariables.GO_PLAYER_EMPTY) as GameObject;
				break;//!
			}
		}

		// inform the clients the new player list
		// Attention: don't do this in OnPlayerConnected.
		// Because when client connected, its info may not arrived at the server yet.
		sendPlayerListToClients ();
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		for(int i = 0; i < playersInfo.Length; ++i)
		{
			if(playersInfo[i].playerID.Equals(player.ToString(),StringComparison.Ordinal))
			{
				playersInfo[i].playerID 	= "";
				playersInfo[i].active = false;
				if(goArray_playerEmpty[i] != null)
				{
					Destroy(goArray_playerEmpty[i]);
					goArray_playerEmpty[i] = null;
				}

				// inform the clients the new player list
				sendPlayerListToClients ();

				break;
			}
		}
	}

	void OnGUI()
	{
		#if UNITY_EDITOR
		if(b_serverStarted == false && b_isServer)
		{
			if (GUI.Button(new Rect(Screen.width/4, Screen.height/4, Screen.width/2, Screen.height/2), "StartServer"))
			{
				StartServer();
				b_serverStarted = true;
			}
		}

		for(int i = 0; i < playersInfo.Length; ++i)
		{
			if(playersInfo[i].playerID != "")
			{
				if(GUI.Button(new Rect(i * Screen.width/4, 0, Screen.width/8, Screen.height/8), "ControlPlayer"+i.ToString()))
				{
//					idPlayerControlled = sArray_viewIDPlayer[i];
					i_indexPlayerControlled = i;
				}
			}
		}
		#elif UNITY_ANDROID
		#elif UNITY_IPHONE
		#endif
	}

	void Update()
	{
		#if UNITY_EDITOR
		UpdatePosition ();

		//server cannot  control the rotation of a player, it can just update the rotation of one player to all other players
		UpdateRotation ();
		#elif UNITY_ANDROID
		#elif UNITY_IPHONE
		#endif
	}
	
	private void UpdatePosition()
	{
		if(i_indexPlayerControlled != -1)
		{
			if(Input.GetKey(KeyCode.UpArrow))
			{
				playersInfo[i_indexPlayerControlled].x += v3_speed.x * Time.deltaTime;
			}
			else if(Input.GetKey(KeyCode.DownArrow))
			{
				playersInfo[i_indexPlayerControlled].x -= v3_speed.x * Time.deltaTime;
			}
			else if(Input.GetKey(KeyCode.LeftArrow))
			{
				playersInfo[i_indexPlayerControlled].z += v3_speed.z * Time.deltaTime;
			}
			else if(Input.GetKey(KeyCode.RightArrow))
			{
				playersInfo[i_indexPlayerControlled].z -= v3_speed.z * Time.deltaTime;
			}

			if(Input.GetKey(KeyCode.Space))
			{
				playersInfo[i_indexPlayerControlled].y += v3_speed.y * Time.deltaTime;
			}
			Vector3 pos = new Vector3(playersInfo[i_indexPlayerControlled].x, playersInfo[i_indexPlayerControlled].y, playersInfo[i_indexPlayerControlled].z);
			if(goArray_playerEmpty[i_indexPlayerControlled] != null)
			{
				goArray_playerEmpty[i_indexPlayerControlled].transform.position = pos;
				networkView.RPC ("client_updatePositionRPC", RPCMode.Others, i_indexPlayerControlled, pos);
				//sendPlayerListToClients();
			}
		}

	}

	private void UpdateRotation()
	{

	}

	// server side code
	void sendPlayerListToClients(){
		BinaryFormatter binFormatter = new BinaryFormatter ();
		MemoryStream memStream = new MemoryStream (); // Stream whose backing store is memory. Defined in namespace System.IO
		binFormatter.Serialize (memStream,playersInfo);
		byte[] serializedPl = memStream.ToArray (); // Convert the serialized object to byte array
		memStream.Close ();
		networkView.RPC ("clientRefreshPlayerList",RPCMode.Others,serializedPl);
	}
	
	// client side code: rpc called by the server
	[RPC]
	void clientRefreshPlayerList(byte[] serializedPlayerList){
		BinaryFormatter binFormatter = new BinaryFormatter ();
		MemoryStream memStream = new MemoryStream (); 
		// Write the byte data we received into the stream 
		// The second parameter specifies the offset to the beginning of the stream
		// The third parameter specifies the maximum number of bytes to be written
		memStream.Write(serializedPlayerList,0,serializedPlayerList.Length); 
		// Stream internal "reader" is now at the last position
		// Shift it back to the beginning for reading
		memStream.Seek(0, SeekOrigin.Begin); 
		playersInfo = (PlayerInfo[])binFormatter.Deserialize (memStream);
		// refresh playerNumber
		int pNum = 0;
		foreach (PlayerInfo p in playersInfo) {
			if(p.active) pNum++;
		}

		for (int i=0; i<playersInfo.Length; i++) {
			if(playersInfo[i].playerID.Equals(Network.player.ToString(),StringComparison.Ordinal)){
				i_indexMyPlayer = i;
			}
			else if(playersInfo[i].active && goArray_playerEmpty[i] == null){
				Vector3 pos = new Vector3(playersInfo[i].x, playersInfo[i].y, playersInfo[i].z);
				goArray_playerEmpty[i] = 
					GameObject.Instantiate(GlobalVariables.GO_PLAYER_EMPTY, 
					                       pos,
					                       Quaternion.identity) as GameObject;
			}
			else if(!playersInfo[i].active && goArray_playerEmpty[i] != null){
				Destroy(goArray_playerEmpty[i]);
				goArray_playerEmpty[i] = null;
			}
		}
	}

	[RPC]
	public void client_updatePositionRPC(int _i_indexPlayer, Vector3 _v3_pos)
	{
		client_updatePositionLocal (_i_indexPlayer, _v3_pos);
	}

	public void client_updatePositionLocal(int _i_indexPlayer, Vector3 _v3_pos)
	{
		if(_i_indexPlayer != i_indexMyPlayer)
		{
			if(goArray_playerEmpty[_i_indexPlayer] != null)
			{
				goArray_playerEmpty[_i_indexPlayer].transform.position = _v3_pos;
			}
		}
		else
		{
			go_player.transform.position = _v3_pos;
		}
	}
}













