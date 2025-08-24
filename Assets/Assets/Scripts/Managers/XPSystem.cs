using UnityEngine;
using System;

public class XPSystem : MonoBehaviour
{
    [Header("XP Settings")]
    public int currentXP = 0;
    public int totalXPEarned = 0; // For statistics
    
    [Header("UI References")]
    public TMPro.TextMeshProUGUI xpDisplayText;
    
    public static XPSystem Instance;
    
    // Events for UI updates
    public static event Action<int, int> OnXPChanged; // current XP, XP gained
    public static event Action<int> OnXPSpent;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        UpdateXPDisplay();
    }
    
    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        
        int oldXP = currentXP;
        currentXP += amount;
        totalXPEarned += amount;
        
        Debug.Log($"Gained {amount} XP. Total: {currentXP} XP");
        
        OnXPChanged?.Invoke(currentXP, amount);
        UpdateXPDisplay();
    }
    
    public bool CanSpendXP(int amount)
    {
        return currentXP >= amount;
    }
    
    public bool SpendXP(int amount)
    {
        if (!CanSpendXP(amount))
        {
            Debug.LogWarning($"Cannot spend {amount} XP. Only have {currentXP} XP");
            return false;
        }
        
        currentXP -= amount;
        Debug.Log($"Spent {amount} XP. Remaining: {currentXP} XP");
        
        OnXPSpent?.Invoke(amount);
        UpdateXPDisplay();
        return true;
    }
    
    public int GetCurrentXP()
    {
        return currentXP;
    }
    
    public int GetTotalXPEarned()
    {
        return totalXPEarned;
    }
    
    private void UpdateXPDisplay()
    {
        if (xpDisplayText != null)
        {
            xpDisplayText.text = $"XP: {currentXP}";
        }
    }
    
    // For save/load system later
    [System.Serializable]
    public struct XPSaveData
    {
        public int currentXP;
        public int totalXPEarned;
    }
    
    public XPSaveData GetSaveData()
    {
        return new XPSaveData
        {
            currentXP = this.currentXP,
            totalXPEarned = this.totalXPEarned
        };
    }
    
    public void LoadSaveData(XPSaveData data)
    {
        currentXP = data.currentXP;
        totalXPEarned = data.totalXPEarned;
        UpdateXPDisplay();
    }
    
    // Reset for new game
    public void ResetXP()
    {
        currentXP = 0;
        totalXPEarned = 0;
        UpdateXPDisplay();
        Debug.Log("XP system reset");
    }
}