using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameLogicManager : SimulationBehaviour
{
    public static GameLogicManager Instance;
    [Networked] public TickTimer m_stormTimer { get; set; }
    public bool GameIsRunning => m_gameIsRunning;
    [SerializeField] private bool m_gameIsRunning;
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
        if (!m_app.AllowInput) return;
        if (m_gameIsRunning) return;

        m_gameIsRunning = true;
        m_stormTimer = TickTimer.CreateFromSeconds(Runner, 10);
        //m_map.SetCountDownText(m_stormTimer.);
        //Setup Game for Starting Conditions i.e) players to respawn into spawn spots, Count Down Starts, and Rules for K/D etc.. apply

    }

    //We want the Timer logic to be as such:
    //1) Display GameStateLabel to Initiate Storm (Center Screen text: Storm Starting in 30 seconds)
    //  -> Meanwhile the CountDownLabel (Counter Text: 30, 29,...)
    //2) Display GameStateLabel Storm Closing state (Center Screen text: Storm is closing!)
    //3) Display GameStateLabel Storm Count Down 

    public void StartStormPhase(int timeStartStorm, int timeStormClose)
    {
        m_stormTimer = TickTimer.CreateFromSeconds(Runner, timeStartStorm);


    }
}
