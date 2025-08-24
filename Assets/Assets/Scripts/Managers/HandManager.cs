using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    public Transform handParent;
    public GameObject cardPrefab;
    public int maxHandSize = 10;
    
    private List<Card> cardsInHand = new List<Card>();
    private List<GameObject> cardGameObjects = new List<GameObject>();
    private DeckManager deckManager;
    
    void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();
    }
    
    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount && cardsInHand.Count < maxHandSize; i++)
        {
            DrawCard();
        }
    }
    
    public void DrawCard()
    {
        if (deckManager == null) return;
        
        Card drawnCard = deckManager.DrawCard();
        if (drawnCard != null)
        {
            AddCardToHand(drawnCard);
        }
    }
    
    private void AddCardToHand(Card card)
    {
        cardsInHand.Add(card);
        
        // Create visual representation
        GameObject cardObj = Instantiate(cardPrefab, handParent);
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(card);
        }
        
        cardGameObjects.Add(cardObj);
        ArrangeCardsInHand();
    }
    
    public void DiscardCard(Card card)
    {
        int index = cardsInHand.IndexOf(card);
        if (index >= 0)
        {
            cardsInHand.RemoveAt(index);
            
            if (index < cardGameObjects.Count)
            {
                Destroy(cardGameObjects[index]);
                cardGameObjects.RemoveAt(index);
            }
            
            deckManager.AddToDiscard(card);
            ArrangeCardsInHand();
        }
    }
    
    private void ArrangeCardsInHand()
    {
        // Simple horizontal layout
        float cardWidth = 120f;
        float spacing = cardWidth * 0.8f;
        float startX = -(cardsInHand.Count - 1) * spacing * 0.5f;
        
        for (int i = 0; i < cardGameObjects.Count; i++)
        {
            Vector3 targetPos = new Vector3(startX + i * spacing, 0, 0);
            cardGameObjects[i].transform.localPosition = targetPos;
        }
    }
}
