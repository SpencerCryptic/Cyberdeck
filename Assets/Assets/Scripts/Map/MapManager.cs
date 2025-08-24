using UnityEngine;
using System.Collections.Generic;
using System;

public class MapManager : MonoBehaviour
{
    [Header("Current Map")]
    public List<MapNode> currentMap = new List<MapNode>();
    public int currentNodeIndex = 0;
    
    [Header("Map Generation")]
    public List<MapNode> patrolNodeTemplates = new List<MapNode>();
    public MapNode checkpointNodeTemplate;
    
    public static MapManager Instance;
    
    // Events
    public static event Action<MapNode> OnCurrentNodeChanged;
    public static event Action OnMapCompleted;
    
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
        // Subscribe to node completion events
        MapNode.OnNodeCompleted += OnNodeCompleted;
        
        // Generate a simple test map if none exists
        if (currentMap.Count == 0)
        {
            GenerateTestMap();
        }
        
        // Start at the first node
        SetCurrentNode(0);
    }
    
    void OnDestroy()
    {
        MapNode.OnNodeCompleted -= OnNodeCompleted;
    }
    
    public void GenerateTestMap()
    {
        Debug.Log("Generating test map...");
        currentMap.Clear();
        
        // Create a simple 4-node test map: Patrol → Patrol → Patrol → Checkpoint
        
        // Create patrol nodes
        for (int i = 0; i < 3; i++)
        {
            if (patrolNodeTemplates.Count > 0)
            {
                MapNode patrolNode = Instantiate(patrolNodeTemplates[0]);
                patrolNode.nodeName = $"Floor {i + 1} - Patrol";
                patrolNode.ResetNode(); // Ensure it's available
                currentMap.Add(patrolNode);
            }
        }
        
        // Add checkpoint at the end
        if (checkpointNodeTemplate != null)
        {
            MapNode checkpoint = Instantiate(checkpointNodeTemplate);
            checkpoint.nodeName = "Floor 4 - Safe Room";
            checkpoint.ResetNode();
            currentMap.Add(checkpoint);
        }
        
        Debug.Log($"Generated map with {currentMap.Count} nodes");
    }
    
    public void SetCurrentNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= currentMap.Count)
        {
            Debug.LogError($"Invalid node index: {nodeIndex}");
            return;
        }
        
        currentNodeIndex = nodeIndex;
        MapNode currentNode = GetCurrentNode();
        
        Debug.Log($"Current node set to: {currentNode.nodeName} (Index: {nodeIndex})");
        OnCurrentNodeChanged?.Invoke(currentNode);
    }
    
    public MapNode GetCurrentNode()
    {
        if (currentNodeIndex < currentMap.Count)
        {
            return currentMap[currentNodeIndex];
        }
        return null;
    }
    
    public void EnterCurrentNode()
    {
        MapNode currentNode = GetCurrentNode();
        if (currentNode != null && currentNode.CanEnterNode())
        {
            currentNode.EnterNode();
        }
        else if (currentNode != null)
        {
            Debug.Log($"Cannot enter node: {currentNode.nodeName}");
        }
    }
    
    private void OnNodeCompleted(MapNode completedNode)
    {
        Debug.Log($"Node completed: {completedNode.nodeName}");
        
        // Check if this is the current node
        if (GetCurrentNode() == completedNode)
        {
            AdvanceToNextNode();
        }
    }
    
    public void AdvanceToNextNode()
    {
        if (currentNodeIndex < currentMap.Count - 1)
        {
            SetCurrentNode(currentNodeIndex + 1);
            Debug.Log("Advanced to next node");
        }
        else
        {
            Debug.Log("Map completed!");
            OnMapCompleted?.Invoke();
        }
    }
    
    public bool HasNextNode()
    {
        return currentNodeIndex < currentMap.Count - 1;
    }
    
    public bool IsMapComplete()
    {
        return currentNodeIndex >= currentMap.Count - 1 && 
               GetCurrentNode()?.isCompleted == true;
    }
    
    // Get info for UI display
    public string GetMapProgressText()
    {
        return $"Floor Progress: {currentNodeIndex + 1}/{currentMap.Count}";
    }
    
    public List<MapNode> GetAllNodes()
    {
        return new List<MapNode>(currentMap);
    }
    
    // Reset map for new game
    public void ResetMap()
    {
        foreach (MapNode node in currentMap)
        {
            if (node != null)
            {
                node.ResetNode();
            }
        }
        currentNodeIndex = 0;
        SetCurrentNode(0);
    }
}