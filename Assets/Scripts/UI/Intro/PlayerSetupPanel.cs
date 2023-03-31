using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI.Intro
{
	public class PlayerSetupPanel : MonoBehaviour
	{
		[SerializeField] private TMP_InputField m_nameInput;
		[SerializeField] private Slider _sliderR;
		[SerializeField] private Slider _sliderG;
		[SerializeField] private Slider _sliderB;
		[SerializeField] private Image _color;
		[SerializeField] private GameObject _playerReady;
		[SerializeField] private bool _closeOnReady;

		private App _app;

		public void Show(bool s)
		{
			gameObject.SetActive(s);
		}

		private void OnEnable()
		{
			_app = App.FindInstance();
			//_playerReady.SetActive(false);
			_app.AllowInput = false;
		}

		private void OnDisable()
		{
			_app.AllowInput = true;
		}

		public void OnNameChanged(string name)
		{
			Player ply = _app.GetPlayer();
			ply.RPC_SetName(name);
		}
	
		public void OnColorUpdated()
		{
			Player ply = _app.GetPlayer();
			Color c = new Color(_sliderR.value, _sliderG.value, _sliderB.value);
			_color.color = c;
			ply.RPC_SetColor( c);
		}
		
		public void OnToggleIsReady()
		{
			if(_closeOnReady)
				Show(false);
			else
			{
				Player ply = _app.GetPlayer();
				//_playerReady.SetActive(!ply.Ready);
				ply.RPC_SetIsReady(!ply.Ready);
			}
		}

		public string GetNameInputValue()
        {
			return string.IsNullOrEmpty(m_nameInput?.text) ? "no name" : m_nameInput.text;
        }
	}
}