using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public Image artworkImage;
    public Image cardFrame;
    
    [Header("Visual Settings")]
    public Color attackColor = Color.red;
    public Color skillColor = Color.green;
    public Color powerColor = Color.blue;
    
    private Card cardData;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private bool cardPlayed = false;
    
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private PlayerCharacter player;
    private GameObject enemyTarget;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Add CanvasGroup for drag transparency
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // CRITICAL: Ensure the main card image can receive raycasts
        Image mainImage = GetComponent<Image>();
        if (mainImage != null)
        {
            mainImage.raycastTarget = true;
            Debug.Log($"Card {gameObject.name} - Main Image raycastTarget: {mainImage.raycastTarget}");
        }
        
        // Disable raycast blocking on child images that might interfere
        Image[] childImages = GetComponentsInChildren<Image>();
        foreach (var img in childImages)
        {
            if (img != mainImage) // Keep main image, disable others
            {
                img.raycastTarget = false;
                Debug.Log($"Disabled raycast on child image: {img.name}");
            }
        }
    }
    
    void Start()
    {
        player = FindObjectOfType<PlayerCharacter>();
        enemyTarget = GameObject.Find("EnemyTarget");
        
        Debug.Log($"=== CARD SETUP: {gameObject.name} ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Local Position: {transform.localPosition}");
        Debug.Log($"Canvas: {parentCanvas?.name}");
        Debug.Log($"Can drag: {IsRaycastEnabled()}");
        
        // Force this card to front of sorting order
        Canvas cardCanvas = GetComponent<Canvas>();
        if (cardCanvas == null)
        {
            cardCanvas = gameObject.AddComponent<Canvas>();
            cardCanvas.overrideSorting = true;
            cardCanvas.sortingOrder = 10; // Higher than default
            Debug.Log($"Added Canvas to {gameObject.name} with sorting order 10");
        }
    }
    
    // Add click detection for debugging
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"*** CLICK DETECTED on {cardData?.cardName} ***");
        Debug.Log($"Click position: {eventData.position}");
        Debug.Log($"Card world position: {transform.position}");
        
        // Debug raycast at click position
        DebugRaycastAtPosition(eventData.position);
    }
    
    private void DebugRaycastAtPosition(Vector2 screenPosition)
    {
        GraphicRaycaster raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null) return;
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        
        Debug.Log($"=== RAYCAST DEBUG at {screenPosition} ===");
        Debug.Log($"Total hits: {results.Count}");
        
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            Debug.Log($"Hit {i}: {result.gameObject.name} (depth: {result.depth}, sortingLayer: {result.sortingLayer}, sortingOrder: {result.sortingOrder})");
        }
    }
    
    private bool IsRaycastEnabled()
    {
        Image img = GetComponent<Image>();
        return img != null && img.raycastTarget;
    }
    
    public void SetupCard(Card card)
    {
        cardData = card;
        cardPlayed = false;
        
        if (cardNameText) cardNameText.text = card.cardName;
        if (costText) costText.text = card.cost.ToString();
        if (descriptionText) descriptionText.text = card.description;
        if (artworkImage && card.artwork) artworkImage.sprite = card.artwork;
        
        // Set card frame color
        if (cardFrame)
        {
            switch (card.type)
            {
                case CardType.Attack:
                    cardFrame.color = attackColor;
                    break;
                case CardType.Skill:
                    cardFrame.color = skillColor;
                    break;
                case CardType.Power:
                    cardFrame.color = powerColor;
                    break;
            }
        }
        
        gameObject.name = $"Card_{card.cardName}";
        Debug.Log($"Card setup complete: {gameObject.name}");
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"*** HOVER ENTER: {cardData?.cardName} ***");
        if (!cardPlayed && !isDragging)
        {
            transform.localScale = Vector3.one * 1.05f;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"*** HOVER EXIT: {cardData?.cardName} ***");
        if (!cardPlayed && !isDragging)
        {
            transform.localScale = Vector3.one;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardPlayed) return;
        
        Debug.Log($"*** BEGIN DRAG: {cardData?.cardName} ***");
        
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        
        // Make card semi-transparent during drag
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
        
        // Scale up slightly
        transform.localScale = Vector3.one * 1.1f;
        
        // Bring to very front
        Canvas cardCanvas = GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = 1000;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (cardPlayed || !isDragging) return;
        
        // Move the card to follow mouse
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (cardPlayed) return;
        
        Debug.Log($"*** END DRAG: {cardData?.cardName} ***");
        
        isDragging = false;
        
        // Restore appearance
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.localScale = Vector3.one;
        
        // Return to normal sorting
        Canvas cardCanvas = GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            cardCanvas.sortingOrder = 10;
        }
        
        // Check if we're over a valid target
        bool validPlay = false;
        
        if (cardData.type == CardType.Attack)
        {
            // Attack cards need to be over enemy target
            if (IsOverEnemyTarget(eventData.position))
            {
                var enemy = FindObjectOfType<EnemyCharacter>();
                if (enemy != null && player != null)
                {
                    PlayCard(enemy);
                    validPlay = true;
                }
            }
        }
        else if (cardData.type == CardType.Skill || cardData.type == CardType.Power)
        {
            // Skill/Power cards can be played anywhere
            if (player != null)
            {
                PlayCard(player);
                validPlay = true;
            }
        }
        
        if (!validPlay)
        {
            // Return to original position
            rectTransform.anchoredPosition = originalPosition;
            Debug.Log($"Card {cardData?.cardName} returned to hand");
        }
    }
    
    private bool IsOverEnemyTarget(Vector2 screenPosition)
    {
        if (enemyTarget == null) return false;
        
        RectTransform enemyRect = enemyTarget.GetComponent<RectTransform>();
        if (enemyRect == null) return false;
        
        return RectTransformUtility.RectangleContainsScreenPoint(
            enemyRect, 
            screenPosition, 
            parentCanvas.worldCamera
        );
    }
    
    private void PlayCard(Character target)
    {
        if (cardPlayed) return;
        
        cardPlayed = true;
        
        Debug.Log($"=== PLAYING CARD: {cardData.cardName} on {target.name} ===");
        
        // Check energy
        if (player.energy < cardData.cost)
        {
            Debug.Log("Not enough energy!");
            cardPlayed = false;
            rectTransform.anchoredPosition = originalPosition;
            return;
        }
        
        // Execute card effects
        if (cardData.type == CardType.Attack && target != player)
        {
            // Attack enemy
            cardData.ExecuteEffects(player, target);
        }
        else
        {
            // Skill/Power on self
            cardData.ExecuteEffects(player, player);
        }
        
        // Spend energy
        player.ConsumeEnergy(cardData.cost);
        
        // Remove from hand
        HandManager handManager = FindObjectOfType<HandManager>();
        if (handManager != null)
        {
            handManager.RemoveCardFromHand(this);
        }
        
        // Destroy card
        Destroy(gameObject, 0.1f);
    }
}