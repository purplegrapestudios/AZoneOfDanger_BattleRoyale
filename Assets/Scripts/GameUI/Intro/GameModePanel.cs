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

		private App _app;

		private void Awake()
		{
			_app = App.FindInstance();
			_sessionsPanel.Hide();

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;


			m_buttonPublicServers.onClick.AddListener(() => { SelectGameMode(0); });
			m_buttonArenaServers.onClick.AddListener(() => { SelectGameMode(1); });
			m_buttonSandboxServers.onClick.AddListener(() => { SelectGameMode(2); });
		}

		private void Start()
        {
			if (_app.IsBatchMode())
			{
				if (ServerGameModeCoroutine != null)
					StopCoroutine(ServerGameModeCoroutine);
				ServerGameModeCoroutine = ServerGameModeCO();
				StartCoroutine(ServerGameModeCoroutine);
			}
		}

		private IEnumerator ServerGameModeCoroutine;
		private IEnumerator ServerGameModeCO()
        {
			yield return new WaitForSeconds(5);
			SetGameMode();

		}

        private void SetGameMode()
		{
			ServerManager.Instance.SetPlayMode((PlayMode)ServerConfigData.PlayModeInt);
			_sessionsPanel.Show((PlayMode)(PlayMode)ServerConfigData.PlayModeInt);
		}

		public void SelectGameMode(int playMode)
        {
			_sessionsPanel.Show((PlayMode) playMode);
		}
	}
}