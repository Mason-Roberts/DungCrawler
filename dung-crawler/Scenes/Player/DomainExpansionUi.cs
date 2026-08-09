using Godot;
using System;

public partial class DomainExpansionUi : CanvasLayer
{
	private const float HideDuration = 4f;
	private const float FadeInDuration = 2f;
	private const float HoldDuration = 6f;
	private const float FadeOutDuration = 2f;
	private const float TotalDuration = HideDuration + FadeInDuration + HoldDuration + FadeOutDuration;

	private Control _expandDomain;
	private float _elapsed = 0f;

	public override void _Ready()
	{
		_expandDomain = GetNode<Control>("ExpandDomain");
		_expandDomain.Modulate = new Color(1, 1, 1, 0);
	}

	public override void _Process(double delta)
	{
		_elapsed += (float)delta;

		if (_elapsed >= TotalDuration)
		{
			_expandDomain.Modulate = new Color(1, 1, 1, 0);
			return;
		}

		if (_elapsed < HideDuration)
		{
			// Still hidden
		}
		else if (_elapsed < HideDuration + FadeInDuration)
		{
			float t = (_elapsed - HideDuration) / FadeInDuration;
			_expandDomain.Modulate = new Color(1, 1, 1, t);
		}
		else if (_elapsed < HideDuration + FadeInDuration + HoldDuration)
		{
			_expandDomain.Modulate = new Color(1, 1, 1, 1);
		}
		else
		{
			float t = (_elapsed - HideDuration - FadeInDuration - HoldDuration) / FadeOutDuration;
			_expandDomain.Modulate = new Color(1, 1, 1, 1 - t);
		}
	}
}
