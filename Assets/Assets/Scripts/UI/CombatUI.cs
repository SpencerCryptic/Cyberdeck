using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour
{
    [Header("Player UI")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerEnergyText;
    public TextMeshProUGUI playerBlockText;
    public Slider playerHealthSlider;
    
    [Header("Enemy UI")]
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI enemyBlockText;
    public TextMeshProUGUI enemyIntentionText;
    public Slider enemyHealthSlider;
    public Image intentionIcon;
    
    [Header("Game State UI")]
    public TextMeshProUGUI turnIndicatorText;
    
    [Header("Intention Icons")]
    public Sprite attackIcon;
    public Sprite blockIcon;
    public Sprite unknownIcon;
    
    private PlayerCharacter player;
    private EnemyCharacter enemy;
    
    public void Initialize(PlayerCharacter playerChar, EnemyCharacter enemyChar)
    {
        // Unsubscribe from old events first (if any)
        if (player != null)
        {
            player.OnHealthChanged -= UpdatePlayerHealth;
            player.OnEnergyChanged -= UpdatePlayerEnergy;
            player.OnBlockChanged -= UpdatePlayerBlock;
        }
        
        if (enemy != null)
        {
            enemy.OnHealthChanged -= UpdateEnemyHealth;
            enemy.OnBlockChanged -= UpdateEnemyBlock;
            enemy.OnIntentionChanged -= UpdateEnemyIntention;
        }
        
        // Set new references
        player = playerChar;
        enemy = enemyChar;
        
        // Subscribe to new events
        if (player != null)
        {
            player.OnHealthChanged += UpdatePlayerHealth;
            player.OnEnergyChanged += UpdatePlayerEnergy;
            player.OnBlockChanged += UpdatePlayerBlock;
            
            // Initialize player UI
            if (playerHealthSlider != null)
            {
                playerHealthSlider.maxValue = player.maxHealth;
            }
            UpdatePlayerHealth(player.currentHealth);
            UpdatePlayerEnergy(player.energy);
            UpdatePlayerBlock(player.currentBlock);
        }
        
        if (enemy != null)
        {
            enemy.OnHealthChanged += UpdateEnemyHealth;
            enemy.OnBlockChanged += UpdateEnemyBlock;
            enemy.OnIntentionChanged += UpdateEnemyIntention;
            
            // Initialize enemy UI
            if (enemyHealthSlider != null)
            {
                enemyHealthSlider.maxValue = enemy.maxHealth;
            }
            UpdateEnemyHealth(enemy.currentHealth);
            UpdateEnemyBlock(enemy.currentBlock);
        }
    }
    
    private void UpdatePlayerHealth(int newHealth)
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"{newHealth}/{player.maxHealth}";
        }
        
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = newHealth;
        }
    }
    
    private void UpdatePlayerEnergy(int newEnergy)
    {
        if (playerEnergyText != null)
        {
            playerEnergyText.text = $"Energy: {newEnergy}";
        }
    }
    
    private void UpdatePlayerBlock(int newBlock)
    {
        if (playerBlockText != null)
        {
            if (newBlock > 0)
            {
                playerBlockText.text = $"🛡️ {newBlock}";
                playerBlockText.color = Color.cyan;
            }
            else
            {
                playerBlockText.text = "";
            }
        }
        Debug.Log($"Player block UI updated: {newBlock}");
    }
    
    private void UpdateEnemyHealth(int newHealth)
    {
        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"{newHealth}/{enemy.maxHealth}";
        }
        
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.value = newHealth;
        }
    }
    
    private void UpdateEnemyBlock(int newBlock)
    {
        if (enemyBlockText != null)
        {
            if (newBlock > 0)
            {
                enemyBlockText.text = $"🛡️ {newBlock}";
                enemyBlockText.color = Color.cyan;
            }
            else
            {
                enemyBlockText.text = "";
            }
        }
        Debug.Log($"Enemy block UI updated: {newBlock}");
    }
    
    private void UpdateEnemyIntention(EnemyIntention intention, int value)
    {
        if (enemyIntentionText != null)
        {
            string intentionStr = intention == EnemyIntention.Attack ? $"Attack {value}" : 
                                 intention == EnemyIntention.Block ? $"Block {value}" : 
                                 "Unknown";
            enemyIntentionText.text = intentionStr;
        }
        
        if (intentionIcon != null)
        {
            switch (intention)
            {
                case EnemyIntention.Attack:
                    intentionIcon.sprite = attackIcon;
                    break;
                case EnemyIntention.Block:
                    intentionIcon.sprite = blockIcon;
                    break;
                default:
                    intentionIcon.sprite = unknownIcon;
                    break;
            }
        }
    }
    
    public void SetTurnIndicator(string text)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = text;
        }
    }
}