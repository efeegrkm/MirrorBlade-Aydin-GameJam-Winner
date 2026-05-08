using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject floatingTextPrefab;
    public Transform worldCanvasTransform;
    public GameObject explosionPrefab;
    private void OnEnable()
    {
        GameEvents.OnShowFloatingText += ShowText;
        GameEvents.OnSpawnExplosion += SpawnExplosion;
    }

    private void OnDisable()
    {
        GameEvents.OnShowFloatingText -= ShowText;
        GameEvents.OnSpawnExplosion -= SpawnExplosion;
    }

    private void ShowText(Vector3 position, string text, Color color)
    {
        if (floatingTextPrefab != null && worldCanvasTransform != null)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);

            GameObject go = Instantiate(floatingTextPrefab, position + randomOffset, Quaternion.identity, worldCanvasTransform);

            FloatingText ft = go.GetComponent<FloatingText>();
            if (ft != null) ft.Setup(text, color);
        }
    }

    private void SpawnExplosion(Vector3 position, float scale)
    {
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, position, Quaternion.identity);
            exp.SetActive(true);
            exp.transform.localScale = new Vector3(scale, scale, 1f);

            Destroy(exp, 0.5f);
        }
    }
}