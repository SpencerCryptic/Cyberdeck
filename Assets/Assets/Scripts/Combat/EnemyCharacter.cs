using UnityEngine;

public enum EnemyIntention
{
    Attack,
    Block,
    Buff,
    Debuff,
    Unknown
}

public class EnemyCharacter : Character
{
    [Header("Enemy AI")]
    public EnemyIntention currentIntention;
    public int intentionValue; // Damage amount, block amount, etc.
    public string intentionDescription;
    
    public event System.Action<EnemyIntention, int> OnIntentionChanged;
    
    public void SetIntention(EnemyIntention intention, int value, string description = "")
    {
        currentIntention = intention;
        intentionValue = value;
        intentionDescription = description;
        OnIntentionChanged?.Invoke(intention, value);
    }
    
    public void ExecuteIntention()
    {
        var player = FindObjectOfType<PlayerCharacter>();
        
        switch (currentIntention)
        {
            case EnemyIntention.Attack:
                if (player != null)
                {
                    // Use the proper TakeDamage method that respects block
                    player.TakeDamage(intentionValue);
                    Debug.Log($"{name} attacks for {intentionValue} damage!");
                }
                break;
            case EnemyIntention.Block:
                // Enemy gains block
                AddBlock(intentionValue);
                Debug.Log($"{name} gains {intentionValue} block!");
                break;
        }
        
        // Set next intention (simple random for now)
        PlanNextTurn();
    }
    
    public void PlanNextTurn()
    {
        // Simple AI: randomly choose between attack (8-12 damage) or block (5-8)
        if (Random.Range(0f, 1f) > 0.3f)
        {
            SetIntention(EnemyIntention.Attack, Random.Range(8, 13), "Attack");
        }
        else
        {
            SetIntention(EnemyIntention.Block, Random.Range(5, 9), "Block");
        }
    }
}