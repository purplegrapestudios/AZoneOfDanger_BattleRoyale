using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual representation of a Player - the Character is instantiated by the map once it's loaded.
/// This class handles camera tracking and player movement and is destroyed when the map is unloaded.
/// (I.e. the player gets a new avatar in each map)
/// </summary>

public class Character : NetworkBehaviour
{
	[SerializeField] private Transform m_headingTransform;
	[SerializeField] private Text _name;
	[SerializeField] private MeshRenderer _mesh;

	[SerializeField] private float sensitivityX = 15f;
	[SerializeField] private float sensitivityY = 15f;
	[SerializeField] private float minimumX = -360F;
	[SerializeField] private float maximumX = 360F;
	[SerializeField] private float minimumY = -60F;
	[SerializeField] private float maximumY = 60F;
	[SerializeField] private float rotationX = 0f;
	[SerializeField] private float rotationY = 0f;
	[Networked][SerializeField] private bool m_canJump { get; set; }
	[Networked] [SerializeField] private float m_speed { get; set; }
	[Networked] public Player Player { get; set; }
	[Networked] Vector2 m_aimDirection { get; set; }
	[Networked] private Quaternion m_lookRotation { get; set; }
	[Networked] private Vector3 m_directionForward { get; set; }
	[Networked] private Vector3 m_directionRight { get; set; }

	private Transform _camera;

	public override void Spawned()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		m_canJump = true;
		if (HasInputAuthority && string.IsNullOrWhiteSpace(Player.Name.Value))
		{
			//App.FindInstance().ShowPlayerSetup();
		}
	}

	public void LateUpdate()
	{
		if (Object.HasInputAuthority)
		{
			if (_camera == null)
				_camera = Camera.main.transform;
			Transform t = _mesh.transform;
			Vector3 p = t.position;
			_camera.position = p - 10 * t.forward + 5*Vector3.up;
			_camera.LookAt(p+2*Vector3.up);

		}
		
		// This is a little brute-force, but it gets the job done.
		// Could use an OnChanged listener on the properties instead.
		_name.text = Player.Name.Value;
		_mesh.material.color = Player.Color;
	}


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = Cursor.visible ? false : true;
        }

	}

	private IEnumerator JumpCoroutineHandle;
	private IEnumerator JumpCoroutine()
	{
		float startingHeight = transform.position.y;    //Will change implementation later with ground detection as opposed to using position.y < 0
		float jumpheight = transform.position.y + 5f;
		float dt = 0f;

		m_canJump = false;
		while (transform.position.y < jumpheight)
		{
			transform.position += Runner.DeltaTime * 25f * transform.up;
			yield return new WaitForSeconds(Runner.DeltaTime);
            if (dt >= jumpheight)
            {
				Debug.Log("JumpHeight: " + (int)transform.position.y);
            }
		}

		dt = 0;
		yield return new WaitForSeconds(Runner.DeltaTime);

		while (transform.position.y > 0)
		{
			transform.position -= Runner.DeltaTime * 20f * transform.up;
			yield return new WaitForSeconds(Runner.DeltaTime);
			if (transform.position.y < 0)
			{
				transform.position = new Vector3(transform.position.x, startingHeight, transform.position.z);
				m_canJump = true;
				StopCoroutine(JumpCoroutineHandle);
				JumpCoroutineHandle = null;
			}
		}
	}

    //private void OnApplicationFocus(bool focus)
    //{
	//	Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
	//	Cursor.visible = focus ? false : true;
	//}

    public override void FixedUpdateNetwork()
	{
		if (Player && Player.InputEnabled && GetInput(out InputData data))
		{
			if (data.GetButton(ButtonFlag.LEFT))
				transform.position -= Runner.DeltaTime * m_speed * transform.right;
			if (data.GetButton(ButtonFlag.RIGHT))
				transform.position += Runner.DeltaTime * m_speed * transform.right;
			if (data.GetButton(ButtonFlag.FORWARD))
				transform.position += Runner.DeltaTime * m_speed * transform.forward;
			if (data.GetButton(ButtonFlag.BACKWARD))
				transform.position -= Runner.DeltaTime * m_speed * transform.forward;

			if (data.GetButton(ButtonFlag.JUMP) && m_canJump && JumpCoroutineHandle == null)
            {
				JumpCoroutineHandle = JumpCoroutine();
				StartCoroutine(JumpCoroutineHandle);
			}

			m_aimDirection = data.aimDirection;
			//rotationY = ClampAngle(m_aimDirection.y, minimumY, maximumY);		//Don't need for now. But will need later for say head movement up and down.
			Quaternion xQuaternion = Quaternion.AngleAxis(m_aimDirection.x, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);	//Don't need for now. But will need later for say head movement up and down.

			m_lookRotation = transform.rotation *= xQuaternion;

			m_directionForward = m_lookRotation * Vector3.forward;
			m_directionRight = m_lookRotation * Vector3.right;

			return;
		}
	}
	private float ClampAngle(float angle, float min, float max)
	{
		if (angle > -360f)
         angle += 360f;
		if (angle < 360f)
         angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}
}