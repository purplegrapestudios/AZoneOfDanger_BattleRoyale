using UnityEngine;
using Fusion;
using System.Collections;

public class ProjectileBehavior : NetworkBehaviour
{

    Coroutine CoroutineMoveFromTo;
    Transform tr;
    Vector3 lastPosition;
    public LayerMask m_damagableLayerMask = ~(1 << 9);  //Do not hit itemLayer
    Vector3 direction;
    Ray ray;
    RaycastHit[] bulletHits;
    Transform hitTransform;

    public float rayCastDistance = 5f;
    public float bulletSpeed = 10f;

    [SerializeField] private Vector3 targetPosition;

    [SerializeField] private PlayerRef m_ownerRef;
    [SerializeField] private Character m_ownerCharacter;

    private IEnumerator m_destroyBulletCoroutine;
    private App m_app;
    private NetworkRunner m_runner;
    System.Action<float, CharacterHealthComponent> m_damageCallback;

    // Use this for initialization
    private void OnEnable()
    {
        tr = GetComponent<Transform>();
        lastPosition = tr.position;
        bulletHits = new RaycastHit[255];
        m_app = App.FindInstance();
    }

    bool ret;
    public void OnProjectileFixedUpdate()
    {
        //if (ret) return;
        //
        //if (!m_app)
        //    m_app = App.FindInstance();
        //
        //if (m_app ?? null == null)
        //{
        //    Debug.Log("No App");
        //    ret = true;
        //}
        //if (m_app?.Session ?? null == null)
        //{
        //    Debug.Log("No Session");
        //    ret = true;
        //}
        //if (m_app?.Session?.Runner ?? null == null)
        //{
        //    Debug.Log("No Session Runner");
        //    ret = true;
        //}
        //if (m_app?.Session?.Object ?? null == null)
        //{
        //    Debug.Log("No Session Object");
        //    ret = true;
        //}

        if (!m_ownerRef.IsValid) return;

        //Debug.LogWarning($"Bullet {name}, OwnerID: {m_ownerRef.PlayerId}, deltaTime= {m_app.Session.Runner.DeltaTime}");
        ray = new Ray(lastPosition, direction);

        m_runner.LagCompensation.Raycast(origin: lastPosition, direction: direction, transform.localScale.z / 2, player: m_ownerRef, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IncludePhysX);

        float hitDistance = 100;
        if (hitInfo.Distance > 0)
            hitDistance = hitInfo.Distance;


        if (hitInfo.Hitbox != null)
        {
            var characterRoot = hitInfo.Hitbox.Root;
            if (characterRoot.GetComponent<Character>().Object.Id != m_ownerCharacter.Object.Id)
            {
                //Debug.Log($"We hit a HitBox Object: {hitInfo.Hitbox.transform.root.name}, Pos: {hitInfo.Point}");
                ObjectPoolManager.Instance.SpawnImpact(hitInfo.Point, hitInfo.Normal, HitTargets.Player);

                if (HasStateAuthority)
                {
                    Debug.Log($"{characterRoot.GetComponent<Character>().Player.Name} took {5} damage");
                    if (hitInfo.Hitbox.HitboxIndex == 0)
                        m_damageCallback(75, GetComponent<CharacterHealthComponent>());
                    else
                        m_damageCallback(150, GetComponent<CharacterHealthComponent>());
                }
                DestroyProjectile();

            }
        }
        else if (hitInfo.Collider != null)
        {
            //Debug.Log($"We hit a Physx Object: {hitInfo.Collider.transform.name}, Pos: {hitInfo.Point}");
            ObjectPoolManager.Instance.SpawnImpact(hitInfo.Point, hitInfo.Normal, HitTargets.Environment);
        }
        else
        {
            tr.position += direction.normalized * bulletSpeed * m_runner.DeltaTime;
        }
        lastPosition = tr.position;

        //if (Physics.RaycastNonAlloc(ray, bulletHits, rayCastDistance, m_damagableLayerMask) > 0)
        //{
        //    foreach (RaycastHit hit in bulletHits)
        //    {
        //        if (hit.collider == null) continue;
        //
        //        hitTransform = hit.transform;
        //        if (hitTransform.CompareTag("Player"))
        //        {
        //            if (hitTransform.GetComponent<PlayerRef>() == null) continue;
        //
        //            if (hitTransform.GetComponent<PlayerRef>().PlayerId != m_ownerRef.PlayerId)
        //            {
        //                ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnBulletFixedUpdate);
        //                ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletList);
        //                return;
        //            }
        //            else
        //            {
        //                tr.position += direction.normalized * bulletSpeed * m_app.Session.Runner.DeltaTime;
        //            }
        //        }
        //        else
        //        {
        //            ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnBulletFixedUpdate);
        //            ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletList);
        //            return;
        //        }
        //    }
        //}
        //else
        //{
        //    tr.position += direction.normalized * bulletSpeed * m_app.Session.Runner.DeltaTime;
        //}
        //lastPosition = tr.position;
    }

    private void DestroyProjectile()
    {
        if (m_destroyBulletCoroutine != null)
        {
            StopCoroutine(m_destroyBulletCoroutine);
            m_destroyBulletCoroutine = null;
        }
        m_destroyBulletCoroutine = DestroyProjectileCO(0f);
        StartCoroutine(m_destroyBulletCoroutine);
    }

    private IEnumerator DestroyProjectileCO(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnProjectileFixedUpdate);
        ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.projectileList);
    }

    public void SetBulletDirection(Vector3 dir)
    {
        direction = dir;
    }

    public void SetOwner(PlayerRef ownerRef, Character ownerCharacter)
    {
        m_ownerRef = ownerRef;
        m_ownerCharacter = ownerCharacter;
        ObjectPoolManager.Instance.SubscribeToProjectileUpdate(OnProjectileFixedUpdate);
    }

    public void SetRunner(NetworkRunner runner)
    {
        m_runner = runner;
    }

    public void SetDamageCallback(System.Action<float, CharacterHealthComponent> damageCallback)
    {
        m_damageCallback = damageCallback;
    }
}
