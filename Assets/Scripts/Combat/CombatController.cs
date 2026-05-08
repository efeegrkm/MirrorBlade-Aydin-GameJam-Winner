using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CombatController : MonoBehaviour
{
    [Header("Combat Settings")]
    public DamageSource myDamageSource = DamageSource.Player;
    public int playerDamage = 1;
    public float damageDuration = 0.2f;
    public float parryAngleFluctuation = 15f;

    [Header("Attack Hitbox")]
    public Transform attackPoint;
    public float attackRange = 2f;
    public float attackWidth = 1.5f;

    [Header("Dash Hitbox")]
    public float dashAttackRange = 3.5f; 
    public float dashAttackWidth = 1.5f;

    [Header("Layer Masks")]
    public LayerMask enemyLayer;
    public LayerMask projectileLayer;

    private bool isAttacking = false;
    private bool isDashAttacking = false;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerAttack += InitiateAttack;
        GameEvents.OnPlayerDash += InitiateDashAttack;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerAttack -= InitiateAttack;
        GameEvents.OnPlayerDash -= InitiateDashAttack;
    }

    private void InitiateAttack()
    {
        if (!isAttacking)
        {
            GameEvents.OnPlaySound?.Invoke(GameEvents.SoundType.Attack);
            StartCoroutine(AttackRoutine());
        }
    }

    private void InitiateDashAttack()
    {
        if (!isDashAttacking) StartCoroutine(DashAttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        float timer = 0f;
        if (anim != null) anim.SetTrigger("attack");

        HashSet<Collider2D> processedTargets = new HashSet<Collider2D>();

        while (timer < damageDuration)
        {
            ProcessHitboxes(attackWidth, attackRange, processedTargets);
            timer += Time.deltaTime;
            yield return null;
        }
        isAttacking = false;
    }

    private IEnumerator DashAttackRoutine()
    {
        isDashAttacking = true;
        float timer = 0f;
        float duration = 0.15f;

        HashSet<Collider2D> processedTargets = new HashSet<Collider2D>();

        while (timer < duration)
        {
            ProcessHitboxes(dashAttackWidth, dashAttackRange, processedTargets);
            timer += Time.deltaTime;
            yield return null;
        }
        isDashAttacking = false;
    }

    private void ProcessHitboxes(float currentWidth, float currentRange, HashSet<Collider2D> processedTargets)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(currentWidth, currentRange), transform.eulerAngles.z, enemyLayer);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (!processedTargets.Contains(enemyCollider))
            {
                processedTargets.Add(enemyCollider);
                IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
                damageable?.TakeDamage(playerDamage, myDamageSource);
            }
        }

        Collider2D[] hitProjectiles = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(currentWidth, currentRange), transform.eulerAngles.z, projectileLayer);
        foreach (Collider2D projCollider in hitProjectiles)
        {
            if (!processedTargets.Contains(projCollider))
            {
                processedTargets.Add(projCollider);
                EnemyProjectile projectile = projCollider.GetComponent<EnemyProjectile>();
                if (projectile != null)
                {
                    Transform target = FindNearestEnemy(projectile.transform.position);
                    Vector2 baseParryDir = target != null ? (Vector2)(target.position - projectile.transform.position) : (Vector2)transform.up;
                    float randomAngle = Random.Range(-parryAngleFluctuation, parryAngleFluctuation);
                    Vector2 deviatedParryDir = Quaternion.Euler(0, 0, randomAngle) * baseParryDir;
                    projectile.Parry(deviatedParryDir, playerDamage);
                }
            }
        }
    }

    private Transform FindNearestEnemy(Vector2 startPos)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(startPos, 20f, enemyLayer);
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            float dist = Vector2.Distance(startPos, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, transform.rotation, Vector3.one);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(attackWidth, attackRange, 0));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(dashAttackWidth, dashAttackRange, 0));
    }
}