using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The Map represents the network aspects of a game scene. It is itself spawned as part of the scene
/// and delegates game-scene specific logic as well as handles spawning of player avatars
/// </summary>
public class Map : SimulationBehaviour, ISpawned
{
	[SerializeField] private Transform[] _spawnPoints;
	private bool _sendMapLoadedMessage;
	private App _app;

	public void Spawned()
	{
		Debug.Log("Map spawned");
		_sendMapLoadedMessage = true;
		_app = App.FindInstance();
	}
	
	public override void FixedUpdateNetwork()
	{
		Session session = _app.Session;
		if (session.Object == null || !session.Object.IsValid)
			return;
		if (_sendMapLoadedMessage)
		{
			// Tell the master that we're done loading and set the sessions map so the rest of the game know that we are ready
			Debug.Log("Finished loading");
			RpcInvokeInfo invokeinfo = _app.Session.RPC_FinishedLoading(Runner.LocalPlayer);
			Debug.Log($"RPC returned {invokeinfo}");
			if ((invokeinfo.LocalInvokeResult == RpcLocalInvokeResult.Invoked) || (invokeinfo.SendResult.Result & RpcSendMessageResult.MaskSent) != 0)
			{
				_app.Session.Map = this;
				GameUIViewController.Instance.SetCrosshairActive(true);
				_sendMapLoadedMessage = false;
			}
			else
				Debug.Log($"RPC failed trying again later");
		}
		if (!session.PostLoadCountDown.Expired(Runner))
			GameUIViewController.Instance.SetGameStateLabel($"Battle Begins In {Mathf.CeilToInt(session.PostLoadCountDown.RemainingTime(Runner) ?? 0)}");
		else
		{
			//To see if the idle state of the server app will be efficient enough to remain under the AWS server burst threshold.
			//If wait 5 seconds and playercount > kminStartPlayerCount=10, start game
			//otherwise keep scanning
			if(_app.Session.Info.PlayerCount > GameLogicManager.Instance.kMinStartPlayerCount || GameLogicManager.Instance.NetworkedForceStart)
            {
				if (_app.Session.Runner.IsServer)
				{
					Time.fixedDeltaTime = 1 / 60;
					_app.Session.Runner.Simulation.Config.ClientPacketInterval = 1;
					_app.Session.Runner.Simulation.Config.ServerPacketInterval = 1;
					GameLogicManager.Instance.StartGameLogic();
				}
				_app.AllowInput = true;
			}

			if (_app.Session.Runner.IsServer)
			{
				Time.fixedDeltaTime = 1 / 6;
				_app.Session.Runner.Simulation.Config.ClientPacketInterval = 10;
				_app.Session.Runner.Simulation.Config.ServerPacketInterval = 10;
			}
			//While starting game condition Not met & No Players other than server in game,
			//Server will make TickRate = 3, FixedDeltaTime = 1/3, TargetFrameRate = 1/3
			//else TickRate = 100, FixedDeltaTime = 1/60, TargetFrameRate = 144

			//GameLogicManager.Instance.StartGameLogic();
		}
	}

	/// <summary>
	/// UI hooks
	/// </summary>

	public void OnDisconnect()
	{
		_app.Disconnect();
	}

	public void OnLoadMap1()
	{
		_app.Session.LoadMap(MapIndex.Map1);
	}

	public void OnLoadMap(int mapIndex)
	{
		_app.Session.LoadMap((MapIndex) mapIndex);
	}

	public void OnGameOver()
	{
		_app.Session.LoadMap(MapIndex.GameOver);
	}

	public Transform GetSpawnPoint(PlayerRef objectInputAuthority)
	{
		// Note: This only works if the number of spawnpoints in the map matches the maximum number of players - otherwise there's a risk of spawning multiple players in the same location.
		return _spawnPoints[((int) objectInputAuthority) % _spawnPoints.Length];
	}

}