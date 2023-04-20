using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHumanoidTestControl : MonoBehaviour
{
    public Animator anim;
    public Transform targetFootL;
    public Transform targetFootR;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if (!anim) return;
        SetAnimatorIKValues(AvatarIKGoal.LeftFoot, targetFootL);
        SetAnimatorIKValues(AvatarIKGoal.RightFoot, targetFootR);
    }

    private void SetAnimatorIKValues(AvatarIKGoal avatarIKGoal, Transform target)
    {
        anim.SetIKPositionWeight(avatarIKGoal, 1);
        anim.SetIKRotationWeight(avatarIKGoal, 1);
        anim.SetIKPosition(avatarIKGoal, target.position);
        anim.SetIKPosition(avatarIKGoal, target.position);
    }
}
