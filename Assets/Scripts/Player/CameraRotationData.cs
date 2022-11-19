using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationData : NetworkBehaviour
{

    public void SetLocalRotation(Quaternion q)
    {
        transform.localRotation = q;
    }

    public float GetRotationX()
    {
        Debug.Log($"Rot: {-transform.localRotation.x * 124.4574f / 80f}");
        return -transform.localRotation.x;
    }
}
