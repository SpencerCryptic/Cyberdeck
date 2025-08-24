using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card")]
public class Card : ScriptableObject
{
    public string cardName = "New Card";
    public int cost = 1;
    public CardType type = CardType.Attack;
    public string description = "";
    public Sprite artwork;
    
    [Header("Card Effects")]
    public List<DamageEffect> damageEffects = new List<DamageEffect>();
    public List<HealEffect> healEffects = new List<HealEffect>();
    public List<BlockEffect> blockEffects = new List<BlockEffect>();
    
    // Execute all effects on the card
    public void ExecuteEffects(Character caster, Character target)
    {
        // Execute damage effects
        foreach (var effect in damageEffects)
        {
            effect.Execute(caster, target);
        }
        
        // Execute heal effects  
        foreach (var effect in healEffects)
        {
            effect.Execute(caster, target);
        }
        
        // Execute block effects
        foreach (var effect in blockEffects)
        {
            effect.Execute(caster, target);
        }
    }
}

public enum CardType
{
    Attack,
    Skill,
    Power
}

// ===== CONCRETE CARD EFFECTS =====

[System.Serializable]
public class DamageEffect
{
    public int damage = 6;
    
    public void Execute(Character caster, Character target)
    {
        if (target != null && caster != null)
        {
            // Apply caster's damage modifiers (like Weak effect)
            int modifiedDamage = caster.GetModifiedDamage(damage);
            target.TakeDamage(modifiedDamage);
            Debug.Log($"{caster.name} deals {modifiedDamage} damage to {target.name}!");
        }
    }
}

[System.Serializable]
public class HealEffect
{
    public int healAmount = 5;
    
    public void Execute(Character caster, Character target)
    {
        if (caster != null)
        {
            caster.Heal(healAmount);
            Debug.Log($"{caster.name} heals for {healAmount} HP!");
        }
    }
}

[System.Serializable]
public class BlockEffect
{
    public int blockAmount = 5;
    
    public void Execute(Character caster, Character target)
    {
        Debug.Log($"BlockEffect.Execute called - caster: {caster?.name}, target: {target?.name}");
        
        if (caster != null)
        {
            caster.AddBlock(blockAmount);
            Debug.Log($"{caster.name} gains {blockAmount} block!");
        }
        else
        {
            Debug.LogError("BlockEffect: Caster is null!");
        }
    }
}