using UnityEngine;

public class PlayerCharacter : Character
{
    [Header("Player Stats")]
    public int energy = 3;
    public int maxEnergy = 3;
    
    [Header("Turn Settings")]
    public int cardsPerTurn = 5;
    
    public event System.Action<int> OnEnergyChanged;
    
    private HandManager handManager;
    
    protected override void Start()
    {
        base.Start();
        
        // Cache HandManager reference
        handManager = FindObjectOfType<HandManager>();
        if (handManager == null)
        {
            Debug.LogError("PlayerCharacter: No HandManager found in scene!");
        }
    }
    
    public void StartTurn()
    {
        Debug.Log("Player turn started");
        
        // Reset energy
        energy = maxEnergy;
        OnEnergyChanged?.Invoke(energy);
        
        // Draw cards for the turn
        if (handManager != null)
        {
            handManager.DrawCards(cardsPerTurn);
        }
        else
        {
            Debug.LogError("Cannot draw cards: HandManager is null!");
        }
    }
    
    public bool CanPlayCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("CanPlayCard: Card is null");
            return false;
        }
        
        bool canPlay = energy >= card.cost;
        
        if (!canPlay)
        {
            Debug.Log($"Cannot play {card.cardName}: need {card.cost} energy, have {energy}");
        }
        
        return canPlay;
    }
    
    public void PlayCard(Card card, Character target = null)
    {
        if (card == null)
        {
            Debug.LogError("PlayCard: Card is null!");
            return;
        }
        
        if (!CanPlayCard(card)) 
        {
            Debug.LogWarning($"Cannot play {card.cardName}: insufficient energy");
            return;
        }
        
        // Spend energy
        energy -= card.cost;
        OnEnergyChanged?.Invoke(energy);
        
        Debug.Log($"Playing {card.cardName} (cost: {card.cost}, remaining energy: {energy})");
        
        // Execute card effects using the new system
        card.ExecuteEffects(this, target);
        
        // Remove card from hand
        if (handManager != null)
        {
            handManager.DiscardCard(card);
        }
        else
        {
            Debug.LogError("Cannot discard card: HandManager is null!");
        }
    }
    
    public override void ProcessEndOfTurn()
    {
        base.ProcessEndOfTurn();
        
        // Discard remaining hand
        if (handManager != null)
        {
            handManager.DiscardHand();
        }
        
        Debug.Log("Player turn ended");
    }
    
    // Helper method to get current energy for UI
    public int GetCurrentEnergy()
    {
        return energy;
    }
    
    public int GetMaxEnergy()
    {
        return maxEnergy;
    }
    
    // Method to modify max energy (for upgrades/relics later)
    public void ModifyMaxEnergy(int amount)
    {
        maxEnergy = Mathf.Max(0, maxEnergy + amount);
        energy = Mathf.Min(energy, maxEnergy); // Don't exceed new max
        OnEnergyChanged?.Invoke(energy);
        Debug.Log($"Max energy changed by {amount}. New max: {maxEnergy}");
    }
}