using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : NetworkBehaviour
{
    //Networked Variables
    [SerializeField] [Networked] public float NetworkedRotationY { get; set; }
    [SerializeField] [Networked] public Vector3 NetworkedPosition { get; set; }

    //Public Variables
    public float FieldOfView => m_camera.fieldOfView;
    public float MinimumY => m_minimumY;
    public float MaximumY => m_maximumY;

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

        if (m_character.Player && m_character.Player.InputEnabled && GetInput(out InputData data))
        {
            rotAverageY = 0f;
            
            NetworkedRotationY += data.aimDirection.y * (m_characterMoveComponent.m_moveData.V_MouseSensitivity * 1f) * Time.timeScale;
            
            NetworkedRotationY = Mathf.Clamp(NetworkedRotationY, m_minimumY, m_maximumY);
            rotArrayY.Add(NetworkedRotationY);

            if (rotArrayY.Count > m_framesOfSmoothing)
                rotArrayY.RemoveAt(0);

            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            rotAverageY /= rotArrayY.Count;
            rotAverageY = m_framesOfSmoothing > 0 ? rotAverageY / rotArrayY.Count : NetworkedRotationY;
        }
    }

    public override void Render()
    {
        if (!m_character) return;
        if (!m_initialized) return;

        Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
        transform.localRotation = originalRotation * yQuaternion;
        //NetworkedPosition = transform.position;
    }

    public void LateUpdate()
    {
        NetworkedPosition = transform.position;
    }

    public float GetCameraRotationY()
    {
        return NetworkedRotationY;
    }
}