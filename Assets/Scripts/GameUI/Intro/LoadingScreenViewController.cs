using System.Collections;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI.Intro
{
	public class LoadingScreenViewController : MonoBehaviour
	{
		[SerializeField] private InputField _inputName;
		[SerializeField] private Text _textMaxPlayers;
		[SerializeField] private Toggle _toggleMap1;
		[SerializeField] private Toggle _toggleMap2;
		[SerializeField] private Toggle _allowLateJoin;
		[SerializeField] private NetworkRunner _runner;
		[SerializeField] private TMP_Text m_loadingText;
		[SerializeField] private Text _textDebug;

		private int _maxPly = 4;
		private PlayMode _playMode;

		public void Show(PlayMode mode, bool useHostInsteadOfServer)
		{
			//We've configured this button to automatically be called when the Server is running this.

			gameObject.SetActive(true);
			_playMode = mode;
			UpdateLoadingText();

			ServerManager.Instance.StartGame(useHostInsteadOfServer);
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

		public void OnCreateSession()
		{
			SessionProps props = new SessionProps();
			props.StartMap = _toggleMap1.isOn ? MapIndex.Map0 : MapIndex.Map1;
			props.PlayMode = _playMode;
			props.PlayerLimit = _maxPly;
			props.RoomName = _inputName.text;
			props.AllowLateJoin = _allowLateJoin.isOn;
			
			// Pass the session properties to the app - this will unload the current scene and load the staging area if successful
			App.FindInstance().CreateSession(props, useHostInsteadOfServer: false);
		}

		private void UpdateUI()
		{
			_textMaxPlayers.text = $"Max Players: {_maxPly}";
			if(!_toggleMap1.isOn && !_toggleMap2.isOn)
				_toggleMap1.isOn = true;
			if(string.IsNullOrWhiteSpace(_inputName.text))
				_inputName.text = "Room1";

			UpdateLoadingText();
		}

		private void UpdateLoadingText()
        {
			if (UpdateLoadingTextCoroutine != null)
				StopCoroutine(UpdateLoadingTextCoroutine);
			UpdateLoadingTextCoroutine = UpdateLoadingTextCO();
			StartCoroutine(UpdateLoadingTextCoroutine);
		}

		private IEnumerator UpdateLoadingTextCoroutine;
		private IEnumerator UpdateLoadingTextCO()
        {
			string str = "Loading";
			m_loadingText.text = str;
			int count = 0;

			while (true)
            {
				yield return new WaitForSeconds(1);
				m_loadingText.text += count % 3 == 0 ? " ." : ".";
				count++;
				if (count > 41)
                {
					count = 0;
					m_loadingText.text = str;
                }
			}
        }
	}
}