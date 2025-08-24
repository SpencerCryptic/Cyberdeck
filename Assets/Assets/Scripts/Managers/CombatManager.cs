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
        player.StartTurn();
        
        if (combatUI != null)
        {
            combatUI.SetTurnIndicator("Your Turn");
        }
    }
    
    public void EndPlayerTurn()
    {
        if (currentState != CombatState.PlayerTurn) return;
        
        // Process end of player turn effects
        if (player != null)
        {
            player.ProcessEndOfTurn();
        }
        
        currentState = CombatState.EnemyTurn;
        StartEnemyTurn();
    }
    
    public void StartEnemyTurn()
    {
        if (combatUI != null)
        {
            combatUI.SetTurnIndicator("Enemy Turn");
        }
        
        // Execute enemy intention
        if (enemy != null)
        {
            enemy.ExecuteIntention();
            
            // Process end of enemy turn effects
            enemy.ProcessEndOfTurn();
        }
        
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