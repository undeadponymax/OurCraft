using Godot;
using System;

[Tool]
public partial class Chunk : StaticBody3D
{
	[Export]
	public CollisionShape3D CollisionShape { get; set; }

	[Export]
	public MeshInstance3D MeshInstance { get; set; }

	public static Vector3I Dimensions = new Vector3I(16, 64, 16);

	private SurfaceTool _surfaceTool = new();

	private Block[,,] _blocks = new Block[Dimensions.X, Dimensions.Y, Dimensions.Z];

	public Vector2I ChunkPosition { get; private set; }

	[Export]
	public FastNoiseLite Noise { get; set; }

	[Export]
	public FastNoiseLite caveNoiseGenerator { get; set; }

	[Export]
	public float caveThreshold = 0.5f;


	public void SetChunkPosition(Vector2I position)
	{
		GD.Print($"Setting chunk position to: {position}");

		ChunkManager.Instance.UpdateChunkPosition(this, position, ChunkPosition);
		ChunkPosition = position;
		CallDeferred(Node3D.MethodName.SetGlobalPosition, new Vector3(ChunkPosition.X * Dimensions.X, 0, ChunkPosition.Y * Dimensions.Z));

		if (Noise == null)
		{
			GD.PrintErr("Noise generator not set, initializing default FastNoiseLite.");
			Noise = new FastNoiseLite();
		}

		if (caveNoiseGenerator == null)
		{
			GD.PrintErr("Cave noise generator not set, initializing default FastNoiseLite.");
			caveNoiseGenerator = new FastNoiseLite();
		}

		Generate();
		Update();
	}

	public void Generate()
	{

		// Step 1: Generate base terrain (with caves)
		GenerateBaseTerrain();

		// Step 2: Replace top blocks with grass and dirt
		ReplaceTopBlocks();

	}

	public void ReplaceTopBlocks()
	{
		for (int x = 0; x < Dimensions.X; x++)
		{
			for (int z = 0; z < Dimensions.Z; z++)
			{
				int surfaceY = -1;

				// Find the topmost stone block
				for (int y = Dimensions.Y - 1; y >= 0; y--)
				{
					if (_blocks[x, y, z] == BlockManager.Instance.Stone)
					{
						surfaceY = y;
						break;
					}
				}

				// Replace top block with grass and dirt
				if (surfaceY != -1)
				{
					_blocks[x, surfaceY, z] = BlockManager.Instance.Grass;
					for (int dirtDepth = 1; dirtDepth <= 3 && surfaceY - dirtDepth >= 0; dirtDepth++)
					{
						_blocks[x, surfaceY - dirtDepth, z] = BlockManager.Instance.Dirt;
					}
				}
			}
		}
	}

	public void GenerateBaseTerrain()
	{
		for (int x = 0; x < Dimensions.X; x++)
		{
			for (int z = 0; z < Dimensions.Z; z++)
			{
				for (int y = 0; y < Dimensions.Y; y++)
				{
					Block block;
					// Generate height based on noise map
					var globalBlockPosition = ChunkPosition * new Vector2I(Dimensions.X, Dimensions.Z) + new Vector2(x, z);
					var groundHeight = (int)(Dimensions.Y * ((Noise.GetNoise2D(globalBlockPosition.X, globalBlockPosition.Y) + 1f) / 2f));
					if (y <= groundHeight)
					{
						// Carve out caves based on a 3D noise map
						float caveNoise = caveNoiseGenerator.GetNoise3D(globalBlockPosition.X, y, globalBlockPosition.Y);
						if (caveNoise < caveThreshold)
						{
							block = BlockManager.Instance.Stone;
						}
						else
						{
							block = BlockManager.Instance.Air;
						}
					}
					else
					{
						block = BlockManager.Instance.Air;
					}

					_blocks[x, y, z] = block;
				}
			}
		}
	}

	public void Update()
	{
		_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		for (int x = 0; x < Dimensions.X; x++)
		{
			for (int y = 0; y < Dimensions.Y; y++)
			{
				for (int z = 0; z < Dimensions.Z; z++)
				{
					CreateBlockMesh(new Vector3I(x, y, z));
				}
			}
		}

		_surfaceTool.SetMaterial(BlockManager.Instance.ChunkMaterial);
		var mesh = _surfaceTool.Commit();

		MeshInstance.Mesh = mesh;
		CollisionShape.Shape = mesh.CreateTrimeshShape();
	}

	private void CreateBlockMesh(Vector3I blockPosition)
	{
		var block = _blocks[blockPosition.X, blockPosition.Y, blockPosition.Z];

		if (block == BlockManager.Instance.Air) return;

		if (CheckTransparent(blockPosition + Vector3I.Up))
			CreateFaceMesh(BlockRenderData.top, blockPosition, block.TopTexture ?? block.Texture);

		if (CheckTransparent(blockPosition + Vector3I.Down))
			CreateFaceMesh(BlockRenderData.bottom, blockPosition, block.BottomTexture ?? block.Texture);

		if (CheckTransparent(blockPosition + Vector3I.Left))
			CreateFaceMesh(BlockRenderData.left, blockPosition, block.Texture);

		if (CheckTransparent(blockPosition + Vector3I.Right))
			CreateFaceMesh(BlockRenderData.right, blockPosition, block.Texture);

		if (CheckTransparent(blockPosition + Vector3I.Forward))
			CreateFaceMesh(BlockRenderData.front, blockPosition, block.Texture);

		if (CheckTransparent(blockPosition + Vector3I.Back))
			CreateFaceMesh(BlockRenderData.back, blockPosition, block.Texture);
	}

	private void CreateFaceMesh(int[] face, Vector3I blockPosition, Texture2D texture)
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

		var a = BlockRenderData.vertices[face[0]] + blockPosition;
		var b = BlockRenderData.vertices[face[1]] + blockPosition;
		var c = BlockRenderData.vertices[face[2]] + blockPosition;
		var d = BlockRenderData.vertices[face[3]] + blockPosition;

		var uvTriangle1 = new Vector2[] { uvA, uvB, uvC };
		var uvTriangle2 = new Vector2[] { uvA, uvC, uvD };

		var triangle1 = new Vector3[] { a, b, c };
		var triangle2 = new Vector3[] { a, c, d };

		var normal = ((Vector3)(c - a)).Cross(((Vector3)(b - a))).Normalized();
		var normals = new Vector3[] { normal, normal, normal };

		_surfaceTool.AddTriangleFan(triangle1, uvTriangle1, normals: normals);
		_surfaceTool.AddTriangleFan(triangle2, uvTriangle2, normals: normals);
	}

	private bool CheckTransparent(Vector3I blockPosition)
	{
		if (blockPosition.X < 0 || blockPosition.X >= Dimensions.X) return true;
		if (blockPosition.Y < 0 || blockPosition.Y >= Dimensions.Y) return true;
		if (blockPosition.Z < 0 || blockPosition.Z >= Dimensions.Z) return true;

		return _blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] == BlockManager.Instance.Air;
	}

	public void SetBlock(Vector3I blockPosition, Block block)
	{
		_blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] = block;
		Update();
	}
}
