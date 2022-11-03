using System.Collections.Generic;
using Fusion;
using UIComponents;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;

namespace GameUI.Intro
{
	public class SessionListPanel : MonoBehaviour
	{
		[SerializeField] private Button m_buttonQuickPlay;
		[SerializeField] private GridBuilder _sessionGrid;
		[SerializeField] private SessionListItem _sessionListItemPrefab;
		[SerializeField] private Text _error;
		[SerializeField] private ScrollRect m_scrollView;
		[SerializeField] private Button m_buttonRefreshServerList;
		[SerializeField] private LoadingScreenViewController _newSessionPanel = null;

		//private Dictionary<string, SessionListItem> m_sessionDict;
		private Dictionary<string, (SessionListItem, float)> m_sessionDict;
		private PlayMode _playMode;
		private App _app;
		private int m_sessionCount;


        private void Awake()
        {
            for(int i = 0; i < m_scrollView.content.childCount; i++)
            {
				Debug.Log($"Destroying Child from scrollView");
				Destroy(m_scrollView.content.GetChild(i).gameObject);
            }
			m_sessionCount = 0;
			m_buttonRefreshServerList.onClick.AddListener(async () => { await RefreshServerList(); });
		}

        public async void Show(PlayMode mode)
		{
			gameObject.SetActive(true);
			_playMode = mode;
			_error.text = "";
			_app = App.FindInstance();
			OnSessionListUpdated(new List<SessionInfo>());
			await _app.EnterLobby($"GameMode{mode}", OnSessionListUpdated);


			if (_app.IsBatchMode())
				OnShowNewSessionUI();
		}

		public void Hide()
		{
			_app?.Disconnect();
			gameObject.SetActive(false);
		}

		public void OnSessionListUpdated(List<SessionInfo> sessions)
		{
			//_sessionGrid.BeginUpdate();
			float curY = 0;
			float itemHeight = 50 + 10;//10=buffer
			
			if (m_sessionDict == null) m_sessionDict = new Dictionary<string, (SessionListItem, float)>();
			
			//Adding all sessions to a dictionary by sessionInfo name.
			if (sessions != null)
            {
				Debug.Log($"Session Info Coming: {sessions.Count}");
				foreach (SessionInfo info in sessions)
				{
					SessionProps props = new SessionProps(info.Properties);
					if (props.PlayMode != _playMode) return;

					if (m_sessionDict.ContainsKey(info.Name))
                    {
						m_sessionDict[info.Name].Item1.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, m_sessionDict[info.Name].Item2);
					}
					else
					{
						SessionListItem sessionListItem = Instantiate(_sessionListItemPrefab, m_scrollView.content);
						curY = (m_sessionDict.Values.Count * -itemHeight);
						sessionListItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, curY);
						sessionListItem.Setup(info, (selectedSession) => {
							_app.JoinSession(selectedSession);
			
						});
						m_sessionDict.Add(info.Name, (sessionListItem, curY));
						Debug.Log($"Adding {info.Name} sesison @ {m_sessionDict[info.Name].Item2}");
					}
					//curY -= itemHeight;
				}
            }
			else
			{
				Hide();
				_error.text = "Failed to join lobby";
			}
			//_sessionGrid.EndUpdate();
		}

		private Dictionary<float, (string, SessionListItem)> RemoveFromDictAt(Dictionary<float, (string, SessionListItem)> dict, float key)
		{
			(string, SessionListItem) sessionListObj;
			dict.Remove(key, out sessionListObj);
			Debug.Log($"Removing {sessionListObj.Item1} from dictionary");
			Destroy(sessionListObj.Item2);

			return dict;
		}
		private async Task RefreshServerList()
        {
			m_sessionDict.Clear();
			for (int i = 0; i < m_scrollView.content.childCount; i++)
			{
				Destroy(m_scrollView.content.GetChild(i).gameObject);
			}
			OnSessionListUpdated(new List<SessionInfo>());
			await _app.EnterLobby($"GameMode{_playMode}", OnSessionListUpdated);
		}

		public void OnShowNewSessionUI()
		{
			_newSessionPanel.Show(_playMode);
		}
	}
}