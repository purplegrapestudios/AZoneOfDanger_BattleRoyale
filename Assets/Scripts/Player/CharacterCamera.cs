using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : NetworkBehaviour
{
    //Networked Variables
    [SerializeField] [Networked] public float NetworkedRotationY { get; set; }
    [SerializeField] [Networked] public Vector3 NetworkedPosition { get; set; }
    [SerializeField] [Networked] public Vector3 NetworkedForwward { get; set; }

    //Public Variables
    public float FieldOfView => m_camera.fieldOfView;
    public float MinimumY => m_minimumY;
    public float MaximumY => m_maximumY;
    public Camera Camera => m_camera;
    //Serialized Variables in Inspector
    [SerializeField] private float m_framesOfSmoothing = 1;
    [SerializeField] private float m_minimumY = -80F;
    [SerializeField] private float m_maximumY = 80F;

    //Private Variables
    private Character m_character;
    private CharacterMoveComponent m_characterMoveComponent;
    private Camera m_camera;
    private List<float> rotArrayY = new List<float>();
    private float rotAverageY = 0F;
    private Quaternion originalRotation;
    private bool m_initialized;
      
    public void Initialize(Character character)
    {
        GetComponent<Camera>().enabled = Object.HasInputAuthority;
        GetComponent<AudioListener>().enabled = Object.HasInputAuthority;

        m_character = character;
        m_characterMoveComponent = m_character.GetComponent<CharacterMoveComponent>();
        m_camera = GetComponent<Camera>();

        m_camera.fieldOfView = 70f;
        originalRotation = transform.localRotation;
        m_initialized = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_character) return;
        if (!m_initialized) return;

        if (m_character.Player && m_character.Player.InputEnabled)
        {
            rotAverageY = 0f;
            
            NetworkedRotationY += m_character.FixedAimDirDelta.y * (m_characterMoveComponent.m_moveData.V_MouseSensitivity) * Time.timeScale;
            
            NetworkedRotationY = Mathf.Clamp(NetworkedRotationY, m_minimumY, m_maximumY);
            //rotArrayY.Add(NetworkedRotationY);
            //
            //if (rotArrayY.Count > m_framesOfSmoothing)
            //    rotArrayY.RemoveAt(0);
            //
            //for (int j = 0; j < rotArrayY.Count; j++)
            //{
            //    rotAverageY += rotArrayY[j];
            //}
            //rotAverageY /= rotArrayY.Count;
            //rotAverageY = m_framesOfSmoothing > 0 ? rotAverageY / rotArrayY.Count : NetworkedRotationY;
            //RotateCamera(NetworkedRotationY);
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
        transform.localRotation = originalRotation * yQuaternion;
    }

    public void LateUpdate()
    {
        //NetworkedPosition = transform.position;
        //NetworkedForwward = transform.forward;
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