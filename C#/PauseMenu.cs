using Godot;
using static Godot.DisplayServer;
using static Godot.Input;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		// Ensure this node processes even when the game is paused
		this.ProcessMode = ProcessModeEnum.Always;  // Or ProcessModeEnum.WhenPaused

		var resumeButton = GetNode<Button>("Panel/VBoxContainer/resumeButton");
		resumeButton.Pressed += OnResumePressed;

		var quitButton = GetNode<Button>("Panel/VBoxContainer/quitButton");
		quitButton.Pressed += OnQuitPressed;

		Visible = false;  // Hide the pause menu initially
	}

	public override void _Input(InputEvent @event)
	{
		// Toggle pause menu with Escape
		if (@event.IsActionPressed("ui_cancel"))
		{
			TogglePauseMenu();
		}
	}

	private void TogglePauseMenu()
	{
		Visible = !Visible;
		GetTree().Paused = Visible;

		if (Visible)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetNode<Button>("Panel/VBoxContainer/resumeButton").GrabFocus();  // Focus resume button
		}
		else
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	private void OnResumePressed()
	{
		// Resume the game
		TogglePauseMenu();
	}

	private void OnQuitPressed()
	{
		GetTree().Paused = false;  // Ensure the game is unpaused when quitting
		GetTree().ChangeSceneToFile("res://MainMenu.tscn");
	}
}
