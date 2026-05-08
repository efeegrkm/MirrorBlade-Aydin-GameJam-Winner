using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class CardData
{
    public string cardName;
    public Sprite cardSprite;
    [Tooltip("Kartýn satýn alma bedeli (Varsayýlan 100)")]
    public int cost = 100; 
    public bool isOneTimeUse = false;
    public UnityEvent onCardSelected;
}

public class CardSystemManager : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject cardPanel;
    public GameObject cardPrefab;

    [Header("Card Database")]
    public List<CardData> availableCards;

    private List<GameObject> activeCardObjects = new List<GameObject>();
    public SoulManager soulManager; 


    public void toggleCardPanel()
    {
        GameEvents.OnPlaySound?.Invoke(GameEvents.SoundType.UIClick);
        if (cardPanel.activeSelf) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
        cardPanel.SetActive(true);
        GenerateRandomCards();
    }

    public void ClosePanel()
    {
        cardPanel.SetActive(false);
        ClearCards();
    }

    private void GenerateRandomCards()
    {
        ClearCards();

        if (availableCards.Count == 0)
        {
            Debug.LogWarning("Kart havuzunda hiç kart kalmadý!");
            return;
        }

        List<CardData> pool = new List<CardData>(availableCards);
        int cardsToSpawn = Mathf.Min(3, pool.Count);

        for (int i = 0; i < cardsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            CardData selectedCard = pool[randomIndex];
            pool.RemoveAt(randomIndex);

            GameObject newCard = Instantiate(cardPrefab, cardPanel.transform);
            activeCardObjects.Add(newCard);

            Image cardImage = newCard.GetComponent<Image>();
            if (cardImage != null) cardImage.sprite = selectedCard.cardSprite;

            Button cardButton = newCard.GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(() => OnCardClicked(selectedCard));
            }
        }
    }

    private void OnCardClicked(CardData card)
    {
        if (soulManager == null) return;

        if (soulManager.SpendSouls(card.cost))
        {
            card.onCardSelected?.Invoke();

            if (card.isOneTimeUse)
            {
                availableCards.Remove(card);
            }

            ClosePanel();

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            GameEvents.OnShowFloatingText?.Invoke(mousePos, "Alýndý!", Color.green);
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            int missingSouls = card.cost - soulManager.GetCurrentSouls();

            GameEvents.OnShowFloatingText?.Invoke(mousePos, missingSouls.ToString() + " Ruh Eksik", Color.red);
        }
    }

    private void ClearCards()
    {
        foreach (var cardObj in activeCardObjects)
        {
            if (cardObj != null) Destroy(cardObj);
        }
        activeCardObjects.Clear();
    }
}