using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class StartGameGUI : MonoBehaviour
{
	public UnityEvent OnStartGame;

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(10, 10, 300, 300));
		if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
		{
			StartButtons();
		}
		else
		{
			StatusLabels();
		}

		GUILayout.EndArea();
	}

	void onStartGameInvoke()
	{
		OnStartGame.Invoke();
	}

	void StartButtons()
	{
		if (GUILayout.Button("Host"))
		{
			NetworkManager.Singleton.StartHost();
			Invoke("onStartGameInvoke", 1.0f);
		}
		if (GUILayout.Button("Client"))
		{
			NetworkManager.Singleton.StartClient();
			Invoke("onStartGameInvoke", 1.0f);
		}
		if (GUILayout.Button("Server"))
		{
			NetworkManager.Singleton.StartServer();
			Invoke("onStartGameInvoke", 1.0f);
		}

	}

	void StatusLabels()
	{
		string mode = "None";
		if (NetworkManager.Singleton.IsHost)
		{
			mode = "Host";
		}
		if (NetworkManager.Singleton.IsServer)
		{
			mode = "Server";
		}
		if (NetworkManager.Singleton.IsClient)
		{
			mode = "Client";
		}

		GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
		GUILayout.Label("Mode: " + mode);
	}
}
