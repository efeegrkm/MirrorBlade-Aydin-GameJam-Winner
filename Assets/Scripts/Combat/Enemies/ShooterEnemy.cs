using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))] 
public class ShooterEnemy : EnemyBase
{
    [Header("Shooter Settings")]
    [SerializeField] private float stopDistance = 4f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float stopDistanceFluctuation = 1.5f;

    private bool isShooting = false;
    private Animator anim; 

    public override void Initialize(Transform core)
    {
        base.Initialize(core);

        anim = GetComponent<Animator>(); 

        stopDistance = Mathf.Max(0.5f, stopDistance + Random.Range(-stopDistanceFluctuation, stopDistanceFluctuation));
    }

    private void FixedUpdate()
    {
        if (coreTransform == null) return;

        float distanceToCore = Vector2.Distance(transform.position, coreTransform.position);

        if (distanceToCore > stopDistance)
        {
            Vector2 direction = (coreTransform.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

        }
        else if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;
        while (currentHealth > 0)
        {
            if (anim != null)
            {
                anim.SetTrigger("shoot");
            }

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector2 shootDir = (coreTransform.position - firePoint.position).normalized;

            proj.GetComponent<EnemyProjectile>().Initialize(shootDir);

            yield return new WaitForSeconds(fireRate);
        }
    }
}