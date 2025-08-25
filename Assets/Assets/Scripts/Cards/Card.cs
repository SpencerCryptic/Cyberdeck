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
        // Execute damage effects on target
        foreach (var effect in damageEffects)
        {
            effect.Execute(caster, target);
        }
        
        // Execute heal effects on caster
        foreach (var effect in healEffects)
        {
            effect.Execute(caster, caster);
        }
        
        // Execute block effects on caster (block always targets self)
        foreach (var effect in blockEffects)
        {
            effect.Execute(caster, caster);
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
            Debug.Log($"DamageEffect.Execute - Caster: {caster.name}, Target: {target.name}, Damage: {damage}");
            
            // Apply caster's damage modifiers (like Weak effect)
            int modifiedDamage = caster.GetModifiedDamage(damage);
            target.TakeDamage(modifiedDamage);
            Debug.Log($"{caster.name} deals {modifiedDamage} damage to {target.name}!");
        }
        else
        {
            Debug.LogError($"DamageEffect.Execute - Caster: {(caster ? caster.name : "null")}, Target: {(target ? target.name : "null")}");
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
            Debug.Log($"HealEffect.Execute - Caster: {caster.name}, Heal: {healAmount}");
            caster.Heal(healAmount);
            Debug.Log($"{caster.name} heals for {healAmount} HP!");
        }
        else
        {
            Debug.LogError("HealEffect.Execute - Caster is null!");
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
            Debug.Log($"BlockEffect: Adding {blockAmount} block to {caster.name}");
            caster.AddBlock(blockAmount);
            Debug.Log($"{caster.name} gains {blockAmount} block!");
        }
        else
        {
            Debug.LogError("BlockEffect: Caster is null!");
        }
    }
}