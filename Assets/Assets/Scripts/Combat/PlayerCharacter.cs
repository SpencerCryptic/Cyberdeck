using UnityEngine;

public class PlayerCharacter : Character
{
    [Header("Player Stats")]
    public int energy = 3;
    public int maxEnergy = 3;
    
    public event System.Action<int> OnEnergyChanged;
    
    public void StartTurn()
    {
        energy = maxEnergy;
        OnEnergyChanged?.Invoke(energy);
        
        // Find and tell hand manager to draw cards
        HandManager handManager = FindObjectOfType<HandManager>();
        if (handManager != null)
        {
            handManager.DrawCards(5);
        }
        
        Debug.Log($"Player turn started. Energy: {energy}");
    }
    
    public bool CanPlayCard(Card card)
    {
        bool canPlay = energy >= card.cost;
        Debug.Log($"CanPlayCard({card.cardName}): Energy {energy} >= Cost {card.cost} = {canPlay}");
        return canPlay;
    }
    
    public void PlayCard(Card card, Character target = null)
    {
        if (!CanPlayCard(card))
        {
            Debug.Log($"Cannot play {card.cardName} - not enough energy!");
            return;
        }
        
        Debug.Log($"Playing {card.cardName} - Energy before: {energy}");
        
        // Consume energy FIRST
        energy -= card.cost;
        OnEnergyChanged?.Invoke(energy);
        Debug.Log($"Energy after playing {card.cardName}: {energy}");
        
        // Execute card effects using the new system
        card.ExecuteEffects(this, target);
        
        // Tell hand manager to discard card
        HandManager handManager = FindObjectOfType<HandManager>();
        if (handManager != null)
        {
            handManager.DiscardCard(card);
        }
    }
    
    // Alternative method for CardDisplay to call
    public void ConsumeEnergy(int amount)
    {
        energy = Mathf.Max(0, energy - amount);
        OnEnergyChanged?.Invoke(energy);
        Debug.Log($"Energy consumed: {amount}, remaining: {energy}");
    }
}