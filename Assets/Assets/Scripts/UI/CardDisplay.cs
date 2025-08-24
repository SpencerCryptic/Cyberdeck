using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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
    private bool isDragging = false;
    private Canvas canvas;
    private PlayerCharacter player;
    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        player = FindObjectOfType<PlayerCharacter>();
        originalScale = transform.localScale;
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
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        transform.localScale = originalScale;
        
        // Determine target based on card type
        Character target = null;
        bool validDrop = false;
        
        if (cardData.type == CardType.Attack)
        {
            // Attack cards target enemy
            var enemy = FindObjectOfType<EnemyCharacter>();
            if (enemy != null && IsOverTarget(eventData.position, enemy.transform))
            {
                target = enemy;
                validDrop = true;
            }
        }
        else if (cardData.type == CardType.Skill || cardData.type == CardType.Power)
        {
            // Skill/Power cards target self (or drag anywhere to play)
            target = player; // Self-target
            validDrop = true; // Can play anywhere
        }
        
        // Try to play the card
        if (validDrop && player != null && player.CanPlayCard(cardData))
        {
            player.PlayCard(cardData, target);
            return;
        }
        
        // Return to original position if not played
        transform.position = originalPosition;
    }
    
    private bool IsOverTarget(Vector2 screenPosition, Transform target)
    {
        // Simple screen-based collision detection
        Vector2 targetScreenPos = Camera.main.WorldToScreenPoint(target.position);
        float distance = Vector2.Distance(screenPosition, targetScreenPos);
        return distance < 100f; // Adjust threshold as needed
    }
}