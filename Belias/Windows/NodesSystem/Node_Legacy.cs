using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Belias.Windows.NodesSystem;

public class Node
{
    public int Id { get; }
    public string Title { get; set; }
    public Vector2 Position { get; set; }
    public List<NodeInput> Inputs { get; } = new();
    public List<NodeOutput> Outputs { get; } = new();
    private static int NextId = 1;
    
    public Node(string title, Vector2 position)
    {
        Id = NextId++;
        Title = title;
        Position = position;
    }
    
    public virtual void Draw()
    {
        ImGui.SetCursorScreenPos(Position);
        
        // Begin node frame
        ImGui.BeginGroup();
        
        // Node header
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.Text(Title);
        ImGui.PopStyleColor();
        
        // Node content
        ImGui.BeginGroup();
        
        // Draw inputs
        foreach (var input in Inputs)
        {
            input.Draw();
        }
        
        // Draw outputs
        foreach (var output in Outputs)
        {
            output.Draw();
        }
        
        ImGui.EndGroup();
        
        // Frame border
        var frameMin = ImGui.GetItemRectMin();
        var frameMax = ImGui.GetItemRectMax();
        ImGui.GetWindowDrawList().AddRect(
            frameMin, 
            frameMax, 
            ImGui.GetColorU32(ImGuiCol.Border),
            4.0f);
        
        ImGui.EndGroup();
    }
}

public class NodeInput
{
    public string Name { get; }
    public Vector2 Position { get; set; }
    
    public NodeInput(string name)
    {
        Name = name;
    }
    
    public void Draw()
    {
        ImGui.Text(Name);
        Position = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddCircleFilled(
            Position, 
            4.0f, 
            ImGui.GetColorU32(ImGuiCol.Button));
    }
}

public class NodeOutput
{
    public string Name { get; }
    public Vector2 Position { get; set; }
    
    public NodeOutput(string name)
    {
        Name = name;
    }
    
    public void Draw()
    {
        var textSize = ImGui.CalcTextSize(Name);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetWindowWidth() - textSize.X - 20);
        ImGui.Text(Name);
        Position = ImGui.GetCursorScreenPos();
        var position = Position;
        position.X += textSize.X + 10;
        Position = position;
        ImGui.GetWindowDrawList().AddCircleFilled(
            Position, 
            4.0f, 
            ImGui.GetColorU32(ImGuiCol.Button));
    }
}
