using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PostCombatManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject postCombatPanel;
    public Button bookPerpButton;
    public Button takeEvidenceButton;
    public TextMeshProUGUI choiceDescriptionText;
    public TextMeshProUGUI bookPerpRewardsText;
    public TextMeshProUGUI evidenceRewardsText;
    
    [Header("Card Reward UI")]
    public GameObject cardRewardPanel;
    public Transform cardRewardParent;
    public GameObject cardRewardButtonPrefab;
    public Button cardRewardBackButton;
    
    [Header("Settings")]
    public List<Card> availableRewardCards = new List<Card>();
    
    private int pendingXPReward;
    private int pendingHealReward;
    private XPSystem xpSystem;
    private PlayerCharacter player;
    
    void Start()
    {
        // Find systems
        xpSystem = FindObjectOfType<XPSystem>();
        player = FindObjectOfType<PlayerCharacter>();
        
        // Setup buttons
        if (bookPerpButton != null)
            bookPerpButton.onClick.AddListener(OnBookPerp);
        if (takeEvidenceButton != null)
            takeEvidenceButton.onClick.AddListener(OnTakeEvidence);
        if (cardRewardBackButton != null)
            cardRewardBackButton.onClick.AddListener(OnCardRewardBack);
        
        // Hide panels initially
        HideAllPanels();
    }
    
    public void ShowPostCombatChoice(int xpReward, int healReward)
    {
        pendingXPReward = xpReward;
        pendingHealReward = healReward;
        
        // Show the choice panel
        if (postCombatPanel != null)
        {
            postCombatPanel.SetActive(true);
        }
        
        // Update UI text
        UpdateChoiceText();
        
        Debug.Log($"Post-combat choice shown. XP: {xpReward}, Heal: {healReward}");
    }
    
    private void UpdateChoiceText()
    {
        if (choiceDescriptionText != null)
        {
            choiceDescriptionText.text = "Suspect neutralized. How do you proceed?";
        }
        
        if (bookPerpRewardsText != null)
        {
            bookPerpRewardsText.text = $"Book Perp\n• Gain {pendingXPReward} XP\n• Heal {pendingHealReward} HP\n• Steady progress";
        }
        
        if (evidenceRewardsText != null)
        {
            evidenceRewardsText.text = $"Take Evidence\n• Choose new card\n• No healing\n• Immediate power";
        }
    }
    
    public void OnBookPerp()
    {
        Debug.Log("Player chose: Book Perp");
        
        // Give XP reward
        if (xpSystem != null)
        {
            xpSystem.AddXP(pendingXPReward);
            Debug.Log($"Awarded {pendingXPReward} XP");
        }
        
        // Give heal reward
        if (player != null)
        {
            player.Heal(pendingHealReward);
            Debug.Log($"Healed {pendingHealReward} HP");
        }
        
        // Track booking for progression system (later)
        // BookingTracker.Instance?.AddBooking();
        
        // Complete the current node
        CompleteCurrentNode();
        
        // Hide UI
        HideAllPanels();
    }
    
    public void OnTakeEvidence()
    {
        Debug.Log("Player chose: Take Evidence");
        
        // Show card reward screen
        ShowCardRewardScreen();
    }
    
    private void ShowCardRewardScreen()
    {
        if (cardRewardPanel == null)
        {
            Debug.LogError("Card reward panel not set up!");
            return;
        }
        
        // Hide post-combat panel, show card reward panel
        if (postCombatPanel != null)
            postCombatPanel.SetActive(false);
        cardRewardPanel.SetActive(true);
        
        // Generate 3 random card choices
        List<Card> cardChoices = GetRandomCardChoices(3);
        
        // Clear any existing card buttons
        ClearCardRewardButtons();
        
        // Create card choice buttons
        for (int i = 0; i < cardChoices.Count; i++)
        {
            CreateCardRewardButton(cardChoices[i]);
        }
    }
    
    private List<Card> GetRandomCardChoices(int count)
    {
        List<Card> choices = new List<Card>();
        
        if (availableRewardCards.Count == 0)
        {
            Debug.LogWarning("No available reward cards configured!");
            return choices;
        }
        
        // Simple random selection (no rarity weighting for now)
        for (int i = 0; i < count && i < availableRewardCards.Count; i++)
        {
            Card randomCard;
            do
            {
                randomCard = availableRewardCards[Random.Range(0, availableRewardCards.Count)];
            }
            while (choices.Contains(randomCard) && choices.Count < availableRewardCards.Count);
            
            choices.Add(randomCard);
        }
        
        return choices;
    }
    
    private void CreateCardRewardButton(Card card)
    {
        if (cardRewardButtonPrefab == null || cardRewardParent == null)
        {
            Debug.LogError("Card reward UI components not set up!");
            return;
        }
        
        GameObject buttonObj = Instantiate(cardRewardButtonPrefab, cardRewardParent);
        Button cardButton = buttonObj.GetComponent<Button>();
        
        // Set up the card display
        CardDisplay cardDisplay = buttonObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(card);
        }
        
        // Set up the button click
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(() => OnCardRewardSelected(card));
        }
        
        // Add text for card name if no CardDisplay
        TextMeshProUGUI cardText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (cardText != null && cardDisplay == null)
        {
            cardText.text = $"{card.cardName}\nCost: {card.cost}\n{card.description}";
        }
    }
    
    private void OnCardRewardSelected(Card selectedCard)
    {
        Debug.Log($"Player selected card: {selectedCard.cardName}");
        
        // Add card to deck safely
        DeckManager deckManager = DeckManager.Instance;
        if (deckManager != null)
        {
            deckManager.AddCardToDeck(selectedCard);
            Debug.Log($"Added {selectedCard.cardName} to deck");
        }
        else
        {
            Debug.LogError("No DeckManager found!");
        }
        
        // Complete the current node
        CompleteCurrentNode();
        
        // Hide UI
        HideAllPanels();
    }
    
    public void OnCardRewardBack()
    {
        // Go back to main post-combat choice
        cardRewardPanel.SetActive(false);
        if (postCombatPanel != null)
            postCombatPanel.SetActive(true);
    }
    
    private void ClearCardRewardButtons()
    {
        if (cardRewardParent == null) return;
        
        foreach (Transform child in cardRewardParent)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void CompleteCurrentNode()
    {
        MapManager mapManager = MapManager.Instance;
        if (mapManager != null)
        {
            MapNode currentNode = mapManager.GetCurrentNode();
            if (currentNode != null)
            {
                currentNode.CompleteNode();
            }
        }
    }
    
    private void HideAllPanels()
    {
        if (postCombatPanel != null)
            postCombatPanel.SetActive(false);
        if (cardRewardPanel != null)
            cardRewardPanel.SetActive(false);
    }
}