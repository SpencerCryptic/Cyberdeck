using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
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
    private Vector3 originalScale;
    public bool isDragging = false;
    private Canvas canvas;
    private PlayerCharacter player;
    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        player = FindObjectOfType<PlayerCharacter>();
        originalScale = transform.localScale;
        
        // CRITICAL FIXES for interaction:
        
        // 1. Ensure the main card GameObject has an Image with raycast enabled
        Image cardImage = GetComponent<Image>();
        if (cardImage == null)
        {
            cardImage = gameObject.AddComponent<Image>();
            cardImage.color = new Color(1, 1, 1, 0.01f); // Nearly transparent but still raycast-able
        }
        cardImage.raycastTarget = true;
        
        // 2. Remove Button component if it exists (can interfere with drag)
        Button cardButton = GetComponent<Button>();
        if (cardButton != null)
        {
            DestroyImmediate(cardButton);
        }
        
        // 3. Ensure we have a CanvasGroup for better control
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = true;
        
        Debug.Log($"CardDisplay setup complete for {gameObject.name}");
    }
    
    public void SetupCard(Card card)
    {
        cardData = card;
        
        if (cardNameText) cardNameText.text = card.cardName;
        if (costText) costText.text = card.cost.ToString();
        if (descriptionText) descriptionText.text = card.description;
        if (artworkImage && card.artwork) artworkImage.sprite = card.artwork;
        
        // Set card frame color based on type
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
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            transform.localScale = originalScale * 1.1f;
            Debug.Log($"Mouse entered card: {cardData?.cardName}");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            transform.localScale = originalScale;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalPosition = transform.position;
        transform.localScale = originalScale * 1.2f;
        
        // Reset rotation when dragging for better interaction
        transform.rotation = Quaternion.identity;
        
        // Bring to front while dragging
        transform.SetAsLastSibling();
        
        Debug.Log($"Started dragging {cardData?.cardName ?? "unknown card"}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        transform.localScale = originalScale;
        
        // Determine target based on card type and drag position
        Character target = DetermineTarget(eventData.position);
        bool validPlay = false;
        
        if (player == null || !player.CanPlayCard(cardData))
        {
            Debug.Log($"Cannot play {cardData.cardName}: insufficient energy or no player");
            ReturnToOriginalPosition();
            return;
        }
        
        // Enhanced targeting logic
        switch (cardData.type)
        {
            case CardType.Attack:
                var enemy = FindObjectOfType<EnemyCharacter>();
                if (enemy != null && (target == enemy || target == null))
                {
                    target = enemy;
                    validPlay = true;
                }
                break;
                
            case CardType.Skill:
            case CardType.Power:
                // Most skills target self, but check if dragged over enemy for targeted skills
                target = target ?? player; // Default to self
                validPlay = true;
                break;
        }
        
        if (validPlay)
        {
            Debug.Log($"Playing {cardData.cardName} on {target?.name ?? "no target"}");
            player.PlayCard(cardData, target);
            // Card will be destroyed by HandManager
        }
        else
        {
            Debug.Log($"Invalid target for {cardData.cardName}");
            ReturnToOriginalPosition();
        }
    }
    
    private Character DetermineTarget(Vector2 screenPosition)
    {
        // Raycast to find what we're dragging over
        GraphicRaycaster raycaster = canvas?.GetComponent<GraphicRaycaster>();
        if (raycaster == null) 
        {
            Debug.LogWarning("No GraphicRaycaster found on canvas");
            return null;
        }
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        
        var results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        
        foreach (var result in results)
        {
            Character character = result.gameObject.GetComponentInParent<Character>();
            if (character != null)
            {
                Debug.Log($"Found target: {character.name}");
                return character;
            }
        }
        
        return null;
    }
    
    private void ReturnToOriginalPosition()
    {
        transform.position = originalPosition;
    }
    
    public Card GetCardData()
    {
        return cardData;
    }
}