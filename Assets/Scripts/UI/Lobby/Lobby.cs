using System.Text;
using UIComponents;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI.Lobby
{
	public class Lobby : MonoBehaviour
	{
		public static Lobby Instance;
		[SerializeField] private GridBuilder _playerGrid;
		[SerializeField] private PlayerListItem _playerListItemPrefab;
		[SerializeField] private Intro.PlayerSetupPanel m_PlayerSetupPanel;

		[SerializeField] private Button _startButton;
		[SerializeField] private TMP_Text _startLabel;
		[SerializeField] private TMP_Text _sessionInfo;
		[SerializeField] private int count = 0;
		[SerializeField] private int ready = 0;

		private float _sessionRefresh;
		private App _app;

		private void Awake()
		{
			Instance = this;
			_app = App.FindInstance();
			_app.GetPlayer()?.RPC_SetIsReady(false);

			////TESTING
			//if (_app.IsHostMode())
			//{
			//	_app.GetPlayer()?.RPC_SetIsReady(true);
			//	OnStart();
			//}
			////TESTING
		}

		void Update()
		{
			count = 0;
			ready = 0;
			_playerGrid.BeginUpdate();
			_app.ForEachPlayer(ply =>
			{
				_playerGrid.AddRow(_playerListItemPrefab, item => item.Setup(ply));
				count++;
				if (ply.Ready)
					ready++;
			});

			string wait = null;
			if (count > 0)
			{
				if (ready < count)
					wait = $"NumReady: {ready}, TotalPlayers: {count}";//$"Waiting for {count - ready} of {count} players";
				else
				{
					if (!_app.IsSessionOwner)
					{
						wait = "Waiting for session owner to start";
					}

					if (_app.IsBatchMode())
					{
						OnStart();
					}

                    if (_app.IsHostMode())
                    {
						OnStart();
                    }
                    else
                    {
						//Server Mode Test
						Debug.Log("Starting Server mode via game interface");
						OnStart();
                    }
				}
            }
            else
            {
				wait = "Waiting for players to join server";
				Debug.Log(wait);
			}
			_startButton.enabled = wait==null;
			_startLabel.text = wait ?? "Start";
	  
			_playerGrid.EndUpdate();

			if (_sessionRefresh <= 0)
			{
				UpdateSessionInfo();
				_sessionRefresh = 2.0f;
			}
			_sessionRefresh -= Time.deltaTime;
		}

		public void OnStart()
		{
			SessionProps props = _app.Session.Props;
			_app.Session.LoadMap(props.StartMap);
		}

		public void OnDisconnect()
		{
			_app.Disconnect();
		}

		private void UpdateSessionInfo()
		{
			Session s = _app.Session;
			StringBuilder sb = new StringBuilder();
			if (s != null)
			{
				sb.AppendLine($"Session Name: {s.Info.Name}");
				sb.AppendLine($"Region: {s.Info.Region}");
				sb.AppendLine($"Game Type: {s.Props.PlayMode}");
				sb.AppendLine($"Map: {s.Props.StartMap}");
			}
			_sessionInfo.text = sb.ToString();
		}

		public Intro.PlayerSetupPanel GetPlayerSetup()
        {
			return m_PlayerSetupPanel;
        }
	}
}
