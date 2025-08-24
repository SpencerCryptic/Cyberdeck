using UnityEngine;

[CreateAssetMenu(fileName = "New Patrol Node", menuName = "Map/Patrol Node")]
public class PatrolNode : MapNode
{
    [Header("Combat Settings")]
    public GameObject enemyPrefab; // Enemy to spawn for this encounter
    public int baseXPReward = 12;
    public int baseHealReward = 3;
    
    [Header("Enemy Stats Override")]
    public int enemyMaxHealth = 100;
    public string enemyName = "Street Punk";
    
    private void OnEnable()
    {
        nodeType = NodeType.Patrol;
        if (string.IsNullOrEmpty(nodeName))
        {
            nodeName = "Patrol Sector";
        }
    }
    
    public override void ExecuteNode()
    {
        // This triggers the combat encounter
        Debug.Log($"Starting combat at {nodeName}");
        
        // Find the combat manager and start combat
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            StartCombat(combatManager);
        }
        else
        {
            Debug.LogError("No CombatManager found! Cannot start patrol encounter.");
        }
    }
    
    private void StartCombat(CombatManager combatManager)
    {
        // Set up the enemy for this encounter
        EnemyCharacter enemy = FindObjectOfType<EnemyCharacter>();
        if (enemy != null)
        {
            // Configure enemy for this specific patrol
            enemy.name = enemyName;
            enemy.maxHealth = enemyMaxHealth;
            enemy.currentHealth = enemyMaxHealth;
            
            // Subscribe to combat end
            enemy.OnDeath += OnCombatVictory;
        }
        
        // Start the combat
        combatManager.InitializeCombat();
    }
    
    private void OnCombatVictory()
    {
        Debug.Log($"Combat victory at {nodeName}!");
        
        // Unsubscribe from enemy death
        EnemyCharacter enemy = FindObjectOfType<EnemyCharacter>();
        if (enemy != null)
        {
            enemy.OnDeath -= OnCombatVictory;
        }
        
        // Trigger post-combat choice
        PostCombatManager postCombat = FindObjectOfType<PostCombatManager>();
        if (postCombat != null)
        {
            postCombat.ShowPostCombatChoice(baseXPReward, baseHealReward);
        }
        else
        {
            // Fallback: just complete the node if no post-combat system
            Debug.Log("No PostCombatManager found, completing node automatically");
            CompleteNode();
        }
    }
    
    public override string GetDisplayText()
    {
        if (isCompleted)
            return $"{nodeName} [CLEARED]";
        else
            return $"{nodeName}\n{enemyName} - {enemyMaxHealth} HP";
    }
}