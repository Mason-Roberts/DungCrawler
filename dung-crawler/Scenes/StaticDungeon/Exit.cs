using Godot;
using System;

public partial class Exit : Area3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnBodyEntered(Node3D body)
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;

		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/UI/MainMenu.tscn");
	}
}
