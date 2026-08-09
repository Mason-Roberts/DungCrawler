using Godot;
using System;

public partial class Breathing : Node
{
	private AudioStreamPlayer2D _calmBreathing;
	private AudioStreamPlayer2D _lessCalmBreathing;
	private AudioStreamPlayer2D _franticBreathing;

	private AudioStreamPlayer2D _currentPlayer;
	private int _distressLevel = -1;
	private double _elapsedTime = 0;
	private double _ttBreath = 0;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public void IncreaseDistress()
	{
		_distressLevel++;

		if (_distressLevel < 2)
		{
			_currentPlayer = _calmBreathing;
		}
		else if (_distressLevel == 2)
		{
			_currentPlayer = _lessCalmBreathing;
		}
		else if (_distressLevel == 3)
		{
			_currentPlayer = _franticBreathing;
		}

		_ttBreath = GetBreathInterval();
	}

	private double GetBreathInterval()
	{
		switch(_distressLevel)
		{
			case 0:
				return _rng.RandfRange(30, 45);
			case 1:
				return _rng.RandfRange(20, 30);
			case 2:
				return _rng.RandfRange(18, 25);
			case 3:
				return _rng.RandfRange(10, 15);
			default:
				return double.MaxValue;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_calmBreathing = GetNode<AudioStreamPlayer2D>("CalmBreathing");
		_lessCalmBreathing = GetNode<AudioStreamPlayer2D>("LessCalmBreathing");
		_franticBreathing = GetNode<AudioStreamPlayer2D>("FranticBreathing");

		IncreaseDistress();
		_currentPlayer.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_elapsedTime += delta;

		if (_elapsedTime >= _ttBreath && !_currentPlayer.Playing)
		{
			_elapsedTime = 0;
			_ttBreath = GetBreathInterval();

			_currentPlayer.Play();
		}
	}
}
