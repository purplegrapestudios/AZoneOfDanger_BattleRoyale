using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum PlayerCameraView
{
    FirstPerson,
    ThirdPerson,
}

public class CharacterAnimation : NetworkBehaviour
{
    /// <summary>
    /// VARIABLE SECTION
    /// </summary>
    //Player components required in animating the player
    private CharacterMoveComponent m_characterMoveComponent;
    private CharacterShootComponent m_characterShootComponent;
    private CharacterHealthComponent m_characterHealthComponent;
    private CharacterCamera m_characterCamera;
    private Animator Animator_3rdPerson;

    //Animator Component: Parameters (Parameters Hashed. We update these values for actual animation to happen)
    private int Param_3rdPerson_AimAngle;
    private int Param_CrouchInt;
    private int Param_JumpInt;
    private int Param_DeadInt;
    private int Param_Speed;
    private int Param_FireInt;
    private int Param_ReloadInt;
    private int Param_SwitchWeaponInt;

    //Player camer view variables (This is for switching between First and Third person views)
    public PlayerCameraView playerCameraView;
    private bool isInitialized;

    public void Initialize () {

        m_characterMoveComponent = GetComponent<CharacterMoveComponent>();
        m_characterShootComponent = GetComponent<CharacterShootComponent>();
        m_characterHealthComponent = GetComponent<CharacterHealthComponent>();
        m_characterCamera = GetComponent<CharacterComponents>().PlayerCamera.GetComponent<CharacterCamera>();
        Animator_3rdPerson = GetComponent<CharacterComponents>().animator3;

        Param_3rdPerson_AimAngle = Animator.StringToHash("Param_3rdPerson_AimAngle");
        Param_CrouchInt = Animator.StringToHash("Param_CrouchInt");
        Param_DeadInt = Animator.StringToHash("Param_DeadInt");
        Param_FireInt = Animator.StringToHash("Param_FireInt");
        Param_JumpInt = Animator.StringToHash("Param_JumpInt");
        Param_ReloadInt = Animator.StringToHash("Param_ReloadInt");
        Param_Speed = Animator.StringToHash("Param_Speed");
        Param_SwitchWeaponInt = Animator.StringToHash("Param_SwitchWeaponInt");
        isInitialized = true;
    }

    private void LateUpdate()
    {
        if (!isInitialized) { return; }
        AnimationBehavior_OurPlayer(isPlayerAlive: true);
    }

    private void AnimationBehavior_OurPlayer(bool isPlayerAlive)
    {
        //PLAYER IS ALIVE
        if (isPlayerAlive)
        {
            SetStateFloat(ref Param_3rdPerson_AimAngle, m_characterCamera.NetworkedRotationY / 90f, smooth: .9f);
            SetStateInt(ref Param_CrouchInt, m_characterMoveComponent.NetworkedIsCrouched ? 1 : 0);
            SetStateInt(ref Param_DeadInt, m_characterHealthComponent.NetworkedRespawn ? 1 : 0);
            SetStateInt(ref Param_FireInt, m_characterShootComponent.NetworkedFire ? 1 : 0);
            SetStateInt(ref Param_JumpInt, !m_characterMoveComponent.NetworkedFloorDetected ? 1 : 0);
            SetStateInt(ref Param_ReloadInt, m_characterShootComponent.NetworkedReload ? 1 : 0);
            SetStateFloat(ref Param_Speed, m_characterMoveComponent.NetworkedVelocity.magnitude);
            SetStateInt(ref Param_SwitchWeaponInt, m_characterShootComponent.NetworkedSwitchWeapon ? 1 : 0);
        }
    }

    private void SetStateFloat(ref int param, float val, float smooth = 1)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson)) && Animator_3rdPerson.gameObject.activeSelf)
        {
            if (smooth == 1)
            {
                Animator_3rdPerson.SetFloat(param, val);
                return;
            }
        }
        var lerpedValue = Mathf.Lerp(Animator_3rdPerson.GetFloat(param), val, smooth);
        Animator_3rdPerson.SetFloat(param, lerpedValue);
    }

    private void SetStateInt(ref int param, int val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson)) && Animator_3rdPerson.gameObject.activeSelf)
            Animator_3rdPerson.SetInteger(param, val);
    }

    /// <summary>
    /// Switch between First Person and Third Person camera view
    /// </summary>
    private void SwitchCameraPerspective(PlayerCameraView view)
    {
        if (view.Equals(PlayerCameraView.FirstPerson))
        {
            GetComponent<CharacterComponents>().Dolly.GetComponent<CharacterCameraDolly>().RestDollyPosition();
            GetComponent<CharacterComponents>().PlayerCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
            playerCameraView = PlayerCameraView.FirstPerson;
            GetComponent<CharacterComponents>().ThirdPersonPlayer.SetActive(false);
            GetComponent<CharacterComponents>().FirstPersonPlayer.SetActive(true);
        }
        else if (view.Equals(PlayerCameraView.ThirdPerson))
        {
            GetComponent<CharacterComponents>().Dolly.GetComponent<CharacterCameraDolly>().RestDollyPosition();
            GetComponent<CharacterComponents>().PlayerCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("Player");
            playerCameraView = PlayerCameraView.ThirdPerson;
            GetComponent<CharacterComponents>().ThirdPersonPlayer.SetActive(true);
            GetComponent<CharacterComponents>().FirstPersonPlayer.SetActive(false);

        }
    }
}