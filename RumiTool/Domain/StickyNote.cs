namespace RumiTool.Domain;

public class StickyNote(string name, string content)
{
    public string Name { get; set; } = name;
    public string Content { get; set; } = content;
}
