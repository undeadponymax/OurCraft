using Godot;

[Tool]
[GlobalClass]
public partial class Item : Resource
{
    [Export] public Texture2D Texture { get; set; }
 
    public Item() { }
}
