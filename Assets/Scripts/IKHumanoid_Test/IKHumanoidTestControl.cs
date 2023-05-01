using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;

enum HandPosAimEnum
{
    Upright = 0,
    UprightMove = 1,
    Crouch = 2,
    CrouchMove = 3,
}

[Serializable]
public struct HandPosAimAngleData
{

    public Vector3 Down { get; set; }
    public Vector3 Neutral { get; set; }
    public Vector3 Up { get; set; }

    public HandPosAimAngleData(Vector3 down, Vector3 neutral, Vector3 up)
    {
        Down = down;
        Neutral = neutral;
        Up = up;
    }
}

public class IKHumanoidTestControl : NetworkBehaviour
{
    public Animator anim;
    [Networked] public Vector3 NetworkedTargetFootLPos { get; set; }
    [Networked] public Vector3 NetworkedTargetFootRPos { get; set; }
    [Networked] public Quaternion NetworkedTargetFootLRot { get; set; }
    [Networked] public Quaternion NetworkedTargetFootRRot { get; set; }
    public Transform targetFootL;
    public Transform targetFootR;
    public Transform targetHandL;
    public Transform targetHandR;
    public Transform trHands;
    public Transform trFootL;
    public Transform trFootR;
    public Transform trHandL;
    public Transform trHandR;
    public Transform wepContainerFK;
    public Transform wepContainerIK;
    public Transform Root;
    public Character m_character;

    [Networked] public Vector3 NetworkedHitPointL { get; set; }
    [Networked] public Vector3 NetworkedHitPointR { get; set; }
    [Networked] public Vector3 NetworkedHitNormalL { get; set; }
    [Networked] public Vector3 NetworkedHitNormalR { get; set; }
    public float FootDistance = 0.5f;
    public LayerMask layerMask;
    public float testAimAngle;
    public int testCrouch;
    public int testJump;
    public int testDead;
    public int testReload;
    public int testSwitchWep;
    public int testFire;
    public float testSpeed;

    private bool snapHandsPos;
    [SerializeField] private HandPosAimEnum handPosEnum;
    [SerializeField] private List<HandPosAimAngleData> handsPosAimData;
    public Quaternion handsOrigRot;
    public Vector3 targetOrigPosHandL = default;
    public Vector3 targetOrigPosHandR = default;
    public Quaternion targetOrigRotHandL = default;
    public Quaternion targetOrigRotHandR = default;

    [SerializeField] private float footTargetOffset;
    public float IKCurveFoot_L;
    public float IKCurveFoot_R;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        handsOrigRot = Quaternion.Euler(0, 0, 0);
        targetOrigPosHandL = targetHandL.localPosition;
        targetOrigPosHandR = targetHandR.localPosition;

        targetOrigRotHandL = targetHandL.localRotation;
        targetOrigRotHandR = targetHandR.localRotation;
        handsPosAimData = new List<HandPosAimAngleData>();

        //Upright Hand Aim Positions
        AddHandsPosAimData(new Vector3(0.2f, 0.65f, 0.7f),
            new Vector3(0.35f, 1.5f, 0.8f),
            new Vector3(0.2f, 1.7f, 0.6f));

        //Upright Moving Hand Aim Positions
        AddHandsPosAimData(new Vector3(0.2f, 0.5f, 1.2f),
            new Vector3(0.35f, 1.5f, 0.8f),
            new Vector3(0.4f, 1.7f, 0.7f));

        //Crouched Hand Aim Positions
        AddHandsPosAimData(new Vector3(0.35f, -0.8f, 1f),
            new Vector3(0.35f, 0f, 1.1f),
            new Vector3(0.35f, 0.1f, 0.7f));

        //Crouched Moving Hand Aim Positions
        AddHandsPosAimData(new Vector3(0.5f, -0.55f, 1.2f),
            new Vector3(0.35f, 0f, 1.1f),
            new Vector3(0.35f, 1f, 0.7f));
    }

    private void AddHandsPosAimData(Vector3 down, Vector3 neutral, Vector3 up)
    {
        handsPosAimData.Add(new HandPosAimAngleData(down, neutral, up));
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if (!anim) return;

        if (layerIndex == 1)
        {
            IKCurveFoot_L = anim.GetFloat("IKCurveFoot_L");
            IKCurveFoot_R = anim.GetFloat("IKCurveFoot_R");
        }
        else
        {
            IKCurveFoot_L = 0;
            IKCurveFoot_R = 0;
        }
        //IKCurveFoot_L = anim.GetFloat("IKCurveFoot_L");
        //IKCurveFoot_R = anim.GetFloat("IKCurveFoot_R");
        //SetAnimatorIKValues(AvatarIKGoal.LeftFoot, IKCurveFoot_L, IKCurveFoot_L, targetFootL);
        //SetAnimatorIKValues(AvatarIKGoal.RightFoot, IKCurveFoot_R, IKCurveFoot_R, targetFootR);

        //Animations to make:
        // - Dead
        // - Then add FootPlacement Logic

        testAimAngle = anim.GetFloat("Param_3rdPerson_AimAngle");
        testCrouch = anim.GetInteger("Param_CrouchInt");
        testJump = anim.GetInteger("Param_JumpInt");
        testDead = anim.GetInteger("Param_DeadInt");
        testReload = anim.GetInteger("Param_ReloadInt");
        testSwitchWep = anim.GetInteger("Param_SwitchWeaponInt");
        testFire = anim.GetInteger("Param_FireInt");
        testSpeed = anim.GetFloat("Param_Speed");

        OnAnimateHands(layerIndex);
        OnAnimateFeet(layerIndex);

        //if (ikFootL == 1) targetFootL.rotation = Quaternion.Euler(leftUp);
        //if (ikFootR == 1) targetFootR.rotation = Quaternion.Euler(rightUp);
        //
        SetIKValues(AvatarIKGoal.LeftHand, targetHandL);
        SetIKValues(AvatarIKGoal.RightHand, targetHandR);
    }

    private void SetIKWeights(AvatarIKGoal avatarIKGoal, float posWeight, float rotWeight)
    {
        anim.SetIKPositionWeight(avatarIKGoal, posWeight);
        anim.SetIKRotationWeight(avatarIKGoal, rotWeight);
    }

    private void SetIKValues(AvatarIKGoal avatarIKGoal, Transform target)
    {
        anim.SetIKPosition(avatarIKGoal, target.position);
        anim.SetIKRotation(avatarIKGoal, target.rotation);
    }

    private void SetIKValues(AvatarIKGoal avatarIKGoal, NetworkPositionRotation target)
    {
        anim.SetIKPosition(avatarIKGoal, target.ReadPosition());
        anim.SetIKRotation(avatarIKGoal, target.ReadRotation());
    }

    private void SetIKValuesRaw(AvatarIKGoal avatarIKGoal, Vector3 position, Quaternion rotation)
    {
        anim.SetIKPosition(avatarIKGoal, position);
        anim.SetIKRotation(avatarIKGoal, rotation);
    }

    private void OnAnimateHands(int layerIndex)
    {
        bool useFK = (testFire == 0 && (testJump == 1 || testDead == 1 || testSwitchWep == 1 || testReload == 1));

        wepContainerIK.gameObject.SetActive(!useFK);
        wepContainerFK.gameObject.SetActive(useFK);

        if (useFK)
        {
            SetIKWeights(AvatarIKGoal.LeftHand, 0, 0);
            SetIKWeights(AvatarIKGoal.RightHand, 0, 0);
            //targetHandL.position = trHandL.position;
            //targetHandR.position = trHandR.position;
            //targetHandL.rotation = trHandL.rotation;
            //targetHandR.rotation = trHandR.rotation;

            if (testJump == 1 || testDead == 1)
            {
                //If Dealing with Foot Weights with Animation Curves, then do not need to set weights here
                SetIKWeights(AvatarIKGoal.LeftFoot, 0, 0);
                SetIKWeights(AvatarIKGoal.RightFoot, 0, 0);
            }
        }
        else
        {
            targetHandL.localPosition = targetOrigPosHandL;
            targetHandR.localPosition = targetOrigPosHandR;
            targetHandL.localRotation = targetOrigRotHandL;
            targetHandR.localRotation = targetOrigRotHandR;
            SetIKWeights(AvatarIKGoal.LeftHand, 1, 1);
            SetIKWeights(AvatarIKGoal.RightHand, 1, 1);
        }
    }

    private void OnAnimateFeet(int layerIndex)
    {
        //IKCurveFoot_L = anim.GetFloat("IKCurveFoot_L");
        //IKCurveFoot_R = anim.GetFloat("IKCurveFoot_R");


        rayL = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(rayL, out hitL, FootDistance, layerMask))
        {
            NetworkedHitPointL = hitL.point;
            NetworkedHitNormalL = hitL.normal;
            //SetIKValuesRaw(AvatarIKGoal.LeftFoot, hitL.point, Quaternion.LookRotation(Root.forward, hitL.normal));
        }
            NetworkedTargetFootLPos = NetworkedHitPointL + m_character.GetUp() * footTargetOffset;
            targetFootL.position = NetworkedTargetFootLPos;
            NetworkedTargetFootLRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(m_character.GetForward(), NetworkedHitNormalL), NetworkedHitNormalL);
            targetFootL.rotation = NetworkedTargetFootLRot;

        rayR = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(rayR, out hitR, FootDistance, layerMask))
        {
            NetworkedHitPointR = hitR.point;
            NetworkedHitNormalR = hitR.normal;
            //SetIKValuesRaw(AvatarIKGoal.LeftFoot, hitL.point, Quaternion.LookRotation(Root.forward, hitL.normal));
        }
        NetworkedTargetFootRPos = NetworkedHitPointR + m_character.GetUp() * footTargetOffset;
            targetFootR.position = NetworkedTargetFootRPos;
            NetworkedTargetFootRRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(m_character.GetForward(), NetworkedHitNormalR), NetworkedHitNormalR);
            targetFootR.rotation = NetworkedTargetFootRRot;
        SetIKValues(AvatarIKGoal.LeftFoot, targetFootL);
        SetIKValues(AvatarIKGoal.RightFoot, targetFootR);
        SetIKWeights(AvatarIKGoal.LeftFoot, IKCurveFoot_L, IKCurveFoot_L);
        SetIKWeights(AvatarIKGoal.RightFoot, IKCurveFoot_R, IKCurveFoot_R);
    }

    Ray rayL, rayR;
    RaycastHit hitL, hitR;
    public float ikFootL;
    public float ikFootR;
    Vector3 leftUp;
    Vector3 rightUp;

    Ray rayView;
    RaycastHit hitView;

    public override void Render()
    {

        if (testSpeed > 0 || testJump == 1)
        {
            snapHandsPos = (handPosEnum != ((testCrouch == 1) ? HandPosAimEnum.CrouchMove : HandPosAimEnum.UprightMove));
            if (handPosEnum != ((testCrouch == 1) ? HandPosAimEnum.CrouchMove : HandPosAimEnum.UprightMove))
                handPosEnum = (testCrouch == 1) ? HandPosAimEnum.CrouchMove : HandPosAimEnum.UprightMove;
        }
        else
        {
            snapHandsPos = (handPosEnum != ((testCrouch == 1) ? HandPosAimEnum.Crouch : HandPosAimEnum.Upright));
            if (handPosEnum != ((testCrouch == 1) ? HandPosAimEnum.Crouch : HandPosAimEnum.Upright))
                handPosEnum = (testCrouch == 1) ? HandPosAimEnum.Crouch : HandPosAimEnum.Upright;
        }

        trHands.localRotation = handsOrigRot * Quaternion.AngleAxis(testAimAngle * 90, Vector3.left);
        if (testAimAngle > 0)
        {
            var lerpToPos = handsPosAimData[(int)handPosEnum].Up;
            var neutralPos = trHands.localPosition = handsPosAimData[(int)handPosEnum].Neutral;
            trHands.localPosition = Vector3.Lerp(neutralPos, lerpToPos, snapHandsPos ? 1 : testAimAngle);
            //trHands.localPosition = Vector3.Lerp(trHands.localPosition, lerpToPos, (testAimAngle / 1f));
        }
        else if (testAimAngle < 0)
        {
            var lerpToPos = trHands.localPosition = handsPosAimData[(int)handPosEnum].Down;
            var neutralPos = trHands.localPosition = handsPosAimData[(int)handPosEnum].Neutral;
            trHands.localPosition = Vector3.Lerp(neutralPos, lerpToPos, snapHandsPos ? 1 : -testAimAngle);
            //trHands.localPosition = Vector3.Lerp(trHands.localPosition, lerpToPos, (testAimAngle / 1f));
        }
        else
        {
            trHands.localPosition = handsPosAimData[(int)handPosEnum].Neutral;
        }
    }
}