using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class CharacterMoveData
{
    public class MoveCmd
    {
        public float ForwardMove;
        public float RightMove;
    }

    public float CmdScale()
    {
        int max;
        float total;
        float scale;

        max = (int)Mathf.Abs(moveCmd.ForwardMove);

        if (Mathf.Abs(moveCmd.RightMove) > max)
            max = (int)Mathf.Abs(moveCmd.RightMove);
        if (max == 0)
        {
            return 0;
        }
        total = Mathf.Sqrt(moveCmd.ForwardMove * moveCmd.ForwardMove + moveCmd.RightMove * moveCmd.RightMove);
        scale = P_MoveSpeed * max / (P_MoveScale * total);

        return scale;
    }


    //Parameters Movement
    [Range(0, 200)] [Header("Physics: Player acceleration moving in air")] public float P_AirAcceleration = 2.0f;
    [Range(0, 200)] [Header("Physics: Player decceleration moving in air")] public float P_AirDeacceleration = 2.0f;
    [Range(0, 20)] [Header("Physics: Player strafe Left/Right Weight moving in air")] public float P_AirControl = 0.3f;
    [Range(0, 2)] [Header("Physics: Amout of jumps player is allowed to make")] public int P_JumpAttempts = 2;
    [Range(0, 20)] [Header("Physics: Time interval for Second Jump")] public float P_DoubleJumpDeltaTime = 0.25f;
    [Range(0, 200)] [Header("Physics: Friction applied to player when grounded")] public float P_Friction = 6;
    [Range(0, 200)] [Header("Physics: Gravity force applied to player")] public float P_Gravity = 20.0f;
    [Range(0, 200)] [Header("Physics: Jump force applied by player")] public float P_JumpSpeed = 8.0f;
    [Range(0, 200)] [Header("Physics: Maximum speed player can move")] public float P_MaxSpeed = 150f;
    [Range(0, 200)] [Header("Physics: Speed V.S. Input Weight (Usually Double Movespeed for best results)")] public float P_MoveScale = 1.0f;
    [Range(0, 200)] [Header("Physics: Player movement speed")] public float P_MoveSpeed = 7.0f;
    [Range(0, 200)] [Header("Physics: Player acceleration moving on ground")] public float P_RunAcceleration = 14;
    [Range(0, 200)] [Header("Physics: Player decceleration  moving on ground")] public float P_RunDeacceleration = 10;
    [Range(0, 200)] [Header("Physics: Player strafe Left/Right acceleration")] public float P_SideStrafeAcceleration = 50;
    [Range(0, 200)] [Header("Physics: Player strafe Left/Right speed")] public float P_SideStrafeSpeed = 1;
    [Range(0, 20)] [Header("Audio: Player Footsteps")] public float P_WalkSoundRate = 0.15f;
    //Values: Movement General
    public bool V_Airjump = false;
    public Vector3 V_BoostVelocity;
    public bool V_IsBoosted;                              // For Jumppads / Bounce Pads
    public bool V_IsBouncePadWallDetected;
    public bool V_IsDisplayDustFX;
    public bool V_IsDoubleJumping = false;
    public bool V_IsFloorDetected;                        // True Measure of 'Grounded', since IsPlayerGrounded may be false upon landing on Jump Pad / Bounce Pad / Speed Ramp
    public bool V_IsGrounded = false;
    public bool V_IsHitCeiling = false;
    public bool V_IsJumping = false;
    public bool V_IsLanded = true;
    public bool V_IsQuietWalk = false;
    public bool V_IsSliding;                              // For Speed Ramps
    public bool V_KnockBackOverride = false;
    public float V_MouseSensitivity = 100f;
    public Vector3 V_MoveDirectionNorm = Vector3.zero;
    public float V_RotationX = 0.0f;
    public float V_RotationY = 0.0f;
    public float V_TopVelocity = 0.0f;
    public float V_PlayerFriction = 0.0f;                 // Used to display real time friction values
    public Vector3 V_PlayerVelocity = Vector3.zero;
    public bool V_WishJump = false;
    public Vector2 V_AimDirection = Vector2.zero;
    //Values: Raycasting
    [HideInInspector] public int V_RaycastFloorType = -1;
    [HideInInspector] public Ray[] V_Rays_Ground = new Ray[5];
    [HideInInspector] public Ray V_Ray_Ceiling;
    [HideInInspector] public Ray V_Ray_Velocity;
    [HideInInspector] public RaycastHit[] V_CeilingHits;
    [HideInInspector] public RaycastHit[] V_WallHits;
    [HideInInspector] public RaycastHit[] V_GroundHits;
    [HideInInspector] public RaycastHit V_GroundHit;
    [HideInInspector] public Vector3 V_SpeedReduction;
    [HideInInspector] public Transform V_GroundHitTransform;
    [HideInInspector] public float V_TempJumpSpeed;
    [HideInInspector] public float V_TempRampJumpSpeed;
    [HideInInspector] public string v_GroundHitTransformName;
    [HideInInspector] public MoveCmd moveCmd;
}
