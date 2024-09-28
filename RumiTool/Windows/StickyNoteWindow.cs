using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using RumiTool.Domain;
using RumiTool.Service;

namespace RumiTool.Windows;

public class StickyNoteWindow : Window, IDisposable
{
    private const int DefaultX = 275;
    private const int DefaultY = 240;
    private const int MaxNoteLength = 5000;

    private readonly IStickyNoteService service;
    private readonly string name;
    private string content = "";
    private StickyNoteConfig config = new();

    public StickyNoteWindow(string name, IStickyNoteService service) : base($"Note - {name}##{name}")
    {
        this.service = service;
        this.name = name;
        AllowPinning = true;
        Flags = ImGuiWindowFlags.Modal;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(DefaultX - 70, DefaultY - 70),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        GetContent();
        Refresh();
    }

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, config.Colour);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, config.Colour);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, config.Colour);
    }
    
    public override void Draw()
    {
        DrawInput();
    }
    
    public override void PostDraw()
    {
        ImGui.PopStyleColor(3);
    }

    public void DrawInput()
    {
        var input = content;
        ImGui.InputTextMultiline("", ref input, MaxNoteLength, ImGui.GetContentRegionAvail());
        if (input != content)
        {
            content = input;
            SaveContent();
        }
    }

    private void GetContent()
    {
        content = service.LoadSticky(name).Content;
    }

    public void Refresh()
    {
        config = service.GetStickyConfig(name);
    }

    private void SaveContent()
    {
        service.SaveSticky(name, content);
    }

    public void Dispose() { }
}
