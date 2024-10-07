using Godot;

// Mostly handles visual part of the GUI
[Tool]
public partial class Gui : CanvasLayer
{
	private Label _fpsLabel;
	private int _currentSelection = 0;
	private const int _totalSelections = 9;
	private InventorySlot cell;
	[Export] public int scaleSetting;

	public override void _Ready()
	{
		_fpsLabel = GetNode<Label>("FPS");

		UpdateSelection();

		CallDeferred(nameof(InitializeInventoryCell));

		// Applies GUI scale to the whole bottom section
		Vector2 newScale = new Vector2(scaleSetting, scaleSetting);
		GetNode<Control>("Hotbar").Scale = newScale;

	}

	public override void _Process(double delta)
	{
		_fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
	}

	// Here just to test inventory functionality, remove later
	private void InitializeInventoryCell()
	{
		cell = GetNode<InventorySlot>("Hotbar/InventorySlot1");
		GD.Print("InventorySlot found: " + (cell != null));
		cell.SetItem(ItemManager.Instance.stone);  // Or any other item.
	}

	public override void _Input(InputEvent @event)
	{
		// Handles scrolling through hotbar, but only visual for nows
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.IsPressed())
			{
				ScrollRight();
			}
			else if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.IsPressed())
			{
				ScrollLeft();
			}
		}
	}

	private void ScrollRight()
	{
		_currentSelection = (_currentSelection + 1) % _totalSelections;
		UpdateSelection();
	}

	private void ScrollLeft()
	{
		_currentSelection = (_currentSelection - 1 + _totalSelections) % _totalSelections;
		UpdateSelection();
	}

	// Handles showing the correct selection square on the hotbar, only visual for now
	private void UpdateSelection()
	{
		for (int i = 0; i < _totalSelections; i++)
		{
			var selectNode = GetNode<TextureRect>($"Hotbar/Select{i + 1}");
			if (i == _currentSelection)
			{
				selectNode.Visible = true;
			}
			else
			{
				selectNode.Visible = false;
			}
		}
	}
}
