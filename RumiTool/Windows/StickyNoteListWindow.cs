using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using RumiTool.Domain;
using RumiTool.Service;

namespace RumiTool.Windows;

public class StickyNoteListWindow : Window, IDisposable
{
    private readonly IStickyNoteService service;
    private readonly Plugin plugin;
    
    private readonly Dictionary<string, StickyNoteConfig> notes = new();
    
    public StickyNoteListWindow(IStickyNoteService service, Plugin plugin) : base("Sticky List")
    {
        this.service = service;
        this.plugin = plugin;
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(1100, 300);
    }
    
    public override void Draw()
    {
        if (ImGui.Button("Refresh"))
        {
            Refresh();
        }
        
        ImGui.SameLine();

        if (ImGui.Button("Toggle All"))
        {
            notes.Keys.ToList().ForEach(plugin.OpenStickyNoteWindow);
        }

        ImGui.Separator();
        
        Dictionary<string, String> truncated = new();
        const string truncation = "...   ";
        var maxWidth = ImGui.CalcTextSize(new string('a', 25) + truncation).X;
        
        const int maxChars = 25;
        foreach (var (note, _) in notes)
        {
            var display = note;
            if (display.Length > maxChars)
            {
                display = display[..maxChars] + truncation;
            } else
            {
                display += new string(' ', maxChars - display.Length);
            }
            truncated.Add(note, display);
            var width = ImGui.CalcTextSize(display).X;
            if (width > maxWidth)
            {
                maxWidth = width;
            }
        }
        
        foreach (var (note, config) in notes)
        {
            var display = truncated[note];
            var padding = new string(' ', (int)((maxWidth - ImGui.CalcTextSize(display).X) / ImGui.CalcTextSize(" ").X));
            ImGui.Text(display + padding);

            ImGui.SameLine();
            if (ImGui.Button($"Toggle##{note}"))
            {
                plugin.OpenStickyNoteWindow(note);
            }

            ImGui.SameLine();
            if (ImGui.Button($"Delete##{note}"))
            {
                service.DeleteSticky(note);
                plugin.CloseStickyNoteWindow(note);
                Refresh();
            }
            
            ImGui.SameLine();
            var col = config.Colour;
            ImGui.ColorEdit4($"Colour Picker##{note}", ref col);
            if (col != config.Colour)
            {
                config.Colour = col;
                SaveConfig(note);
            }

            ImGui.Separator();
        }
    }

    public void Refresh()
    {
        var stickies = service.GetAllStickies();
        notes.Clear();
        stickies.ForEach(note => notes.Add(note, service.GetStickyConfig(note)));
    }

    private void SaveConfig(string note)
    {
        service.SaveStickyConfig(note, notes[note]);
        plugin.RefreshStickyNoteWindow(note);
    }

    public void Dispose() { }
}
