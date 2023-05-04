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
public class Character : NetworkBehaviour, IBeforeTick, IBeforeUpdate
{
	private App _app;
	[SerializeField] private CharacterComponents m_components;
	public NetworkRigidbody NetworkRigidbody => m_networkRigidbody;
	[SerializeField] private NetworkRigidbody m_networkRigidbody;
	public CharacterMoveComponent CharacterMove => m_characterMoveComponent;
	[SerializeField] private CharacterMoveComponent m_characterMoveComponent;
	public CharacterHealthComponent CharacterHealth => m_characterHealth;
	[SerializeField] private CharacterHealthComponent m_characterHealth;
	public CharacterShootComponent CharacterShoot => m_characterShoot;
	[SerializeField] private CharacterShootComponent m_characterShoot;
	public CharacterWeapons CharacterWeapons => m_characterWeapons;
	[SerializeField] private CharacterWeapons m_characterWeapons;
	public CharacterMuzzleComponent CharacterMuzzle => m_characterMuzzle;
	[SerializeField] private CharacterMuzzleComponent m_characterMuzzle;
	public CharacterAudioComponent CharacterAudio => m_characterAudio;
	[SerializeField] private CharacterAudioComponent m_characterAudio;
	public CharacterAnimation CharacterAnimation => m_characterAnimation;
	[SerializeField] private CharacterAnimation m_characterAnimation;
	public RenderedShotDolly CharacterMuzzleDolly => m_characterMuzzleDolly;
	[SerializeField] private RenderedShotDolly m_characterMuzzleDolly;
	public CharacterCameraDolly CharacterCameraDolly => m_characterCameraDolly;
	[SerializeField] private CharacterCameraDolly m_characterCameraDolly;
	public CharacterCamera CharacterCamera => m_characterCamera;
	[SerializeField] private CharacterCamera m_characterCamera;
	public ParticleSystem CharacterMuzzleFlash => m_muzzleFlash;
	[SerializeField] private ParticleSystem m_muzzleFlash;
	public Animator CharacterAnimator3rd => m_characterAnimator3rd;
	[SerializeField] private Animator m_characterAnimator3rd;
	public Animator CharacterAnimator1st => m_characterAnimator1st;
	[SerializeField] private Animator m_characterAnimator1st;
	public Hitbox CharactHitboxBody => m_characterHitboxBody;
	[SerializeField] private Hitbox m_characterHitboxBody;
	public CapsuleCollider CapsuleCollider => m_capsuleCollider;
	[SerializeField] private CapsuleCollider m_capsuleCollider;
	public CharacterRagdollComponent CharacterRagdoll => m_characterRagdoll;
	[SerializeField] private CharacterRagdollComponent m_characterRagdoll;
	public GameObject MainModel => m_mainModel;
	[SerializeField] private GameObject m_mainModel;
	public GameObject RagDollModel => m_ragDollModel;
	[SerializeField] private GameObject m_ragDollModel;
	public SkinnedMeshRenderer SkinnedMesh => m_skinnedMesh;
	[SerializeField] private SkinnedMeshRenderer m_skinnedMesh;
	public MinimapWorldObject MinimapWorldObj => m_minimapWorldObj;
	[SerializeField] private MinimapWorldObject m_minimapWorldObj;

	[SerializeField] private TMP_Text _name;
	[SerializeField] private MeshRenderer _mesh;
	[SerializeField] private GameObject m_characterModel;
	[SerializeField] public Transform m_headingTransform;

	public bool m_inputForward { get; set; }
	public bool m_inputBack { get; set; }
	public bool m_inputRight { get; set; }
	public bool m_inputLeft { get; set; }
	public bool m_inputJump { get; set; }
	public bool m_inputCrouch { get; set; }

	public Vector2 RenderAimDirDelta => m_renderAimDirDelta;
	private Vector2 m_renderAimDirDelta;
	public Vector2 CachedAimDirDelta => m_cachedAimDirDelta;
	private Vector2 m_cachedAimDirDelta;
	public Vector2 FixedAimDirDelta => m_fixedAimDirDelta;
	private Vector2 m_fixedAimDirDelta;
	private Vector2 m_lastKnownAimDir;

	private (Vector3 hitBoxExtents, Vector3 offset) m_hitboxSettingsCrouch = (new Vector3(1, 1.5f, 1), new Vector3(0, 0, 0));
	private (Vector3 hitBoxExtents, Vector3 offset) m_hitboxSettingsUpright = (new Vector3(1, 2, 1), new Vector3(0, -0.4f, 0));


	[Networked] public Player Player { get; set; }
	[Networked] Vector2 m_aimDirectionDelta { get; set; }

	private void Awake()
    {
		_app = App.FindInstance();
	}

	public override void Spawned()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		SceneCamera.Instance.SetSceneCameraActive(false);
		m_characterMoveComponent.InitCharacterMovement(m_characterHealth, 
			(relativeDollyPos) => { m_characterCameraDolly.SetDollyDir(relativeDollyPos); }, 
			(value) => { SetHitboxCrouch(value); });
		m_characterHealth.Initialize(this, m_characterModel);
		m_characterRagdoll.Init(this);
		m_characterAudio.Initialize(m_characterCamera.GetComponent<AudioSource>());
		m_characterShoot.Initialize(
			character: this,
			characterHealth: m_characterHealth,
			characterCamera: m_characterCamera,
			characterWeapons: m_characterWeapons,
			characterMuzzle: m_characterMuzzle,
			muzzleFlash: m_muzzleFlash,
			damageCallback: (hitData) => { m_characterHealth.OnTakeDamage(hitData); },
			audioCallback: (audioClipKey) => { m_characterAudio.OnPlayClip(audioClipKey); },
			ammoCounterCallback: (ammoRemaining, maxAmmoRemaining, clipSize) => { GameUIViewController.Instance.SetAmmoInfo(Object.HasInputAuthority, ammoRemaining, maxAmmoRemaining, clipSize); },
			crosshairCallback: (m_characterShoot) => { GameUIViewController.Instance.GetCrosshair().SetWeaponCrosshair(m_characterShoot); },
			aimCallback: (fov) => { m_characterCamera.SetCameraFOV(fov); });
		m_characterAnimation.Initialize(this);
		transform.rotation = Quaternion.identity;
		InterpolationDataSource = InterpolationDataSources.NoInterpolation;
		m_characterCameraDolly.Initialize(this);
		m_characterMuzzleDolly.Initialize(this, m_characterCamera);
		m_minimapWorldObj.Init(this);
		_name.text = $"PlayerID: {Player.Object.InputAuthority.PlayerId}";

		if (HasInputAuthority && string.IsNullOrWhiteSpace(Player.Name.Value))
		{
			//App.FindInstance().ShowPlayerSetup();
		}
	}

	private void SetHitboxCrouch(bool crouch)
	{
		m_characterHitboxBody.BoxExtents = crouch ? m_hitboxSettingsCrouch.hitBoxExtents : m_hitboxSettingsUpright.hitBoxExtents;
		m_characterHitboxBody.Offset = crouch ? m_hitboxSettingsCrouch.offset : m_hitboxSettingsUpright.offset;
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
		//////_name.text = Player.Name.Value;
		//////_mesh.material.color = Player.Color;
	}


	private void Update()
	{
		if (!HasInputAuthority) return;

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SwitchCursorMode(shouldUnlock: Cursor.lockState == CursorLockMode.Locked);
		}
	}

	void IBeforeTick.BeforeTick()
	{
		if (!m_characterHealth.NetworkedIsAlive) return;

		m_fixedAimDirDelta = m_lastKnownAimDir;
		if (PlayerInputEnabled() && GetInput(out InputData data))
		{
			m_fixedAimDirDelta = data.aimDirectionDelta;
			m_lastKnownAimDir = m_fixedAimDirDelta;
		}

	}

    void IBeforeUpdate.BeforeUpdate()
    {
		if (PlayerInputEnabled() && Runner.LocalPlayer == Object.InputAuthority)
		{
			float DeltaTime = Runner.DeltaTime;
			m_renderAimDirDelta = new Vector2(Input.GetAxis("Mouse X") * DeltaTime, Input.GetAxis("Mouse Y") * DeltaTime);
			//Debug.Log($"RenderAimData: {m_renderAimDirDelta}");
			m_cachedAimDirDelta += m_renderAimDirDelta;

			if (_app.ResetCachedInput)
			{
				_app.ResetCachedInput = false;
				m_cachedAimDirDelta = Vector2.zero;
				m_renderAimDirDelta = Vector2.zero;
			}
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

			m_aimDirectionDelta = data.aimDirectionDelta;
		}
	}

	public void SwitchCursorMode(bool shouldUnlock)
    {
		Cursor.lockState = shouldUnlock ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = shouldUnlock ? true : false;

	}

	public bool PlayerInputEnabled()
    {
		return (Player && Player.InputEnabled);
    }

	public Vector2 GetAimDirectionDelta()
    {
		return m_aimDirectionDelta;
	}

	public Vector3 GetAimDirection()
	{
		var rot = GetComponent<NetworkRigidbody>().ReadRotation() * Quaternion.AngleAxis(m_characterCamera.NetworkedRotationY, Vector3.left);
		return rot * Vector3.forward;
	}

	public Vector3 GetForward()
    {
		return GetComponent<NetworkRigidbody>().ReadRotation() * Vector3.forward;
	}
	public Vector3 GetUp()
	{
		return GetComponent<NetworkRigidbody>().ReadRotation() * Vector3.up;
	}

}