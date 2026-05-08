using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<int> OnMirrorCountChanged;

    public static Action OnPlayerAttack;

    public static Action<int, int> OnCoreHealthChanged; 

    public static Action<int, int> OnWaveChanged; 

    public static Action<int> OnEnemyKilled; 

    public static Action<int> OnSoulCountChanged;

    public static Action<int> OnKillStreakAdded;

    public static Action OnKillStreakReset;    

    public static Action<int> OnKillStreakUIUpdated;

    public static Action OnPlayerDash;

    public static Action<float, float> OnDashManaChanged;

    public static Action<Vector3, string, Color> OnShowFloatingText;
    public enum SoundType { Attack, EnemyDeath, WalkStep, UIClick, Dash }
    public static Action<SoundType> OnPlaySound;

    public static Action<Vector3, float> OnSpawnExplosion;
}
