using System.Collections;
using UnityEngine;

namespace GameUI.Intro
{
	public class GameModePanel : MonoBehaviour
	{
		[SerializeField] private SessionListPanel _sessionsPanel;
		private App _app;

		private void Awake()
		{
			_app = App.FindInstance();
			_sessionsPanel.Hide();

		}

        private void Start()
        {
			if (_app.IsBatchMode())
			{
				if (StartCTFCoroutine != null)
					StopCoroutine(StartCTFCoroutine);
				StartCTFCoroutine = StartCTFCO();
				StartCoroutine(StartCTFCoroutine);
			}
		}

		private IEnumerator StartCTFCoroutine;
		private IEnumerator StartCTFCO()
        {
			yield return new WaitForSeconds(5);
			StartCTF();
        }
		private void StartCTF()
        {
			OnGameModeSelected(0);
		}

        public void OnGameModeSelected(int mode)
		{
			_sessionsPanel.Show((PlayMode) mode);
		}
	}
}