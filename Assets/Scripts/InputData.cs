using Fusion;
using UnityEngine;

[System.Flags]
public enum ButtonFlag
{
	FORWARD = 1 << 0,
	BACKWARD = 1 << 1,
	LEFT = 1 << 2,
	RIGHT = 1 << 3,
	RESPAWN = 1 << 4,
	JUMP = 1 << 5,
	CROUCH = 1 << 6,
	FIRE = 1 << 7,
	AIM = 1 << 8,
	RELOAD = 1 << 9,
	WEAPON_00 = 1 << 10,
	WEAPON_01 = 1 << 11,
	WEAPON_02 = 1 << 12,
}

public struct InputData : INetworkInput
{
	public ButtonFlag ButtonFlags;
	public Vector2 aimDirection;
	public Vector2 moveDirection;
	public Vector3 cameraForward;

	public bool GetButton(ButtonFlag button)
	{
		return (ButtonFlags & button) == button;
	}
}