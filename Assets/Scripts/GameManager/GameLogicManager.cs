using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameLogicManager : SimulationBehaviour
{
    public static GameLogicManager Instance;
    [Networked] public TickTimer StormTimer { get; set; }
    public bool GameIsRunning => m_gameIsRunning;
    [SerializeField] private bool m_gameIsRunning;
    [Networked] public int NetworkedGameStartTick { get; set; }

    public int MinPlayersToStart => m_minPlayersToStart;
    [SerializeField] private int m_minPlayersToStart = 2;
    private App m_app;
    private Map m_map;



    private void Awake()
    {
        Instance = this;
        m_app = App.FindInstance();
        m_map = m_app.Session.Map;
    }

    public void StartGameLogic()
    {
        if (m_gameIsRunning) return;
        //if (m_app.Session.Info.PlayerCount - (m_app.IsServerMode() ? 0 : 0) < m_minPlayersToStart) return;

        NetworkedGameStartTick = m_app.Session.Runner.Tick;
        m_gameIsRunning = true;
        //GameUIViewController.S
        //m_map.SetCountDownText(m_stormTimer.);
        //Setup Game for Starting Conditions i.e) players to respawn into spawn spots, Count Down Starts, and Rules for K/D etc.. apply

    }

    public override void FixedUpdateNetwork()
    {
        if (!m_gameIsRunning) return;
        GameUIViewController.Instance.FixedUpdateMinimapTime();
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
    //Display Winner
    //Display Game Stats


    public bool StartStormPhase(int timeStartStorm, int timeStormClose)
    {
        if (StormTimer.IsRunning) return false;

        StormTimer = TickTimer.CreateFromSeconds(Runner, timeStartStorm);
        return true;
    }
}
