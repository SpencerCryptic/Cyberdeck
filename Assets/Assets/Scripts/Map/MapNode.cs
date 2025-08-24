using UnityEngine;
using System;

// Base class for all map nodes
public abstract class MapNode : ScriptableObject
{
    [Header("Node Info")]
    public string nodeName = "Unknown Node";
    public NodeType nodeType;
    public Sprite nodeIcon;
    public string description = "";
    
    [Header("Completion")]
    public bool isCompleted = false;
    public bool isAvailable = true;
    
    // Events for UI updates
    public static event Action<MapNode> OnNodeCompleted;
    public static event Action<MapNode> OnNodeEntered;
    
    // Execute this node's functionality
    public abstract void ExecuteNode();
    
    // Called when node is completed successfully
    public virtual void CompleteNode()
    {
        isCompleted = true;
        OnNodeCompleted?.Invoke(this);
        Debug.Log($"Node completed: {nodeName}");
    }
    
    // Called when player enters this node
    public virtual void EnterNode()
    {
        OnNodeEntered?.Invoke(this);
        Debug.Log($"Entering node: {nodeName}");
        ExecuteNode();
    }
    
    // Check if this node can be entered
    public virtual bool CanEnterNode()
    {
        return isAvailable && !isCompleted;
    }
    
    // Reset node state (for new runs)
    public virtual void ResetNode()
    {
        isCompleted = false;
        isAvailable = true;
    }
    
    // Get display info for UI
    public virtual string GetDisplayText()
    {
        string status = isCompleted ? "[CLEARED]" : "[ACTIVE]";
        return $"{nodeName} {status}";
    }
}

// Types of nodes available
public enum NodeType
{
    Patrol,      // Standard combat
    Elite,       // Harder combat  
    Checkpoint,  // Rest/shop
    Event,       // Story choices
    Boss,        // Sector bosses
    Start        // Starting position
}