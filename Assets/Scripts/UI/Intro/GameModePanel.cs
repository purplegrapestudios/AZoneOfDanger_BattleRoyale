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
		[SerializeField] private PlayerSelectionViewController m_playerSelectionViewController;
		[SerializeField] private Button m_buttonPlayerSelection;
		[SerializeField] private GameObject m_menuPanel;
		[SerializeField] private GameObject m_menuBg;
		[SerializeField] private Camera m_sceneCamera;
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
			m_buttonPlayerSelection.onClick.AddListener(() => { TogglePlayerSelectionViewController(true); });
			m_playerSelectionViewController.Initialize(() => { TogglePlayerSelectionViewController(false); });
			TogglePlayerSelectionViewController(false);

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

		private void TogglePlayerSelectionViewController(bool show)
        {
			m_playerSelectionViewController.gameObject.SetActive(show);
			m_menuPanel.SetActive(!show);
			//m_menuBg.SetActive(!show);
			float sceneViewScreenWidth = 0.6f; // Occupies 60% of the width
			m_sceneCamera.rect = show ? new Rect(1 - sceneViewScreenWidth, 0f, sceneViewScreenWidth, 1) : new Rect(0, 0, 1, 1);
		}
	}
}