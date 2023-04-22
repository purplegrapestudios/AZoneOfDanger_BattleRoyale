using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHumanoidTestControl : MonoBehaviour
{
    public Animator anim;
    public Transform targetFootL;
    public Transform targetFootR;
    public Transform targetHandL;
    public Transform targetHandR;

    public Transform trFootL;
    public Transform trFootR;
    public Transform Root;

    public float FootDistance = 0.5f;
    public LayerMask layerMask;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public float IKCurveFoot_L;
    public float IKCurveFoot_R;

    private void OnAnimatorIK(int layerIndex)
    {
        if (!anim) return;

        IKCurveFoot_L = anim.GetFloat("IKCurveFoot_L");
        IKCurveFoot_R = anim.GetFloat("IKCurveFoot_R");
        //SetAnimatorIKValues(AvatarIKGoal.LeftFoot, IKCurveFoot_L, IKCurveFoot_L, targetFootL);
        //SetAnimatorIKValues(AvatarIKGoal.RightFoot, IKCurveFoot_R, IKCurveFoot_R, targetFootR);
        targetFootL.position = trFootL.position;
        targetFootR.position = trFootR.position;

        targetFootL.rotation = Quaternion.Euler(leftUp);
        targetFootR.rotation = Quaternion.Euler(rightUp);
        SetAnimatorIKValues(AvatarIKGoal.LeftFoot, ikFootL, ikFootL, targetFootL);
        SetAnimatorIKValues(AvatarIKGoal.RightFoot, ikFootR, ikFootR, targetFootR);
        SetAnimatorIKValues(AvatarIKGoal.LeftHand, 1, 1, targetHandL);
        SetAnimatorIKValues(AvatarIKGoal.RightHand, 1, 1, targetHandR);
    }

    private void SetAnimatorIKValues(AvatarIKGoal avatarIKGoal, float posWeight, float rotWeight, Transform target)
    {
        anim.SetIKPositionWeight(avatarIKGoal, posWeight);
        anim.SetIKRotationWeight(avatarIKGoal, rotWeight);
        anim.SetIKPosition(avatarIKGoal, target.position);
        anim.SetIKRotation(avatarIKGoal, target.rotation);
    }


    Ray rayL, rayR;
    RaycastHit hitL, hitR;
    public float ikFootL;
    public float ikFootR;
    Vector3 leftUp;
    Vector3 rightUp;

    private void Update()
    {
        rayL = new Ray(trFootL.position, trFootL.position - Root.up);
        if (Physics.Raycast(rayL, out hitL, FootDistance, layerMask))
        {
            ikFootL = 1;
            leftUp = hitL.normal;
        }
        else
        {
            ikFootL = 0;
        }

        rayR = new Ray(trFootR.position, trFootR.position - Root.up);
        if (Physics.Raycast(rayR, out hitR, FootDistance, layerMask))
        {
            ikFootR = 1;
            rightUp = hitL.normal;
        }
        else
        {
            ikFootR = 0;
        }
    }
}