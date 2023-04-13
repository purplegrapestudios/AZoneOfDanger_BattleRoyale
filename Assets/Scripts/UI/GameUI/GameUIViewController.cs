using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIViewController : MonoBehaviour
{
    public static GameUIViewController Instance;
    [SerializeField] private TMP_Text m_healthTxt;
    [SerializeField] private TMP_Text m_killsTxt;
    [SerializeField] private TMP_Text m_deathsTxt;
    [SerializeField] private TMP_Text m_ammoInClipTxt;
    [SerializeField] private TMP_Text m_clipSizeTxt;
    [SerializeField] private TMP_Text m_ammoRemainingTxt;

    [SerializeField] private TMP_Text m_gameStateLabel;
    [SerializeField] private TMP_Text m_timerToNextStateTxt;
    [SerializeField] private TMP_Text m_timerTxt;
    [SerializeField] private GameObject m_spectatePlayerOptions;
    [SerializeField] private GameObject m_crosshairObject;
    private Crosshair m_crosshair;
    private App m_app;
    private float m_tickRate;
    private System.Action<bool> m_enableSpectateOptionsCallback;

    private void Awake()
    {
        Instance = this;
        m_crosshair = m_crosshairObject.GetComponent<Crosshair>();
    }

    public void InitSpectatePlayerButtons(App app)
    {
        m_spectatePlayerOptions.GetComponent<SpectateOptions>().Init(app, GameLogicManager.Instance, Instance, SceneCamera.Instance);
    }

    public void SetCallback(System.Action<bool> action)
    {
        m_enableSpectateOptionsCallback = action;
    }

    public void UpdateHealthText(string value) => m_healthTxt.text = value;
    public void UpdateKillsText(string value) => m_killsTxt.text = value;
    public void UpdateDeathsText(string value) => m_deathsTxt.text = value;

    public void SetCrosshairActive(bool val)
    {
        m_crosshairObject.SetActive(val);
    }

    public Crosshair GetCrosshair()
    {
        return m_crosshair;
    }

    public void SetAmmoInfo(bool hasInputAuthority, int ammoInClip, int ammoRemaining, int clipSize)
    {
        if (!hasInputAuthority) return;
        m_ammoInClipTxt.text = ammoInClip.ToString();
        m_ammoRemainingTxt.text = ammoRemaining.ToString();
        m_clipSizeTxt.text = clipSize.ToString();
    }
    public void InitGameStateLabel(string txt)
    {
        m_gameStateLabel.gameObject.SetActive(true);
        SetGameStateLabel(txt);
    }

    public void SetGameStateLabel(string txt)
    {
        m_gameStateLabel.text = txt;
    }

    public void SetGameStateTimer(string txt)
    {
        m_timerToNextStateTxt.text = txt;
    }

    public void DeactivateGameStateLabel()
    {
        m_gameStateLabel.gameObject.SetActive(false);
        SetGameStateLabel(string.Empty);
    }

    public void FixedUpdateMinimapTime(float GameStartTick)
    {
        if (!m_app)
        {
            m_app = App.FindInstance();
            m_tickRate = 1 / m_app.Session.Runner.DeltaTime;
            return;
        }

        float timeInSecElapsed = (m_app.Session.Runner.Tick - GameStartTick) / m_tickRate;
        float minElapsed = Mathf.FloorToInt(timeInSecElapsed / 60);
        float secondsRemain = Mathf.FloorToInt(timeInSecElapsed - (minElapsed * 60));
        m_timerTxt.text = $"{(minElapsed < 10 ? "0" + minElapsed : minElapsed)}:{(secondsRemain < 10 ? "0" + secondsRemain : secondsRemain)}";

        //if (GameLogicManager.Instance.StormTimer.IsRunning)
        //    m_timerToNextStateTxt.text = Mathf.FloorToInt(GameLogicManager.Instance.StormTimer.RemainingTime(m_app.Session.Runner) ?? 0).ToString();
        //else
        //    m_timerToNextStateTxt.text = string.Empty;
    }

    public void ShowSpectatePlayerOptions(bool val)
    {
        m_enableSpectateOptionsCallback(val);
    }

}
