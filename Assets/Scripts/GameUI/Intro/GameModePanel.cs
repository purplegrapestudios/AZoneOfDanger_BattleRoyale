using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI.Intro
{
	public class GameModePanel : MonoBehaviour
	{
		[SerializeField] private SessionListPanel _sessionsPanel;
		[SerializeField] private Button m_buttonPublicServers;
		[SerializeField] private Button m_buttonArenaServers;
		[SerializeField] private Button m_buttonSandboxServers;
		[SerializeField] private Button m_buttonHostServers;

		private App _app;

		private void Awake()
		{
			Application.targetFrameRate = 144;
			_app = App.FindInstance();
			_sessionsPanel.Hide();

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;


			m_buttonPublicServers.onClick.AddListener(() => { SelectGameMode(0); });
			m_buttonArenaServers.onClick.AddListener(() => { SelectGameMode(1); });
			m_buttonSandboxServers.onClick.AddListener(() => { SelectGameMode(2); });
			m_buttonHostServers.onClick.AddListener(() => { StartAsHost(); });
		}

		private void Start()
        {
			if (_app.IsBatchMode())
			{
				if (ServerGameModeCoroutine != null)
					StopCoroutine(ServerGameModeCoroutine);
				ServerGameModeCoroutine = ServerGameModeCO(useHostInsteadOfServer: false);
				StartCoroutine(ServerGameModeCoroutine);
			}
			//StartAsHost();
		}

		private IEnumerator ServerGameModeCoroutine;
		private IEnumerator ServerGameModeCO(bool useHostInsteadOfServer)
		{
			yield return new WaitForSeconds(5);
			SetGameMode(useHostInsteadOfServer);

		}

        private void SetGameMode(bool useHostInsteadOfServer)
		{
			ServerManager.Instance.SetPlayMode((PlayMode)ServerConfigData.PlayModeInt);
			_sessionsPanel.Show((PlayMode)(PlayMode)ServerConfigData.PlayModeInt, useHostInsteadOfServer);
		}

		//Used for Clients to join Game Mode. So Set UseHostInsteadOfServer to false, as we won't be using that for joining anyway
		public void SelectGameMode(int playMode)
        {
			_sessionsPanel.Show((PlayMode) playMode, useHostInsteadOfServer: false);
		}

		public void StartAsHost()
        {
			ServerConfigData.TargetFrameRate = 144;
			Application.targetFrameRate = ServerConfigData.TargetFrameRate;
			QualitySettings.vSyncCount = 0;
			ServerConfigData.ServerName = $"local - ";
			ServerConfigData.PlayModeInt = (int)PlayMode.Sandbox;
			ServerConfigData.MapIndexInt = (int)MapIndex.Map0;
			ServerConfigData.MaxPlayers = 22;

			if (ServerGameModeCoroutine != null)
				StopCoroutine(ServerGameModeCoroutine);
			ServerGameModeCoroutine = ServerGameModeCO(useHostInsteadOfServer: true);
			StartCoroutine(ServerGameModeCoroutine);
		}
	}
}