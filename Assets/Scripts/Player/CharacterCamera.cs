using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : NetworkBehaviour
{
    [Networked] public float NetworkedRotationY { get; set; }
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Vector3 NetworkedForwward { get; set; }

    public float FieldOfView => m_camera.fieldOfView;
    public float MinimumY => m_minimumY;
    [SerializeField] private float m_minimumY = -80F;
    public float MaximumY => m_maximumY;
    [SerializeField] private float m_maximumY = 80F;
    public Camera Camera => m_camera;

    private Character m_character;
    private CharacterMoveComponent m_characterMoveComponent;
    private Camera m_camera;
    private Quaternion m_originalRotation;
    private bool m_initialized;
      
    public void Initialize(Character character)
    {
        GetComponent<Camera>().enabled = Object.HasInputAuthority;
        GetComponent<AudioListener>().enabled = Object.HasInputAuthority;

        m_character = character;
        m_characterMoveComponent = m_character.GetComponent<CharacterMoveComponent>();
        m_camera = GetComponent<Camera>();

        m_camera.fieldOfView = 70f;
        m_originalRotation = transform.localRotation;
        m_initialized = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_character) return;
        if (!m_initialized) return;

        if (m_character.Player && m_character.Player.InputEnabled)
        {
            NetworkedRotationY += m_character.FixedAimDirDelta.y * (m_characterMoveComponent.m_moveData.V_MouseSensitivity) * Time.timeScale;
            NetworkedRotationY = Mathf.Clamp(NetworkedRotationY, m_minimumY, m_maximumY);
        }
        NetworkedPosition = transform.position;
        NetworkedForwward = transform.forward;
    }

    public override void Render()
    {
        if (!m_character) return;
        if (!m_initialized) return;
        //float renderRotationY = Mathf.Clamp((NetworkedRotationY + m_character.CachedAimDirDelta.y * m_characterMoveComponent.m_moveData.V_MouseSensitivity * Time.timeScale), m_minimumY, m_maximumY);
        //float lerpedRotationY = Mathf.Lerp(NetworkedRotationY, renderRotationY, Time.deltaTime);

        RotateCamera(NetworkedRotationY + m_character.CachedAimDirDelta.y);
    }

    private void RotateCamera(float rotationAlongX)
    {
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationAlongX, Vector3.left);
        transform.localRotation = m_originalRotation * yQuaternion;
    }

    public float GetCameraRotationY()
    {
        return NetworkedRotationY;
    }

    public void SetCameraFOV(float fieldOfView)
    {
        m_camera.fieldOfView = fieldOfView;
    }
}