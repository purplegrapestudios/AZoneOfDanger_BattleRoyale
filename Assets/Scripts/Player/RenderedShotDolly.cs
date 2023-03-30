using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RenderedShotDolly : NetworkBehaviour
{
    /* The purpose is to calculate NetworkedMuzzlePosition.
     * It is used for positioning the Rendered Projectiles.
     */

    [SerializeField] [Networked] public Vector3 NetworkedRenderedShotPosition { get; set; }
    [SerializeField] private Vector3 m_renderedShotOffset = new(0, 0, 0.5f);
    [SerializeField] private Vector3 m_renderedShotUpPos = new(0, 0.75f, 0);
    [SerializeField] private Vector3 m_renderedShotCrouchPos = new(0, 0, 0);
    [SerializeField]private Transform m_renderedShotStartTransform;
    private Character m_character;
    private CharacterCamera m_characterCamera;
    private CharacterMoveComponent m_characterMoveComponent;
    private Quaternion originalRotation;
    private bool m_initialized = false;

    public void Initialize(Character character, CharacterCamera characterCamera)
    {
        m_character = character;
        m_characterMoveComponent = m_character.GetComponent<CharacterMoveComponent>();
        m_characterCamera = characterCamera;
        originalRotation = transform.localRotation;

        m_renderedShotStartTransform.localPosition = m_renderedShotOffset;
        
        m_initialized = true;
    }

    public override void Render()
    {
        if (!m_character) return;
        if (!m_initialized) return;

        RotateMuzzleDolly(m_characterCamera.NetworkedRotationY + m_character.CachedAimDirDelta.y);
        NetworkedRenderedShotPosition = m_renderedShotStartTransform.position;
        transform.localPosition = m_characterMoveComponent.NetworkedIsCrouched ? m_renderedShotCrouchPos : m_renderedShotUpPos;
    }

    private void RotateMuzzleDolly(float rotationAlongX)
    {
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationAlongX, Vector3.left);
        transform.localRotation = originalRotation * yQuaternion;
    }
}