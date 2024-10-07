using Godot;
using System;

[Tool]
[GlobalClass]
public partial class BlockItem : Item
{
    public string BlockName { get; set; }

    public BlockItem() { }

    public BlockItem(string blockName)
    {
        BlockName = blockName;
    }
}
