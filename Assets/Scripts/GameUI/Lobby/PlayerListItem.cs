using UIComponents;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI.Lobby
{
	public class PlayerListItem : GridCell
	{
		[SerializeField] private TMP_Text _name;
		[SerializeField] private Image _color;
		[SerializeField] private GameObject _ready;

		public void Setup(Player ply)
		{
			_name.text = string.IsNullOrEmpty(ply.Name.Value) ? "no name" : ply.Name.Value;
			_color.color = ply.Color;
			_ready.SetActive(ply.Ready);
		}
	}
}