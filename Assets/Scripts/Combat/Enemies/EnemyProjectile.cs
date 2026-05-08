using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private float speedFluctuation = 1.5f;
    private Vector2 moveDirection;
    private bool isParried = false;
    private int parriedDamage = 0;

    private float actualSpeed;
    public void Initialize(Vector2 direction)
    {
        moveDirection = direction;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 10f);
    }
    private void Start()
    {
        actualSpeed = Mathf.Max(0.5f, speed + Random.Range(-speedFluctuation, speedFluctuation));
    }
    private void Update()
    {
        transform.Translate(Vector3.right * actualSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Core") && !isParried)
        {
            IDamageable core = collision.GetComponent<IDamageable>();
            core?.TakeDamage(defaultDamage, DamageSource.Enemy);
            GameEvents.OnSpawnExplosion?.Invoke(transform.position, 1f);
            Destroy(gameObject);
        }
        else if (isParried && collision.CompareTag("Enemy"))
        {
            IDamageable enemy = collision.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(parriedDamage, DamageSource.Projectile);
                GameEvents.OnSpawnExplosion?.Invoke(transform.position, 1f);
                Destroy(gameObject);
            }
        }
    }

    public void Parry(Vector2 newDirection, int playerDamageAmount)
    {
        isParried = true;
        parriedDamage = playerDamageAmount;

        Initialize(newDirection.normalized);

        gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
    }
}