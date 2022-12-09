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
    //private PlayerShooting PlayerShooting;
    private CharacterCamera m_characterCamera;
    private Animator Animator_1stPerson;
    private Animator Animator_3rdPerson;
    private Camera PlayerCamera;
    private Camera DeathCamera;

    //Animator Component: Parameters (Parameters Hashed. We update these values for actual animation to happen)
    private int Param_3rdPersonLowerBody;
    private int Param_3rdPersonUpperBody;
    private int Param_3rdPerson_AimAngle;
    private int Param_JumpBool;
    private int Param_DeathBool;
    private int Param_1stPersonUpperBody_AR;

    //Networked PlayerAnimator Component: Parameter values
    private int Anim_INT_AssaultRifle;           //For TPS View Only (Set by !Photonview.isMine)
    private int Anim_INT_AssaultRifle_Current;   //For TPS View Only (Set by !Photonview.isMine)
    private int Anim_INT_LegRunning;             //For TPS View Only (Set by !Photonview.isMine)
    private bool Anim_BOOL_Jump;                 //For TPS View Only (Set by !Photonview.isMine)
    private bool Anim_BOOL_Death;                //For TPS View Only (Set by !Photonview.isMine)

    //Local PlayerAnimator Component: Parameter values
    private int AnimLocal_INT_UpperBody;    //For TPS View (Set by PhtonView.isMine)
    private int AnimLocal_INT_LowerBody;    //For TPS View (Set by PhtonView.isMine)
    private bool AnimLocal_BOOL_Jump;       //For TPS View (Set by PhtonView.isMine)
    private bool AnimLocal_BOOL_Death;      //For TPS View (Set by PhtonView.isMine)
    private int AnimLocal_INT_Arms;         //For FPS view (Set by PhtonView.isMine)

    //Variables to control animation priorities
    private bool armPriorityAnimation;
    private bool shotPriorityAnimation;

    //Player camer view variables (This is for switching between First and Third person views)
    public PlayerCameraView playerCameraView;
    private PlayerCameraView originalPlayerCamerView;
    private bool isInitialized;

    public void Initialize () {

        m_characterMoveComponent = GetComponent<CharacterMoveComponent>();
        m_characterCamera = GetComponent<CharacterComponents>().PlayerCamera.GetComponent<CharacterCamera>();
        Animator_1stPerson = GetComponent<CharacterComponents>().animator1;
        Animator_3rdPerson = GetComponent<CharacterComponents>().animator3;

        Param_3rdPersonLowerBody = Animator.StringToHash("Param_3rdPersonLowerBody");
        Param_3rdPersonUpperBody = Animator.StringToHash("Param_3rdPersonUpperBody");
        Param_3rdPerson_AimAngle = Animator.StringToHash("Param_3rdPerson_AimAngle");
        Param_JumpBool = Animator.StringToHash("JumpBool");
        Param_DeathBool = Animator.StringToHash("DeathBool");
        Param_1stPersonUpperBody_AR = Animator.StringToHash("Param_1stPersonUpperBody_AR");
        isInitialized = true;
    }


    bool __isFiringBullet;
    bool __isAiming;

    private void LateUpdate()
    {
        if (!isInitialized) return;

        AnimationBehavior_OurPlayer(isPlayerAlive: true);
        __isFiringBullet = false;
        __isAiming = false;
    }

    private void AnimationBehavior_OurPlayer(bool isPlayerAlive)
    {
        //PLAYER IS ALIVE
        if (isPlayerAlive)
        {
            SetStateFloat_3rdPersonAimAngle(m_characterCamera.NetworkedRotationY / 90f, smooth: .9f);

            //1) SET DEATHBOOL TO FALSE (SET BOTH 1ST AND 3RD PERSON - AND AFFECTS WHOLE BODY)
            if (AnimLocal_BOOL_Death)
            {
                DeathCamera.enabled = false;
                PlayerCamera.enabled = true;

                if (originalPlayerCamerView.Equals(PlayerCameraView.FirstPerson))
                    GetComponent<CharacterAnimation>().SwitchCameraPerspective(PlayerCameraView.FirstPerson);

                AnimLocal_BOOL_Death = false;
                SetStateBool_3rdPersonDeath(AnimLocal_BOOL_Death);
            }

            //SHOOTING (UPPER BODY)
            if (__isFiringBullet)
            {
                armPriorityAnimation = true;

                if (__isAiming)
                {
                    //AIM FIRE UPPER BODY
                    if (AnimLocal_INT_Arms != 3)
                    {
                        AnimLocal_INT_Arms = 3;
                        AnimLocal_INT_UpperBody = 3;
                        SetStateInt_1stPersonArms(AnimLocal_INT_Arms);  //Local AR AimFire
                        SetStateInt_3rdPersonUpperBody(AnimLocal_INT_UpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else
                {
                    //FIRE UPPER BODY
                    if (AnimLocal_INT_Arms != 2)
                    {
                        AnimLocal_INT_Arms = 2;
                        AnimLocal_INT_UpperBody = 2;
                        SetStateInt_1stPersonArms(AnimLocal_INT_Arms);  //Local AR Fire
                        SetStateInt_3rdPersonUpperBody(AnimLocal_INT_UpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
            }
            else if (__isAiming)
            {
                armPriorityAnimation = true;
                if (__isFiringBullet)
                {
                    if (AnimLocal_INT_Arms != 2)
                    {
                        AnimLocal_INT_Arms = 2;
                        AnimLocal_INT_UpperBody = 2;
                        SetStateInt_1stPersonArms(AnimLocal_INT_Arms);  //Local AR Fire
                        SetStateInt_3rdPersonUpperBody(AnimLocal_INT_UpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else
                {    //AIM UPPER BODY
                    if (AnimLocal_INT_Arms != 1)
                    {
                        AnimLocal_INT_Arms = 1;
                        AnimLocal_INT_UpperBody = 1;
                        SetStateInt_1stPersonArms(AnimLocal_INT_Arms);  //Local AR Aim
                        SetStateInt_3rdPersonUpperBody(AnimLocal_INT_UpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
            }
            else if (!__isAiming && !__isFiringBullet)
            {
                //IDLE UPPER BODY (NOT SHOOTING)
                if (!armPriorityAnimation)
                {
                    AnimLocal_INT_Arms = 0;
                    AnimLocal_INT_UpperBody = 0;
                    SetStateInt_1stPersonArms(AnimLocal_INT_Arms);  //Local Arms Idle
                    SetStateInt_3rdPersonUpperBody(AnimLocal_INT_UpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                }
                armPriorityAnimation = false;
            }


            //MOVEMENT (WHOLE BODY)
            if (m_characterMoveComponent.NetworkedFloorDetected)
            {
                //JUMP
                if (AnimLocal_BOOL_Jump != false)
                {
                    AnimLocal_BOOL_Jump = false;
                    SetStateBool_3rdPersonJump(AnimLocal_BOOL_Jump);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                }

                if (m_characterMoveComponent.NetworkedVelocity.magnitude > 15)
                {
                    //RUNNING LOWER BODY
                    if (AnimLocal_INT_LowerBody != 1)
                    {
                        AnimLocal_INT_LowerBody = 1;
                        SetStateInt_3rdPersonLowerBody(AnimLocal_INT_LowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                    //RUNNING UPPER BODY
                    if (AnimLocal_INT_Arms != 5 && !armPriorityAnimation)
                    {
                        AnimLocal_INT_Arms = 5;
                        AnimLocal_INT_UpperBody = 5;
                        SetStateInt_1stPersonArms(AnimLocal_INT_Arms);  //Local Arms Run
                        SetStateInt_3rdPersonUpperBody(AnimLocal_INT_UpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else if (m_characterMoveComponent.NetworkedVelocity.magnitude <= 15 && m_characterMoveComponent.NetworkedVelocity.magnitude > 0)
                {
                    //WALKING LOWER BODY
                    if (AnimLocal_INT_LowerBody != 1)
                    {
                        AnimLocal_INT_LowerBody = 1;
                        SetStateInt_3rdPersonLowerBody(AnimLocal_INT_LowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                    //WALKING UPPER BODY
                    if (AnimLocal_INT_Arms != 4 && !armPriorityAnimation)
                    {
                        AnimLocal_INT_Arms = 4;
                        AnimLocal_INT_LowerBody = 4;
                        SetStateInt_1stPersonArms(AnimLocal_INT_Arms);  //Local Arms Walk
                        SetStateInt_3rdPersonUpperBody(AnimLocal_INT_LowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else
                {
                    //IDLE LOWER BODY (HANDLED IN MOVEMENT/JUMP BECAUSE WANT JUMP ANIMATION TO TAKE PRIORITY)
                    if (AnimLocal_INT_LowerBody != 0)
                    {
                        AnimLocal_INT_LowerBody = 0;
                        SetStateInt_3rdPersonLowerBody(AnimLocal_INT_LowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
            }
            else
            {
                //JUMPING (SET BOTH 1ST AND 3RD PERSON - AND AFFECTS WHOLE BODY)
                if (AnimLocal_BOOL_Jump != true)
                {
                    AnimLocal_BOOL_Jump = true;
                    SetStateBool_3rdPersonJump(AnimLocal_BOOL_Jump);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                }

            }


        }
        else
        {
            //DEATH ANIMATION: LESS THAN OR EQUAL TO 0 HP
            if (!AnimLocal_BOOL_Death)
            {
                originalPlayerCamerView = playerCameraView;
                if (originalPlayerCamerView.Equals(PlayerCameraView.FirstPerson))
                    GetComponent<CharacterAnimation>().SwitchCameraPerspective(PlayerCameraView.ThirdPerson);

                DeathCamera.enabled = true;
                PlayerCamera.enabled = false;
                AnimLocal_BOOL_Death = true;
                SetStateBool_3rdPersonDeath(AnimLocal_BOOL_Death);

                //SetStateFloat_3rdPersonAimAngle(0);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
            }

        }

    }


    /// <summary>
    /// Set Animator Component Parameters
    /// </summary>
    private void SetStateInt_3rdPersonLowerBody(int val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson)) && Animator_3rdPerson.gameObject.activeSelf)
            Animator_3rdPerson.SetFloat(Param_3rdPersonLowerBody, val);
    }

    private void SetStateInt_3rdPersonUpperBody(int val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson)) && Animator_3rdPerson.gameObject.activeSelf)
            Animator_3rdPerson.SetInteger(Param_3rdPersonUpperBody, val);
    }

    private void SetStateBool_3rdPersonJump(bool val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson)) && Animator_3rdPerson.gameObject.activeSelf)
            Animator_3rdPerson.SetBool(Param_JumpBool, val);
    }

    private void SetStateBool_3rdPersonDeath(bool val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson)) && Animator_3rdPerson.gameObject.activeSelf)
            Animator_3rdPerson.SetBool(Param_DeathBool, val);
    }

    private void SetStateFloat_3rdPersonAimAngle(float val, float smooth = 1f)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson)) && Animator_3rdPerson.gameObject.activeSelf)
        {
            if (smooth == 1)
            {
                Animator_3rdPerson.SetFloat(Param_3rdPerson_AimAngle, val);
                return;
            }

            var curAimAngle = Animator_3rdPerson.GetFloat(Param_3rdPerson_AimAngle);
            var lerpedAimAngle = Mathf.Lerp(curAimAngle, val, smooth);
            Animator_3rdPerson.SetFloat(Param_3rdPerson_AimAngle, lerpedAimAngle);
        }
    }

    private void SetStateInt_1stPersonArms(int val)
    {
        return;
        if (playerCameraView.Equals(PlayerCameraView.FirstPerson))
            Animator_1stPerson.SetInteger(Param_1stPersonUpperBody_AR, val);
    }

    /// <summary>
    /// UPDATE: Switch camera view behavior
    /// </summary>
    private void SwitchCameraBehavior()
    {
        //if (InputManager.Instance.GetKeyDown(InputCode.SwitchPerspective))
        //{
        //    if (EventManager.Instance.GetScore(PhotonView.owner.NickName, PlayerStatCodes.Health) > 0)
        //    {
        //        if (playerCameraView.Equals(PlayerCameraView.FirstPerson))
        //        {
        //            SwitchCameraPerspective(PlayerCameraView.ThirdPerson);
        //        }
        //        else if (playerCameraView.Equals(PlayerCameraView.ThirdPerson))
        //        {
        //            SwitchCameraPerspective(PlayerCameraView.FirstPerson);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Switch between First Person and Third Person camera view
    /// </summary>
    private void SwitchCameraPerspective(PlayerCameraView view)
    {
        //GetComponent<PlayerObjectComponents>().PlayerCamera.GetComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("Player");
        AnimLocal_INT_UpperBody = -1;
        AnimLocal_INT_LowerBody = -1;
        AnimLocal_BOOL_Jump = !AnimLocal_BOOL_Jump;
        AnimLocal_INT_Arms = -1;

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