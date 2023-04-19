using System;
using UnityEngine;

public interface TipTargetTransforms
{
    public Transform Tip { get; set; }
    public Transform Target { get; set; }
    public float PosWeight { get; set; }
    public float RotWeight { get; set; }
    public bool Grounded { get; set; }
    public bool TipTargetTransforms { get; set; }
    public RaycastHit[] RayCastHit { get; set; }
    public Vector3 LastFootPosition { get; set; }
    public Vector3 HandObjPivotPos { get; set; }
    public Quaternion HandObjPivotRot { get; set; }

    public bool Equals(TipTargetTransforms t1, TipTargetTransforms t2)
    {
        return (t1.Target == t2.Target && t1.Tip == t2.Tip);
    }
}

[Serializable]
public struct TipTargetTransformsHand : TipTargetTransforms
{
    Transform TipTargetTransforms.Tip { get => Tip; set => Tip = value; }
    Transform TipTargetTransforms.Target { get => Target; set => Target = value; }
    float TipTargetTransforms.PosWeight { get => PosWeight; set => PosWeight = value; }
    float TipTargetTransforms.RotWeight { get => RotWeight; set => RotWeight = value; }
    Vector3 TipTargetTransforms.HandObjPivotPos { get => HandObjPivotPos; set => HandObjPivotPos = value; }
    Quaternion TipTargetTransforms.HandObjPivotRot { get => HandObjPivotRot; set => HandObjPivotRot = value; }
    public RaycastHit[] RayCastHit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Vector3 LastFootPosition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool Grounded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool TipTargetTransforms { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool Equals(TipTargetTransforms t1, TipTargetTransforms t2) { return (t1.Target == t2.Target && t1.Tip == t2.Tip); }


    public Transform Tip;
    public Transform Target;
    public float PosWeight;
    public float RotWeight;
    public Vector3 HandObjPivotPos;
    public Quaternion HandObjPivotRot;
}

[Serializable]
public struct TipTargetTransformsFoot : TipTargetTransforms
{
    Transform TipTargetTransforms.Tip { get => Tip; set => Tip = value; }
    Transform TipTargetTransforms.Target { get => Target; set => Target = value; }
    float TipTargetTransforms.PosWeight { get => PosWeight; set => PosWeight = value; }
    float TipTargetTransforms.RotWeight { get => RotWeight; set => RotWeight = value; }
    bool TipTargetTransforms.Grounded { get => Grounded; set => Grounded = value; }
    RaycastHit[] TipTargetTransforms.RayCastHit { get => RayCastHit; set => RayCastHit = value; }
    public Vector3 HandObjPivotPos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Quaternion HandObjPivotRot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    Vector3 TipTargetTransforms.LastFootPosition { get => LastFootPosition; set => LastFootPosition = value; }
    public bool TipTargetTransforms { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool TipTargetTransformsEquals(TipTargetTransforms t1, TipTargetTransforms t2) { return (t1.Target == t2.Target && t1.Tip == t2.Tip); }

    public Transform Tip;
    public Transform Target;
    public float PosWeight;
    public float RotWeight;
    public bool Grounded;
    public RaycastHit[] RayCastHit;
    public Vector3 LastFootPosition;
}


[Serializable]
public struct IKTarget
{
    public Vector3 Pos;
    public Quaternion Rot;

    public IKTarget(Vector3 pos, Quaternion q)
    {
        Pos = pos;
        Rot = q;
    }
}

[RequireComponent(typeof(Animator))]
public class CharacterIKControls : MonoBehaviour
{
    protected Animator animator;
    public bool UseArmIK => m_isArmIK;
    [SerializeField] private bool m_isArmIK;
    public bool UseLegIK => m_isLegIK;
    [SerializeField] private bool m_isLegIK;

    [SerializeField] private TipTargetTransformsFoot m_tipTargetLeftFoot;
    [SerializeField] private TipTargetTransformsFoot m_tipTargetRightFoot;
    [SerializeField] private TipTargetTransformsHand m_tipTargetLeftHand;
    [SerializeField] private TipTargetTransformsHand m_tipTargetRightHand;

    //Predetermined Target Position / Rotations to choose from
    [SerializeField] private IKTarget m_ikTargetAimUp;
    [SerializeField] private IKTarget m_ikTargetAimDown;
    [SerializeField] private IKTarget m_ikTargetFootR;
    [SerializeField] private IKTarget m_ikTargetFootL;
    [SerializeField] private Transform m_weapons;
    Character m_character;
    CharacterAnimation m_characterAnim;
    private bool m_init;
    private RaycastHit[] m_raycastHitFootL;
    private RaycastHit[] m_raycastHitFootR;
    private bool m_isGroundedFootL;
    private bool m_isGroundedFootR;
    [SerializeField] private float m_groundDist;
    [SerializeField] private LayerMask m_groundLayer;

    private Vector3 m_stepPositionL;
    private Vector3 m_stepPositionR;
    private Vector3 m_lastStepPositionL;
    private Vector3 m_lastStepPositionR;


    private void Awake()
    {
        Init(transform.root.GetComponent<Character>());
    }

    public void Init(Character c)
    {
        m_character = c;
        m_characterAnim = m_character.CharacterAnimation;
        if (animator == null) animator = GetComponent<Animator>();
        m_raycastHitFootL = new RaycastHit[1];
        m_raycastHitFootR = new RaycastHit[1];
        m_tipTargetLeftFoot.RayCastHit = new RaycastHit[1];
        m_tipTargetRightFoot.RayCastHit = new RaycastHit[1];

        m_init = true;
    }

    void OnAnimatorIK()
    {
        if (!m_init) return;
        if (!animator) return;

        if (m_characterAnim.p_3rdPersonAimAngle == 1)
        {
            //Hand Target Positions to change according to AimAngle param
            // iKTargetLeft = new IKTarget(Vector3.one, Quaternion.identity);
            // iKTargetRight = new IKTarget(Vector3.one, Quaternion.identity);
            // IKUpdateForPosition(ikTarget, handPositionLeft, handRotationLeft);
            // IKUpdateForPosition(ikTarget, handPositionRight, handRotationRight);
        }

        if (m_characterAnim.p_FireInt == 1)
        {
            //Similr to aimAngle requirements, but we should also add some Recoil animation for Firing via IK
            //We can make gun LookAt Camera.position + 100f
        }

        if (m_characterAnim.p_CrouchInt == 1)
        {
            //Technically this does not need a condition, as crouchWalk, crouchShoot, crouch (Since FK should drive the torso, feet/arms etc are driven over here already

        }

        if (m_characterAnim.p_Speed > 0)
        {
        }
        //Foot Target Step distance to change according to Speed param
        var stepDist = m_characterAnim.p_Speed;
        // Left + Right Foot: IKTarget iKTarget = new IKTarget(Vector3.one, Quaternion.identity);

        if (m_tipTargetRightFoot.Grounded)
        {
            Debug.DrawRay(m_tipTargetRightFoot.Tip.position, m_tipTargetRightFoot.Tip.position + Vector3.down * 1.01f, Color.blue, 1f);
            IKUpdateForTarget(m_isLegIK, m_tipTargetRightFoot, AvatarIKGoal.RightFoot);
        }
        if (m_tipTargetLeftFoot.Grounded)
        {
            Debug.DrawRay(m_tipTargetRightFoot.Tip.position, m_tipTargetRightFoot.Tip.position + Vector3.down * 1.01f, Color.blue, 1f);
            IKUpdateForTarget(m_isLegIK, m_tipTargetLeftFoot, AvatarIKGoal.LeftFoot);
        }

        IKUpdateForTarget(m_isArmIK, m_tipTargetRightHand, AvatarIKGoal.RightHand);
        IKUpdateForTarget(m_isArmIK, m_tipTargetLeftHand, AvatarIKGoal.LeftHand);

        UseIKSwitchCheck();

        //IKUpdateForTarget(m_isLegIK, m_tipTargetLeftFoot, AvatarIKGoal.LeftFoot);
        //IKUpdateForTarget(m_isLegIK, m_tipTargetRightFoot, AvatarIKGoal.RightFoot);
        //IKUpdateForTarget(m_isArmIK, m_tipTargetLeftHand, AvatarIKGoal.LeftHand);
        //IKUpdateForTarget(m_isArmIK, m_tipTargetRightHand, AvatarIKGoal.RightHand);

    }

    private void UseIKSwitchCheck()
    {
        m_isArmIK = !(m_characterAnim.p_JumpInt == 1 ||
                m_characterAnim.p_ReloadInt == 1 ||
                m_characterAnim.p_SwitchWeaponInt == 1 ||
                m_characterAnim.p_DeadInt == 1
                );

        m_isLegIK = !(m_characterAnim.p_JumpInt == 1 || m_characterAnim.p_DeadInt == 1);
    }

    private void Update()
    {
        if (!m_init) return;
        if (!animator) return;

        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            m_isArmIK = !m_isArmIK;
        }

        FootGroundCheck(ref m_tipTargetRightFoot, m_raycastHitFootR);
        FootGroundCheck(ref m_tipTargetLeftFoot, m_raycastHitFootL);
    }

    private void IKUpdateForPosition(IKTarget ikTarget, TipTargetTransforms tipTargetData, AvatarIKGoal avatarGoal)
    {
        if (!m_isArmIK) return;

        tipTargetData.Target.localPosition = ikTarget.Pos;
        tipTargetData.Target.localRotation = ikTarget.Rot;

        SetPositionRotationWeight(avatarGoal, tipTargetData.PosWeight, tipTargetData.RotWeight);
    }

    private void IKUpdateForTarget(bool useIK, TipTargetTransforms tipTargetData, AvatarIKGoal avatarGoal, float lookAtTargetWeight = 0)
    {

        if (useIK)
        {

            // Set the look target position, if one has been assigned
            if (tipTargetData.Target != null)
            {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(tipTargetData.Target.position);
            }

            // Set the (Tip) right hand, to Target position and rotation, if one has been assigned
            if (tipTargetData.Tip != null)
            {
                if (tipTargetData.Equals(m_tipTargetLeftFoot) || tipTargetData.Equals(m_tipTargetRightFoot))
                {
                    return;
                    if (tipTargetData.Grounded)
                    {

                        //SetPositionRotationWeight(avatarGoal, lerpWeight, lerpWeight);
                        //tipTargetData.Target.position = tipTargetData.RayCastHit[0].point;
                        //animator.SetIKPosition(avatarGoal, tipTargetData.RayCastHit[0].point);
                        //animator.SetIKRotation(avatarGoal, tipTargetData.Target.rotation);
                    }
                    else
                    {
                        tipTargetData.LastFootPosition = transform.position;
                    }

                    SetPositionRotationWeight(avatarGoal, tipTargetData.PosWeight, tipTargetData.RotWeight);
                    tipTargetData.Target.position = tipTargetData.RayCastHit[0].point;
                    animator.SetIKPosition(avatarGoal, tipTargetData.RayCastHit[0].point);
                    animator.SetIKRotation(avatarGoal, tipTargetData.Target.rotation);

                }
                else
                {
                    SetPositionRotationWeight(avatarGoal, 1,1);
                    //tipTargetData.Target.position = m_weapons.transform.position;
                    animator.SetIKPosition(avatarGoal, tipTargetData.Target.position);
                    animator.SetIKRotation(avatarGoal, tipTargetData.Target.parent.rotation);
                }

            }
        }

        //if the IK is not active, set the position and rotation of the hand and head back to the original position
        else
        {
            SetPositionRotationWeight(avatarGoal, 0, 0);
            animator.SetLookAtWeight(lookAtTargetWeight);
        }

    }

    private void SetPositionRotationWeight(AvatarIKGoal avatarGoal, float posWeight = 1, float rotWeight = 1)
    {
        animator.SetIKPositionWeight(avatarGoal, posWeight);
        animator.SetIKRotationWeight(avatarGoal, rotWeight);

    }

    private void FootGroundCheck(ref TipTargetTransformsFoot tipTargetData, RaycastHit[] rayHits)
    {
        var characterHeight = m_character.transform.localScale.y;
        Ray ray = new Ray(tipTargetData.Tip.position, Vector3.down);
        tipTargetData.Grounded = (Physics.RaycastNonAlloc(ray, rayHits, m_groundDist, m_groundLayer) > 0);
        var yStepHeight = Mathf.Sin(Time.deltaTime);
        tipTargetData.Target.up = rayHits[0].normal + yStepHeight * Vector3.up;
        tipTargetData.RayCastHit[0] = rayHits[0];
        Debug.DrawRay(tipTargetData.Tip.position, m_groundDist * Vector3.down, Color.blue, m_groundDist);
        Debug.DrawRay(rayHits[0].point, rayHits[0].normal, Color.red, 1f);
    }

    private void Example()
    {
        //Example
        IKUpdateForTarget(m_isLegIK, m_tipTargetLeftFoot, AvatarIKGoal.LeftFoot);
        IKUpdateForTarget(m_isLegIK, m_tipTargetRightFoot, AvatarIKGoal.RightFoot);
        IKUpdateForTarget(m_isArmIK, m_tipTargetLeftHand, AvatarIKGoal.LeftHand);
        IKUpdateForTarget(m_isArmIK, m_tipTargetRightHand, AvatarIKGoal.RightHand);
    }
}