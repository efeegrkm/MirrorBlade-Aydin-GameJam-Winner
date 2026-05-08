using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Core UI")]
    public Slider coreHealthSlider;
    public TextMeshProUGUI soulText;
    public TextMeshProUGUI killStreakText;
    public GameObject killStreakObject;

    [Header("Wave UI")]
    public TextMeshProUGUI waveText;
    public Slider dashManaSlider;

    private Queue<int> streakQueue = new Queue<int>();
    private bool isAnimatingStreak = false;

    private void OnEnable()
    {
        GameEvents.OnCoreHealthChanged += UpdateHealthUI;
        GameEvents.OnSoulCountChanged += UpdateSoulUI;
        GameEvents.OnWaveChanged += UpdateWaveUI;
        GameEvents.OnKillStreakUIUpdated += UpdateKillStreakUI;
        GameEvents.OnDashManaChanged += UpdateManaUI;
    }

    private void OnDisable()
    {
        GameEvents.OnCoreHealthChanged -= UpdateHealthUI;
        GameEvents.OnSoulCountChanged -= UpdateSoulUI;
        GameEvents.OnWaveChanged -= UpdateWaveUI;
        GameEvents.OnKillStreakUIUpdated -= UpdateKillStreakUI;
        GameEvents.OnDashManaChanged -= UpdateManaUI;
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (coreHealthSlider != null)
        {
            coreHealthSlider.maxValue = maxHealth;
            coreHealthSlider.value = currentHealth;
        }
    }

    private void UpdateSoulUI(int totalSouls)
    {
        if (soulText != null)
        {
            soulText.text = totalSouls.ToString();
        }
    }

    private void UpdateWaveUI(int currentWave, int maxWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"{currentWave}/{maxWaves}";
        }
    }

    private void UpdateKillStreakUI(int streak)
    {
        if (streak <= 1) return;
        
        streakQueue.Enqueue(streak);

        if (!isAnimatingStreak)
        {
            StartCoroutine(ProcessKillStreakQueue());
        }
    }
    private void UpdateManaUI(float currentMana, float maxMana)
    {
        if (dashManaSlider != null)
        {
            dashManaSlider.maxValue = maxMana;
            dashManaSlider.value = currentMana;
        }
    }
    private IEnumerator ProcessKillStreakQueue()
    {
        isAnimatingStreak = true;

        CanvasGroup canvasGroup = null;
        if (killStreakObject != null)
        {
            canvasGroup = killStreakObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = killStreakObject.AddComponent<CanvasGroup>();
            }
        }

        while (streakQueue.Count > 0)
        {
            int currentStreak = streakQueue.Dequeue();
            if (currentStreak % 10 == 0 && currentStreak != 0)
            {
                killStreakObject.GetComponent<TextMeshProUGUI>().text = "AMANSIZ";
            }
            else if (currentStreak % 5 == 0 && currentStreak != 0)
            {
                killStreakObject.GetComponent<TextMeshProUGUI>().text = "KATLIAM";
            }
            else
            {
                killStreakObject.GetComponent<TextMeshProUGUI>().text = " ";
            }
            if (killStreakObject != null)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;

                killStreakObject.SetActive(false);
                killStreakObject.SetActive(true);

                Animator anim = killStreakObject.GetComponent<Animator>();
                if (anim != null)
                {
                    int selectedAnim = Random.Range(1, 4);
                    anim.SetInteger("Streak", selectedAnim);
                }

                if (killStreakText != null)
                {
                    killStreakText.text = currentStreak.ToString() + "X";
                }

                yield return new WaitForSeconds(1f);

                if (canvasGroup != null)
                {
                    float fadeDuration = 0.5f;
                    float timer = 0f;

                    while (timer < fadeDuration)
                    {
                        timer += Time.deltaTime;
                        canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                        yield return null; 
                    }

                    canvasGroup.alpha = 0f;
                }
                if (anim != null)
                {
                    anim.SetInteger("Streak", 0);
                }
            }
        }

        if (killStreakObject != null)
        {
            killStreakObject.SetActive(false);
        }

        isAnimatingStreak = false;
    }
}