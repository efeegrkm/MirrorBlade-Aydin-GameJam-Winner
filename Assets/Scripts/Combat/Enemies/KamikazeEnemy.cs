using UnityEngine;

public class KamikazeEnemy : EnemyBase
{
    private void FixedUpdate()
    {
        if (coreTransform == null) return;

        // Merkeze doðru sürekli hareket et
        Vector2 direction = (coreTransform.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

        // TODO: Yürüme animasyon blend tree'sini direction vektörü ile güncelle
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Core'a çarptý mý? (Core objesinin Tag'ini "Core" yapmalýsýn)
        if (collision.CompareTag("Core"))
        {
            DamageCore(); // Vur ve patla
        }
    }
}