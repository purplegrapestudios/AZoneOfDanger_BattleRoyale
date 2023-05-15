using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI.Intro
{
	public class GameModePanel : MonoBehaviour
	{
		[SerializeField] private SessionListPanel _sessionsPanel;
		[SerializeField] private GameModeMenuItem m_buttonPublicServers;
		[SerializeField] private GameModeMenuItem m_buttonArenaServers;
		[SerializeField] private GameModeMenuItem m_buttonSandboxServers;
		[SerializeField] private GameModeMenuItem m_buttonHostServers;
		[SerializeField] private PlayerSelectionViewController m_playerSelectionViewController;
		[SerializeField] private GameModeMenuItem m_buttonPlayerSelection;
		[SerializeField] private GameObject m_menuPanel;
		[SerializeField] private GameObject m_menuBg;
		[SerializeField] private Camera m_sceneCamera;
		[SerializeField] private bool m_uselocalServerInGame;
		private App _app;

		private void Awake()
		{
			Application.targetFrameRate = 144;
			_app = App.FindInstance();
			_sessionsPanel.Hide();

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			m_buttonPublicServers.Initialize(onclickCallbackInt: SelectGameMode, intValue: 0);
			m_buttonArenaServers.Initialize(onclickCallbackInt: SelectGameMode, intValue: 1);
			m_buttonSandboxServers.Initialize(onclickCallbackInt: SelectGameMode, intValue: 2);

			m_buttonHostServers.Initialize(onclickCallback: StartAsHost);
			m_buttonPlayerSelection.Initialize(onclickCallbackBool: TogglePlayerSelectionViewController, boolValue: true);
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

        private void Update()
        {
			m_uselocalServerInGame = Input.GetKey(KeyCode.LeftShift);
			m_buttonHostServers.UpdateLabel(m_uselocalServerInGame? "Run Local Server" : "Host Game");
			m_buttonHostServers.UpdateGradientBG(m_uselocalServerInGame);
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
			_sessionsPanel.Show((PlayMode)ServerConfigData.PlayModeInt, isClient: false, useHostInsteadOfServer);
		}

		//Used for Clients to join Game Mode. So Set UseHostInsteadOfServer to false, as we won't be using that for joining anyway
		public void SelectGameMode(int playMode)
        {
			_sessionsPanel.Show((PlayMode)playMode, isClient: true, useHostInsteadOfServer: false);
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
			//If holding shift
			ServerGameModeCoroutine = ServerGameModeCO(useHostInsteadOfServer: m_uselocalServerInGame ? false : true);
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