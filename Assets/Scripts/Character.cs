using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Visual representation of a Player - the Character is instantiated by the map once it's loaded.
/// This class handles camera tracking and player movement and is destroyed when the map is unloaded.
/// (I.e. the player gets a new avatar in each map)
/// </summary>

[RequireComponent(typeof(CharacterMoveComponent))]
public class Character : NetworkBehaviour
{
	private App _app;
	[SerializeField] private CharacterComponents m_components;
	[SerializeField] private CharacterMoveComponent m_characterMoveComponent;
	[SerializeField] private CharacterHealthComponent m_characterHealth;
	[SerializeField] private CharacterShootComponent m_characterShoot;
	[SerializeField] private CharacterWeapons m_characterWeapons;
	[SerializeField] private CharacterMuzzleComponent m_characterMuzzle;
	[SerializeField] private CharacterAudioComponent m_characterAudio;
	[SerializeField] private CharacterAnimation m_characterAnimation;
	[SerializeField] private TMP_Text _name;
	[SerializeField] private MeshRenderer _mesh;
	[SerializeField] private GameObject m_characterModel;
	[SerializeField] public Transform m_headingTransform;

	[SerializeField] private float sensitivityX = 15f;
	[SerializeField] private float sensitivityY = 15f;
	[SerializeField] private float minimumX = -360F;
	[SerializeField] private float maximumX = 360F;
	[SerializeField] private float minimumY = -60F;
	[SerializeField] private float maximumY = 60F;
	[SerializeField] private float rotationX = 0f;
	[SerializeField] private float rotationY = 0f;
	[SerializeField] private float m_baseSpeed = 25f;
	[SerializeField] private LayerMask m_localPlayerLayerMask;

	public bool m_inputForward { get; set; }
	public bool m_inputBack { get; set; }
	public bool m_inputRight { get; set; }
	public bool m_inputLeft { get; set; }
	public bool m_inputJump { get; set; }
	public bool m_inputCrouch { get; set; }

	[Networked] public Player Player { get; set; }
	[Networked] Vector2 m_aimDirection { get; set; }

	private void Awake()
    {
		_app = App.FindInstance();
	}

	public override void Spawned()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		SceneCamera.instance.SetSceneCameraActive(false);

		m_characterMoveComponent.InitCharacterMovement(m_characterHealth, (relativeDollyPos) => { m_components.Dolly.GetComponent<CharacterCameraDolly>().SetDollyDir(relativeDollyPos); });
		m_characterHealth.Initialize(this, m_characterModel);
		m_characterAudio.Initialize(m_components.PlayerCamera.GetComponent<AudioSource>());
		m_characterShoot.Initialize(
			character: this,
			characterHealth: m_characterHealth,
			characterCamera: m_components.PlayerCamera.GetComponent<CharacterCamera>(),
			characterWeapons: m_characterWeapons,
			characterMuzzle: m_characterMuzzle,
			muzzleFlash: m_components.MuzzleFlash,
			damageCallback: (damage, instigator) => { m_characterHealth.OnTakeDamage(damage, instigator); },
			audioCallback: (audioClipKey) => { m_characterAudio.OnPlayClip(audioClipKey); },
			ammoCounterCallback: (ammoRemaining, maxAmmoRemaining, clipSize) => { GameUIViewController.Instance.SetAmmoInfo(Object.HasInputAuthority, ammoRemaining, maxAmmoRemaining, clipSize); },
			crosshairCallback: (m_characterShoot) => { GameUIViewController.Instance.GetCrosshair().SetWeaponCrosshair(m_characterShoot); });
		m_characterAnimation.Initialize();
		transform.rotation = Quaternion.identity;
		InterpolationDataSource = InterpolationDataSources.NoInterpolation;
		m_components.Dolly.GetComponent<CharacterCameraDolly>().Initialize(this);

		if (HasInputAuthority && string.IsNullOrWhiteSpace(Player.Name.Value))
		{
			//App.FindInstance().ShowPlayerSetup();
		}
	}

	public void LateUpdate()
	{
		if (Object.HasInputAuthority)
		{
			//if (_camera == null)
			//	_camera = Camera.main.transform;
			//Transform t = _mesh.transform;
			//Vector3 p = t.position;
			//_camera.position = p - 10 * t.forward + 5 * Vector3.up;
			//_camera.LookAt(p + 2 * Vector3.up);
			//m_components.Dolly.GetComponent<CharacterCameraDolly>().UpdatePlayerPosition(t.localPosition.normalized);
			//m_components.CameraContainerTransform.position = p - transform.forward * 4 + transform.up * 2 + transform.right * 2f;
			//m_components.CameraContainerTransform.LookAt(p + 2 * transform.up + transform.forward);
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

	public override void FixedUpdateNetwork()
	{
		if (PlayerInputEnabled() && GetInput(out InputData data))
		{
			//Input Update
			m_inputLeft = m_inputRight = m_inputBack = m_inputForward = false;

			if (data.GetButton(ButtonFlag.LEFT))
			{
				m_inputLeft = true;
			}
			else if (data.GetButton(ButtonFlag.RIGHT))
			{
				m_inputRight = true;
			}
			if (data.GetButton(ButtonFlag.FORWARD))
			{
				m_inputForward = true;
			}
			else if (data.GetButton(ButtonFlag.BACKWARD))
			{
				m_inputBack = true;
			}

			//Jump Update
			if (data.GetButton(ButtonFlag.JUMP))
			{
				m_inputJump = true;
			}
            else
            {
				m_inputJump = false;
			}

			if (data.GetButton(ButtonFlag.CROUCH))
			{
				m_inputCrouch = true;
			}
			else
			{
				m_inputCrouch = false;
			}

			m_aimDirection = data.aimDirection;
		}
	}

	public bool PlayerInputEnabled()
    {
		return (Player && Player.InputEnabled);
    }

	public Vector2 GetAimDirection()
    {
		return m_aimDirection;
	}
}