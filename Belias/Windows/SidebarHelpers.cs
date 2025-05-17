using System;
using System.Numerics;
using ImGuiNET;

namespace Belias.Windows;

/// <summary>
/// Helper methods for drawing sidebar UI elements
/// Reduces cognitive complexity of MainWindow class
/// </summary>
public static class SidebarHelpers
{    /// <summary>
    /// Draws a sidebar background with gradient effect
    /// </summary>
    public static void DrawSidebarBackground(ImDrawListPtr drawList, Vector2 windowPos, Vector2 windowSize, float sidebarWidth)
    {
        // Enhanced sidebar background with more professional gradient
        Vector4 sidebarTopColor = new Vector4(0.11f, 0.11f, 0.12f, 1.0f);
        Vector4 sidebarBottomColor = new Vector4(0.08f, 0.08f, 0.09f, 1.0f);
        
        // Use a gradient for the sidebar background
        drawList.AddRectFilledMultiColor(
            windowPos,
            new Vector2(windowPos.X + sidebarWidth, windowPos.Y + windowSize.Y),
            ImGui.ColorConvertFloat4ToU32(sidebarTopColor),
            ImGui.ColorConvertFloat4ToU32(sidebarTopColor),
            ImGui.ColorConvertFloat4ToU32(sidebarBottomColor),
            ImGui.ColorConvertFloat4ToU32(sidebarBottomColor)
        );
        
        // Add a subtle highlight on the edge to make it look more polished
        drawList.AddLine(
            new Vector2(windowPos.X + sidebarWidth - 1, windowPos.Y),
            new Vector2(windowPos.X + sidebarWidth - 1, windowPos.Y + windowSize.Y),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.15f, 0.15f, 0.15f, 1.0f)),
            1.0f
        );
        
        // Add improved separator line with gradient
        Vector4 separatorTopColor = new Vector4(0.4f, 0.4f, 0.45f, 0.7f);
        Vector4 separatorBottomColor = new Vector4(0.2f, 0.2f, 0.25f, 0.4f);
        
        drawList.AddRectFilledMultiColor(
            new Vector2(windowPos.X + sidebarWidth - 1, windowPos.Y),
            new Vector2(windowPos.X + sidebarWidth + 1, windowPos.Y + windowSize.Y),
            ImGui.ColorConvertFloat4ToU32(separatorTopColor),
            ImGui.ColorConvertFloat4ToU32(separatorTopColor),
            ImGui.ColorConvertFloat4ToU32(separatorBottomColor),
            ImGui.ColorConvertFloat4ToU32(separatorBottomColor)
        );
        
        // Add subtle depth effect on left edge
        drawList.AddRectFilled(
            new Vector2(windowPos.X, windowPos.Y),
            new Vector2(windowPos.X + 2, windowPos.Y + windowSize.Y),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 0.5f))
        );
        
        // Add subtle highlight at the top
        drawList.AddRectFilled(
            new Vector2(windowPos.X, windowPos.Y),
            new Vector2(windowPos.X + sidebarWidth, windowPos.Y + 2),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.35f, 0.5f))
        );
    }    /// <summary>
    /// Draws a navigation group with header and items
    /// </summary>
    public static void DrawNavigationGroup(
        (string Header, int StartIndex, int Count) group, 
        (string Label, string Icon)[] navItems, 
        bool isCollapsed, 
        ref int selectedNavItem, 
        float sidebarWidth,
        int[]? notificationCounts = null)
    {
        // Draw group header
        if (!isCollapsed)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            ImGui.SetCursorPosX(10);
            ImGui.Text(group.Header);
            ImGui.PopStyleColor();
            ImGui.Spacing();
        }
        
        // Draw items in this group
        for (int i = group.StartIndex; i < group.StartIndex + group.Count && i < navItems.Length; i++)
        {
            // Get notification count for this item if available
            int badgeCount = 0;
            if (notificationCounts != null && i < notificationCounts.Length)
            {
                badgeCount = notificationCounts[i];
            }
            
            DrawNavigationItem(i, navItems[i], isCollapsed, ref selectedNavItem, sidebarWidth, badgeCount);
        }
        
        // Add separator between groups
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }    /// <summary>
    /// Draws a single navigation item with enhanced styling for better visual appearance
    /// </summary>
    public static void DrawNavigationItem(int index, 
                                   (string Label, string Icon) navItem, 
                                   bool isCollapsed, 
                                   ref int selectedNavItem, 
                                   float sidebarWidth,
                                   int badgeCount = 0)
    {
        bool isSelected = selectedNavItem == index;
        bool hasBadge = badgeCount > 0;
        
        // Enhanced selection styling with subtle gradient effect
        ImGui.PushStyleColor(ImGuiCol.Header, isSelected ? 
            new Vector4(0.35f, 0.55f, 0.78f, 0.7f) : // Selected color
            new Vector4(0.15f, 0.15f, 0.17f, 0.0f)); // Normal color (transparent)
        
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, 
            new Vector4(0.4f, 0.6f, 0.8f, 0.5f)); // Hover color
        
        // Draw selection indicator with enhanced styling
        if (isSelected)
        {
            DrawSelectionIndicator();
        }
        
        // Add subtle highlight to create a more interactive feel
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
        
        // Prepare the item content with better spacing for collapsed mode
        string label;
        if (isCollapsed) {
            // Center icon in collapsed mode for better alignment
            label = navItem.Icon;
            ImGui.SetCursorPosX((sidebarWidth - ImGui.CalcTextSize(navItem.Icon).X) / 2 - 2);
        } else {
            label = $"{navItem.Icon}  {navItem.Label}";
        }
        
        float itemWidth = isCollapsed ? sidebarWidth - 10 : sidebarWidth - 20;
        
        // Store the cursor position before drawing the selectable to calculate badge position
        Vector2 cursorPos = ImGui.GetCursorPos();
        Vector2 windowPos = ImGui.GetWindowPos();
        
        // Draw the selectable item
        if (ImGui.Selectable(label, isSelected, ImGuiSelectableFlags.None, new Vector2(itemWidth, 35)))
        {
            selectedNavItem = index;
        }
        
        // Draw notification badge if needed
        if (hasBadge)
        {
            DrawNotificationBadge(windowPos, cursorPos, badgeCount, isCollapsed, sidebarWidth);
        }
        
        // Add tooltip for collapsed mode
        if (isCollapsed && ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(navItem.Label);
            
            // Also show badge count in tooltip if there is one
            if (hasBadge)
            {
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.4f, 0.4f, 1.0f));
                ImGui.Text($"({badgeCount})");
                ImGui.PopStyleColor();
            }
            
            ImGui.EndTooltip();
        }
        
        ImGui.PopStyleColor(2);
    }
    
    /// <summary>
    /// Draws a notification badge with count
    /// </summary>
    private static void DrawNotificationBadge(Vector2 windowPos, Vector2 cursorPos, int count, bool isCollapsed, float sidebarWidth)
    {
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        
        // Badge position calculation
        float badgeRadius = 9.0f;
        Vector2 badgePos;
        
        if (isCollapsed)
        {
            // For collapsed sidebar, position badge in top-right of icon
            badgePos = new Vector2(
                windowPos.X + sidebarWidth - badgeRadius - 4,
                windowPos.Y + cursorPos.Y + 6
            );
        }
        else
        {
            // For expanded sidebar, position badge to the right of label
            badgePos = new Vector2(
                windowPos.X + sidebarWidth - badgeRadius * 2 - 8,
                windowPos.Y + cursorPos.Y + 17
            );
        }
        
        // Draw badge background circle
        drawList.AddCircleFilled(
            badgePos,
            badgeRadius,
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.3f, 0.3f, 1.0f)) // Red notification
        );
        
        // Draw count text
        string countText = count > 99 ? "99+" : count.ToString();
        Vector2 textSize = ImGui.CalcTextSize(countText);
        
        drawList.AddText(
            new Vector2(
                badgePos.X - textSize.X / 2,
                badgePos.Y - textSize.Y / 2
            ),
            ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
            countText
        );
    }    /// <summary>
    /// Draws an enhanced indicator for the selected navigation item
    /// </summary>
    private static void DrawSelectionIndicator()
    {
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 cursor = ImGui.GetCursorScreenPos();
        
        // Create a gradient effect for the selection indicator
        Vector4 topColor = new Vector4(0.5f, 0.8f, 1.0f, 1.0f);      // Lighter blue at top
        Vector4 bottomColor = new Vector4(0.3f, 0.6f, 0.9f, 1.0f);   // Darker blue at bottom
        
        // Draw the selection indicator with gradient
        drawList.AddRectFilledMultiColor(
            new Vector2(cursor.X, cursor.Y),
            new Vector2(cursor.X + 3, cursor.Y + 35),
            ImGui.ColorConvertFloat4ToU32(topColor),
            ImGui.ColorConvertFloat4ToU32(topColor),
            ImGui.ColorConvertFloat4ToU32(bottomColor),
            ImGui.ColorConvertFloat4ToU32(bottomColor)
        );
        
        // Add subtle glow effect
        for (int i = 1; i <= 3; i++)
        {
            float alpha = 0.2f - (i * 0.05f);
            if (alpha > 0)
            {
                drawList.AddRectFilled(
                    new Vector2(cursor.X + 3, cursor.Y),
                    new Vector2(cursor.X + 3 + i, cursor.Y + 35),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.4f, 0.7f, 1.0f, alpha))
                );
            }
        }
    }
    
    /// <summary>
    /// Draws the main content area with grid background and content based on selected nav item
    /// </summary>
    public static void DrawContentArea(
        Vector2 windowSize, 
        float contentStartX, 
        float contentWidth, 
        bool showGrid,
        Vector4 contentBgColor,
        int selectedNavItem)
    {
        float toolbarHeight = 30;
        
        // Main node editor area
        ImGui.SetCursorPos(new Vector2(contentStartX, toolbarHeight));
        ImGui.BeginChild("ContentArea", new Vector2(contentWidth, windowSize.Y - toolbarHeight), 
            false, ImGuiWindowFlags.NoScrollbar);
        
        // Content area background
        Vector2 contentPos = ImGui.GetWindowPos();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(
            contentPos, 
            new Vector2(contentPos.X + contentWidth, contentPos.Y + windowSize.Y - toolbarHeight), 
            ImGui.ColorConvertFloat4ToU32(contentBgColor)
        );
          // Draw grid in content area only in Editor view (nav index 1)
        if (showGrid && selectedNavItem == 1)
        {
            DrawGrid(contentPos, new Vector2(contentWidth, windowSize.Y - toolbarHeight));
        }
        
        // Draw content based on selected nav item
        DrawContent(selectedNavItem);
        
        ImGui.EndChild(); // End ContentArea
    }
    
    /// <summary>
    /// Draws a grid with major and minor lines
    /// </summary>
    private static void DrawGrid(Vector2 pos, Vector2 size)
    {
        // Grid sizes defined here as constants
        const float gridSize = 20f;
        
        // Get draw list from current window
        var drawList = ImGui.GetWindowDrawList();
        
        // Grid colors
        Vector4 gridColor = new Vector4(0.12f, 0.12f, 0.12f, 0.6f);
        Vector4 gridMajorColor = new Vector4(0.18f, 0.18f, 0.18f, 0.8f);
        Vector4 gridBgColor = new Vector4(0.03f, 0.03f, 0.03f, 1.0f);
        
        // Fill background with the dark grid background color
        drawList.AddRectFilled(pos, new Vector2(pos.X + size.X, pos.Y + size.Y), 
            ImGui.ColorConvertFloat4ToU32(gridBgColor));
        
        // Draw simple grid lines - Blender style
        uint minorGridColor = ImGui.ColorConvertFloat4ToU32(gridColor);
        
        // Calculate grid step
        float step = gridSize; 
        int linesX = (int)(size.X / step) + 1;
        int linesY = (int)(size.Y / step) + 1;
        
        // Draw vertical grid lines
        for (int i = 0; i <= linesX; i++)
        {
            float x = pos.X + i * step;
            drawList.AddLine(
                new Vector2(x, pos.Y), 
                new Vector2(x, pos.Y + size.Y), 
                minorGridColor);
        }
        
        // Draw horizontal grid lines
        for (int j = 0; j <= linesY; j++)
        {
            float y = pos.Y + j * step;
            drawList.AddLine(
                new Vector2(pos.X, y), 
                new Vector2(pos.X + size.X, y), 
                minorGridColor);
        }
          
        // Draw major grid lines (every 5 cells)
        uint majorGridColor = ImGui.ColorConvertFloat4ToU32(gridMajorColor);
        float majorStep = gridSize * 5; // Every 5 grid cells
        
        // Major vertical lines
        for (float x = pos.X; x <= pos.X + size.X; x += majorStep)
        {
            drawList.AddLine(
                new Vector2(x, pos.Y), 
                new Vector2(x, pos.Y + size.Y), 
                majorGridColor, 
                1.5f);
        }
        
        // Major horizontal lines
        for (float y = pos.Y; y <= pos.Y + size.Y; y += majorStep)
        {
            drawList.AddLine(
                new Vector2(pos.X, y), 
                new Vector2(pos.X + size.X, y), 
                majorGridColor, 
                1.5f);
        }
    }
    
    /// <summary>
    /// Draws content based on the selected navigation item
    /// </summary>
    private static void DrawContent(int navIndex)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(20, 20));
        
        switch (navIndex)
        {
            case 0: // Dashboard
                DrawDashboardContent();
                break;
            case 1: // Editor
                DrawEditorContent();
                break;
            case 2: // Statistics
                DrawStatisticsContent();
                break;
            case 3: // Settings
                DrawSettingsContent();
                break;
            case 4: // Help
                DrawHelpContent();
                break;
            default:
                ImGui.Text("Select an item from the navigation panel");
                break;
        }
        
        ImGui.PopStyleVar();
    }
    
    private static void DrawDashboardContent()
    {
        ImGui.Text("Dashboard Content");
    }
    
    private static void DrawEditorContent()
    {
        ImGui.Text("Editor Content");
    }
    
    private static void DrawStatisticsContent()
    {
        ImGui.Text("Statistics Content");
    }
      private static void DrawSettingsContent()
    {
        // Settings section header
        ImGui.PushFont(ImGui.GetFont());
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.44f, 0.73f, 0.99f, 1.0f));
        ImGui.Text("Settings");
        ImGui.PopStyleColor();
        ImGui.PopFont();
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Spacing();
        
        // UI Customization Section
        ImGui.Text("Sidebar Customization");
        ImGui.Spacing();
        
        // Create a collapsible section for sidebar settings
        if (ImGui.CollapsingHeader("Appearance", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(20);
              // Color theme selector
            ImGui.Text("Color Theme:");
            string[] themes = { "Default", "Dark Blue", "Light", "High Contrast" };
            int selectedTheme = 0; // Changed from static to instance
            
            if (ImGui.BeginCombo("##ThemeSelector", themes[selectedTheme]))
            {
                for (int i = 0; i < themes.Length; i++)
                {
                    bool isSelected = (selectedTheme == i);
                    if (ImGui.Selectable(themes[i], isSelected))
                    {
                        selectedTheme = i;
                        // TODO: Apply theme changes
                    }
                    
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
            
            ImGui.Spacing();
              // Sidebar width adjustment
            ImGui.Text("Sidebar Width:");
            float sidebarWidth = 250.0f; // Changed from static to instance
            if (ImGui.SliderFloat("##SidebarWidth", ref sidebarWidth, 200.0f, 350.0f, "%.0f px"))
            {
                // TODO: Apply sidebar width change
            }
            
            ImGui.Spacing();
              // Font size adjustment
            ImGui.Text("UI Scale:");
            float uiScale = 1.0f; // Changed from static to instance
            if (ImGui.SliderFloat("##UIScale", ref uiScale, 0.8f, 1.5f, "%.1f"))
            {
                // TODO: Apply UI scale change
            }
              // Animation toggle
            ImGui.Spacing();
            bool enableAnimations = true; // Changed from static to instance
            if (ImGui.Checkbox("Enable Animations", ref enableAnimations))
            {
                // TODO: Apply animation setting
            }
            
            ImGui.Unindent(20);
        }
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Navigation Settings Section
        if (ImGui.CollapsingHeader("Navigation", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(20);
              // Show/hide group headers
            bool showGroupHeaders = true; // Changed from static to instance
            if (ImGui.Checkbox("Show Group Headers", ref showGroupHeaders))
            {
                // TODO: Apply group header visibility change
            }
              ImGui.Spacing();
            
            // Show/hide notifications
            bool showNotifications = true;
            if (ImGui.Checkbox("Show Notification Badges", ref showNotifications))
            {
                // TODO: Apply notification visibility change
            }
              ImGui.Spacing();
            
            // Sidebar position (left/right)
            bool sidebarOnRight = false;
            if (ImGui.Checkbox("Sidebar on Right Side", ref sidebarOnRight))
            {
                // TODO: Apply sidebar position change
            }
            
            ImGui.Unindent(20);
        }
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Keyboard shortcuts
        if (ImGui.CollapsingHeader("Keyboard Shortcuts"))
        {
            ImGui.Indent(20);
            
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 200);
            
            ImGui.Text("Toggle Sidebar"); ImGui.NextColumn();
            ImGui.Text("Ctrl+B"); ImGui.NextColumn();
            
            ImGui.Text("Dashboard"); ImGui.NextColumn();
            ImGui.Text("Alt+1"); ImGui.NextColumn();
            
            ImGui.Text("Editor"); ImGui.NextColumn();
            ImGui.Text("Alt+2"); ImGui.NextColumn();
            
            ImGui.Text("Statistics"); ImGui.NextColumn();
            ImGui.Text("Alt+3"); ImGui.NextColumn();
            
            ImGui.Text("Settings"); ImGui.NextColumn();
            ImGui.Text("Alt+4"); ImGui.NextColumn();
            
            ImGui.Text("Help"); ImGui.NextColumn();
            ImGui.Text("Alt+5"); ImGui.NextColumn();
            
            ImGui.Columns(1);
            
            ImGui.Unindent(20);
        }
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        // Reset to defaults button
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 150);
        if (ImGui.Button("Reset to Defaults", new Vector2(140, 30)))
        {
            // TODO: Reset all settings to default values
        }
    }
    
    private static void DrawHelpContent()
    {
        ImGui.Text("Help Content");
    }
}
