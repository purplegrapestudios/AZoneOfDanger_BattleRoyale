using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;
	public Button m_startPublicGame = null;
	public Button m_startArenaGame = null;
	public Button m_startSandboxGame = null;
	public PlayMode m_playMode;

	private App _app;

	private void Awake()
	{
		Instance = this;
		_app = App.FindInstance();
		InitCommandLineArguments();
	}

	private void InitCommandLineArguments()
    {
		CommandLineUtility.GetCommandLineArgument("-ip", out ServerConfigData.IPAddress);
		CommandLineUtility.GetCommandLineArgument("-port", out ServerConfigData.Port);
		CommandLineUtility.GetCommandLineArgument("-serverName", out ServerConfigData.ServerName);
		CommandLineUtility.GetCommandLineArgument("-playModeInt", out ServerConfigData.PlayModeInt);
		CommandLineUtility.GetCommandLineArgument("-map", out ServerConfigData.MapIndexInt);
		CommandLineUtility.GetCommandLineArgument("-maxPlayers", out ServerConfigData.MaxPlayers);
	}

	public void StartGame()
	{
		if (_app.IsBatchMode())
		{
			SessionProps props = new SessionProps();
			props.StartMap = (MapIndex)ServerConfigData.MapIndexInt;
			props.PlayMode = (PlayMode)ServerConfigData.PlayModeInt;
			props.PlayerLimit = ServerConfigData.MaxPlayers;
			props.RoomName = $"Server {ServerConfigData.ServerName} Room";
			props.AllowLateJoin = true;

			App.FindInstance().CreateSession(props);
		}
	}

	public void SetPlayMode(PlayMode playMode)
	{
		m_playMode = playMode;
	}
}
