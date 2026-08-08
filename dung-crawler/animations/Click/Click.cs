using Godot;
using System;

public partial class Click : AnimatedSprite2D
{
	private AudioStreamPlayer2D _player;
	private bool _lock = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.Frame == 4)
		{
			if (!_lock)
			{
				_lock = true;
				_player.Play();				
			}
		}
		else
		{
			_lock = false;
		}
	}
}
