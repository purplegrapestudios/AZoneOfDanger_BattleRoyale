using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterRagdollComponent : SimulationBehaviour
{
    [SerializeField] private GameObject m_ragdollCollider;
    [SerializeField] private float m_hitImpulse;
    private Character m_character;

    public Transform m_pelvis;
    public Transform m_thighL;
    public Transform m_calfL;
    public Transform m_thighR;
    public Transform m_calfR;
    public Transform m_spineMid;
    public Transform m_head;
    public Transform m_upperarmL;
    public Transform m_forearmL;
    public Transform m_upperarmR;
    public Transform m_forearmR;
    public Vector3 m_pelvisPosition;
    public Vector3 m_thighPositionL;
    public Vector3 m_calfPositionL;
    public Vector3 m_thighPositionR;
    public Vector3 m_calfPositionR;
    public Vector3 m_spineMidPosition;
    public Vector3 m_headPosition;
    public Vector3 m_upperarmPositionL;
    public Vector3 m_forearmPositionL;
    public Vector3 m_upperarmPositionR;
    public Vector3 m_forearmPositionR;
    public Quaternion m_pelvisRotation;
    public Quaternion m_thighRotationL;
    public Quaternion m_calfRotationL;
    public Quaternion m_thighRotationR;
    public Quaternion m_calfRotationR;
    public Quaternion m_spineMidRotation;
    public Quaternion m_headRotation;
    public Quaternion m_upperarmRotationL;
    public Quaternion m_forearmRotationL;
    public Quaternion m_upperarmRotationR;
    public Quaternion m_forearmRotationR;

    [SerializeField] private bool m_ragdollEnabled;
    [SerializeField] private bool init;

    public void Init(Character character)
    {
        GetChildPositions();
        m_character = character;
        EnableRagdoll(false, true);
        //Debug.Log("Init(): Disabled Ragdoll");
        m_character.CharacterHealth.SubscribeFatalHitCallback(OnFatalHit);
        init = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!init) return;
        EnableRagdoll(m_character.CharacterHealth.NetworkedIsAlive == false);

    }
    private void OnEnable()
    {
        if (m_character == null) return;

        EnableRagdoll(false, true);
        m_character.CharacterHealth.SubscribeFatalHitCallback(OnFatalHit);
    }

    private void OnDisable()
    {
        //Do Reset Stuff Here?
        m_character.CharacterHealth.UnsubscribeFatalHitCallback(OnFatalHit);
    }

    public void OnFatalHit(HitData hit)
    {
        if (!init) return;

        if (Runner.IsServer == false)
            return;

        EnableRagdoll(true);
        m_pelvis.GetComponent<NetworkRigidbody>().Rigidbody.AddForceAtPosition(hit.Direction.normalized * m_hitImpulse, hit.Position);
        Debug.Log($"FatalHit(): Pelvis Rigidbody Impulse: {hit.Direction.normalized * m_hitImpulse}");
    }

    private void EnableRagdoll(bool value, bool immediateUpdate = false)
    {
        if (value == m_ragdollEnabled && immediateUpdate == false)
        {
            //Debug.Log($"No Force Update, RagDollEnabled: {value}");
            return;
        }

        //if (m_character.CapsuleCollider != null)
        //    m_character.CapsuleCollider.enabled = value;
        //m_character.MainModel.SetActive(!value);
        //if (!value) ReturnChildPositions();
        //m_ragdollCollider.SetActive(value);
        //m_ragdollEnabled = value;
        //return;
        //





        //Disable our regular Capsule Collider used for Character.
        m_character.CapsuleCollider.enabled = !value;

        //m_character.SkinnedMesh.updateWhenOffscreen = value;
        if (value)
            GetChildPositions();
        else
            ReturnChildPositions();
        m_character.CharacterAnimation.enabled = !value;
        m_character.NetworkRigidbody.InterpolationTarget.localPosition = Vector3.zero;
        m_character.NetworkRigidbody.InterpolationTarget.localRotation = Quaternion.identity;

        if (m_character.NetworkRigidbody != null)
            m_character.NetworkRigidbody.TeleportToPositionRotation(transform.position, transform.rotation);

        m_character.MainModel.SetActive(!value);
        m_character.CharacterMove.enabled = !value;


        m_character.NetworkRigidbody.Rigidbody.velocity = Vector3.zero;
        m_character.NetworkRigidbody.Rigidbody.angularVelocity = Vector3.zero;
        m_character.NetworkRigidbody.Rigidbody.isKinematic = value;
        m_character.NetworkRigidbody.Rigidbody.useGravity = value;
        m_character.NetworkRigidbody.Rigidbody.constraints = value ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeRotation;

        //if (!value) ReturnChildPositions();
        m_ragdollCollider.SetActive(value);
        //Debug.Log($"RagDollEnabled: {value}");
        m_ragdollEnabled = value;
    }

    public void GetChildPositions()
    {
        m_pelvisPosition = m_pelvis.localPosition;
        m_thighPositionL = m_thighL.localPosition;
        m_calfPositionL = m_calfL.localPosition;
        m_thighPositionR = m_thighR.localPosition;
        m_calfPositionR = m_calfR.localPosition;
        m_spineMidPosition = m_spineMid.localPosition;
        m_headPosition = m_head.localPosition;
        m_upperarmPositionL = m_upperarmL.localPosition;
        m_forearmPositionL = m_forearmL.localPosition;
        m_upperarmPositionR = m_upperarmR.localPosition;
        m_forearmPositionR = m_forearmR.localPosition;

        m_pelvisRotation = m_pelvis.rotation;
        m_thighRotationL = m_thighL.rotation;
        m_calfRotationL = m_calfL.rotation;
        m_thighRotationR = m_thighR.rotation;
        m_calfRotationR = m_calfR.rotation;
        m_spineMidRotation = m_spineMid.rotation;
        m_headRotation = m_head.rotation;
        m_upperarmRotationL = m_upperarmL.rotation;
        m_forearmRotationL = m_forearmL.rotation;
        m_upperarmRotationR = m_upperarmR.rotation;
        m_forearmRotationR = m_forearmR.rotation;
    }

    [ContextMenu("ResetChildPositions")]
    public void ReturnChildPositions()
    {
        m_pelvis.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_thighL.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_calfL.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_thighR.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_calfR.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_spineMid.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_head.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_upperarmL.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_forearmL.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_upperarmR.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_forearmR.GetComponent<Rigidbody>().velocity = Vector3.zero;

        m_pelvis.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_thighL.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_calfL.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_thighR.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_calfR.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_spineMid.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_head.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_upperarmL.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_forearmL.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_upperarmR.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_forearmR.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        m_pelvis.localPosition = m_pelvisPosition;
        m_thighL.localPosition = m_thighPositionL;
        m_calfL.localPosition = m_calfPositionL;
        m_thighR.localPosition = m_thighPositionR;
        m_calfR.localPosition = m_calfPositionR;
        m_spineMid.localPosition = m_spineMidPosition;
        m_head.localPosition = m_headPosition;
        m_upperarmL.localPosition = m_upperarmPositionL;
        m_forearmL.localPosition = m_forearmPositionL;
        m_upperarmR.localPosition = m_upperarmPositionR;
        m_forearmR.localPosition = m_forearmPositionR;

        m_pelvis.rotation = m_pelvisRotation;
        m_thighL.rotation = m_thighRotationL;
        m_calfL.rotation = m_calfRotationL;
        m_thighR.rotation = m_thighRotationR;
        m_calfR.rotation = m_calfRotationR;
        m_spineMid.rotation = m_spineMidRotation;
        m_head.rotation = m_headRotation;
        m_upperarmL.rotation = m_upperarmRotationL;
        m_forearmL.rotation = m_forearmRotationL;
        m_upperarmR.rotation = m_upperarmRotationR;
        m_forearmR.rotation = m_forearmRotationR;
    }
}