using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int damageToCore = 1;
    [SerializeField] protected int soulValue = 1;
    [SerializeField] protected float speedFluctuation = 0.5f;

    [Header("Vulnerabilities")]
    [SerializeField] protected DamageVulnerability vulnerability = DamageVulnerability.All;

    protected int currentHealth;
    protected Transform coreTransform;
    protected Rigidbody2D rb;

    public virtual void Initialize(Transform core)
    {
        coreTransform = core;
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        moveSpeed = Mathf.Max(0.1f, moveSpeed + Random.Range(-speedFluctuation, speedFluctuation));
    }

    public virtual void TakeDamage(int amount, DamageSource source)
    {
        if (vulnerability == DamageVulnerability.PlayerOnly && source != DamageSource.Player) return;
        if (vulnerability == DamageVulnerability.MirrorOnly && source != DamageSource.Mirror) return;

        currentHealth -= amount;

        GameEvents.OnShowFloatingText?.Invoke(transform.position, "-" + amount.ToString(), Color.red);

        if (currentHealth <= 0)
        {
            Die(source);
        }
    }

    protected virtual void Die(DamageSource source)
    {
        GameEvents.OnPlaySound?.Invoke(GameEvents.SoundType.EnemyDeath);
        if (source == DamageSource.Player || source == DamageSource.Mirror)
        {
            GameEvents.OnEnemyKilled?.Invoke(soulValue);
            GameEvents.OnKillStreakAdded?.Invoke(1);
            GameEvents.OnShowFloatingText?.Invoke(transform.position, "+" + soulValue.ToString() + " Soul", Color.yellow);
        }
        else if (source == DamageSource.Projectile)
        {
            GameEvents.OnEnemyKilled?.Invoke(soulValue * 2);
            GameEvents.OnKillStreakAdded?.Invoke(2);
            GameEvents.OnShowFloatingText?.Invoke(transform.position, "+" + (soulValue * 2).ToString() + " Soul", new Color(1f, 0.8f, 0f));
        }
        else if (source == DamageSource.SelfDamage)
        {
            GameEvents.OnKillStreakReset?.Invoke();
        }
        GameEvents.OnSpawnExplosion?.Invoke(transform.position, 0.3f);
        Destroy(gameObject);
    }

    protected void DamageCore()
    {
        IDamageable coreDamageable = coreTransform.GetComponent<IDamageable>();
        if (coreDamageable != null)
        {
            coreDamageable.TakeDamage(damageToCore, DamageSource.Enemy);
            Die(DamageSource.SelfDamage);
        }
    }
}