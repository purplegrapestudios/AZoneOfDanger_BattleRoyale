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

		if (_app.IsBatchMode())
		{
			Application.targetFrameRate = ServerConfigData.TargetFrameRate;
			Debug.Log($"Setting TargetFrameRate: {Application.targetFrameRate}");
		}
	}

	private void InitCommandLineArguments()
    {
		CommandLineUtility.GetCommandLineArgument("-ip", out ServerConfigData.IPAddress);
		CommandLineUtility.GetCommandLineArgument("-port", out ServerConfigData.Port);
		CommandLineUtility.GetCommandLineArgument("-targetFrameRate", out ServerConfigData.TargetFrameRate);
		CommandLineUtility.GetCommandLineArgument("-serverName", out ServerConfigData.ServerName);
		CommandLineUtility.GetCommandLineArgument("-playModeInt", out ServerConfigData.PlayModeInt);
		CommandLineUtility.GetCommandLineArgument("-mapIndexInt", out ServerConfigData.MapIndexInt);
		CommandLineUtility.GetCommandLineArgument("-maxPlayers", out ServerConfigData.MaxPlayers);
		//ServerConfigData.IPAddress = "3.98.173.106";
		//ServerConfigData.Port = 7777;
		//ServerConfigData.TargetFrameRate = 60;
		//ServerConfigData.ServerName = $"AWS - {ServerConfigData.IPAddress}";
		//ServerConfigData.PlayModeInt = (int)PlayMode.Sandbox;
		//ServerConfigData.MapIndexInt = (int)MapIndex.Map0;
		//ServerConfigData.MaxPlayers = 44;
	}

	public void StartGame(bool useHostInsteadOfServer)
	{
		if (_app.IsBatchMode())
		{
			SessionProps props = new SessionProps();
			props.StartMap = (MapIndex)ServerConfigData.MapIndexInt;
			props.PlayMode = (PlayMode)ServerConfigData.PlayModeInt;
			props.PlayerLimit = ServerConfigData.MaxPlayers;
			props.RoomName = $"Server {ServerConfigData.ServerName} Room";
			props.AllowLateJoin = true;

			App.FindInstance().CreateSession(props, useHostInsteadOfServer: false);
		}

        if (useHostInsteadOfServer)
        {
			SessionProps props = new SessionProps();
			props.StartMap = (MapIndex)ServerConfigData.MapIndexInt;
			props.PlayMode = (PlayMode)ServerConfigData.PlayModeInt;
			props.PlayerLimit = ServerConfigData.MaxPlayers;
			props.RoomName = $"Host {ServerConfigData.ServerName} Room";
			props.AllowLateJoin = true;

			App.FindInstance().CreateSession(props, useHostInsteadOfServer: true);
		}
	}

	public void SetPlayMode(PlayMode playMode)
	{
		m_playMode = playMode;
	}
}