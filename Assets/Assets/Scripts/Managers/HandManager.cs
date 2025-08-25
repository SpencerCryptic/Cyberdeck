using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class HandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    public Transform handParent;
    public GameObject cardPrefab;
    public int maxHandSize = 10;
    
    [Header("Layout Settings")]
    public float cardSpacing = 120f;
    public float animationDuration = 0.3f;
    
    private List<Card> cardsInHand = new List<Card>();
    private List<GameObject> cardGameObjects = new List<GameObject>();
    private DeckManager deckManager;
    private HorizontalLayoutGroup layoutGroup;
    private bool isRepositioning = false;
    
    // Track which card is currently being dragged to prevent multi-selection
    public static CardDisplay currentlyDraggedCard = null;
    
    void Start()
    {
        deckManager = FindObjectOfType<DeckManager>();
        
        // Get or create HorizontalLayoutGroup with proper settings
        layoutGroup = handParent.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = handParent.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        
        // Configure layout group for manual control
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = cardSpacing;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
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
        
        // Set unique name for debugging
        cardObj.name = $"Card_{cardsInHand.Count}_{card.cardName}";
        
        // Ensure proper canvas setup for layering
        Canvas cardCanvas = cardObj.GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = cardObj.AddComponent<Canvas>();
            cardCanvas.overrideSorting = true;
        }
        
        cardGameObjects.Add(cardObj);
        
        // Smooth reposition after adding
        StartCoroutine(RepositionCardsSmooth());
        
        Debug.Log($"Added {card.cardName} to hand. Total cards: {cardsInHand.Count}");
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
            StartCoroutine(RepositionCardsSmooth());
        }
    }
    
    public void RemoveCardFromHand(CardDisplay cardDisplay)
    {
        GameObject cardObj = cardDisplay.gameObject;
        int index = cardGameObjects.IndexOf(cardObj);
        
        Debug.Log($"RemoveCardFromHand called for {cardObj.name} at index {index}");
        
        if (index >= 0)
        {
            // Remove from both lists
            Card removedCard = null;
            if (index < cardsInHand.Count)
            {
                removedCard = cardsInHand[index];
                cardsInHand.RemoveAt(index);
            }
            cardGameObjects.RemoveAt(index);
            
            // Add to discard pile
            if (deckManager != null && removedCard != null)
            {
                deckManager.AddToDiscard(removedCard);
            }
            
            Debug.Log($"Removed card from hand. Remaining cards: {cardsInHand.Count}");
            
            // Smoothly reposition remaining cards
            StartCoroutine(RepositionCardsSmooth());
        }
    }
    
    private IEnumerator RepositionCardsSmooth()
    {
        if (isRepositioning) yield break; // Prevent overlapping animations
        
        isRepositioning = true;
        
        // Wait a frame for layout group to do its initial positioning
        yield return new WaitForEndOfFrame();
        
        // Store starting positions
        Vector3[] startPositions = new Vector3[cardGameObjects.Count];
        for (int i = 0; i < cardGameObjects.Count; i++)
        {
            if (cardGameObjects[i] != null)
            {
                startPositions[i] = cardGameObjects[i].transform.localPosition;
            }
        }
        
        // Calculate target positions (simple horizontal spread)
        Vector3[] targetPositions = new Vector3[cardGameObjects.Count];
        float totalWidth = (cardGameObjects.Count - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;
        
        for (int i = 0; i < cardGameObjects.Count; i++)
        {
            targetPositions[i] = new Vector3(startX + i * cardSpacing, 0, 0);
            
            // Set canvas sorting order (center cards slightly higher)
            if (cardGameObjects[i] != null)
            {
                Canvas cardCanvas = cardGameObjects[i].GetComponent<Canvas>();
                if (cardCanvas != null)
                {
                    float distanceFromCenter = Mathf.Abs(i - (cardGameObjects.Count - 1) * 0.5f);
                    cardCanvas.sortingOrder = Mathf.RoundToInt((cardGameObjects.Count - distanceFromCenter) * 10);
                }
            }
        }
        
        // Animate to target positions
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            progress = Mathf.SmoothStep(0f, 1f, progress);
            
            for (int i = 0; i < cardGameObjects.Count; i++)
            {
                if (cardGameObjects[i] != null)
                {
                    Vector3 currentPos = Vector3.Lerp(startPositions[i], targetPositions[i], progress);
                    cardGameObjects[i].transform.localPosition = currentPos;
                }
            }
            
            yield return null;
        }
        
        // Ensure final positions are exact
        for (int i = 0; i < cardGameObjects.Count; i++)
        {
            if (cardGameObjects[i] != null)
            {
                cardGameObjects[i].transform.localPosition = targetPositions[i];
            }
        }
        
        isRepositioning = false;
        Debug.Log("Card repositioning animation complete");
    }
    
    // Called when a card starts being dragged
    public void BringCardToFront(GameObject cardObj)
    {
        Canvas cardCanvas = cardObj.GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = 1000; // Very high value for dragging
        }
    }
    
    // Called when drag ends to return to normal sorting
    public void ReturnCardSorting(GameObject cardObj)
    {
        // Will be recalculated in next RepositionCardsSmooth call
        if (!isRepositioning)
        {
            StartCoroutine(RepositionCardsSmooth());
        }
    }
    
    // Debug method
    public void DebugHandState()
    {
        Debug.Log($"=== HAND STATE ===");
        Debug.Log($"Cards in hand: {cardsInHand.Count}");
        Debug.Log($"Card GameObjects: {cardGameObjects.Count}");
        Debug.Log($"Currently repositioning: {isRepositioning}");
        Debug.Log($"Currently dragged card: {(currentlyDraggedCard ? currentlyDraggedCard.name : "None")}");
    }
}