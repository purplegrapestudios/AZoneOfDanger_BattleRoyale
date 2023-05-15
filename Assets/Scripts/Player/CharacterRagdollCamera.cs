using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterRagdollCamera : MonoBehaviour
{
    [SerializeField] private Transform m_lookAtTarget;
    public Camera Camera => m_camera;
    private Camera m_camera;
    public AudioListener AudioListener => m_audioListener;
    private AudioListener m_audioListener;
    private Character m_character;
    private Transform m_characterTransform;
    private Transform m_cameraTransform;
    private bool m_initialized;
    
    public void Initialize(Character character)
    {
        m_camera = GetComponent<Camera>();
        m_audioListener = GetComponent<AudioListener>();
        m_character = character;
        m_characterTransform = m_character.transform;
        m_cameraTransform = m_character.transform;
        m_camera.fieldOfView = 70f;
        m_initialized = true;
    }

//    public override void Render()
//    {
//        if (!m_initialized) return;
//        if (!m_camera.enabled) return;
//        //m_cameraTransform.LookAt(m_lookAtTarget.position + 2 * Vector3.up);
//    }
}
