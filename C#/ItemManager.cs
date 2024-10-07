using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Godot;

[Tool]
public partial class ItemManager : Node
{
	[Export]
	public Item stupid_bug_fix { get; set; }
	[Export]
	public Item stone_axe { get; set; }
	[Export]
	public Item stone_hoe { get; set; }
	[Export]
	public Item stone_sword { get; set; }
	[Export]
	public Item stone_shovel { get; set; }
	[Export]
	public Item stone_pickaxe { get; set; }
	[Export]
	public BlockItem stone { get; set; }

	private readonly Dictionary<Texture2D, Vector2I> _atlasLookup = new();

	private int _gridWidth = 3;
	private int _gridHeight;

	public Vector2I ItemTextureSize { get; } = new(16, 16);

	public Vector2 TextureAtlasSize { get; private set; }

	public static ItemManager Instance { get; private set; }

	public StandardMaterial3D ItemMaterial { get; private set; }

	public override void _Ready()
	{
		Instance = this;

		stone.BlockName = "Stone";

		var itemTextures = new Item[] { stupid_bug_fix, stone_hoe, stone_axe, stone_sword, stone_shovel, stone_pickaxe }.Select(item => item.Texture).Where(texture => texture != null).Distinct().ToArray();

		for (int i = 0; i < itemTextures.Length; i++)
		{
			var texture = itemTextures[i];
			_atlasLookup.Add(texture, new Vector2I(i % _gridWidth, Mathf.FloorToInt(i / _gridWidth)));
		}

		_gridHeight = Mathf.CeilToInt(itemTextures.Length / (float)_gridWidth);

		var image = Image.CreateEmpty(_gridWidth * ItemTextureSize.X, _gridHeight * ItemTextureSize.Y, false, Image.Format.Rgba8);

		for (var x = 0; x < _gridWidth; x++)
		{
			for (var y = 0; y < _gridHeight; y++)
			{
				var imgIndex = x + y * _gridWidth;

				if (imgIndex >= itemTextures.Length) continue;

				var currentImage = itemTextures[imgIndex].GetImage();
				currentImage.Convert(Image.Format.Rgba8);

				image.BlitRect(currentImage, new Rect2I(Vector2I.Zero, ItemTextureSize), new Vector2I(x, y) * ItemTextureSize);
			}
		}

		var textureAtlas = ImageTexture.CreateFromImage(image);

		ItemMaterial = new()
		{
			AlbedoTexture = textureAtlas,
			TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest
		};

		TextureAtlasSize = new Vector2(_gridWidth, _gridHeight);

		GD.Print($"Done loading {itemTextures.Length} images to make {_gridWidth} x {_gridHeight} atlas for items");
	}

	public Vector2I GetTextureAtlasPosition(Texture2D texture)
	{
		if (texture == null)
		{
			GD.PrintErr($"Texture not found in the atlas.");
			return Vector2I.Zero;
		}
		else
		{
			if (texture == null)
			{
				GD.PrintErr($"Failed to load texture from the atlas.");
			}

			return _atlasLookup[texture];
		}
	}
}
