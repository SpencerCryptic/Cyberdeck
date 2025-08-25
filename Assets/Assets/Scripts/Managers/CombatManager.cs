using UnityEngine;
using UnityEngine.UI;

public enum CombatState
{
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

public class CombatManager : MonoBehaviour
{
    [Header("Combat References")]
    public PlayerCharacter player;
    public EnemyCharacter enemy;
    public Button endTurnButton;
    
    [Header("UI References")]
    public CombatUI combatUI;
    
    private CombatState currentState;
    
    void Start()
    {
        InitializeCombat();
    }
    
    public void InitializeCombat()
    {
        currentState = CombatState.PlayerTurn;
        
        // Setup UI
        if (combatUI != null)
        {
            combatUI.Initialize(player, enemy);
        }
        
        // Setup button
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(EndPlayerTurn);
        }
        
        // Subscribe to death events
        if (player != null)
        {
            player.OnDeath += OnPlayerDeath;
        }
        
        if (enemy != null)
        {
            enemy.OnDeath += OnEnemyDeath;
            enemy.PlanNextTurn(); // Set initial intention
        }
        
        StartPlayerTurn();
    }
    
    public void StartPlayerTurn()
    {
        currentState = CombatState.PlayerTurn;
        Debug.Log("=== PLAYER TURN START ===");
        
        // 1. Clear old block at START of player turn (Slay the Spire timing)
        if (player != null)
        {
            if (player.currentBlock > 0)
            {
                Debug.Log($"Clearing player's old block: {player.currentBlock} → 0");
                player.ResetBlock();
            }
        }
        
        // 2. Start player turn (draw cards, reset energy)
        player.StartTurn();
        
        if (combatUI != null)
        {
            combatUI.SetTurnIndicator("Your Turn");
        }
        
        Debug.Log("Player turn ready - can play cards");
    }
    
    public void EndPlayerTurn()
    {
        if (currentState != CombatState.PlayerTurn) return;
        
        Debug.Log("=== PLAYER TURN END ===");
        
        // 1. Discard hand, reset energy (done by player)
        // 2. End-of-turn effects would fire here (like Metallicize giving block)
        // 3. Block STAYS on player (protects during enemy phase)
        
        Debug.Log($"Player ending turn with {player.currentBlock} block (will protect during enemy phase)");
        
        currentState = CombatState.EnemyTurn;
        StartEnemyTurn();
    }
    
    public void StartEnemyTurn()
    {
        Debug.Log("=== ENEMY TURN START ===");
        
        if (combatUI != null)
        {
            combatUI.SetTurnIndicator("Enemy Turn");
        }
        
        // Enemy processes its turn
        if (enemy != null)
        {
            // 1. Enemy start-of-turn effects (like Poison damage)
            // 2. Enemy executes its intention
            Debug.Log($"Enemy executing intention. Player has {player.currentBlock} block for protection.");
            enemy.ExecuteIntention();
            
            // 3. Enemy end-of-turn effects (like gaining block from Metallicize)
            // Note: Enemy block would also be cleared at START of enemy's next turn
        }
        
        Debug.Log("Enemy turn complete");
        
        // After a short delay, start player turn again
        Invoke(nameof(StartPlayerTurn), 1.5f);
    }
    
    private void OnPlayerDeath()
    {
        currentState = CombatState.Defeat;
        if (combatUI != null)
        {
            combatUI.SetTurnIndicator("DEFEAT");
        }
        Debug.Log("Player defeated!");
    }
    
    private void OnEnemyDeath()
    {
        currentState = CombatState.Victory;
        if (combatUI != null)
        {
            combatUI.SetTurnIndicator("VICTORY!");
        }
        Debug.Log("Enemy defeated!");
    }
}