using Godot;

[Tool]
[GlobalClass]
public partial class Block : Resource
{
	[Export] public Texture2D Texture { get; set; }
	[Export] public Texture2D TopTexture { get; set; }
	[Export] public Texture2D BottomTexture { get; set; }

    public Texture2D[] Textures => new Texture2D[] { Texture, TopTexture, BottomTexture };

	public Block() { }

    public MeshInstance3D RenderBlockInInventory(Block block)
    {
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        CreateInventoryBlockMesh(surfaceTool, block);

        surfaceTool.SetMaterial(BlockManager.Instance.ChunkMaterial);
        var mesh = surfaceTool.Commit();
        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;

        return meshInstance;
    }

    private void CreateInventoryBlockMesh(SurfaceTool surfaceTool, Block block)
    {
        CreateFaceMesh(surfaceTool, BlockRenderData.top, block.TopTexture ?? block.Texture);
        //CreateFaceMesh(surfaceTool, BlockRenderData.bottom, block.BottomTexture ?? block.Texture);
        CreateFaceMesh(surfaceTool, BlockRenderData.left, block.Texture);
        CreateFaceMesh(surfaceTool, BlockRenderData.right, block.Texture);
        CreateFaceMesh(surfaceTool, BlockRenderData.front, block.Texture);
        CreateFaceMesh(surfaceTool, BlockRenderData.back, block.Texture);
    }

    private void CreateFaceMesh(SurfaceTool surfaceTool, int[] face, Texture2D texture)
    {
        var texturePosition = BlockManager.Instance.GetTextureAtlasPosition(texture);
        var textureAtlasSize = BlockManager.Instance.TextureAtlasSize;

        var uvOffset = texturePosition / textureAtlasSize;
        var uvWidth = 1f / textureAtlasSize.X;
        var uvHeight = 1f / textureAtlasSize.Y;

        var uvA = uvOffset + new Vector2(0, 0);
        var uvB = uvOffset + new Vector2(0, uvHeight);
        var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
        var uvD = uvOffset + new Vector2(uvWidth, 0);

        var a = BlockRenderData.vertices[face[0]];
        var b = BlockRenderData.vertices[face[1]];
        var c = BlockRenderData.vertices[face[2]];
        var d = BlockRenderData.vertices[face[3]];

        var uvTriangle1 = new Vector2[] { uvA, uvB, uvC };
        var uvTriangle2 = new Vector2[] { uvA, uvC, uvD };

        var triangle1 = new Vector3[] { a, b, c };
        var triangle2 = new Vector3[] { a, c, d };

        var normal = ((Vector3)(c - a)).Cross(((Vector3)(b - a))).Normalized();
        var normals = new Vector3[] { normal, normal, normal };

        surfaceTool.AddTriangleFan(triangle1, uvTriangle1, normals: normals);
        surfaceTool.AddTriangleFan(triangle2, uvTriangle2, normals: normals);
    }
}
