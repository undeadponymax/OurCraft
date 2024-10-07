using Godot;

// Data needed to render any block. Put it here to reduce duplication
[Tool]
public class BlockRenderData
{
    public static readonly Vector3I[] vertices = new Vector3I[]
    {
        new Vector3I(0, 0, 0),
        new Vector3I(1, 0, 0),
        new Vector3I(0, 1, 0),
        new Vector3I(1, 1, 0),
        new Vector3I(0, 0, 1),
        new Vector3I(1, 0, 1),
        new Vector3I(0, 1, 1),
        new Vector3I(1, 1, 1)
    };

    public static readonly int[] top = new int[] { 2, 3, 7, 6 };
    public static readonly int[] bottom = new int[] { 0, 4, 5, 1 };
    public static readonly int[] left = new int[] { 6, 4, 0, 2 };
    public static readonly int[] right = new int[] { 3, 1, 5, 7 };
    public static readonly int[] back = new int[] { 7, 5, 4, 6 };
    public static readonly int[] front = new int[] { 2, 0, 1, 3 };
}

