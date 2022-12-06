using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum RotationAxes { MouseX = 1, MouseY = 2 }

public class CharacterCamera : NetworkBehaviour
{
    [SerializeField] private NetworkTransform simulatedCameraTransform;
    [SerializeField] private Character m_character;
    private CharacterMoveComponent m_characterMoveComponent;
    private Camera m_camera;

    public RotationAxes axes = RotationAxes.MouseX;
    public bool invertY = false;

    public float FieldOfView => m_camera.fieldOfView;
    public float sensitivityX = 10F;
    public float sensitivityY = 10F;

    public float minimumX = -60F;
    public float maximumX = 60F;

    public float minimumY = -80F;
    public float maximumY = 80F;

    [SerializeField] private float rotationX = 0F;
    [SerializeField] [Networked] public float NetworkedRotationY { get; set; }
    [SerializeField] [Networked] public Vector3 NetworkedPosition { get; set; }
    [SerializeField] [Networked(OnChanged = nameof(UpdateNetworkedForward))] public Vector3 NetworkedForward { get; set; }
    [SerializeField] [Networked] public Vector3 NetworkedForwardRotation { get; set; }
    public Vector3 CurrentForward;

    private List<float> rotArrayX = new List<float>();
    public float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    public float rotAverageY = 0F;

    public float framesOfSmoothing = 1;

    Quaternion originalRotation;

    private bool m_initialized;
    private App m_app;
      
    public void Initialize(Character character)
    {
        if (Object.HasInputAuthority)
        {
            GetComponent<Camera>().enabled = true;
            GetComponent<AudioListener>().enabled = true;
        }
        m_camera = GetComponent<Camera>();
        m_camera.fieldOfView = 70f;
        originalRotation = transform.localRotation;
        m_initialized = true;
        m_character = character;
        m_characterMoveComponent = m_character.GetComponent<CharacterMoveComponent>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_character) return;
        if (!m_initialized) return;

        if (m_character.Player && m_character.Player.InputEnabled && GetInput(out InputData data))
        {
            rotAverageY = 0f;
            
            float invertFlag = 1f;
            if (invertY)
            {
                invertFlag = -1f;
            }

            NetworkedRotationY += data.aimDirection.y * (m_characterMoveComponent.m_moveData.V_MouseSensitivity / 3f) * 0.1f * Time.timeScale;// m_character.GetAimDirection().y;// Input.GetAxis("Mouse Y") * (50 / 3f) * 0.2f * invertFlag * Time.timeScale;
            
            NetworkedRotationY = Mathf.Clamp(NetworkedRotationY, minimumY, maximumY);
            NetworkedForward = transform.forward;
            //NetworkedPosition = transform.position;
            //rotArrayY.Add(rotationY);
            rotArrayY.Add(NetworkedRotationY);

            if (rotArrayY.Count > framesOfSmoothing)
            {
                rotArrayY.RemoveAt(0);
            }
            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            rotAverageY /= rotArrayY.Count;
            rotAverageY = framesOfSmoothing > 0 ? rotAverageY / rotArrayY.Count : NetworkedRotationY;

            NetworkedForwardRotation = originalRotation * Quaternion.AngleAxis(rotAverageX, Vector3.up) * Quaternion.AngleAxis(rotAverageY, Vector3.left) * m_character.transform.forward;

            simulatedCameraTransform.Transform.position = transform.position;
            simulatedCameraTransform.Transform.rotation = transform.rotation;
        }
    }

    //private void LateUpdate()
    public override void Render()
    {
        if (!m_character) return;
        if (!m_initialized) return;

        if (axes == RotationAxes.MouseX)
        {
            rotAverageX = 0f;

            rotationX += m_character.GetAimDirection().x;// Input.GetAxis("Mouse X") * (50 / 30f) * Time.timeScale;

            rotArrayX.Add(rotationX);

            if (rotArrayX.Count >= framesOfSmoothing)
            {
                rotArrayX.RemoveAt(0);
            }
            for (int i = 0; i < rotArrayX.Count; i++)
            {
                rotAverageX += rotArrayX[i];
            }
            rotAverageX /= rotArrayX.Count;
            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        else
        {

            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            transform.localRotation = originalRotation * yQuaternion;
            //NetworkedPosition = transform.position;
        }
    }

    public void LateUpdate()
    {
        NetworkedPosition = transform.position;
    }

    public void SetSensitivity(float s)
    {
        sensitivityX = s;
        sensitivityY = s;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    public float GetCameraRotationY()
    {
        return NetworkedRotationY;
    }

    public static void UpdateNetworkedForward(Changed<CharacterCamera> changed)
    {
        changed.LoadNew();
        changed.Behaviour.SetCurrentForward(changed.Behaviour.NetworkedForward);
    }

    private void SetCurrentForward(Vector3 forward)
    {
        CurrentForward = forward;
    }

    public Vector3 GetSimulatedCameraForward()
    {
        return simulatedCameraTransform.Transform.forward;
    }
}
