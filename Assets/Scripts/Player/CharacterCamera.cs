using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum RotationAxes { MouseX = 1, MouseY = 2 }

public class CharacterCamera : MonoBehaviour
{
    [SerializeField] private Character m_character;

    public RotationAxes axes = RotationAxes.MouseX;
    public bool invertY = false;

    public float sensitivityX = 10F;
    public float sensitivityY = 10F;

    public float minimumX = -60F;
    public float maximumX = 60F;

    public float minimumY = -80F;
    public float maximumY = 80F;

    [SerializeField] private float rotationX = 0F;
    [SerializeField] private float rotationY = 0F;

    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;

    public float framesOfSmoothing = 1;

    Quaternion originalRotation;

    private bool m_initialized;

    public void Initialize(Character character)
    {
        GetComponent<Camera>().enabled = true;
        GetComponent<AudioListener>().enabled = true;
        originalRotation = transform.localRotation;
        m_initialized = true;
        m_character = character;
    }

    private void LateUpdate()
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
            rotAverageY = 0f;

            float invertFlag = 1f;
            if (invertY)
            {
                invertFlag = -1f;
            }
            rotationY += m_character.GetAimDirection().y;// Input.GetAxis("Mouse Y") * (50 / 3f) * 0.2f * invertFlag * Time.timeScale;

            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            rotArrayY.Add(rotationY);

            if (rotArrayY.Count >= framesOfSmoothing)
            {
                rotArrayY.RemoveAt(0);
            }
            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            rotAverageY /= rotArrayY.Count;

            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            transform.localRotation = originalRotation * yQuaternion;
        }
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
        return rotationY;
    }
}
