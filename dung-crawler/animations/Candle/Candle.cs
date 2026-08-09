using Godot;
using System;

public partial class Candle : Node2D
{
	private AnimatedSprite2D[] _candleStages;
	private int _currentIndex = 0;

	public void NextCandle()
	{
		_currentIndex++;
		SelectStage(_currentIndex);
	}

	public int GetIndex()
	{
		return _currentIndex;
	}

	private void SelectStage(int index)
	{
		foreach (AnimatedSprite2D candle in _candleStages)
		{
			candle.Hide();
		}

		if (index > 3 || index < 0)
		{
			return;
		}

		_candleStages[index].Show();
		_candleStages[index].Play();
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_candleStages = new AnimatedSprite2D[]
		{
			GetNode<AnimatedSprite2D>("MaxCandle"),
			GetNode<AnimatedSprite2D>("CandleShortening"),
			GetNode<AnimatedSprite2D>("CandleShortened"),
			GetNode<AnimatedSprite2D>("MinCandle")
		};

		SelectStage(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
