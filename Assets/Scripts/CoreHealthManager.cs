using UnityEngine;

public class CoreHealthManager : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        GameEvents.OnCoreHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount, DamageSource source)
    {
        if (source != DamageSource.Enemy) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        GameEvents.OnCoreHealthChanged?.Invoke(currentHealth, maxHealth);

        GameEvents.OnShowFloatingText?.Invoke(transform.position, "-" + amount.ToString(), new Color(0.8f, 0f, 0f));
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }
    public void UpgradeMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        GameEvents.OnCoreHealthChanged?.Invoke(currentHealth, maxHealth);

        GameEvents.OnShowFloatingText?.Invoke(transform.position, "+Max Health", Color.green);
    }
    private void GameOver()
    {
        Debug.Log("GAME OVER! Core yok edildi.");
    }
}