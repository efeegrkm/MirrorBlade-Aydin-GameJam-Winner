using UnityEngine;

public enum DamageSource
{
    Player,
    Mirror,
    Enemy,
    Projectile,
    SelfDamage
}

public enum DamageVulnerability
{
    All,
    PlayerOnly,
    MirrorOnly
}

public interface IDamageable
{
    void TakeDamage(int amount, DamageSource source);
}