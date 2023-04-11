using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI
{
	public class ErrorBox : MonoBehaviour
	{
		[SerializeField] private TMP_Text m_status;
		[SerializeField] private TMP_Text m_message;

		private void Awake()
		{
			gameObject.SetActive(false);
		}

		public void Show(ConnectionStatus stat, string message)
		{
			m_status.text = stat.ToString();
			m_message.text = message;
			gameObject.SetActive(true);
		}

		public void OnClose()
		{
			gameObject.SetActive(false);
		}
	}
}