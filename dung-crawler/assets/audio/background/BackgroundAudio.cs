using Godot;
using System;

public partial class BackgroundAudio : Node2D
{
	private AudioStreamPlayer2D _main;

	public override void _Ready()
	{
		_main = GetNode<AudioStreamPlayer2D>("Main");
		_main.Finished += () => _main.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
