using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(AudioListener))]
public class SceneCamera : MonoBehaviour
{
    public static SceneCamera instance;
    [SerializeField] private Camera m_sceneCamera;
    [SerializeField] private AudioListener m_sceneAudioListener;

    private void Awake()
    {
        instance = this;
    }
    public void SetSceneCameraActive(bool val)
    {
        m_sceneCamera.enabled = val;
        m_sceneAudioListener.enabled = val;
        m_sceneCamera.gameObject.SetActive(false);
    }
}
