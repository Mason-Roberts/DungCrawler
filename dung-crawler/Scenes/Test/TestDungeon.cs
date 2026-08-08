using Godot;
using System;

public partial class TestDungeon : Node3D
{
	private Player _player;
	private PulseExpansion _currentExpansion;

	private void OnAnimationChange()
	{
		if (_currentExpansion == null || !_currentExpansion.IsInsideTree())
		{
			_currentExpansion = GD.Load<PackedScene>("res://Scenes/PulseExpansion/PulseExpansion.tscn").Instantiate<PulseExpansion>();
			_currentExpansion.MaxScale = 30;
			_currentExpansion.ExpansionSpeed = 10f;
			_currentExpansion.FadeDuration = 1;

			this.AddChild(_currentExpansion);
			_currentExpansion.GlobalPosition = _player.GlobalPosition;	
		}
	}

	private void OnPlayerMove()
	{
		if (_currentExpansion != null && _currentExpansion.IsInsideTree() && _currentExpansion.direction == 1)
		{
			_currentExpansion.GlobalPosition = _player.GlobalPosition;
		}
	}

	public override void _Ready()
	{
		_player = GetNode<Player>("Player");

		_player.Expansion += () => OnAnimationChange();
		_player.Move += () => OnPlayerMove();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
