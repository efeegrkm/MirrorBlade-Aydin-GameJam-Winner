using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    public EnemyBase enemyPrefab;
    public int count; 
    public float spawnInterval; 
}

[System.Serializable]
public class WaveConfig
{
    public string waveName = "Wave";
    public List<EnemySpawnConfig> enemiesToSpawn;
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public Transform coreTransform;
    public float spawnRadius = 15f; 
    public float timeBetweenWaves = 5f;
    public List<WaveConfig> waves;
    public GameObject finishScreen;

    public int currentWaveIndex = 0;
    public int activeEnemyCount = 0;
    public Animator bg;
    private void Start()
    {
        StartCoroutine(StartNextWave(true));
    }

    private IEnumerator StartNextWave(bool first)
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("TÜM WAVELER BÝTTÝ! SUCCESS!");
            finishScreen.SetActive(true);
            Time.timeScale = 0f; 
            
            yield break;
        }

        if (!first)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
        }
        if(currentWaveIndex + 1 == 3)
        {
            bg.gameObject.SetActive(true);
            bg.SetTrigger("startAnim");
        }
        WaveConfig currentWave = waves[currentWaveIndex];
        GameEvents.OnWaveChanged?.Invoke(currentWaveIndex + 1, waves.Count);

        foreach (var spawnConfig in currentWave.enemiesToSpawn)
        {
            for (int i = 0; i < spawnConfig.count; i++)
            {
                SpawnEnemy(spawnConfig.enemyPrefab);
                yield return new WaitForSeconds(spawnConfig.spawnInterval);
            }
        }

        while (activeEnemyCount > 0)
        {
            yield return null;
        }

        Debug.Log($"Wave {currentWaveIndex + 1} tamamlandý!");
        currentWaveIndex++;

        CardSystemManager cm = this.gameObject.GetComponent<CardSystemManager>();
        if (cm.cardPanel.activeSelf) cm.ClosePanel();
        else cm.OpenPanel();

        StartCoroutine(StartNextWave(false));
    }

    private void SpawnEnemy(EnemyBase prefab)
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = (Vector2)coreTransform.position + (randomDir * spawnRadius);

        EnemyBase newEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        newEnemy.Initialize(coreTransform);

        activeEnemyCount++;

        StartCoroutine(TrackEnemyDeath(newEnemy));
    }
    private IEnumerator TrackEnemyDeath(EnemyBase enemy)
    {
        while (enemy != null)
        {
            yield return null;
        }
        activeEnemyCount--;
    }
}