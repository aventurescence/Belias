using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using Belias.Windows.NodesSystem.Types;
using Belias.Actions;
using Belias.Services;

namespace Belias.Windows.NodesSystem;

public class NodeEditorWindow : Window, IDisposable
{    
    private readonly List<VisualNode> nodes = new();
    private Vector2 scrolling = Vector2.Zero;
    private VisualNode? selectedNode;
    private NodeInput? selectedInput;
    private NodeOutput? selectedOutput;
    private const bool TypesAreCompatible = true;
    private bool disposed = false;
    
    // Node creation menu state
    private bool isNodeCreationMenuOpen = false;
    private Vector2 nodeCreationPos = Vector2.Zero;
    private Vector2 popupPos = Vector2.Zero; // Track popup position separately
    private bool popupInitialized = false;   // Flag to set position only once

    public NodeEditorWindow() : base("Rotation Node Editor")
    {
        Size = ImGuiHelpers.ScaledVector2(800, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    }    

    public override void Draw()
    {
        // Display the node toolbar at the top
        DrawNodeToolbar();
        
        // Main canvas area
        if (ImGui.BeginChild("NodesCanvas", Vector2.Zero, true, 
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove))
        {
            DrawGrid();
            HandleInput();
            DrawNodes();
            DrawConnections();
            
            // Draw node creation popup if active
            if (isNodeCreationMenuOpen)
            {
                DrawNodeCreationMenu();
            }
        }
        ImGui.EndChild();
    }
    
    private void DrawNodeToolbar()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
        
        // Add Node button
        DrawAddNodeButton();
        
        ImGui.SameLine();
        
        // Clear All button
        DrawClearAllButton();
        
        ImGui.SameLine();
        
        // Help text
        DrawHelpText();
        
        ImGui.PopStyleVar();
        ImGui.Separator();
    }
    
    private void DrawAddNodeButton()
    {
        if (ImGui.Button("Add Node"))
        {
            // When Add Node is clicked, use the center position for the new node
            nodeCreationPos = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetWindowSize().Y / 2) - scrolling;
            isNodeCreationMenuOpen = true;
            
            // Set initial popup position at the button location
            popupPos = ImGui.GetCursorScreenPos();
            popupInitialized = true;
        }
        
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Add a new node to the editor");
            ImGui.Text("You can also right-click anywhere on the canvas");
            ImGui.EndTooltip();
        }
    }
    
    private void DrawClearAllButton()
    {
        if (ImGui.Button("Clear All"))
        {
            nodes.Clear();
            selectedNode = null;
            selectedInput = null;
            selectedOutput = null;
        }
        
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Remove all nodes from the editor");
            ImGui.EndTooltip();
        }
    }
    
    private static void DrawHelpText()
    {
        ImGui.TextDisabled("(?)");
        
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Node Editor Controls:");
            ImGui.BulletText("Middle-click + drag to pan the canvas");
            ImGui.BulletText("Left-click + drag to move nodes");
            ImGui.BulletText("Right-click to add new nodes");
            ImGui.BulletText("Click on inputs/outputs to create connections");
            ImGui.EndTooltip();
        }
    }

    private void DrawGrid()
    {
        var drawList = ImGui.GetWindowDrawList();
        var canvasP0 = ImGui.GetCursorScreenPos();
        var canvasSize = ImGui.GetContentRegionAvail();

        // Background grid
        const float gridStep = 32.0f;
        for (var x = scrolling.X % gridStep; x < canvasSize.X; x += gridStep)
        {
            drawList.AddLine(
                canvasP0 + new Vector2(x, 0.0f),
                canvasP0 + new Vector2(x, canvasSize.Y),
                ImGui.GetColorU32(ImGuiCol.TextDisabled));
        }
        for (var y = scrolling.Y % gridStep; y < canvasSize.Y; y += gridStep)
        {
            drawList.AddLine(
                canvasP0 + new Vector2(0.0f, y),
                canvasP0 + new Vector2(canvasSize.X, y),
                ImGui.GetColorU32(ImGuiCol.TextDisabled));
        }
    }
    
    private void HandleInput()
    {
        Vector2 mousePos = ImGui.GetIO().MousePos;
        Vector2 canvasPos = ImGui.GetCursorScreenPos();
        Vector2 canvasMousePos = mousePos - canvasPos - scrolling;
        
        if (ImGui.IsWindowHovered() && !ImGui.IsAnyItemActive())
        {
            HandleCanvasNavigation();
            HandleNodeSelection(canvasMousePos);
            HandleNodeDragging();
            HandleContextMenu(canvasMousePos);
        }        HandleConnectionLogic();
    }

    private void DrawNodes()
    {
        var drawList = ImGui.GetWindowDrawList();
        var offset = ImGui.GetCursorScreenPos() + scrolling;

        foreach (var node in nodes)
        {
            DrawNode(drawList, offset, node);
        }
    }    private void DrawNode(ImDrawListPtr drawList, Vector2 offset, VisualNode node)
    {
        var nodePos = offset + node.Position;

        // Calculate base node dimensions
        const float NODE_WIDTH = 150.0f;
        var nodeHeight = 100.0f;

        // Calculate node rect corners
        var nodeMin = nodePos;
        var nodeMax = nodePos + new Vector2(NODE_WIDTH, nodeHeight);
        
        // Determine if this node is being hovered
        Vector2 mousePos = ImGui.GetIO().MousePos;
        Vector2 canvasMousePos = mousePos - ImGui.GetCursorScreenPos() - scrolling;
        bool isHovered = IsMouseOverNode(canvasMousePos, node);
        
        // Draw node background with appropriate color based on selection/hover state
        uint bgColor;
        if (node == selectedNode)
        {
            // Selected node gets a more vibrant color
            bgColor = ImGui.GetColorU32(ImGuiCol.HeaderActive);
        }
        else if (isHovered)
        {
            // Hovered node gets a slightly lighter color
            bgColor = ImGui.GetColorU32(ImGuiCol.HeaderHovered);
        }
        else
        {
            // Normal node color
            bgColor = ImGui.GetColorU32(ImGuiCol.TitleBg);
        }
        
        // Draw node background and border
        drawList.AddRectFilled(nodeMin, nodeMax, bgColor, 4.0f);
        
        // Draw a more prominent border for the selected node
        float borderThickness = node == selectedNode ? 2.0f : 1.0f;
        uint borderColor = node == selectedNode ? 
            ImGui.GetColorU32(ImGuiCol.PlotHistogram) : ImGui.GetColorU32(ImGuiCol.Border);
        
        drawList.AddRect(nodeMin, nodeMax, borderColor, 4.0f, ImDrawFlags.None, borderThickness);

        if (node is ActionNode actionNode)
        {
            DrawNodeIconAndTitle(drawList, nodePos, actionNode);
        }
        
        // Add a hint for dragging
        if (node == selectedNode)
        {
            string dragHint = "Drag to move";
            Vector2 textSize = ImGui.CalcTextSize(dragHint);
            Vector2 textPos = nodePos + new Vector2((NODE_WIDTH - textSize.X) / 2, nodeHeight - textSize.Y - 5);
            drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), dragHint);
        }
    }

    private static void DrawNodeIconAndTitle(ImDrawListPtr drawList, Vector2 nodePos, ActionNode actionNode)
    {
        const float ICON_SIZE = 32.0f;
        var iconId = actionNode.RSAction.ID;
        var textureWrap = IconService.GetIcon(iconId);

        if (textureWrap != null)
        {
            // Draw icon
            ImGui.SetCursorPos(nodePos + new Vector2(5, 5));
            ImGui.Image(textureWrap.ImGuiHandle, new Vector2(ICON_SIZE, ICON_SIZE));

            // Draw title to the right of icon
            drawList.AddText(nodePos + new Vector2(ICON_SIZE + 10, 5), ImGui.GetColorU32(ImGuiCol.Text), actionNode.Title);
        }
    }

    private void DrawConnections()
    {
        var drawList = ImGui.GetWindowDrawList();
        var offset = ImGui.GetCursorScreenPos() + scrolling;

        // Draw existing connections
        foreach (var node in nodes)
        {
            foreach (var input in node.Inputs)
            {
                if (input.ConnectedOutput != null)
                {
                    var outputNode = input.ConnectedOutput.Node;
                    var outputIndex = outputNode.Outputs.IndexOf(input.ConnectedOutput);
                    var inputIndex = node.Inputs.IndexOf(input);

                    var startPos = offset + outputNode.Position + 
                        new Vector2(150, 30 + outputIndex * 20);
                    var endPos = offset + node.Position + 
                        new Vector2(0, 30 + inputIndex * 20);

                    drawList.AddBezierCubic(
                        startPos,
                        startPos + new Vector2(50, 0),
                        endPos - new Vector2(50, 0),
                        endPos,
                        ImGui.GetColorU32(ImGuiCol.PlotLines),
                        2.0f);
                }
            }
        }

        // Draw in-progress connection
        if (selectedOutput != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            var startPos = offset + selectedOutput.Node.Position +
                new Vector2(150, 30 + selectedOutput.Node.Outputs.IndexOf(selectedOutput) * 20);
            var endPos = ImGui.GetMousePos();

            drawList.AddBezierCubic(
                startPos,
                startPos + new Vector2(50, 0),
                endPos - new Vector2(50, 0),
                endPos,
                ImGui.GetColorU32(ImGuiCol.PlotLines),
                2.0f);
        }    
    }
    
    private void DrawNodeCreationMenu()
    {
        // Only open the popup once when isNodeCreationMenuOpen becomes true
        // This is important because OpenPopup needs to be called only once
        if (!ImGui.IsPopupOpen("NodeCreationMenu"))
        {
            // Open the popup and position it at the previously calculated position
            ImGui.OpenPopup("NodeCreationMenu");
            if (popupInitialized)
            {
                ImGui.SetNextWindowPos(popupPos);
            }
        }
        
        // Set style and display the popup
        if (ImGui.BeginPopup("NodeCreationMenu"))
        {
            ImGui.Text("Add New Node");
            ImGui.Separator();
            
            // Display simplified menu items
            DisplayActionNodeMenu();
            DisplayConditionNodeMenu();
            
            ImGui.EndPopup();
        }
        else
        {
            isNodeCreationMenuOpen = false;
            popupInitialized = false; // Reset flag when popup closes
        }
    }
    
    private void DisplayActionNodeMenu()
    {
        if (!ImGui.BeginMenu("Action Nodes")) return;
        
        // GCD Action
        if (ImGui.MenuItem("GCD Action"))
        {
            CreateActionNode(1, "Example GCD", true);
        }
        
        // oGCD Action
        if (ImGui.MenuItem("oGCD Action"))
        {
            CreateActionNode(2, "Example oGCD", false);
        }
        
        // Buff Action
        if (ImGui.MenuItem("Buff Action"))
        {
            CreateActionNode(3, "Example Buff", false, true);
        }
        
        // Positional Action
        if (ImGui.MenuItem("Positional Action"))
        {
            CreateActionNode(4, "Example Positional", true, false, true);
        }
        
        ImGui.EndMenu();
    }
    
    private void DisplayConditionNodeMenu()
    {
        if (!ImGui.BeginMenu("Condition Nodes")) return;
        
        // Status Condition
        if (ImGui.MenuItem("Status Condition"))
        {
            CreateStatusNode();
        }
        
        // Resource Condition
        if (ImGui.MenuItem("Resource Condition"))
        {
            CreateResourceNode();
        }
        
        // Positional Condition
        if (ImGui.MenuItem("Positional Condition"))
        {
            CreatePositionalNode();
        }
        
        ImGui.EndMenu();
    }
    
    private void CreateActionNode(uint id, string name, bool isGcd, bool isBuff = false, bool isPositional = false)
    {
        // Create a mock action
        var mockAction = new MockAction(id, name, isGcd, isBuff, isPositional);
        VisualNode node;
        
        // Create the appropriate node type
        if (isPositional)
        {
            node = new PositionalActionNode(GetNextNodeId(), mockAction);
        }
        else if (isBuff)
        {
            node = new BuffActionNode(GetNextNodeId(), mockAction);
        }
        else if (isGcd)
        {
            node = new GCDActionNode(GetNextNodeId(), mockAction);
        }
        else
        {
            node = new OGCDActionNode(GetNextNodeId(), mockAction);
        }
        
        // Set position and add to canvas
        node.Position = nodeCreationPos;
        AddNode(node);
        isNodeCreationMenuOpen = false;
    }
    
    private void CreateStatusNode()
    {
        var node = new StatusConditionNode(GetNextNodeId(), 100, true)
        {
            Position = nodeCreationPos,
            Title = "Status Check"
        };
        AddNode(node);
        isNodeCreationMenuOpen = false;
    }
    
    private void CreateResourceNode()
    {
        var node = new ResourceConditionNode(GetNextNodeId(),
            ResourceConditionNode.ResourceType.MP, 5000)
        {
            Position = nodeCreationPos,
            Title = "MP Check"
        };
        AddNode(node);
        isNodeCreationMenuOpen = false;
    }
    
    private void CreatePositionalNode()
    {
        var node = new PositionalConditionNode(GetNextNodeId(),
            PositionalConditionNode.PositionalType.Flank)
        {
            Position = nodeCreationPos,
            Title = "Position Check"
        };
        AddNode(node);
        isNodeCreationMenuOpen = false;
    }
    
    // Generate a unique ID for each new node
    private int GetNextNodeId()
    {
        return nodes.Count > 0 ? nodes.Max(n => n.Id) + 1 : 1;
    }

    public void AddNode(VisualNode node)
    {
        nodes.Add(node);
    }

    public void RemoveNode(VisualNode node)
    {
        nodes.Remove(node);
        selectedNode = null;
        
        // Clean up connections
        foreach (var n in nodes)
        {
            foreach (var input in n.Inputs)
            {
                if (input.ConnectedOutput?.Node == node)
                {
                    input.ConnectedOutput = null;
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                nodes.Clear();
            }

            disposed = true;
        }
    }    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }    private static bool IsMouseOverNode(Vector2 canvasMousePos, VisualNode node)
    {
        const float NODE_WIDTH = 150.0f;
        float nodeHeight = 100.0f; // Adjust based on actual node height
        
        Vector2 nodeMin = node.Position;
        Vector2 nodeMax = node.Position + new Vector2(NODE_WIDTH, nodeHeight);
        
        return canvasMousePos.X >= nodeMin.X && canvasMousePos.X <= nodeMax.X &&
               canvasMousePos.Y >= nodeMin.Y && canvasMousePos.Y <= nodeMax.Y;
    }
    
    private void HandleCanvasNavigation()
    {
        // Handle middle-click dragging for canvas panning
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
        {
            scrolling += ImGui.GetIO().MouseDelta;
        }
    }
    
    private void HandleNodeSelection(Vector2 canvasMousePos)
    {
        // Handle left-click for node selection
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            bool nodeClicked = false;
            foreach (var node in nodes.OrderByDescending(n => nodes.IndexOf(n))) // Process in reverse to handle overlapping
            {
                if (IsMouseOverNode(canvasMousePos, node))
                {
                    selectedNode = node;
                    nodeClicked = true;
                    break;
                }
            }
            
            // Deselect if didn't click on any node
            if (!nodeClicked)
            {
                selectedNode = null;
            }
        }
    }
    
    private void HandleNodeDragging()
    {
        // Handle node dragging with left mouse button
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && selectedNode != null)
        {
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                selectedNode.Position += ImGui.GetIO().MouseDelta;
            }
        }
    }
    
    private void HandleContextMenu(Vector2 canvasMousePos)
    {
        // Handle right-click for node creation menu
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && !isNodeCreationMenuOpen)
        {
            // Calculate the node position based on where the user clicked in the canvas
            nodeCreationPos = canvasMousePos;
            
            // Store the popup position at mouse click position
            popupPos = ImGui.GetIO().MousePos;
            popupInitialized = true;
            
            isNodeCreationMenuOpen = true;
        }
    }
    
    private void HandleConnectionLogic()
    {
        // Handle input/output slot selection
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            if (selectedInput != null)
            {
                // Connect input to selected output if compatible
                if (selectedOutput != null && TypesAreCompatible)
                {
                    selectedInput.ConnectTo(selectedOutput);
                }
                selectedInput = null;
            }
            selectedOutput = null;
        }
    }
}
