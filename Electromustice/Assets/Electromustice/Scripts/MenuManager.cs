using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class MenuManager : MonoBehaviour {

	/*
	 * Attention :
	 * 		PC is the server and runs the MasterServer
	 * 		these cell phones are the clients
	 * 
	 */

	private GameObject go_menuClient;
	private GameObject go_menuGame;

	private System.Int32 dwFlag = new int();
	private const int INTERNET_CONNECTION_MODEM = 1; 
	private const int INTERNET_CONNECTION_LAN = 2;
	[DllImport("wininet.dll")]
	private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);
	
	private const string s_typeName = "MyUniqueElectromusiticeGame";
	private HostData[] hostList;

	public delegate void MenuEventHandler();
	public event MenuEventHandler MenuEvent;

	Color colorOrigin = Color.blue;
	string s_nameMenu = null;
	Collider col = null;

	void Start()
	{
		go_menuGame = GameObject.Find("MenuGame");
		go_menuClient = GameObject.Find ("MenuClient");
		go_menuGame.SetActive (false);
		go_menuClient.SetActive (true);
		
		#if UNITY_EDITOR

		if (!InternetGetConnectedState(ref dwFlag, 0))
		{
			Debug.Log("no network!");
		}
		else if ((dwFlag & INTERNET_CONNECTION_MODEM) != 0)
		{
			Debug.Log("network by modem!");
		}
		else if((dwFlag & INTERNET_CONNECTION_LAN)!=0)   
		{
			Debug.Log("network by card!");  
		}
		
		#elif UNITY_ANDROID
		go_menuClient.SetActive (true);
		#elif UNITY_IPHONE
		#endif

	}

	void OnTriggerEnter(Collider other)
	{
		s_nameMenu = other.gameObject.name;
		if (s_nameMenu != "player" && s_nameMenu != "plane") {

						colorOrigin = other.gameObject.GetComponent<MeshRenderer> ().material.color;
						col = other;

						EventManager.AddEventFunction(EnumEvent.OnMagnetDown, MenuFunction);

//						MenuEvent += new MenuEventHandler(MenuFunction);

				}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.name != "player" && other.gameObject.name != "plane")
		{
			EventManager.RemoveEventFunction(EnumEvent.OnMagnetDown, MenuFunction);
//			MenuEvent -= new MenuEventHandler(ChangeColor);
			other.gameObject.GetComponent<MeshRenderer> ().material.color = colorOrigin;
		}
	}

	public void MenuFunction()
	{
		
		switch (s_nameMenu) {
		case "menu1":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.red;
			break;
		case "menu2":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.black;
			break;
		case "menu3":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.yellow;
			break;
		case "menu4":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.blue;
			break;
		case "menu5":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.gray;
			break;
		case "menu6":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.white;
			break;
		case "menu7":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.blue;
			break;
		case "menu8":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.red;
			break;
		case "menu9":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.red;
			break;
		case "menuClient":
			col.gameObject.GetComponent<MeshRenderer> ().material.color = Color.red;
			RefreshHostList();
			JoinServer();
			break;
		default:
			break;
		}
	}

	public void menuEventFunction()
	{
		if(MenuEvent != null)
		{
			MenuEvent();
		}
	}

	private void RefreshHostList()
	{
		MasterServer.port = 23466;
		MasterServer.ipAddress = "192.168.1.45";
		MasterServer.RequestHostList (s_typeName);
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if(msEvent == MasterServerEvent.HostListReceived)
		{
			hostList = MasterServer.PollHostList();
		}
	}

	private void JoinServer()
	{
		Network.Connect (hostList[0]);
		go_menuClient.SetActive (false);
		go_menuGame.SetActive (true);
	}
}
