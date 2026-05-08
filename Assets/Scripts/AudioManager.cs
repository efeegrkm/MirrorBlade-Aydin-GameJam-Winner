using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource; 

    [Header("Audio Clips")]
    public AudioClip attackSound;
    public AudioClip enemyDeathSound;
    public AudioClip walkStepSound;
    public AudioClip uiClickSound;
    public AudioClip dashSound;

    private void OnEnable()
    {
        GameEvents.OnPlaySound += PlaySound;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaySound -= PlaySound;
    }

    private void PlaySound(GameEvents.SoundType type)
    {
        if (sfxSource == null) return;

        switch (type)
        {
            case GameEvents.SoundType.Dash: 
                if (dashSound != null) sfxSource.PlayOneShot(dashSound);
                break;
            case GameEvents.SoundType.Attack:
                if (attackSound != null) sfxSource.PlayOneShot(attackSound);
                break;
            case GameEvents.SoundType.EnemyDeath:
                if (enemyDeathSound != null) sfxSource.PlayOneShot(enemyDeathSound);
                break;
            case GameEvents.SoundType.WalkStep:
                if (walkStepSound != null)
                {
                    sfxSource.pitch = Random.Range(0.9f, 1.1f); 
                    sfxSource.PlayOneShot(walkStepSound, 0.5f); 
                    sfxSource.pitch = 1f; 
                }
                break;
            case GameEvents.SoundType.UIClick:
                if (uiClickSound != null) sfxSource.PlayOneShot(uiClickSound);
                break;
        }
    }
}