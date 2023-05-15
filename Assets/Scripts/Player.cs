using Fusion;
using System;
using UnityEngine;

/// <summary>
/// Player is a network object that represents a players core data. One instance is spawned
/// for each player when the game session starts and it lives until the session ends.
/// This is not the visual representation of the player.
/// </summary>

[OrderBefore(typeof(Character))]
public class Player : NetworkBehaviour
{
	[SerializeField] public Character[] m_characterPrefabs;
	[SerializeField] public Character CharacterPrefab;

	[Networked] public NetworkString<_32> Name { get; set; }
	[Networked] public Color Color { get; set; }
	[Networked] public NetworkBool Ready { get; set; }
	[Networked] public NetworkBool DoneLoading { get; set; }
	[Networked] public int NetworkedCharacterIndex { get; set; }

	public bool InputEnabled => _app?.AllowInput ?? false;

	[Networked] public Character NetworkedCharacter { get; set; }
	private App _app;
	public App app => _app;
	private bool initPlayerSelection;

	public override void Spawned()
	{
		_app = App.FindInstance();

		// Make sure we go down with the runner and that we're not destroyed onload unless the runner is!
		transform.SetParent(Runner.gameObject.transform);

		if (!(Runner.IsClient || _app.IsHostMode())) return;
		if (this.Id == _app.GetPlayer().Id)
		{
			RPC_SetCharacterIndex(_app.CharacterSelectionIndex);
		}
	}

	public void SetPlayerPrefab(int characterIndex)
    {
		Debug.Log($"Character List Length: {m_characterPrefabs.Length}, looking for index: {characterIndex}");
		if (characterIndex > m_characterPrefabs.Length - 1 || characterIndex < 0) return;
		CharacterPrefab = m_characterPrefabs[characterIndex];
	}

	public override void FixedUpdateNetwork()
	{
		if (_app == null) return;

		_app.ForEachPlayer(ply =>
		{
			if (ply.Runner.IsClient || _app.IsHostMode())
			{
				if (ply.Id == _app.GetPlayer().Id && _app.GetPlayer().Runner != ply)
				{
					if (!initPlayerSelection)
					{
						Debug.Log($"Setting Confirmed Player Selection, app.CharacterSelectionIndex: {_app.CharacterSelectionIndex}");
						ply.RPC_SetCharacterIndex(_app.CharacterSelectionIndex);
						initPlayerSelection = true;
					}
				}
            }
		});

		if (CharacterPrefab != null && HasStateAuthority && NetworkedCharacter == null && _app != null && _app.Session != null && _app.Session.Map)
		{
			Debug.Log($"Creating Player {Name}, with InputAuthority ?= {Object.InputAuthority}");
			Transform t = _app.Session.Map.GetSpawnPoint(Object.InputAuthority);


			var c = Runner.Spawn(CharacterPrefab, t.position, t.rotation, Object.InputAuthority, (runner, o) =>
			{
				NetworkedCharacter = o.GetComponent<Character>();
				NetworkedCharacter.Player = this;
			});
		}
	}
	
	public void Despawn()
	{
		if (HasStateAuthority)
		{
			if (NetworkedCharacter != null)
			{
				Runner.Despawn(NetworkedCharacter.Object);
				NetworkedCharacter = null;
			}
			Runner.Despawn(Object);
		}
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void RPC_SetIsReady(NetworkBool ready)
	{
		Ready = ready;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void RPC_SetName(NetworkString<_32> name)
	{
		Name = name;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void RPC_SetColor(Color color)
	{
		Color = color;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void RPC_SetCharacterIndex(int characterIndex)
	{
		NetworkedCharacterIndex = characterIndex;
		SetPlayerPrefab(NetworkedCharacterIndex);
		Debug.Log($"Character Spawn should be: {m_characterPrefabs[NetworkedCharacterIndex].name}. InputSelection: {_app.CharacterSelectionIndex}, Result: {NetworkedCharacterIndex}");
	}

}