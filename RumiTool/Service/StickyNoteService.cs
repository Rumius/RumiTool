using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RumiTool.Domain;

namespace RumiTool.Service;

public interface IStickyNoteService
{
    public void SaveSticky(string name, string content);
    public StickyNote LoadSticky(string name);
    public void DeleteSticky(string name);
    public List<string> GetAllStickies();
    public StickyNoteConfig GetStickyConfig(string name);
    public void SaveStickyConfig(string name, StickyNoteConfig config);
}

public class StickyNoteService : IStickyNoteService
{
    private const string SavePath = @"%AppData%\XIVLauncher\pluginConfigs\RumiTool\StickyNotes";

    private static string GetSavePath()
    {
        return Environment.ExpandEnvironmentVariables(SavePath);
    }
    
    public static string SanitiseName(string name)
    {
        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        var invalidReStr = $"[{invalidChars}]+";
        return Regex.Replace(name, invalidReStr, "_").ToLowerInvariant().Trim();
    }

    private static string GetStickyPath(string name, string ext = "txt")
    {
        return Path.Combine(GetSavePath(), SanitiseName(name) + "." + ext);
    }

    public void SaveSticky(string name, string content)
    {
        File.WriteAllText(GetStickyPath(name), content);
    }

    public StickyNote LoadSticky(string name)
    {
        var path = GetStickyPath(name);
        var content = File.Exists(path) ? File.ReadAllText(path) : "";
        return new StickyNote(name, content);
    }

    public void DeleteSticky(string name)
    {
        var path = GetStickyPath(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public List<string> GetAllStickies()
    {
        var path = GetSavePath();
        var files = Directory
                    .GetFiles(path, "*.txt")
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(str => !string.IsNullOrWhiteSpace(str));
        return files.ToList()!;
    }

    public StickyNoteConfig GetStickyConfig(string name)
    {
        var path = GetStickyPath(name, "json");
        if (!File.Exists(path))
        {
            return new StickyNoteConfig();
        }
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<StickyNoteConfig>(json) ?? new StickyNoteConfig();
    }
    
    public void SaveStickyConfig(string name, StickyNoteConfig config)
    {
        var path = GetStickyPath(name, "json");
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(path, json);
    }

}
