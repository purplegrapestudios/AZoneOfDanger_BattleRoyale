using System;
using Fusion;
using UIComponents;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace GameUI.Intro
{
	public class SessionListItem : GridCell
	{
		[SerializeField] private TMP_Text _name;
		[SerializeField] private TMP_Text _map;
		[SerializeField] private TMP_Text _players;
		[SerializeField] private Button m_joinButton;

		private Action<SessionInfo> _onJoin;
		private SessionInfo _info;

		public void Setup(SessionInfo info, Action<SessionInfo> onJoin)
		{
			_info = info;
			_name.text = $"{info.Name} ({info.Region})";
			_map.text = $"Map {new SessionProps(info.Properties).StartMap}";
			_players.text = $"{info.PlayerCount - 1}/{info.MaxPlayers - 1}"; //Subtracting 1 if we do not want to count the server.
			_onJoin = onJoin;
		}
		
		public void OnJoin()
		{
			_onJoin(_info);
		}

		public void InitJoinButton(UnityAction callback)
        {
			m_joinButton.onClick.AddListener(callback);
        }

		public string GetSessionInfoName()
        {
			return _info.Name;
        }
	}
}