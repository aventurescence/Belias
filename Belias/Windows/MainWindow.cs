using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Belias.Services;

namespace Belias.Windows;

public class MainWindow : Window, IDisposable
{
    // UI Layout properties
    private readonly float sidebarWidth = 250f;
    private int selectedNavItem = 0;
    
    // Grid properties - Blender-style
    private static readonly Vector4 GridColor = new Vector4(0.12f, 0.12f, 0.12f, 0.6f);
    private static readonly Vector4 GridMajorColor = new Vector4(0.18f, 0.18f, 0.18f, 0.8f);
    private static readonly Vector4 GridBgColor = new Vector4(0.03f, 0.03f, 0.03f, 1.0f); // Darker like Blender
    private readonly bool showGrid = true; // Always show grid for Blender-like appearance
    
    // Theme colors (Blender shader nodes inspired)
    private static readonly Vector4 ContentBgColor = new Vector4(0.03f, 0.03f, 0.03f, 1.0f);    // Nav item data with icon identifiers (using Unicode for FontAwesome-compatible icons)
    private readonly (string Label, string Icon)[] navItems = { 
        ("Dashboard", ""),  // fa-tachometer-alt 
        ("Editor", ""),     // fa-pencil-alt
        ("Statistics", ""), // fa-chart-line
        ("Settings", ""),   // fa-cog
        ("Help", "")        // fa-question-circle
    };
    
    // Notification badges for navigation items - set to 0 for no badge
    private readonly int[] navNotifications = {
        3,   // Dashboard notifications
        12,  // Editor notifications
        0,   // Statistics notifications
        1,   // Settings notifications
        0    // Help notifications
    };
    
    // Navigation item grouping
    private readonly (string Header, int StartIndex, int Count)[] navGroups = {
        ("MAIN", 0, 2),
        ("DATA", 2, 1),
        ("SYSTEM", 3, 2)
    };
    
    // Sidebar state
    private bool sidebarCollapsed = false;
    private readonly float collapsedSidebarWidth = 50f;

    // Animation properties for sidebar
    private float currentAnimatedSidebarWidth;
    private float targetSidebarWidth;
    private const float SidebarAnimationSpeed = 10.0f; // pixels per frame
    
    public MainWindow()
        : base("Belias##MainWindow", 
               ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoTitleBar)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
          // Initialize animation values        currentAnimatedSidebarWidth = sidebarWidth;
        targetSidebarWidth = sidebarWidth;
    }
    
    public void Dispose()    {
        // Clean up any resources if needed
    }    public override void Draw()
    {
        // Set up modern UI style
        SetupStyle();
        
        // Handle keyboard shortcuts
        HandleKeyboardShortcuts();
        
        // Main layout 
        Vector2 windowSize = ImGui.GetWindowSize();
        Vector2 windowPos = ImGui.GetWindowPos();
          
        // Update sidebar animation
        UpdateSidebarAnimation();
          
        // Draw sidebar components
        DrawSidebar(windowSize, windowPos);
          
        // Draw main content area
        DrawMainContent(windowSize, windowPos);
        
        // Clean up ImGui style stacks
        ImGui.PopStyleVar(4);
        ImGui.PopStyleColor(10); 
    }
    
    private void HandleKeyboardShortcuts()
    {
        // Toggle sidebar with Ctrl+B
        if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) && ImGui.IsKeyPressed(ImGuiKey.B, false))
        {
            sidebarCollapsed = !sidebarCollapsed;
            targetSidebarWidth = sidebarCollapsed ? collapsedSidebarWidth : sidebarWidth;
        }
        
        // Navigate with Alt + number keys
        if (ImGui.IsKeyDown(ImGuiKey.LeftAlt))
        {
            // Alt+1 through Alt+5 for navigation
            for (int i = 0; i < navItems.Length && i < 5; i++)
            {
                if (ImGui.IsKeyPressed((ImGuiKey)((int)ImGuiKey._1 + i), false))
                {
                    selectedNavItem = i;
                    break;
                }
            }
        }
    }
    
    private void UpdateSidebarAnimation()
    {
        // Set target sidebar width based on state
        targetSidebarWidth = sidebarCollapsed ? collapsedSidebarWidth : sidebarWidth;
        
        // Smooth animation for sidebar width
        if (Math.Abs(currentAnimatedSidebarWidth - targetSidebarWidth) > 0.1f)
        {
            currentAnimatedSidebarWidth += Math.Sign(targetSidebarWidth - currentAnimatedSidebarWidth) * SidebarAnimationSpeed;
            currentAnimatedSidebarWidth = Math.Clamp(currentAnimatedSidebarWidth, 
                Math.Min(sidebarWidth, collapsedSidebarWidth), 
                Math.Max(sidebarWidth, collapsedSidebarWidth));
        }
        else
        {
            currentAnimatedSidebarWidth = targetSidebarWidth;
        }
    }
    
    private void DrawSidebar(Vector2 windowSize, Vector2 windowPos)
    {
        // Draw sidebar background
        ImDrawListPtr sidebarDrawList = ImGui.GetWindowDrawList();
        DrawSidebarBackground(sidebarDrawList, windowPos, windowSize, currentAnimatedSidebarWidth);
        
        // Begin sidebar content area
        ImGui.BeginChild("SidebarContent", new Vector2(currentAnimatedSidebarWidth, windowSize.Y), false);
        
        // Draw logo area
        DrawLogoArea(sidebarDrawList, currentAnimatedSidebarWidth);
        
        // Draw navigation menu with icons
        DrawNavigationMenu(currentAnimatedSidebarWidth, ref selectedNavItem, navItems);
        
        // Draw the sidebar footer with version info and collapse button
        DrawSidebarFooter(currentAnimatedSidebarWidth);
        
        ImGui.EndChild();
    }    private void DrawMainContent(Vector2 windowSize, Vector2 windowPos)
    {
        float contentStartX = currentAnimatedSidebarWidth;
        float contentWidth = windowSize.X - currentAnimatedSidebarWidth;
        
        // Add a subtle toolbar above the content with exit button
        DrawToolbar(windowPos, windowSize, contentStartX, this);
        
        // Draw the main content area with grid and content panels
        DrawContentArea(windowSize, contentStartX, contentWidth);
    }    private static void DrawToolbar(Vector2 windowPos, Vector2 windowSize, float contentStartX, MainWindow? window = null)
    {
        float toolbarHeight = 30;
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        
        // Toolbar background - professional menu bar style
        drawList.AddRectFilled(
            new Vector2(contentStartX, windowPos.Y),
            new Vector2(windowPos.X + windowSize.X, windowPos.Y + toolbarHeight),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.12f, 0.12f, 0.13f, 1.0f))
        );
        
        // Add subtle bottom border to menu bar
        drawList.AddLine(
            new Vector2(contentStartX, windowPos.Y + toolbarHeight - 1),
            new Vector2(windowPos.X + windowSize.X, windowPos.Y + toolbarHeight - 1),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f))
        );
        
        // Set up menu bar style
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 4));
        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, new Vector4(0.12f, 0.12f, 0.13f, 0.0f)); // Transparent to use our custom bg
        
        // Position for the menu bar
        ImGui.SetCursorPos(new Vector2(contentStartX + 5, 5));
        
        // Begin the menu bar
        if (ImGui.BeginMenuBar())
        {
            // File Menu
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Project", "Ctrl+N")) {}
                if (ImGui.MenuItem("Open Project", "Ctrl+O")) {}
                if (ImGui.MenuItem("Save", "Ctrl+S")) {}
                if (ImGui.MenuItem("Save As...", "Ctrl+Shift+S")) {}
                ImGui.Separator();
                if (ImGui.MenuItem("Export...", "Ctrl+E")) {}
                ImGui.Separator();
                if (ImGui.MenuItem("Exit", "Alt+F4") && window != null)
                {
                    window.IsOpen = false;
                }
                ImGui.EndMenu();
            }
            
            // Edit Menu
            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo", "Ctrl+Z")) {}
                if (ImGui.MenuItem("Redo", "Ctrl+Y")) {}
                ImGui.Separator();
                if (ImGui.MenuItem("Cut", "Ctrl+X")) {}
                if (ImGui.MenuItem("Copy", "Ctrl+C")) {}
                if (ImGui.MenuItem("Paste", "Ctrl+V")) {}
                ImGui.Separator();
                if (ImGui.MenuItem("Select All", "Ctrl+A")) {}
                ImGui.EndMenu();
            }
            
            // View Menu
            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Toggle Sidebar", "Ctrl+B")) {}
                if (ImGui.MenuItem("Zoom In", "Ctrl++")) {}
                if (ImGui.MenuItem("Zoom Out", "Ctrl+-")) {}
                if (ImGui.MenuItem("Reset Zoom", "Ctrl+0")) {}
                ImGui.Separator();
                if (ImGui.MenuItem("Toggle Grid", "Ctrl+G")) {}
                ImGui.EndMenu();
            }
            
            // Tools Menu
            if (ImGui.BeginMenu("Tools"))
            {
                if (ImGui.MenuItem("Editor", "Alt+2")) {}
                if (ImGui.MenuItem("Statistics", "Alt+3")) {}
                if (ImGui.MenuItem("Settings", "Alt+4")) {}
                ImGui.EndMenu();
            }
            
            // Help Menu
            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("Documentation", "F1")) {}
                if (ImGui.MenuItem("About", "")) {}
                ImGui.EndMenu();
            }
            
            ImGui.EndMenuBar();
        }
        
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor();
        
        // Draw exit button on the far right
        float exitButtonWidth = 28;
        float exitButtonHeight = 28;
        ImGui.SetCursorPos(new Vector2(windowSize.X - exitButtonWidth - 10, 1));
        
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.1f, 0.1f, 0.7f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.2f, 0.2f, 0.9f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 0.3f, 0.3f, 1.0f));
          
        if (ImGui.Button("×", new Vector2(exitButtonWidth, exitButtonHeight)))
        {
            // Close the window if we have a reference to the MainWindow
            if (window != null)
            {
                window.IsOpen = false;
            }
        }
        
        ImGui.PopStyleColor(3);
        
        // Add tooltip for exit button
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Close Application");
            ImGui.EndTooltip();
        }
    }
      private void DrawContentArea(Vector2 windowSize, float contentStartX, float contentWidth)
    {
        // Use helper class to draw content area
        SidebarHelpers.DrawContentArea(
            windowSize, 
            contentStartX, 
            contentWidth, 
            showGrid, 
            ContentBgColor, 
            selectedNavItem
        );
    }private static void SetupStyle()
    {
        // Set up Blender shader nodes style UI
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 2.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 2.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 3.0f);
        
        // Blender shader nodes theme colors (dark theme)
        Vector4 blenderDark = new Vector4(0.05f, 0.05f, 0.06f, 1.0f);
        Vector4 blenderBlue = new Vector4(0.39f, 0.67f, 0.95f, 1.0f);
        Vector4 blenderBlueLight = new Vector4(0.50f, 0.76f, 0.98f, 1.0f);
        Vector4 blenderHeader = new Vector4(0.20f, 0.20f, 0.22f, 1.0f);
        Vector4 blenderButton = new Vector4(0.15f, 0.15f, 0.16f, 1.0f);
        
        ImGui.PushStyleColor(ImGuiCol.WindowBg, blenderDark);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, new Vector4(0.08f, 0.08f, 0.09f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.12f, 0.12f, 0.13f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Button, blenderButton);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, blenderBlueLight);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, blenderBlue);
        ImGui.PushStyleColor(ImGuiCol.Header, blenderHeader);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, blenderBlueLight);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, blenderBlue);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0.02f, 0.02f, 0.03f, 0.8f));
        
        // Adjust other ImGui styles to better match Blender
        ImGui.GetStyle().ItemSpacing = new Vector2(6, 6);
        ImGui.GetStyle().FrameRounding = 2;
        ImGui.GetStyle().FrameBorderSize = 1;
        ImGui.GetStyle().WindowRounding = 0;
        ImGui.GetStyle().ScrollbarSize = 14;
        ImGui.GetStyle().ScrollbarRounding = 0;
    }    // DrawSidebar logic was integrated directly into the Draw method
      private static void DrawSidebarBackground(ImDrawListPtr drawList, Vector2 windowPos, Vector2 windowSize, float sidebarWidth)
    {
        // Use the static helper method to draw the sidebar background
        SidebarHelpers.DrawSidebarBackground(drawList, windowPos, windowSize, sidebarWidth);
    }
      private static void DrawLogoArea(ImDrawListPtr drawList, float sidebarWidth)
    {
        // Determine if we're in collapsed mode
        bool isCollapsed = sidebarWidth < 100;
        
        // Logo area with proper null handling
        float logoSize = isCollapsed ? 30f : 60f;
        float paddingTop = 15f;
        ImGui.SetCursorPos(new Vector2((sidebarWidth - logoSize) / 2, paddingTop));
        
        // Safely render the logo using ThreadImageLoader
        bool logoDisplayed = false;
        Vector2 logoPos = ImGui.GetCursorScreenPos();
        
        // Try all logo locations in sequence until one works
        foreach (string logoPath in Plugin.LogoLocations)
        {
            if (string.IsNullOrEmpty(logoPath)) continue;
            
            if (ThreadImageLoader.TryGetTextureWrap(logoPath, out var logoTexture) && 
                logoTexture != null && 
                logoTexture.ImGuiHandle != IntPtr.Zero)
            {                // Logo is displayed without a glow effect
                // Glow effect has been removed
                
                ImGui.Image(logoTexture.ImGuiHandle, new Vector2(logoSize, logoSize));
                logoDisplayed = true;
                break;
            }
        }
        
        // At this point, all logo loading attempts have failed including embedded fallbacks
        if (!logoDisplayed)
        {            // Empty space for the logo without glow effect
            // Glow effect has been removed
            
            // Fallback if logo can't be loaded
            if (!isCollapsed)
            {
                ImGui.SetCursorPos(new Vector2((sidebarWidth - 100) / 2, paddingTop + 15));
                ImGui.PushFont(ImGui.GetFont());
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.44f, 0.73f, 0.99f, 1.0f));  // Blender blue
                ImGui.Text("Belias");
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
            else
            {
                ImGui.SetCursorPos(new Vector2((sidebarWidth - 12) / 2, paddingTop + 15));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.44f, 0.73f, 0.99f, 1.0f));  // Blender blue
                ImGui.Text("B");
                ImGui.PopStyleColor();
            }
        }
        
        ImGui.SetCursorPosY(paddingTop + logoSize + 15);
        ImGui.Separator();
        ImGui.Spacing();
    }    private void DrawNavigationMenu(float sidebarWidth, ref int selectedNavItem, (string Label, string Icon)[] navItems)
    {
        bool isCollapsed = sidebarWidth < 100;
        
        // Blender-style navigation items
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 8));  // Tighter spacing like Blender
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 6)); // More padding for buttons
          
        // Add some space at the top
        ImGui.Spacing();
        ImGui.Spacing();
          // Use the helper class to draw navigation groups with notification badges
        foreach (var group in navGroups)
        {
            SidebarHelpers.DrawNavigationGroup(group, navItems, isCollapsed, ref selectedNavItem, sidebarWidth, navNotifications);
        }
        
        ImGui.PopStyleVar(2);
    }private static void DrawGrid(Vector2 pos, Vector2 size)
    {
        // Grid sizes defined here as constants
        const float gridSize = 20f;
        
        // Get draw list from current window
        var drawList = ImGui.GetWindowDrawList();
        
        // Fill background with the dark grid background color
        drawList.AddRectFilled(pos, new Vector2(pos.X + size.X, pos.Y + size.Y), 
            ImGui.ColorConvertFloat4ToU32(GridBgColor));
        
        // Draw simple grid lines - Blender style
        uint minorGridColor = ImGui.ColorConvertFloat4ToU32(GridColor);
        
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
        uint majorGridColor = ImGui.ColorConvertFloat4ToU32(GridMajorColor);
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
        ImGui.Text("Settings Content");
    }
    
    private static void DrawHelpContent()
    {
        ImGui.Text("Help Content");
    }
      private void DrawSidebarFooter(float sidebarWidth)
    {
        bool isCollapsed = sidebarWidth < 100;
        
        // Push to bottom of the sidebar
        float footerHeight = isCollapsed ? 80 : 100;
        ImGui.SetCursorPosY(ImGui.GetWindowHeight() - footerHeight);
        
        // Separator line with gradient
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 sepStart = ImGui.GetCursorScreenPos();
        
        drawList.AddRectFilled(
            sepStart,
            new Vector2(sepStart.X + sidebarWidth, sepStart.Y + 1),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.3f, 0.7f))
        );
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Status/version info
        if (!isCollapsed)
        {
            // Version with icon
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.SetCursorPosX(10);
            ImGui.Text("  Version 1.0.0");
            ImGui.PopStyleColor();
            
            // Status indicator - online status
            ImGui.SetCursorPosX(10);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f)); // Green for online
            ImGui.Text("  Online");
            ImGui.PopStyleColor();
            
            // Draw status circle indicator
            Vector2 statusPos = ImGui.GetCursorScreenPos();
            statusPos.X -= ImGui.CalcTextSize("Online").X - 4;
            statusPos.Y -= ImGui.GetTextLineHeight() * 0.8f;
            
            drawList.AddCircleFilled(
                statusPos,
                4.0f,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.8f, 0.2f, 1.0f))
            );
        }
        
        ImGui.Spacing();
        ImGui.Spacing();
          // Improved collapse/expand button with modern styling
        float buttonWidth = isCollapsed ? sidebarWidth - 16 : 110;
        float buttonHeight = 32;
        ImGui.SetCursorPosX((sidebarWidth - buttonWidth) / 2);
        
        // Create a more professional gradient button
        Vector2 buttonPos = ImGui.GetCursorPos();
        Vector2 screenPos = ImGui.GetCursorScreenPos();
        
        // Enhanced button styling with gradient
        Vector4 buttonColorTop = isCollapsed ? 
            new Vector4(0.22f, 0.22f, 0.26f, 1.0f) : 
            new Vector4(0.22f, 0.22f, 0.26f, 1.0f);
            
        Vector4 buttonColorBottom = isCollapsed ? 
            new Vector4(0.18f, 0.18f, 0.21f, 1.0f) : 
            new Vector4(0.18f, 0.18f, 0.21f, 1.0f);
        
        // Draw custom button background with gradient
        drawList.AddRectFilledMultiColor(
            screenPos,
            new Vector2(screenPos.X + buttonWidth, screenPos.Y + buttonHeight),
            ImGui.ColorConvertFloat4ToU32(buttonColorTop),
            ImGui.ColorConvertFloat4ToU32(buttonColorTop),
            ImGui.ColorConvertFloat4ToU32(buttonColorBottom),
            ImGui.ColorConvertFloat4ToU32(buttonColorBottom)
        );
        
        // Draw button border
        drawList.AddRect(
            screenPos,
            new Vector2(screenPos.X + buttonWidth, screenPos.Y + buttonHeight),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.35f, 0.8f)),
            4.0f, // Rounded corners
            ImDrawFlags.None,
            1.0f
        );
        
        // Create invisible button (we're drawing our own)
        ImGui.SetCursorPos(buttonPos);
        bool buttonPressed = ImGui.InvisibleButton("##CollapseButton", new Vector2(buttonWidth, buttonHeight));
        bool buttonHovered = ImGui.IsItemHovered();
        
        // Add hover effect
        if (buttonHovered)
        {
            drawList.AddRectFilled(
                screenPos,
                new Vector2(screenPos.X + buttonWidth, screenPos.Y + buttonHeight),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.25f, 0.25f, 0.29f, 0.3f)),
                4.0f // Rounded corners
            );
        }
        
        // Draw text and arrow
        float textOffsetY = (buttonHeight - ImGui.GetTextLineHeight()) / 2;
        Vector2 arrowCenter;
        
        if (isCollapsed)
        {
            // Centered arrow for collapsed state
            arrowCenter = new Vector2(
                screenPos.X + buttonWidth / 2,
                screenPos.Y + buttonHeight / 2
            );
            
            // Draw right-pointing double arrow (expand)
            drawList.AddTriangleFilled(
                new Vector2(arrowCenter.X - 4, arrowCenter.Y - 5),
                new Vector2(arrowCenter.X - 4, arrowCenter.Y + 5),
                new Vector2(arrowCenter.X + 2, arrowCenter.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1.0f))
            );
            drawList.AddTriangleFilled(
                new Vector2(arrowCenter.X + 2, arrowCenter.Y - 5),
                new Vector2(arrowCenter.X + 2, arrowCenter.Y + 5),
                new Vector2(arrowCenter.X + 8, arrowCenter.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1.0f))
            );
        }
        else
        {
            // Text for expanded state
            string collapseText = "Collapse";
            Vector2 textSize = ImGui.CalcTextSize(collapseText);
            float textPosX = screenPos.X + (buttonWidth - textSize.X) / 2;
            
            drawList.AddText(
                new Vector2(textPosX, screenPos.Y + textOffsetY),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.8f, 0.8f, 0.8f, 1.0f)),
                collapseText
            );
            
            // Draw arrow at the left edge
            arrowCenter = new Vector2(
                screenPos.X + 12,
                screenPos.Y + buttonHeight / 2
            );
            
            // Draw left-pointing double arrow (collapse)
            drawList.AddTriangleFilled(
                new Vector2(arrowCenter.X + 4, arrowCenter.Y - 5),
                new Vector2(arrowCenter.X + 4, arrowCenter.Y + 5),
                new Vector2(arrowCenter.X - 2, arrowCenter.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1.0f))
            );
            drawList.AddTriangleFilled(
                new Vector2(arrowCenter.X - 2, arrowCenter.Y - 5),
                new Vector2(arrowCenter.X - 2, arrowCenter.Y + 5),
                new Vector2(arrowCenter.X - 8, arrowCenter.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1.0f))
            );
        }
        
        // Handle the button click
        if (buttonPressed)
        {
            sidebarCollapsed = !sidebarCollapsed;
            targetSidebarWidth = sidebarCollapsed ? collapsedSidebarWidth : sidebarWidth;
        }
        
        // Add tooltip
        if (buttonHovered)
        {
            ImGui.BeginTooltip();
            ImGui.Text(isCollapsed ? "Expand Sidebar (Ctrl+B)" : "Collapse Sidebar (Ctrl+B)");
            ImGui.EndTooltip();
        }
    }
}
