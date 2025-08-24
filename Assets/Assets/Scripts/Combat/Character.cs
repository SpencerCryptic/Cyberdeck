using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class Character : MonoBehaviour
{
    [Header("Character Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int currentBlock = 0;
    
    [Header("Status Effects")]
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
    
    // Events
    public event Action<int> OnHealthChanged;
    public event Action<int> OnBlockChanged;
    public event Action OnDeath;
    public event Action<StatusEffect> OnStatusEffectAdded;
    public event Action<StatusEffect> OnStatusEffectRemoved;
    
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentBlock = 0;
    }
    
    public virtual void TakeDamage(int damage)
    {
        // Apply status effect modifiers to incoming damage
        int modifiedDamage = GetModifiedDamageTaken(damage);
        
        // Apply block first
        int remainingDamage = modifiedDamage;
        if (currentBlock > 0)
        {
            int blockedAmount = Mathf.Min(currentBlock, modifiedDamage);
            remainingDamage -= blockedAmount;
            RemoveBlock(blockedAmount);
            Debug.Log($"{name} blocked {blockedAmount} damage");
        }
        
        // Apply remaining damage to health
        if (remainingDamage > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remainingDamage);
            OnHealthChanged?.Invoke(currentHealth);
            Debug.Log($"{name} took {remainingDamage} damage");
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public virtual void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"{name} healed {amount} HP");
    }
    
public virtual void AddBlock(int amount)
{
    currentBlock += amount;
    OnBlockChanged?.Invoke(currentBlock);
    Debug.Log($"{name} gained {amount} block (total: {currentBlock})");
    Debug.Log($"ACTUAL currentBlock field value: {currentBlock}"); // ADD THIS LINE
}
    public virtual void RemoveBlock(int amount)
    {
        currentBlock = Mathf.Max(0, currentBlock - amount);
        OnBlockChanged?.Invoke(currentBlock);
    }
    
    public virtual void ResetBlock()
    {
        currentBlock = 0;
        OnBlockChanged?.Invoke(currentBlock);
    }
    
    // Status effect system
    public virtual void AddStatusEffect(StatusEffect effect)
    {
        // Check if we already have this type of effect
        StatusEffect existing = activeStatusEffects.Find(e => e.GetType() == effect.GetType());
        if (existing != null)
        {
            // Stack or refresh the effect
            existing.Refresh(effect.duration, effect.potency);
        }
        else
        {
            // Add new effect
            activeStatusEffects.Add(effect);
            effect.OnApplied(this);
            OnStatusEffectAdded?.Invoke(effect);
        }
        
        Debug.Log($"{name} gained {effect.GetType().Name}");
    }
    
    public virtual void RemoveStatusEffect(StatusEffect effect)
    {
        if (activeStatusEffects.Remove(effect))
        {
            effect.OnRemoved(this);
            OnStatusEffectRemoved?.Invoke(effect);
            Debug.Log($"{name} lost {effect.GetType().Name}");
        }
    }
    
    // Called at end of turn to process status effects
    public virtual void ProcessEndOfTurn()
    {
        // Process status effect durations
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeStatusEffects[i];
            effect.ProcessEndOfTurn();
            
            if (effect.duration <= 0)
            {
                RemoveStatusEffect(effect);
            }
        }
        
        // Reset block at end of turn (standard Slay the Spire rule)
        ResetBlock();
    }
    
    protected virtual void Die()
    {
        OnDeath?.Invoke();
    }
    
    // Helper methods for damage calculation
    public virtual int GetModifiedDamage(int baseDamage)
    {
        float modifier = 1.0f;
        
        // Apply status effect modifiers
        foreach (StatusEffect effect in activeStatusEffects)
        {
            modifier *= effect.GetDamageDealtModifier();
        }
        
        return Mathf.RoundToInt(baseDamage * modifier);
    }
    
    public virtual int GetModifiedDamageTaken(int incomingDamage)
    {
        float modifier = 1.0f;
        
        // Apply status effect modifiers
        foreach (StatusEffect effect in activeStatusEffects)
        {
            modifier *= effect.GetDamageTakenModifier();
        }
        
        return Mathf.RoundToInt(incomingDamage * modifier);
    }
}

// ===== BASIC STATUS EFFECT SYSTEM =====

[System.Serializable]
public abstract class StatusEffect
{
    public int duration;
    public int potency; // For stacking effects like Exposed
    
    public StatusEffect(int duration, int potency = 1)
    {
        this.duration = duration;
        this.potency = potency;
    }
    
    public virtual void OnApplied(Character target) { }
    public virtual void OnRemoved(Character target) { }
    public virtual void ProcessEndOfTurn() 
    { 
        duration--; 
    }
    
    public virtual void Refresh(int newDuration, int newPotency)
    {
        duration = newDuration;
        potency += newPotency; // Stack potency
    }
    
    // Damage modifiers
    public virtual float GetDamageDealtModifier() { return 1.0f; }
    public virtual float GetDamageTakenModifier() { return 1.0f; }
}

// ===== BASIC STATUS EFFECTS =====

[System.Serializable]
public class VulnerableEffect : StatusEffect
{
    public VulnerableEffect(int duration) : base(duration) { }
    
    public override float GetDamageTakenModifier()
    {
        return 1.5f; // +50% damage taken
    }
}

[System.Serializable]
public class WeakEffect : StatusEffect
{
    public WeakEffect(int duration) : base(duration) { }
    
    public override float GetDamageDealtModifier()
    {
        return 0.75f; // -25% damage dealt
    }
}