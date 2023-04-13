using UnityEngine;
using Fusion;

public class GameLogicManager : NetworkBehaviour
{
    public static GameLogicManager Instance;
    [Networked] public NetworkBool NetworkedGameIsRunning { get; set; }
    [Networked] public int NetworkedGameStartTick { get; set; }
    [Networked] public NetworkBool NetworkedRespawnAllowed { get; set; }
    public int MinPlayersToStart => m_minPlayersToStart;
    [SerializeField] private int m_minPlayersToStart = 2;
    private App m_app;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        m_app = App.FindInstance();
    }

    public void StartGameLogic()
    {
        //if (m_app.Session.Info.PlayerCount - (m_app.IsServerMode() ? 0 : 0) < m_minPlayersToStart) return;
        if (!Runner.IsServer) return;
        if (NetworkedGameIsRunning) return;

        NetworkedGameStartTick = m_app.Session.Runner.Tick;
        NetworkedGameIsRunning = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_app.AllowInput) return;
        if (!NetworkedGameIsRunning) return;
        GameUIViewController.Instance.FixedUpdateMinimapTime(NetworkedGameStartTick);
        NetworkedRespawnAllowed = !StormBehavior.Instance.IsStackingPhaseComplete();
    }

    //Initiate the Start Battle Royal Match (Requirement Say 10 -> To make it so you can Press Return to Force Start > 1 player):
    // - All players respawn at random positions (Ideally a drop off but just respawn for now)
    // - Display GameStateLabel (Center Screen text: 3,2,1, Begin)

    //We want the Timer logic to be as such: (30 seconds wait + 30 seconds closing = 1 min)
    //1) Display GameStateLabel to Initiate Storm (Center Screen text: Storm Starting in 30 seconds)
    //  -> Meanwhile the CountDownLabel (Counter Text: 30, 29,...)
    //2) Display GameStateLabel Storm Closing state (Center Screen text: Storm is closing!)
    //3) Display GameStateLabel Storm Count Down (Counter Text: 30, 29,...)

    //Store Circle Position / Size Logic. (7 Zone sizes each 1 minute = 7 Total Minutes) + Say the first 3 minutes of ffa
    //1) Position: Storm Circle must be within game map bounds. (Origin + radius) must be within bounds.
    //2) Size: Storm Size is 100% -> 75% -> 50% -> 25% -> 10% -> 5% -> 0%

    //End Game Condition
    //When 1 player remains
    //Show Winner Cam
    //Winner Gets UI display of Victory Royale
    //Winner Display Game Stats

    //Those who die early (and in the first phase of the game with respawns) Create a Spectate Cam, starting at last instigator. And toggle back / forward throughout entire playerlist
    //Also show stats

    //Server waits like 15 seconds for Winner to check his stats etc, and the ShutsDown the Fusion Network, and finally return back to Starting Scene.

    //UI Minimap: Players remaining.


    //StormBehavior
    //Damage Players who are not within the zone. So it'll need to hold list of all players, which say the App can store for now.
}