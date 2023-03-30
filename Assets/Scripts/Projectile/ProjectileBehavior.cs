using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;

public struct ProjectileData : INetworkStruct
{
    public int StartingTick;
    public Vector3 FirePosition;
    public Vector3 MuzzlePosition;
    public Vector3 FireVelocity;
    public bool IsDestroyed;
    public bool HasFiredImpact;
    [Accuracy(0.01f)] public Vector3 ImpactPosition { get; set; }
    [Accuracy(0.01f)] public Vector3 ImpactNormal { get; set; }
}

public class ProjectileBehavior : MonoBehaviour
{
    private Transform tr;
    private ProjectileData m_projectileData;
    private IEnumerator m_destroyBulletCoroutine;
    private System.Action<float, CharacterHealthComponent> m_damageCallback;
    private App m_app;
    private NetworkRunner m_runner;
    private PlayerRef m_instigator;
    private Character m_owner;
    private CharacterMuzzleComponent m_characterMuzzle;

    public float InstigatorImmunityFactor => m_InstigatorImmunityFactor;
    [Range(0f, 1f)] [SerializeField] private float m_InstigatorImmunityFactor = 0.9f;
    public float StartSpeed => m_startSpeed;
    [SerializeField] private float m_startSpeed = 10f;
    public float ProjectileLength => m_projectileLength;
    [SerializeField] private float m_projectileLength = 1f;
    public float LifeSpan => m_lifeSpan;
    [SerializeField] private float m_lifeSpan = 5f;

    public LayerMask m_damagableLayerMask = ~(1 << 9);  //Do not hit itemLayer

    [Header("Interpolation", order = 2)]
    [SerializeField] private float m_interpolationDuration = 0.3f;
    private float m_currentInterpolationTime;
    private Vector3 m_currentInterpolationOffset;

    [Header("Splash Damage")]
    private List<LagCompensatedHit> m_splashHits;

    // Use this for initialization
    private void OnEnable()
    {
        if (tr == null) tr = GetComponent<Transform>();
        if (m_app == null) m_app = App.FindInstance();
        if (m_splashHits == null) m_splashHits = new List<LagCompensatedHit>();
    }

    public void OnFixedUpdateProjectile()
    {

        if (!m_instigator.IsValid) return;
        if (m_runner.Tick - m_projectileData.StartingTick <= 0) DestroyProjectile(m_lifeSpan);

        if (m_projectileData.IsDestroyed)
        {
            if (!m_projectileData.HasFiredImpact)
            {
                ObjectPoolManager.Instance.SpawnImpact(m_projectileData.ImpactPosition, m_projectileData.ImpactNormal, HitTargets.Explosive_1);
                m_projectileData.HasFiredImpact = true;
            }
            DestroyProjectile(0);
            return;
        }

        var lastPosition = GetFixedPosition(m_runner.Tick - 1);
        var nextPosition = GetFixedPosition(m_runner.Tick);
        var dir = nextPosition - lastPosition;
        var distanceToScan = dir.magnitude;
        dir /= distanceToScan;  //Normalize Distance

        if(m_projectileLength > 0)
        {
            float elapsedDistanceSqr = (lastPosition - nextPosition).sqrMagnitude;
            float closestLengthToScan = elapsedDistanceSqr > m_projectileLength * m_projectileLength ? m_projectileLength : Mathf.Sqrt(elapsedDistanceSqr);

            lastPosition -= dir * closestLengthToScan;
            distanceToScan += closestLengthToScan;
        }

        if(m_runner.LagCompensation.Raycast(origin: lastPosition, direction: dir, distanceToScan, player: m_instigator, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IncludePhysX))
        {
            float hitDistance = 100;
            hitDistance = hitInfo.Distance > 0 ? hitInfo.Distance : hitDistance;

            if (hitInfo.Hitbox != null)
            {
                var characterRoot = hitInfo.Hitbox.Root;
                if (!characterRoot.GetComponent<Character>())
                {
                    //We hit a HitBox that is NOT a chacater. Just Explode for now
                    m_projectileData.IsDestroyed = true;
                    m_projectileData.ImpactPosition = hitInfo.Point;
                    m_projectileData.ImpactNormal = hitInfo.Normal;
                    FireImpact(showBlood: true);
                    DestroyProjectile(0);
                    return;
                }

                var damagedCharacter = characterRoot.GetComponent<Character>();
                if (damagedCharacter.Object.InputAuthority != m_instigator)
                {
                    ProjectileSplashDamage(impactPos: hitInfo.Point, radius: 10f, instigator: m_instigator, splashHits: m_splashHits, damagableLayers: m_damagableLayerMask, hitOpts: HitOptions.None);

                    //Debug.Log($"{damagedCharacter.Player.Name} Took Projectile Damage: {(hitInfo.Hitbox.HitboxIndex == 0 ? 75 : 150)}");
                    //m_damageCallback = damagedCharacter.CharacterHealth.OnTakeDamage;
                    //m_damageCallback(hitInfo.Hitbox.HitboxIndex == 0 ? 99 : 200, m_owner.CharacterHealth);   //callback(float dmg, CharacterHealth Instigator)
                    m_projectileData.IsDestroyed = true;
                    m_projectileData.ImpactPosition = hitInfo.Point;
                    m_projectileData.ImpactNormal = hitInfo.Normal;
                    FireImpact(showBlood: true);
                    DestroyProjectile(0);
                }
            }
            else if (hitInfo.Collider != null)
            {
                ProjectileSplashDamage(impactPos: hitInfo.Point, radius: 10f, instigator: m_instigator, splashHits: m_splashHits, damagableLayers: m_damagableLayerMask, hitOpts: HitOptions.None);
                //if (m_runner.LagCompensation.OverlapSphere(origin: hitInfo.Point, radius: 10f, player: m_instigator, hits: m_splashHits, layerMask: m_damagableLayerMask, options: HitOptions.None) > 0)
                //{
                //    foreach (LagCompensatedHit splashHit in m_splashHits)
                //    {
                //        var splashHitRoot = splashHit.Hitbox.Root;
                //        if (splashHit.Hitbox.HitboxIndex > 0) continue; //No Headshotting when calculating splash damage
                //
                //        if (splashHitRoot.GetComponent<Character>())
                //        {
                //            var splashHitCharacter = splashHitRoot.GetComponent<Character>();
                //            var splashHitMoveComp = splashHitCharacter.CharacterMoveComponent;
                //            var splashDir = (hitInfo.Point - splashHitCharacter.transform.position) * -1;
                //            var oldVel = splashHitMoveComp.m_moveData.V_PlayerVelocity;
                //            //splashHitMoveComp.m_moveData.P_Friction = false;
                //            splashHitMoveComp.m_moveData.V_PlayerVelocity += splashDir * 5f;
                //
                //            if (splashHitCharacter.Object.InputAuthority != m_instigator || 1==1)//Just allow for now, since we want splash damage to ourselves. We'll handle it better next time.
                //            {
                //                var sqrMagnitudeDist = Vector3.SqrMagnitude(hitInfo.Point - splashHitCharacter.transform.position);
                //                var radiusSquare = 10f * 10;
                //                var maxDamage = 100f;
                //                var splashDamage = ((radiusSquare - sqrMagnitudeDist) / radiusSquare) * maxDamage;
                //                Debug.Log($"We Hit {splashHitCharacter} with Dmg: {splashDamage}. sqrDist {sqrMagnitudeDist}");
                //                m_damageCallback = splashHitCharacter.CharacterHealth.OnTakeDamage;
                //                m_damageCallback(splashDamage, m_owner.CharacterHealth);   //callback(float dmg, CharacterHealth Instigator)
                //            }
                //        }
                //    }
                //}
                m_projectileData.IsDestroyed = true;
                m_projectileData.ImpactPosition = hitInfo.Point;
                m_projectileData.ImpactNormal = hitInfo.Normal;
                FireImpact(showBlood: m_splashHits.Count > 0);
                DestroyProjectile(0);


                //if (hitInfo.Collider.CompareTag("Level"))
                //{
                //    Debug.Log($"We hit Level: {hitInfo.Collider.transform.name}, Pos: {hitInfo.Point}");
                //    m_projectileData.ImpactPosition = hitInfo.Point;
                //    m_projectileData.ImpactNormal = hitInfo.Normal;
                //    m_projectileData.IsDestroyed = true;
                //    FireImpact(showBlood: false);
                //}
                DestroyProjectile(0);
            }
        }
    }

    private void ProjectileSplashDamage(Vector3 impactPos, float radius, PlayerRef instigator, List<LagCompensatedHit> splashHits, int damagableLayers = -1, HitOptions hitOpts = HitOptions.None)
    {
        if (m_runner.LagCompensation.OverlapSphere(origin: impactPos, radius: radius, player: instigator, hits: splashHits, layerMask: damagableLayers, options: hitOpts) > 0)
        {
            foreach (LagCompensatedHit splashHit in m_splashHits)
            {
                var splashHitRoot = splashHit.Hitbox.Root;
                if (splashHit.Hitbox.HitboxIndex > 0) continue; //No Headshotting when calculating splash damage

                if (splashHitRoot.GetComponent<Character>())
                {
                    var splashHitCharacter = splashHitRoot.GetComponent<Character>();
                    var splashHitMoveComp = splashHitCharacter.CharacterMoveComponent;
                    var splashDir = (impactPos - splashHitCharacter.transform.position) * -1;
                    var oldVel = splashHitMoveComp.m_moveData.V_PlayerVelocity;
                    ref Vector3 refVelocity = ref splashHitMoveComp.m_moveData.V_PlayerVelocity;
                    if (refVelocity.y < 0)
                        refVelocity = new Vector3(refVelocity.x, 0, refVelocity.z);
                    refVelocity += splashDir * 10f;

                    var sqrMagnitudeDist = Vector3.SqrMagnitude(impactPos - splashHitCharacter.transform.position);
                    var radiusSquare = 10f * 10;
                    var maxDamage = 100f * (splashHitCharacter.Object.InputAuthority == m_instigator ? (1f - m_InstigatorImmunityFactor) : 1f);
                    var splashDamage = Mathf.Max(0f, ((radiusSquare - sqrMagnitudeDist) / radiusSquare) * maxDamage);
                    Debug.Log($"We Hit {splashHitCharacter} with Dmg: {splashDamage}. sqrDist {sqrMagnitudeDist}");
                    m_damageCallback = splashHitCharacter.CharacterHealth.OnTakeDamage;
                    m_damageCallback(splashDamage, m_owner.CharacterHealth);   //callback(float dmg, CharacterHealth Instigator)
                }
            }
        }

    }

    public void OnRenderProjectile()
    {
        if (m_projectileData.IsDestroyed) return;

        var targetPosition = GetRenderPosition(m_runner.Tick);
        float interpolationProgress = 0f;

        if (targetPosition == m_projectileData.FirePosition)
        {
            m_currentInterpolationTime = 0f;
            m_currentInterpolationOffset = m_characterMuzzle.NetworkedMuzzlePosition - m_projectileData.FirePosition;
        }
        else
        {
            m_currentInterpolationTime += Time.deltaTime;

            //We'll use InterpolationTime Squared for the interpolation progress
            interpolationProgress = Mathf.Clamp01((m_currentInterpolationTime * m_currentInterpolationTime) / m_interpolationDuration);
        }
        var offset = Vector3.Lerp(m_currentInterpolationOffset, Vector3.zero, interpolationProgress);
        var nextPos = targetPosition + offset;
        var curPos = tr.position;
        var interpolationDirection = nextPos - curPos;

        tr.position = nextPos;

        if (interpolationDirection != Vector3.zero)
        {
            tr.rotation = Quaternion.LookRotation(interpolationDirection);
        }
    }

    private Vector3 GetFixedPosition(int currentTick)
    {
        var elapsedTicks = currentTick - m_projectileData.StartingTick;
        return (elapsedTicks <= 0) ? m_projectileData.FirePosition : m_projectileData.FirePosition + m_projectileData.FireVelocity * elapsedTicks * m_runner.DeltaTime;
    }

    private Vector3 GetRenderPosition(int currentTick)
    {
        var elapsedTicks = currentTick - m_projectileData.StartingTick;
        return (elapsedTicks <= 0) ? m_projectileData.MuzzlePosition : m_projectileData.MuzzlePosition + m_projectileData.FireVelocity * elapsedTicks * m_runner.DeltaTime;
    }

    private void DestroyProjectile(float time)
    {
        if (m_destroyBulletCoroutine != null)
        {
            StopCoroutine(m_destroyBulletCoroutine);
            m_destroyBulletCoroutine = DestroyProjectileCO(time);
            StartCoroutine(m_destroyBulletCoroutine);
            //return;
        }
        m_destroyBulletCoroutine = DestroyProjectileCO(time);
        StartCoroutine(m_destroyBulletCoroutine);
    }

    private IEnumerator DestroyProjectileCO(float time)
    {
        yield return new WaitForSeconds(time);
        m_projectileData.IsDestroyed = true;
        ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnFixedUpdateProjectile);
        ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnRenderProjectile);
        ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.projectileList);
    }

    private void FireImpact(bool showBlood)
    {
        if (!m_projectileData.HasFiredImpact)
        {
            if (showBlood) ObjectPoolManager.Instance.SpawnImpact(m_projectileData.ImpactPosition, m_projectileData.ImpactNormal, HitTargets.Player);
            ObjectPoolManager.Instance.SpawnImpact(m_projectileData.ImpactPosition, m_projectileData.ImpactNormal, HitTargets.Explosive_1);
            m_projectileData.HasFiredImpact = true;
        }

    }

    public void SetProjectileData(NetworkRunner runner, PlayerRef instigator, Character owner, int startingTick, Vector3 firePosition, Vector3 muzzlePosition, Vector3 fireVelocity)
    {
        m_runner = runner;
        m_instigator = instigator;
        m_owner = owner;
        m_characterMuzzle = m_owner.CharacterMuzzle;
        m_projectileData.StartingTick = startingTick;
        m_projectileData.FirePosition = firePosition;
        m_projectileData.MuzzlePosition = m_characterMuzzle.NetworkedMuzzlePosition;// muzzlePosition;
        m_projectileData.HasFiredImpact = false;
        m_projectileData.IsDestroyed = false;

        m_projectileData.FireVelocity = fireVelocity;
        ObjectPoolManager.Instance.SubscribeToProjectileUpdate(OnFixedUpdateProjectile);
        ObjectPoolManager.Instance.SubscribeToProjectileUpdate(OnRenderProjectile);
    }
}