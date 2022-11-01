using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI.Intro
{
	public class NewSessionPanel : MonoBehaviour
	{
		[SerializeField] private InputField _inputName;
		[SerializeField] private Text _textMaxPlayers;
		[SerializeField] private Toggle _toggleMap1;
		[SerializeField] private Toggle _toggleMap2;
		[SerializeField] private Toggle _allowLateJoin;
		[SerializeField] private NetworkRunner _runner;
		[SerializeField] private Text _textDebug;

		private int _maxPly = 4;
		private PlayMode _playMode;
		private App _app;

        private void Awake()
        {
			_app = App.FindInstance();
		}

		public void Show(PlayMode mode)
		{
			//We've configured this button to automatically be called when the Server is running this.

			gameObject.SetActive(true);
			_playMode = mode;
			UpdateUI();
			string[] arguments = Environment.GetCommandLineArgs();
			_textDebug.text = $"\nparsed Argument len: {arguments?.Length ?? 0}";

			CommandLineUtility.GetCommandLineArgument("-ip", out ServerConfig.IPAddress);
			CommandLineUtility.GetCommandLineArgument("-port", out ServerConfig.Port);
			CommandLineUtility.GetCommandLineArgument("-serverName", out ServerConfig.ServerName);

			if (_app.IsBatchMode())
			{
				SessionProps props = new SessionProps();
				props.StartMap = MapIndex.Map1;
				props.PlayMode = PlayMode.CaptureTheFlag;
				props.PlayerLimit = 13;
				props.RoomName = $"Server {ServerConfig.ServerName} Room";
				props.AllowLateJoin = true;

				App.FindInstance().CreateSession(props);
			}
		}

		private void StartGameInitialized(NetworkRunner runner)
        {
			Debug.Log("StartGameArgs Initialized " + runner.name);
		}
		public void Hide()
		{
			gameObject.SetActive(false);
		}

		public void OnDecreaseMaxPlayers()
		{
			if(_maxPly>2)
				_maxPly--;
			UpdateUI();
		}
		public void OnIncreaseMaxPlayers()
		{
			if(_maxPly<16)
				_maxPly++;
			UpdateUI();
		}

		public void OnEditText()
		{
			UpdateUI();
		}

		private void UpdateUI()
		{
			_textMaxPlayers.text = $"Max Players: {_maxPly}";
			if(!_toggleMap1.isOn && !_toggleMap2.isOn)
				_toggleMap1.isOn = true;
			if(string.IsNullOrWhiteSpace(_inputName.text))
				_inputName.text = "Room1";
		}
		
		public void OnCreateSession()
		{
			SessionProps props = new SessionProps();
			props.StartMap = _toggleMap1.isOn ? MapIndex.Map0 : MapIndex.Map1;
			props.PlayMode = _playMode;
			props.PlayerLimit = _maxPly;
			props.RoomName = _inputName.text;
			props.AllowLateJoin = _allowLateJoin.isOn;
			
			// Pass the session properties to the app - this will unload the current scene and load the staging area if successful
			App.FindInstance().CreateSession(props);
		}
	}
}