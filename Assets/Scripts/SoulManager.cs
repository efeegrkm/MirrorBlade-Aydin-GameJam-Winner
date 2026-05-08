using UnityEngine;

public class SoulManager : MonoBehaviour
{
    private int currentSouls = 0;
    public int currentKillStreak = 0; 

    private void OnEnable()
    {
        GameEvents.OnEnemyKilled += AddSouls;
        GameEvents.OnKillStreakAdded += AddKillStreak;
        GameEvents.OnKillStreakReset += ResetKillStreak;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyKilled -= AddSouls;
        GameEvents.OnKillStreakAdded -= AddKillStreak;
        GameEvents.OnKillStreakReset -= ResetKillStreak;
    }

    private void Start()
    {
        GameEvents.OnSoulCountChanged?.Invoke(currentSouls);
        GameEvents.OnKillStreakUIUpdated?.Invoke(currentKillStreak);
    }

    private void AddSouls(int amount)
    {
        currentSouls += amount;
        GameEvents.OnSoulCountChanged?.Invoke(currentSouls);
    }

    private void AddKillStreak(int amount)
    {
        currentKillStreak += amount;
        GameEvents.OnKillStreakUIUpdated?.Invoke(currentKillStreak);
    }

    private void ResetKillStreak()
    {
        currentKillStreak = 0;
        GameEvents.OnKillStreakUIUpdated?.Invoke(currentKillStreak);
    }

    public bool SpendSouls(int amount)
    {
        if (currentSouls >= amount)
        {
            currentSouls -= amount;
            GameEvents.OnSoulCountChanged?.Invoke(currentSouls);
            return true;
        }
        return false;
    }
    public int GetCurrentSouls()
    {
        return currentSouls;
    }
}