using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class NetworkManager : MonoBehaviour {

	public GameObject ServerCamera;

	private System.Int32 dwFlag = new int();
	private const int INTERNET_CONNECTION_MODEM = 1; 
	private const int INTERNET_CONNECTION_LAN = 2;
	[DllImport("wininet.dll")]
	private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);


	private int i_maxNumOfClients = 4;   // maximum 4 clients in the game
	private const string s_typeName = "MyUniqueElectromusiticeGame";
	private string s_gameName = "DefaultRoomName";
	private bool b_serverStarted = false;
	private bool b_isServer = false;
	private int i_indexMyPlayer = -1;    //just for the clients : the index of the value of the array iArray_idPlayer of my own player
	private int i_indexPlayerControlled = -1; //the index of the value of the array iArray_idPlayer of the player controlled by the server
//	private int idPlayerControlled = -1;   // the networkView ID of the player controlled by the server
	/*
	 * Just used by server, its length is i_maxNumOfClients 
	 * The index of the array is i_idPlayer
	 * The values of the array is the networkView ID
	 * 
	 */
	private string[] sArray_viewIDPlayer;

	private Vector3[] v3Array_playerPosition;
	private Vector3[] v3Array_playerRotation;
	private GameObject[] goArray_playerEmpty;
	private GameObject go_player;

	public Vector3 v3_speed = new Vector3(1, 1, 1);

	void Awake()
	{
		go_player = GameObject.Instantiate (
			GlobalVariables.GO_PLAYER_COMPLETE, new Vector3(0f, 2f, 0f), Quaternion.identity) as GameObject;
		goArray_playerEmpty = new GameObject[i_maxNumOfClients];
		
		for(int i = 0; i < i_maxNumOfClients; ++i)
		{
			goArray_playerEmpty[i] = null;
		}
		
		#if UNITY_EDITOR
		Application.runInBackground = true;

		GameObject go_menuClient = GameObject.Find("MenuClient");
		go_menuClient.SetActive(false);

		go_player.SetActive(false);
		ServerCamera.SetActive(true);
		b_isServer = true;
		b_serverStarted = false;
		sArray_viewIDPlayer = new string[i_maxNumOfClients];
		v3Array_playerPosition = new Vector3[i_maxNumOfClients];
		v3Array_playerRotation = new Vector3[i_maxNumOfClients];
		
		for(int i = 0; i < i_maxNumOfClients; ++i)
		{
			v3Array_playerPosition[i] = new Vector3(0f, 2f, 0f);
			v3Array_playerRotation[i] = Vector3.zero;
			sArray_viewIDPlayer[i] = null;
		}
		#elif UNITY_ANDROID
		b_isServer = false;
		ServerCamera.SetActive (false);
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

	void OnPlayerConnected(NetworkPlayer player)
	{
		for(int i = 0; i < sArray_viewIDPlayer.Length; ++i)
		{
			if(sArray_viewIDPlayer[i] == null)
			{
				NetworkViewID viewID = Network.AllocateViewID();
				sArray_viewIDPlayer[i] = viewID.ToString();

				goArray_playerEmpty[i] = GameObject.Instantiate(GlobalVariables.GO_PLAYER_EMPTY) as GameObject;

				networkView.RPC("client_sendIDToPlayerRPC", player, i, viewID);
	
				break;
			}
		}
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		for(int i = 0; i < sArray_viewIDPlayer.Length; ++i)
		{
			if(sArray_viewIDPlayer[i] == player.ToString())
			{
				sArray_viewIDPlayer[i] 	= null;

				if(goArray_playerEmpty[i] != null)
				{
					Destroy(goArray_playerEmpty[i]);
				}

				networkView.RPC("client_removePlayerExitedRPC", RPCMode.Others, i);

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

		for(int i = 0; i < sArray_viewIDPlayer.Length; ++i)
		{
			if(sArray_viewIDPlayer[i] != null)
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
				v3Array_playerPosition[i_indexPlayerControlled].z += v3_speed.z * Time.deltaTime;
			}
			else if(Input.GetKey(KeyCode.DownArrow))
			{
				v3Array_playerPosition[i_indexPlayerControlled].z -= v3_speed.z * Time.deltaTime;
			}
			else if(Input.GetKey(KeyCode.LeftArrow))
			{
				v3Array_playerPosition[i_indexPlayerControlled].x += v3_speed.x * Time.deltaTime;
			}
			else if(Input.GetKey(KeyCode.RightArrow))
			{
				v3Array_playerPosition[i_indexPlayerControlled].x -= v3_speed.x * Time.deltaTime;
			}

			if(Input.GetKey(KeyCode.Space))
			{
				v3Array_playerPosition[i_indexPlayerControlled].y += v3_speed.y * Time.deltaTime;
			}

			goArray_playerEmpty[i_indexPlayerControlled].transform.position = v3Array_playerPosition[i_indexPlayerControlled];
			networkView.RPC ("client_updatePositionRPC", RPCMode.Others, i_indexPlayerControlled, v3Array_playerPosition[i_indexPlayerControlled]);
		}

	}

	private void UpdateRotation()
	{

	}

	[RPC]
	public void client_sendIDToPlayerRPC(int _i_indexMyPlayer, NetworkViewID _viewID)
	{
		client_sendIDToPlayerLocal (_i_indexMyPlayer, _viewID);

	}

	public void client_sendIDToPlayerLocal (int _i_indexMyPlayer, NetworkViewID _viewID)
	{
		go_player.GetComponent<NetworkView> ().viewID = _viewID;
		i_indexMyPlayer = _i_indexMyPlayer;

		networkView.RPC ("server_initDoneThisPlayerRPC", RPCMode.Server, _i_indexMyPlayer);
	}

	[RPC]
	public void server_initDoneThisPlayerRPC(int _i_indexNewPlayer)
	{
		server_initDoneThisPlayerLocal (_i_indexNewPlayer);
	}

	public void server_initDoneThisPlayerLocal (int _i_indexNewPlayer)
	{
		networkView.RPC ("client_newPlayerJoinedRPC", RPCMode.Others, _i_indexNewPlayer);
	}

	[RPC]
	public void client_newPlayerJoinedRPC(int _i_indexNewPlayer)
	{
		client_newPlayerJoinedLocal (_i_indexNewPlayer);
	}

	public void client_newPlayerJoinedLocal (int _i_indexNewPlayer)
	{
		if(i_indexMyPlayer != _i_indexNewPlayer)
		{
			goArray_playerEmpty[_i_indexNewPlayer] = GameObject.Instantiate(
				GlobalVariables.GO_PLAYER_EMPTY, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		}
	}

	[RPC]
	public void client_removePlayerExitedRPC(int _i_indexPlayerExited)
	{
		client_removePlayerExitedLocal (_i_indexPlayerExited);
	}

	public void client_removePlayerExitedLocal(int _i_indexPlayerExited)
	{
		if(goArray_playerEmpty[_i_indexPlayerExited] != null)
		{
			Destroy(goArray_playerEmpty[_i_indexPlayerExited]);
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
				goArray_playerEmpty[_i_indexPlayer].GetComponent<Transform>().position = _v3_pos;
			}
		}
		else
		{
			go_player.transform.position = _v3_pos;
		}
	}
}













