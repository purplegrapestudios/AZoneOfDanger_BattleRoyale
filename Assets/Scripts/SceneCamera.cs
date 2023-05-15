using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(AudioListener))]
public class SceneCamera : MonoBehaviour
{
    public static SceneCamera Instance;
    [SerializeField] private Camera m_sceneCamera;
    [SerializeField] private AudioListener m_sceneAudioListener;
    public Transform SpectateCamTr => m_spectateCamTr;
    [SerializeField] private Transform m_spectateCamTr;
    private System.Action<string> m_spectatePlayerLabelCallback;
    private App m_app;

    private void Awake()
    {
        Instance = this;
    }
    public void SetSceneCameraActive(bool val)
    {
        if (m_app.IsServerMode()) return;
        m_sceneCamera.enabled = val;
        m_sceneAudioListener.enabled = val;

        //m_sceneCamera.gameObject.SetActive(false);
    }

    public void SetSpectateCamTransform(Transform spectateCamTr, string playerNameOrID)
    {
        m_spectateCamTr = spectateCamTr;
        m_spectatePlayerLabelCallback(playerNameOrID);
    }

    public void SetSpectatePlayerLabelCallback(System.Action<string> callback)
    {
        m_spectatePlayerLabelCallback = callback;
    }

    private void LateUpdate()
    {
        if (!m_app)
        {
            m_app = App.FindInstance();
            return;
        }
        if (!m_app.AllowInput) return;

        if (!GameLogicManager.Instance.Initialized) return;
        if (!GameLogicManager.Instance.NetworkedGameIsRunning) return;
        if (!m_sceneCamera.enabled) return;

        if (!m_spectateCamTr) { GameLogicManager.Instance.GetSpectatePlayerNext(m_app.GetPlayer()); }
        m_sceneCamera.transform.position = m_spectateCamTr.position;
        m_sceneCamera.transform.rotation = m_spectateCamTr.rotation;
    }
}
