using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Setup")]
    public List<Card> startingDeck = new List<Card>();
    public List<Card> currentDeck = new List<Card>(); // Persistent deck that grows
    
    private List<Card> drawPile = new List<Card>();
    private List<Card> discardPile = new List<Card>();
    
    public static DeckManager Instance;
    
    void Awake()
    {
        // Singleton for easy access
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeDeck();
    }
    
    public void InitializeDeck()
    {
        // If currentDeck is empty, use starting deck
        if (currentDeck.Count == 0)
        {
            currentDeck.AddRange(startingDeck);
            Debug.Log($"Initialized deck with {currentDeck.Count} starting cards");
        }
        
        // Set up for combat
        PrepareForCombat();
    }
    
    public void PrepareForCombat()
    {
        drawPile.Clear();
        discardPile.Clear();
        
        // Add all current deck cards to draw pile
        drawPile.AddRange(currentDeck);
        ShuffleDeck();
        
        Debug.Log($"Prepared for combat with {drawPile.Count} cards");
    }
    
    public Card DrawCard()
    {
        // If draw pile empty, shuffle discard into draw pile
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0) return null; // No cards left
            
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }
        
        if (drawPile.Count == 0) return null;
        
        Card drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        return drawnCard;
    }
    
    public void AddToDiscard(Card card)
    {
        discardPile.Add(card);
    }
    
    // NEW: Add card to persistent deck (from rewards)
    public void AddCardToDeck(Card newCard)
    {
        if (newCard != null)
        {
            currentDeck.Add(newCard);
            Debug.Log($"Added {newCard.cardName} to deck. Deck size: {currentDeck.Count}");
        }
    }
    
    // NEW: Remove card from persistent deck
    public bool RemoveCardFromDeck(Card cardToRemove)
    {
        if (currentDeck.Contains(cardToRemove))
        {
            currentDeck.Remove(cardToRemove);
            Debug.Log($"Removed {cardToRemove.cardName} from deck. Deck size: {currentDeck.Count}");
            return true;
        }
        return false;
    }
    
    // NEW: Get current deck for viewing
    public List<Card> GetCurrentDeck()
    {
        return new List<Card>(currentDeck);
    }
    
    // NEW: Get deck statistics
    public string GetDeckStats()
    {
        int attacks = 0, skills = 0, powers = 0;
        
        foreach (Card card in currentDeck)
        {
            switch (card.type)
            {
                case CardType.Attack:
                    attacks++;
                    break;
                case CardType.Skill:
                    skills++;
                    break;
                case CardType.Power:
                    powers++;
                    break;
            }
        }
        
        return $"Deck: {currentDeck.Count} cards\nAttacks: {attacks} | Skills: {skills} | Powers: {powers}";
    }
    
    private void ShuffleDeck()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            Card temp = drawPile[i];
            int randomIndex = Random.Range(i, drawPile.Count);
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }
    
    // Reset deck for new game
    public void ResetDeck()
    {
        currentDeck.Clear();
        currentDeck.AddRange(startingDeck);
        PrepareForCombat();
        Debug.Log("Deck reset to starting configuration");
    }
}