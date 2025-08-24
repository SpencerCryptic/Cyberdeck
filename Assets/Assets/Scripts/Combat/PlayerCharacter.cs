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
    }
    
    public bool CanPlayCard(Card card)
    {
        return energy >= card.cost;
    }
    
    public void PlayCard(Card card, Character target = null)
    {
        if (!CanPlayCard(card)) return;
        
        energy -= card.cost;
        OnEnergyChanged?.Invoke(energy);
        
        // Execute card effects using the new system
        card.ExecuteEffects(this, target);
        
        // Tell hand manager to discard card
        HandManager handManager = FindObjectOfType<HandManager>();
        if (handManager != null)
        {
            handManager.DiscardCard(card);
        }
    }
}