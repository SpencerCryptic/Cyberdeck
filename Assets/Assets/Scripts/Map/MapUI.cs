using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mapPanel;
    public Button enterNodeButton;
    public Button mapToggleButton;
    public TextMeshProUGUI currentNodeText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI nodeDescriptionText;
    
    [Header("Node Display")]
    public Transform nodeDisplayParent;
    public GameObject nodeDisplayPrefab;
    
    private MapManager mapManager;
    private bool mapIsOpen = false;
    private List<GameObject> nodeDisplays = new List<GameObject>();
    
    void Start()
    {
        mapManager = MapManager.Instance;
        
        // Set up buttons
        if (enterNodeButton != null)
            enterNodeButton.onClick.AddListener(OnEnterNodePressed);
        if (mapToggleButton != null)
            mapToggleButton.onClick.AddListener(ToggleMap);
        
        // Subscribe to map events
        if (mapManager != null)
        {
            MapManager.OnCurrentNodeChanged += OnCurrentNodeChanged;
            MapManager.OnMapCompleted += OnMapCompleted;
        }
        
        // Initially hide map
        if (mapPanel != null)
            mapPanel.SetActive(false);
        
        // Set up initial display
        RefreshMapDisplay();
    }
    
    void OnDestroy()
    {
        if (mapManager != null)
        {
            MapManager.OnCurrentNodeChanged -= OnCurrentNodeChanged;
            MapManager.OnMapCompleted -= OnMapCompleted;
        }
    }
    
    public void ToggleMap()
    {
        mapIsOpen = !mapIsOpen;
        
        if (mapPanel != null)
        {
            mapPanel.SetActive(mapIsOpen);
        }
        
        if (mapIsOpen)
        {
            RefreshMapDisplay();
        }
        
        // Update button text
        if (mapToggleButton != null)
        {
            TextMeshProUGUI buttonText = mapToggleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = mapIsOpen ? "Close Map" : "Open Map";
            }
        }
        
        Debug.Log($"Map toggled: {mapIsOpen}");
    }
    
    public void OnEnterNodePressed()
    {
        if (mapManager != null)
        {
            mapManager.EnterCurrentNode();
            
            // Close map after entering node
            if (mapIsOpen)
            {
                ToggleMap();
            }
        }
    }
    
    private void OnCurrentNodeChanged(MapNode newNode)
    {
        RefreshMapDisplay();
    }
    
    private void OnMapCompleted()
    {
        Debug.Log("Map completed!");
        RefreshMapDisplay();
    }
    
    private void RefreshMapDisplay()
    {
        if (mapManager == null) return;
        
        MapNode currentNode = mapManager.GetCurrentNode();
        
        // Update current node info
        if (currentNodeText != null)
        {
            if (currentNode != null)
            {
                currentNodeText.text = $"Current: {currentNode.nodeName}";
            }
            else
            {
                currentNodeText.text = "No Current Node";
            }
        }
        
        // Update progress
        if (progressText != null)
        {
            progressText.text = mapManager.GetMapProgressText();
        }
        
        // Update node description
        if (nodeDescriptionText != null && currentNode != null)
        {
            nodeDescriptionText.text = currentNode.GetDisplayText();
        }
        
        // Update enter button
        if (enterNodeButton != null && currentNode != null)
        {
            enterNodeButton.interactable = currentNode.CanEnterNode();
            
            TextMeshProUGUI buttonText = enterNodeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (currentNode.isCompleted)
                    buttonText.text = "Completed";
                else if (currentNode.CanEnterNode())
                    buttonText.text = $"Enter {currentNode.nodeName}";
                else
                    buttonText.text = "Unavailable";
            }
        }
        
        // Update node list display
        RefreshNodeList();
    }
    
    private void RefreshNodeList()
    {
        if (nodeDisplayParent == null || nodeDisplayPrefab == null) return;
        
        // Clear existing displays
        ClearNodeDisplays();
        
        // Create displays for all nodes
        List<MapNode> allNodes = mapManager.GetAllNodes();
        for (int i = 0; i < allNodes.Count; i++)
        {
            CreateNodeDisplay(allNodes[i], i);
        }
    }
    
    private void CreateNodeDisplay(MapNode node, int index)
    {
        GameObject nodeObj = Instantiate(nodeDisplayPrefab, nodeDisplayParent);
        nodeDisplays.Add(nodeObj);
        
        // Set up the display
        TextMeshProUGUI nodeText = nodeObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nodeText != null)
        {
            string statusIcon = node.isCompleted ? "✓" : 
                               (index == mapManager.currentNodeIndex) ? "→" : "○";
            nodeText.text = $"{statusIcon} {node.nodeName}";
        }
        
        // Set up colors
        Image nodeImage = nodeObj.GetComponent<Image>();
        if (nodeImage != null)
        {
            if (node.isCompleted)
                nodeImage.color = Color.green;
            else if (index == mapManager.currentNodeIndex)
                nodeImage.color = Color.yellow;
            else
                nodeImage.color = Color.white;
        }
        
        // Make current node interactable
        Button nodeButton = nodeObj.GetComponent<Button>();
        if (nodeButton != null && index == mapManager.currentNodeIndex)
        {
            nodeButton.onClick.AddListener(() => OnEnterNodePressed());
        }
    }
    
    private void ClearNodeDisplays()
    {
        foreach (GameObject display in nodeDisplays)
        {
            if (display != null)
                Destroy(display);
        }
        nodeDisplays.Clear();
    }
    
    // Update XP display
    void Update()
    {
        // Simple XP display in map UI
        XPSystem xpSystem = XPSystem.Instance;
        if (xpSystem != null && progressText != null)
        {
            string mapProgress = mapManager?.GetMapProgressText() ?? "";
            progressText.text = $"{mapProgress}\nXP: {xpSystem.GetCurrentXP()}";
        }
    }
}