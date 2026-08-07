using Godot;
using System;

public partial class Dugeon : Node3D
{
	[Export]
	public GridMap GridMap;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
