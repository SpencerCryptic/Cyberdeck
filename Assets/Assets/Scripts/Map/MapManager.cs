using UnityEngine;
using System.Collections.Generic;
using System;

// Data structure for map generation
[System.Serializable]
public class MapNodeData
{
    public MapNode node;
    public Vector2 position; // Position on the map grid
    public int layerIndex; // Which layer/floor this belongs to
    public int nodeIndex; // Index within the layer
    public List<MapNodeData> connectedNodes = new List<MapNodeData>(); // Which nodes this connects to
    public bool isAvailable = false; // Can player access this node
    public bool isCompleted = false; // Has player completed this node
    
    public MapNodeData(MapNode nodeTemplate, int layer, int index, Vector2 pos)
    {
        node = nodeTemplate;
        layerIndex = layer;
        nodeIndex = index;
        position = pos;
    }
}

public class MapManager : MonoBehaviour
{
    [Header("Current Map")]
    public List<MapNode> currentMap = new List<MapNode>();
    public int currentNodeIndex = 0;
    
    [Header("Map Generation")]
    public List<MapNode> patrolNodeTemplates = new List<MapNode>();
    public List<MapNode> eliteNodeTemplates = new List<MapNode>();
    public List<MapNode> eventNodeTemplates = new List<MapNode>();
    public MapNode checkpointNodeTemplate;
    public MapNode bossNodeTemplate;
    
    [Header("Generation Settings")]
    public int totalLayers = 15; // Floors/levels in the map
    public int pathsPerLayer = 3; // Branches per floor
    public float eliteChance = 0.15f; // Chance for elite encounters
    public float eventChance = 0.25f; // Chance for events
    public int checkpointInterval = 5; // Checkpoint every X floors
    
    // Path system
    public List<List<MapNodeData>> mapLayers = new List<List<MapNodeData>>();
    public List<MapNodeData> availableNodes = new List<MapNodeData>();
    
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
        Debug.Log("Generating branching path map...");
        GenerateBranchingMap();
    }
    
    public void GenerateBranchingMap()
    {
        mapLayers.Clear();
        availableNodes.Clear();
        currentMap.Clear();
        
        // Generate each layer
        for (int layer = 0; layer < totalLayers; layer++)
        {
            List<MapNodeData> currentLayer = new List<MapNodeData>();
            
            // Determine node types for this layer
            NodeType layerType = GetLayerType(layer);
            int nodesInLayer = GetNodesInLayer(layer, layerType);
            
            // Create nodes for this layer
            for (int i = 0; i < nodesInLayer; i++)
            {
                MapNode nodeTemplate = GetRandomNodeTemplate(layerType);
                if (nodeTemplate != null)
                {
                    MapNode newNode = Instantiate(nodeTemplate);
                    newNode.nodeName = $"Floor {layer + 1} - {GetNodeTypeName(layerType)} {i + 1}";
                    newNode.ResetNode();
                    
                    Vector2 position = new Vector2(i - (nodesInLayer - 1) * 0.5f, -layer);
                    MapNodeData nodeData = new MapNodeData(newNode, layer, i, position);
                    
                    currentLayer.Add(nodeData);
                    currentMap.Add(newNode);
                }
            }
            
            mapLayers.Add(currentLayer);
            
            // Connect to previous layer (except for first layer)
            if (layer > 0)
            {
                ConnectLayers(mapLayers[layer - 1], currentLayer);
            }
        }
        
        // Make first layer available
        if (mapLayers.Count > 0)
        {
            foreach (var node in mapLayers[0])
            {
                node.isAvailable = true;
                availableNodes.Add(node);
            }
        }
        
        Debug.Log($"Generated branching map with {mapLayers.Count} layers and {currentMap.Count} total nodes");
    }
    
    private NodeType GetLayerType(int layer)
    {
        // Special layers
        if (layer == 0) return NodeType.Patrol; // Always start with combat
        if (layer == totalLayers - 1) return NodeType.Boss; // End with boss
        if ((layer + 1) % checkpointInterval == 0) return NodeType.Checkpoint; // Regular checkpoints
        
        // Random encounters with weighted chances
        float roll = UnityEngine.Random.value;
        if (roll < eliteChance) return NodeType.Elite;
        if (roll < eliteChance + eventChance) return NodeType.Event;
        return NodeType.Patrol; // Default to patrol
    }
    
    private int GetNodesInLayer(int layer, NodeType layerType)
    {
        // Special layers have fewer nodes
        if (layerType == NodeType.Boss || layerType == NodeType.Checkpoint) return 1;
        if (layer == 0) return 2; // Start with 2 options
        
        // Most layers have 2-4 paths
        return UnityEngine.Random.Range(2, pathsPerLayer + 1);
    }
    
    private MapNode GetRandomNodeTemplate(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Patrol:
                return patrolNodeTemplates.Count > 0 ? patrolNodeTemplates[UnityEngine.Random.Range(0, patrolNodeTemplates.Count)] : null;
            case NodeType.Elite:
                return eliteNodeTemplates.Count > 0 ? eliteNodeTemplates[UnityEngine.Random.Range(0, eliteNodeTemplates.Count)] : 
                       (patrolNodeTemplates.Count > 0 ? patrolNodeTemplates[0] : null); // Fallback
            case NodeType.Event:
                return eventNodeTemplates.Count > 0 ? eventNodeTemplates[UnityEngine.Random.Range(0, eventNodeTemplates.Count)] : 
                       (patrolNodeTemplates.Count > 0 ? patrolNodeTemplates[0] : null); // Fallback
            case NodeType.Checkpoint:
                return checkpointNodeTemplate;
            case NodeType.Boss:
                return bossNodeTemplate ?? (patrolNodeTemplates.Count > 0 ? patrolNodeTemplates[0] : null); // Fallback
            default:
                return patrolNodeTemplates.Count > 0 ? patrolNodeTemplates[0] : null;
        }
    }
    
    private string GetNodeTypeName(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Patrol: return "Patrol";
            case NodeType.Elite: return "Elite";
            case NodeType.Event: return "Event";
            case NodeType.Checkpoint: return "Safe Room";
            case NodeType.Boss: return "Boss";
            default: return "Unknown";
        }
    }
    
    private void ConnectLayers(List<MapNodeData> previousLayer, List<MapNodeData> currentLayer)
    {
        // Each node in current layer connects to 1-3 nodes from previous layer
        foreach (var currentNode in currentLayer)
        {
            int connections = UnityEngine.Random.Range(1, Mathf.Min(4, previousLayer.Count + 1));
            List<MapNodeData> shuffledPrevious = new List<MapNodeData>(previousLayer);
            
            // Shuffle for random connections
            for (int i = 0; i < shuffledPrevious.Count; i++)
            {
                var temp = shuffledPrevious[i];
                int randomIndex = UnityEngine.Random.Range(i, shuffledPrevious.Count);
                shuffledPrevious[i] = shuffledPrevious[randomIndex];
                shuffledPrevious[randomIndex] = temp;
            }
            
            // Connect to the first 'connections' nodes
            for (int i = 0; i < connections && i < shuffledPrevious.Count; i++)
            {
                shuffledPrevious[i].connectedNodes.Add(currentNode);
            }
        }
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