using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Belias.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration configuration;
    
    // Typography constants for consistent text styling
    private static readonly Vector4 HeaderColor = new Vector4(0.85f, 0.85f, 0.92f, 1.0f);
    private static readonly Vector4 SubheaderColor = new Vector4(0.7f, 0.7f, 0.9f, 1.0f);
    private static readonly Vector4 RegularTextColor = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 MutedTextColor = new Vector4(0.65f, 0.65f, 0.7f, 1.0f);
    private static readonly Vector4 AccentColor = new Vector4(0.4f, 0.6f, 1.0f, 1.0f);

    public ConfigWindow(Plugin plugin) : base("###ConfigWindow")
    {
        // Remove the red square in header by using NoTitleBar and implementing our own
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.MenuBar;

        Size = new Vector2(480, 520);
        SizeCondition = ImGuiCond.FirstUseEver;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }
    
    /// <summary>
    /// Draw a custom header for the settings window
    /// </summary>
    private void DrawCustomHeader(string title)
    {
        float headerHeight = 45;
        Vector4 headerBackgroundColor = new Vector4(0.18f, 0.2f, 0.3f, 1.0f);
        
        // Get the window draw list for custom drawing
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 windowPos = ImGui.GetWindowPos();
        Vector2 windowSize = ImGui.GetWindowSize();
        
        // Draw header background with gradient
        drawList.AddRectFilledMultiColor(
            windowPos,
            new Vector2(windowPos.X + windowSize.X, windowPos.Y + headerHeight),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.22f, 0.24f, 0.34f, 1.0f)), // Top left
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.22f, 0.24f, 0.34f, 1.0f)), // Top right
            ImGui.ColorConvertFloat4ToU32(headerBackgroundColor), // Bottom right
            ImGui.ColorConvertFloat4ToU32(headerBackgroundColor)  // Bottom left
        );
        
        // Draw header bottom border
        drawList.AddLine(
            new Vector2(windowPos.X, windowPos.Y + headerHeight),
            new Vector2(windowPos.X + windowSize.X, windowPos.Y + headerHeight),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.35f, 0.5f, 1.0f)),
            1.0f
        );
        
        // Draw header title with improved typography
        float titleFontSize = 1.5f; // Scale factor
        
        // Title text with slight shadow for depth
        float titleX = windowPos.X + 20;
        float titleY = windowPos.Y + (headerHeight - ImGui.GetFontSize() * titleFontSize) / 2;
        
        // Shadow
        drawList.AddText(
            ImGui.GetFont(),
            ImGui.GetFontSize() * titleFontSize,
            new Vector2(titleX + 1, titleY + 1),
            ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 0.5f)),
            title
        );
        
        // Main text
        drawList.AddText(
            ImGui.GetFont(),
            ImGui.GetFontSize() * titleFontSize,
            new Vector2(titleX, titleY),
            ImGui.ColorConvertFloat4ToU32(HeaderColor),
            title
        );
        
        // Add close button
        float closeButtonSize = 26;
        float buttonX = windowPos.X + windowSize.X - closeButtonSize - 15;
        float buttonY = windowPos.Y + (headerHeight - closeButtonSize) / 2;
        
        bool isHovered = ImGui.IsMouseHoveringRect(
            new Vector2(buttonX, buttonY),
            new Vector2(buttonX + closeButtonSize, buttonY + closeButtonSize)
        );
        
        // Draw close button
        Vector4 closeButtonColor = isHovered 
            ? new Vector4(0.9f, 0.3f, 0.3f, 0.9f) 
            : new Vector4(0.6f, 0.2f, 0.2f, 0.7f);
            
        drawList.AddCircleFilled(
            new Vector2(buttonX + closeButtonSize/2, buttonY + closeButtonSize/2),
            closeButtonSize/2,
            ImGui.ColorConvertFloat4ToU32(closeButtonColor)
        );
        
        // Draw X
        float crossSize = 10;
        drawList.AddLine(
            new Vector2(buttonX + closeButtonSize/2 - crossSize/2, buttonY + closeButtonSize/2 - crossSize/2),
            new Vector2(buttonX + closeButtonSize/2 + crossSize/2, buttonY + closeButtonSize/2 + crossSize/2),
            ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.9f)),
            2.0f
        );
        
        drawList.AddLine(
            new Vector2(buttonX + closeButtonSize/2 - crossSize/2, buttonY + closeButtonSize/2 + crossSize/2),
            new Vector2(buttonX + closeButtonSize/2 + crossSize/2, buttonY + closeButtonSize/2 - crossSize/2),
            ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.9f)),
            2.0f
        );
        
        // Handle close button click
        if (isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            IsOpen = false;
        }
        
        // Set cursor position below header
        ImGui.SetCursorPosY(headerHeight + 5);
    }

    /// <summary>
    /// Draw a menu bar with common options
    /// </summary>
    private void DrawMenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save Settings", "Ctrl+S"))
                {
                    configuration.Save();
                }
                
                if (ImGui.MenuItem("Reset to Defaults"))
                {
                    // TODO: Implement reset functionality
                }
                
                ImGui.Separator();
                
                if (ImGui.MenuItem("Exit", "Alt+F4"))
                {
                    IsOpen = false;
                }
                
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Copy Settings"))
                {
                    // TODO: Implement copy functionality
                }
                
                if (ImGui.MenuItem("Paste Settings"))
                {
                    // TODO: Implement paste functionality
                }
                
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("Documentation"))
                {
                    // TODO: Open documentation
                }
                
                if (ImGui.MenuItem("About Belias"))
                {
                    // TODO: Show about dialog or switch to About tab
                }
                
                ImGui.EndMenu();
            }
            
            ImGui.EndMenuBar();
        }
    }    public override void Draw()
    {
        // Enhanced style setup for modern look
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 8));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(12, 12));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 16));
        
        // Custom header instead of the default red square header
        DrawCustomHeader("Belias Settings");
        
        // Draw menu bar
        DrawMenuBar();
        
        // Main content area with some padding
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        
        // Create tabs for settings categories with improved styling
        ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(0.15f, 0.15f, 0.18f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(0.25f, 0.25f, 0.28f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.28f, 0.35f, 0.5f, 1.0f));
        
        if (ImGui.BeginTabBar("SettingsTabBar", ImGuiTabBarFlags.None))
        {
            // Appearance Tab
            if (ImGui.BeginTabItem("Appearance"))
            {
                ImGui.Spacing();
                DrawAppearanceSettings();
                ImGui.EndTabItem();
            }
            
            // General Tab
            if (ImGui.BeginTabItem("General"))
            {
                ImGui.Spacing();
                DrawGeneralSettings();
                ImGui.EndTabItem();
            }
            
            // About Tab
            if (ImGui.BeginTabItem("About"))
            {
                ImGui.Spacing();
                DrawAboutSection();
                ImGui.EndTabItem();
            }
            
            ImGui.EndTabBar();
        }
        
        ImGui.PopStyleColor(3);
        
        // Bottom bar with save button
        ImGui.Separator();
        ImGui.Spacing();        // Create a button group centered at the bottom
        float totalWidth = ImGui.GetContentRegionAvail().X;
        float buttonWidth = 140;
        float buttonsGap = 20;
        float buttonsHeight = 36;
        
        // Center the buttons
        ImGui.SetCursorPosX((totalWidth - (buttonWidth * 2 + buttonsGap)) / 2);
        
        // Improved button styling
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.7f, 0.8f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.5f, 0.85f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.4f, 0.6f, 0.9f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
        
        if (ImGui.Button("Save Settings", new Vector2(buttonWidth, buttonsHeight)))
        {
            configuration.Save();
            IsOpen = false;
        }
        
        ImGui.PopStyleColor(4);
        
        ImGui.SameLine(0, buttonsGap);
        
        if (ImGui.Button("Cancel", new Vector2(buttonWidth, buttonsHeight)))
        {
            // Simply close without saving
            IsOpen = false;
        }
        
        // Restore all pushed style variables
        ImGui.PopStyleVar(4);
    }
      private void DrawAppearanceSettings()
    {
        // Section header with improved typography
        DrawSectionHeader("UI Appearance");
        
        // Settings group with subtle background
        DrawSettingsGroup(() => {
            // Dark mode toggle with better labeling
            var darkMode = configuration.DarkMode;
            
            ImGui.PushID("DarkModeToggle");
            if (ImGui.Checkbox("Dark Mode", ref darkMode))
            {
                configuration.DarkMode = darkMode;
            }
            
            // Add helpful tooltip
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted("Switch between dark and light color schemes");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
            ImGui.PopID();
            
            ImGui.Spacing();
            
            // Welcome screen toggle with improved styling
            var showWelcome = configuration.ShowWelcomeOnStartup;
            
            ImGui.PushID("WelcomeToggle");
            if (ImGui.Checkbox("Show Welcome Screen on Startup", ref showWelcome))
            {
                configuration.ShowWelcomeOnStartup = showWelcome;
            }
            ImGui.PopID();
        });
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Accent color section
        DrawSectionHeader("Accent Color");
        
        // Settings group for accent colors
        DrawSettingsGroup(() => {
            // Label with improved typography
            ImGui.Text("Select a color accent for the application:");
            ImGui.Spacing();
            
            // Color buttons with improved styling
            float buttonWidth = 100;
            float buttonHeight = 30;
            
            // Red accent color
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 0.7f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.3f, 0.3f, 0.8f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 0.4f, 0.4f, 0.9f));
            
            if (ImGui.Button("Ruby Red", new Vector2(buttonWidth, buttonHeight)))
            {
                configuration.AccentColor = "#C83C23";
            }
            ImGui.PopStyleColor(3);
            
            ImGui.SameLine();
            
            // Blue accent color
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.8f, 0.7f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.5f, 0.9f, 0.8f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.4f, 0.6f, 1.0f, 0.9f));
            
            if (ImGui.Button("Cobalt Blue", new Vector2(buttonWidth, buttonHeight)))
            {
                configuration.AccentColor = "#2364C8";
            }
            ImGui.PopStyleColor(3);
            
            ImGui.SameLine();
            
            // Green accent color
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.8f, 0.4f, 0.7f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.9f, 0.5f, 0.8f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.4f, 1.0f, 0.6f, 0.9f));
            
            if (ImGui.Button("Emerald Green", new Vector2(buttonWidth, buttonHeight)))
            {
                configuration.AccentColor = "#23C83C";
            }
            ImGui.PopStyleColor(3);
        });
    }
    
    /// <summary>
    /// Draw a consistent section header with improved typography
    /// </summary>
    private void DrawSectionHeader(string title)
    {
        // Add space before header for visual separation
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Draw the header with custom styling
        ImGui.PushFont(ImGui.GetFont());
        ImGui.TextColored(SubheaderColor, title);
        ImGui.PopFont();
        
        // Draw gradient separator line
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 lineStart = ImGui.GetCursorScreenPos();
        float lineWidth = ImGui.GetContentRegionAvail().X * 0.98f;
        float lineHeight = 1.5f;
        
        // Draw separator with gradient
        drawList.AddRectFilledMultiColor(
            new Vector2(lineStart.X, lineStart.Y),
            new Vector2(lineStart.X + lineWidth, lineStart.Y + lineHeight),
            ImGui.ColorConvertFloat4ToU32(new Vector4(SubheaderColor.X, SubheaderColor.Y, SubheaderColor.Z, 0.7f)),
            ImGui.ColorConvertFloat4ToU32(new Vector4(SubheaderColor.X, SubheaderColor.Y, SubheaderColor.Z, 0.1f)),
            ImGui.ColorConvertFloat4ToU32(new Vector4(SubheaderColor.X, SubheaderColor.Y, SubheaderColor.Z, 0.1f)),
            ImGui.ColorConvertFloat4ToU32(new Vector4(SubheaderColor.X, SubheaderColor.Y, SubheaderColor.Z, 0.7f))
        );
        
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + lineHeight + 5);
    }
    
    /// <summary>
    /// Draw a settings group with a subtle background
    /// </summary>
    private void DrawSettingsGroup(Action drawContent)
    {
        // Create a subtle background for the settings group
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.18f, 0.5f));
        
        // Begin the child frame for the settings group
        if (ImGui.BeginChild("##settingsGroup", new Vector2(-1, -1), true, ImGuiWindowFlags.NoScrollbar))
        {
            // Add some padding
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(14, 14));
            ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 8, ImGui.GetCursorPosY() + 8));
            
            // Draw the settings content
            drawContent();
            
            ImGui.PopStyleVar();
        }
        ImGui.EndChild();
        
        ImGui.PopStyleColor();
    }
      private void DrawGeneralSettings()
    {
        // Section header with improved typography
        DrawSectionHeader("Application Settings");
        
        // Settings group with subtle background
        DrawSettingsGroup(() => {
            // Auto-start toggle with better styling
            var autoStart = configuration.AutoStart;
            
            ImGui.PushID("AutoStartToggle");
            if (ImGui.Checkbox("Auto-start on Game Launch", ref autoStart))
            {
                configuration.AutoStart = autoStart;
            }
            
            // Add helpful tooltip
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted("Automatically start Belias when the game launches");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
            ImGui.PopID();
            
            ImGui.Spacing();
            ImGui.Spacing();
            
            // Refresh rate slider with improved styling
            ImGui.PushID("RefreshRateSlider");
            
            ImGui.TextColored(RegularTextColor, "UI Refresh Rate:");
            
            // Add small explanation text below the label
            ImGui.PushStyleColor(ImGuiCol.Text, MutedTextColor);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5); // Reduce gap
            ImGui.TextWrapped("Control how often the UI updates. Higher values are smoother but use more resources.");
            ImGui.PopStyleColor();
            
            ImGui.Spacing();
            
            // Custom slider colors
            ImGui.PushStyleColor(ImGuiCol.SliderGrab, new Vector4(0.4f, 0.5f, 0.8f, 0.8f));
            ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, new Vector4(0.5f, 0.6f, 0.9f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.24f, 1.0f));
            
            var refreshRate = configuration.RefreshRate;
            float sliderWidth = ImGui.GetContentRegionAvail().X * 0.8f;
            ImGui.SetNextItemWidth(sliderWidth);
            
            if (ImGui.SliderInt("##RefreshRate", ref refreshRate, 15, 144, "%d fps"))
            {
                configuration.RefreshRate = refreshRate;
            }
            
            // Show performance indicator text based on selected value
            string performanceText;
            Vector4 performanceColor;
            
            if (refreshRate <= 30)
            {
                performanceText = "Low (Better Performance)";
                performanceColor = new Vector4(0.2f, 0.7f, 0.2f, 1.0f);
            }
            else if (refreshRate <= 60)
            {
                performanceText = "Medium (Balanced)";
                performanceColor = new Vector4(0.7f, 0.7f, 0.2f, 1.0f);
            }
            else
            {
                performanceText = "High (Better Visuals)";
                performanceColor = new Vector4(0.7f, 0.4f, 0.2f, 1.0f);
            }
            
            ImGui.SameLine();
            ImGui.TextColored(performanceColor, performanceText);
            
            ImGui.PopStyleColor(3);
            ImGui.PopID();
        });
    }
      private void DrawAboutSection()
    {
        // Section header with improved typography
        DrawSectionHeader("About Belias");
        
        // About content with improved styling
        DrawSettingsGroup(() => {
            // Logo placeholder (you could load an actual logo image)
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            Vector2 cursor = ImGui.GetCursorScreenPos();
            float logoSize = 60;
            
            // Draw a stylized "B" as a simple logo
            float centerX = cursor.X + logoSize / 2;
            float centerY = cursor.Y + logoSize / 2;
            
            // Draw circular logo background
            drawList.AddCircleFilled(
                new Vector2(centerX, centerY),
                logoSize / 2,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.4f, 0.7f, 1.0f))
            );
            
            // Draw "B" in the center
            float textSize = logoSize * 0.7f;
            Vector2 textPos = new Vector2(
                centerX - textSize/4, 
                centerY - textSize/2
            );
            
            drawList.AddText(
                ImGui.GetFont(),
                textSize,
                textPos,
                ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
                "B"
            );
            
            ImGui.Dummy(new Vector2(logoSize, logoSize));
            ImGui.Spacing();
            
            // Version information with better styling
            ImGui.PushFont(ImGui.GetFont());
            ImGui.TextColored(HeaderColor, "Belias");
            ImGui.PopFont();
            
            ImGui.TextColored(RegularTextColor, "Version: 1.0.0");
            ImGui.TextColored(MutedTextColor, "Built: May 17, 2025");
            
            ImGui.Spacing();
            ImGui.Spacing();
            
            // Description with improved typography
            ImGui.PushTextWrapPos(ImGui.GetContentRegionAvail().X);
            ImGui.TextColored(RegularTextColor, 
                "Belias is a modern interface for enhancing your FFXIV gaming experience. " +
                "Featuring an intuitive node editor, real-time analysis, and customizable workflows.");
            ImGui.PopTextWrapPos();
            
            ImGui.Spacing();
            ImGui.Spacing();
            
            // Credits with improved styling
            if (ImGui.CollapsingHeader("Credits", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, RegularTextColor);
                
                // Team section
                ImGui.TextColored(SubheaderColor, "Development Team");
                ImGui.Indent(20);
                ImGui.BulletText("Aventurescence - Lead Developer");
                ImGui.Unindent(20);
                
                ImGui.Spacing();
                
                // Resources section
                ImGui.TextColored(SubheaderColor, "Resources");
                ImGui.Indent(20);
                ImGui.BulletText("Material Design Icons");
                ImGui.BulletText("ImGui.NET Framework");
                ImGui.Unindent(20);
                
                ImGui.Spacing();
                
                // Special thanks section
                ImGui.TextColored(SubheaderColor, "Special Thanks");
                ImGui.Indent(20);
                ImGui.BulletText("Dalamud plugin community");
                ImGui.BulletText("All beta testers and contributors");
                ImGui.Unindent(20);
                
                ImGui.PopStyleColor();
            }
            
            ImGui.Spacing();
            ImGui.Spacing();
            
            // Links section
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.25f, 0.25f, 0.30f, 0.8f));
            
            // Center the buttons
            float buttonWidth = 120;
            float totalWidth = ImGui.GetContentRegionAvail().X;
            float buttonsWidth = (buttonWidth * 2) + ImGui.GetStyle().ItemSpacing.X;
            float startX = (totalWidth - buttonsWidth) / 2;
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + startX);
            
            if (ImGui.Button("Documentation", new Vector2(buttonWidth, 0)))
            {
                // TODO: Open documentation URL
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("GitHub", new Vector2(buttonWidth, 0)))
            {
                // TODO: Open GitHub repository URL
            }
            
            ImGui.PopStyleColor();
        });
    }
}
