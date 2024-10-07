using Godot;

[Tool]
public partial class InventorySlot : Control
{
	private TextureRect _textureRect;
	private Item _currentItem;
	private SubViewport _viewport;
	private SubViewportContainer _container;

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		_viewport = GetNode<SubViewport>("SubViewportContainer/SubViewport");
		_container = GetNode<SubViewportContainer>("SubViewportContainer");
	}

	public void SetItem(Item item)
	{
		_currentItem = item;

		if (item is BlockItem blockItem)
		{
			RenderBlockModel(blockItem.BlockName);
		}
		else if (item != null && item.Texture != null)
		{
			var atlasPosition = ItemManager.Instance.GetTextureAtlasPosition(item.Texture);
			if (atlasPosition != Vector2I.Zero)
			{
				_textureRect.Texture = item.Texture;
				_textureRect.Visible = true;
				_container.Hide();
			}
			else
			{
				GD.PrintErr("Item texture not found in the atlas.");
				_textureRect.Visible = false;
			}
		}
		else
		{
			ClearSlot();
		}
	}

	public void SetupViewportForRendering()
	{
		var camera = _viewport.GetNodeOrNull<Camera3D>("InventoryCamera");
		var light = _viewport.GetNodeOrNull<DirectionalLight3D>("InventoryLight");

		if (camera != null)
		{
			camera.Position = new Vector3((float)2, (float)2, (float)2);
			camera.LookAt(Vector3.Zero, Vector3.Up);
		}

		if (light != null)
		{
			light.Position = new Vector3(0, 2, 1);
			light.RotationDegrees = new Vector3(-45, 0, 0);
			light.LightEnergy = 1.0f;
		}
	}

	public void RenderBlockModel(string blockName)
	{
		_textureRect.Visible = false;
		_container.Show();

		var block = BlockManager.Instance.BlockDictionary[blockName];

		var blockMeshInstance = block.RenderBlockInInventory(block);

		_viewport.AddChild(blockMeshInstance);

		SetupViewportForRendering();
	}

	public void ClearSlot()
	{
		_currentItem = null;
		_textureRect.Visible = false;
		_container.Hide();
	}
}
