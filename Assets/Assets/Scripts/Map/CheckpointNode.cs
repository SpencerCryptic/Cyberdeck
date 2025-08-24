using UnityEngine;

[CreateAssetMenu(fileName = "New Checkpoint", menuName = "Map/Checkpoint Node")]
public class CheckpointNode : MapNode
{
    [Header("Checkpoint Settings")]
    public int freeHealAmount = 15;
    public bool allowsFullRest = true;
    
    private void OnEnable()
    {
        nodeType = NodeType.Checkpoint;
        if (string.IsNullOrEmpty(nodeName))
        {
            nodeName = "Safe Room";
        }
        if (string.IsNullOrEmpty(description))
        {
            description = "A secure checkpoint where you can rest and recover.";
        }
    }
    
    public override void ExecuteNode()
    {
        Debug.Log($"Entered checkpoint: {nodeName}");
        
        // For now, just do basic rest and complete
        // TODO: Add CheckpointManager later for XP spending
        Debug.Log("Checkpoint entered - doing basic rest");
        DoBasicRest();
        CompleteNode();
    }
    
    public void DoBasicRest()
    {
        PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
        if (player != null)
        {
            player.Heal(freeHealAmount);
            Debug.Log($"Rested at checkpoint. Healed {freeHealAmount} HP");
        }
    }
    
    public void DoFullRest()
    {
        if (!allowsFullRest) return;
        
        PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
        if (player != null)
        {
            int healAmount = player.maxHealth - player.currentHealth;
            player.Heal(healAmount);
            Debug.Log($"Full rest at checkpoint. Healed {healAmount} HP");
        }
    }
    
    public override string GetDisplayText()
    {
        if (isCompleted)
            return $"{nodeName} [RESTED]";
        else
            return $"{nodeName}\nHeal {freeHealAmount} HP";
    }
}