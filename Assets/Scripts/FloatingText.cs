using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float destroyTime = 1f;

    private TextMeshProUGUI tmp;
    private Color textColor;

    public void Setup(string text, Color color)
    {
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        textColor = color;
        tmp.color = textColor;

        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (tmp != null)
        {
            textColor.a -= Time.deltaTime / destroyTime;
            tmp.color = textColor;
        }
    }
}