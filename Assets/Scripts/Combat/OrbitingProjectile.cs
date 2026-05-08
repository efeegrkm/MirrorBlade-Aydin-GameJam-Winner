using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class OrbitingProjectile : MonoBehaviour
{
    public float orbitSpeed = 150f; 
    public float orbitRadius = 2.5f;
    public int damage = 1;
    public float damageCooldown = 0.5f;

    private Transform centerTarget;
    private float currentAngle;
    private Dictionary<Collider2D, float> hitCooldowns = new Dictionary<Collider2D, float>();

    public void Initialize(Transform center, float initialAngle)
    {
        centerTarget = center;
        currentAngle = initialAngle;
    }

    private void Update()
    {
        if (centerTarget == null) return;

        currentAngle += orbitSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0) * orbitRadius;
        transform.position = centerTarget.position + offset;

        List<Collider2D> keys = new List<Collider2D>(hitCooldowns.Keys);
        foreach (var key in keys)
        {
            if (hitCooldowns[key] > 0) hitCooldowns[key] -= Time.deltaTime;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!hitCooldowns.ContainsKey(collision) || hitCooldowns[collision] <= 0)
            {
                IDamageable enemy = collision.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, DamageSource.Player);
                    hitCooldowns[collision] = damageCooldown; 
                }
            }
        }
    }
}