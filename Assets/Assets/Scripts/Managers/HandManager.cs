using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    public Transform handParent;
    public GameObject cardPrefab;
    public int maxHandSize = 10;
    
    [Header("Layout Settings")]
    public float cardWidth = 120f;
    public float spacingMultiplier = 0.8f;
    
    [Header("Fan Effect Settings")]
    public float fanAngle = 25f; // Maximum angle for fan spread
    public float fanRadius = 300f; // Radius of the fan arc
    public float verticalOffset = 50f; // How much cards lift up when fanned
    public bool useFanEffect = true;
    
    [Header("Animation Settings")]
    public float animationSpeed = 0.3f; // Time to animate to new positions
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private List<Card> cardsInHand = new List<Card>();
    private List<GameObject> cardGameObjects = new List<GameObject>();
    private DeckManager deckManager;
    
    void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();
        
        if (handParent == null)
        {
            Debug.LogError("HandManager: handParent is not set!");
        }
        
        if (cardPrefab == null)
        {
            Debug.LogError("HandManager: cardPrefab is not set!");
        }
        
        if (deckManager == null)
        {
            Debug.LogError("HandManager: No DeckManager found in scene!");
        }
    }
    
    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount && cardsInHand.Count < maxHandSize; i++)
        {
            DrawCard();
        }
        
        Debug.Log($"Drew {amount} cards. Hand size: {cardsInHand.Count}");
    }
    
    public void DrawCard()
    {
        if (deckManager == null)
        {
            Debug.LogError("Cannot draw card: DeckManager is null");
            return;
        }
        
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.LogWarning("Hand is full, cannot draw more cards");
            return;
        }
        
        Card drawnCard = deckManager.DrawCard();
        if (drawnCard != null)
        {
            AddCardToHand(drawnCard);
        }
        else
        {
            Debug.Log("No more cards to draw");
        }
    }
    
    private void AddCardToHand(Card card)
    {
        if (handParent == null || cardPrefab == null)
        {
            Debug.LogError("Cannot add card to hand: missing handParent or cardPrefab");
            return;
        }
        
        cardsInHand.Add(card);
        
        // Create visual representation
        GameObject cardObj = Instantiate(cardPrefab, handParent);
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        
        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(card);
        }
        else
        {
            Debug.LogError($"Card prefab {cardPrefab.name} doesn't have CardDisplay component!");
        }
        
        cardGameObjects.Add(cardObj);
        ArrangeCardsInHand();
        
        Debug.Log($"Added {card.cardName} to hand");
    }
    
    public void DiscardCard(Card card)
    {
        int index = cardsInHand.IndexOf(card);
        if (index >= 0)
        {
            // Find the corresponding GameObject by matching the Card reference
            GameObject cardObjToRemove = null;
            for (int i = 0; i < cardGameObjects.Count; i++)
            {
                CardDisplay display = cardGameObjects[i].GetComponent<CardDisplay>();
                if (display != null && display.GetCardData() == card)
                {
                    cardObjToRemove = cardGameObjects[i];
                    cardGameObjects.RemoveAt(i);
                    break;
                }
            }
            
            cardsInHand.RemoveAt(index);
            
            if (cardObjToRemove != null)
            {
                Destroy(cardObjToRemove);
            }
            
            if (deckManager != null)
            {
                deckManager.AddToDiscard(card);
            }
            
            ArrangeCardsInHand();
            Debug.Log($"Discarded {card.cardName}");
        }
        else
        {
            Debug.LogWarning($"Tried to discard {card.cardName} but it wasn't in hand");
        }
    }
    
    public void DiscardHand()
    {
        while (cardsInHand.Count > 0)
        {
            DiscardCard(cardsInHand[0]);
        }
        Debug.Log("Discarded entire hand");
    }
    
    private void ArrangeCardsInHand()
    {
        if (cardGameObjects.Count == 0) return;
        
        if (useFanEffect && cardGameObjects.Count > 1)
        {
            ArrangeCardsInFan();
        }
        else
        {
            ArrangeCardsLinear();
        }
    }
    
    private void ArrangeCardsLinear()
    {
        // Simple horizontal layout with overlap (fallback for single card or disabled fan)
        float spacing = cardWidth * spacingMultiplier;
        float totalWidth = (cardGameObjects.Count - 1) * spacing;
        float startX = -totalWidth * 0.5f;
        
        for (int i = 0; i < cardGameObjects.Count; i++)
        {
            if (cardGameObjects[i] != null)
            {
                Vector3 targetPos = new Vector3(startX + i * spacing, 0, 0);
                Quaternion targetRot = Quaternion.identity;
                
                // Animate to position smoothly
                StartCoroutine(AnimateCardToPosition(cardGameObjects[i], targetPos, targetRot));
                
                // Set sibling index for proper layering
                cardGameObjects[i].transform.SetSiblingIndex(i);
            }
        }
    }
    
    private void ArrangeCardsInFan()
    {
        int cardCount = cardGameObjects.Count;
        
        // Calculate horizontal spacing that scales with card count
        float spacing = cardWidth * spacingMultiplier;
        float totalWidth = (cardCount - 1) * spacing;
        float startX = -totalWidth * 0.5f;
        
        // Calculate angle step between cards
        float totalAngle = Mathf.Min(fanAngle * 2f, (cardCount - 1) * (fanAngle * 2f / cardCount));
        float angleStep = cardCount > 1 ? totalAngle / (cardCount - 1) : 0f;
        float startAngle = -totalAngle * 0.5f;
        
        for (int i = 0; i < cardCount; i++)
        {
            if (cardGameObjects[i] != null)
            {
                // Calculate angle for this card
                float currentAngle = startAngle + (i * angleStep);
                float angleRad = currentAngle * Mathf.Deg2Rad;
                
                // Combine horizontal spacing with arc positioning
                float baseX = startX + i * spacing; // Linear horizontal spacing
                float arcX = Mathf.Sin(angleRad) * fanRadius * 0.3f; // Reduced arc influence
                float x = baseX + arcX;
                
                // Arc positioning for Y - inverted so center cards are higher
                float y = (1 - Mathf.Cos(angleRad)) * fanRadius * 0.2f + verticalOffset;
                
                Vector3 targetPos = new Vector3(x, y, 0);
                Quaternion targetRot = Quaternion.Euler(0, 0, -currentAngle);
                
                // Animate to position smoothly
                StartCoroutine(AnimateCardToPosition(cardGameObjects[i], targetPos, targetRot));
                
                // Set sibling index - center cards should be on top
                int centerIndex = cardCount / 2;
                int distanceFromCenter = Mathf.Abs(i - centerIndex);
                cardGameObjects[i].transform.SetSiblingIndex(cardCount - distanceFromCenter);
            }
        }
    }
    
    private IEnumerator AnimateCardToPosition(GameObject card, Vector3 targetPos, Quaternion targetRot)
    {
        if (card == null) yield break;
        
        // Skip animation if the card is being dragged
        CardDisplay cardDisplay = card.GetComponent<CardDisplay>();
        if (cardDisplay != null && cardDisplay.isDragging)
        {
            yield break;
        }
        
        Vector3 startPos = card.transform.localPosition;
        Quaternion startRot = card.transform.localRotation;
        float elapsed = 0f;
        
        while (elapsed < animationSpeed)
        {
            // Skip if card is being dragged during animation
            if (cardDisplay != null && cardDisplay.isDragging)
            {
                yield break;
            }
            
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            float curve = animationCurve.Evaluate(t);
            
            card.transform.localPosition = Vector3.Lerp(startPos, targetPos, curve);
            card.transform.localRotation = Quaternion.Lerp(startRot, targetRot, curve);
            
            yield return null;
        }
        
        // Ensure final position is exact
        if (cardDisplay == null || !cardDisplay.isDragging)
        {
            card.transform.localPosition = targetPos;
            card.transform.localRotation = targetRot;
        }
    }
    
    public int GetHandSize()
    {
        return cardsInHand.Count;
    }
    
    public List<Card> GetHandCards()
    {
        return new List<Card>(cardsInHand);
    }
    
    public bool IsHandFull()
    {
        return cardsInHand.Count >= maxHandSize;
    }
    
    // Clean up when turn ends
    public void EndTurn()
    {
        DiscardHand();
    }
}