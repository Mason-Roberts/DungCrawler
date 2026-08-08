using Godot;
using System;

public partial class PulseExpansion : Area3D
{

	[Export]
	public double MaxScale { get; set; }

	[Export]
	public double MinScale { get; set; }

	[Export]
	public float ExpansionSpeed { get; set; }

	[Export]
	public double FadeDuration { get; set; }

	private MeshInstance3D _meshInstance { get; set; }
	private ShaderMaterial _shader { get; set; }

	private float _currentScale { get; set; } = 1;
	private Vector3 _originalScale;
	public int direction = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_meshInstance = GetNode<MeshInstance3D>("PulseMeshInstance");
		_shader = _meshInstance.GetActiveMaterial(0) as ShaderMaterial;

		var tween = CreateTween();
		tween.TweenProperty(_shader, "shader_parameter/sphere_opacity", 0.0, 0);

		_originalScale = _meshInstance.Scale;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (direction < 1)
		{
			if (_currentScale < MaxScale)
			{
				_currentScale += ExpansionSpeed * (float)delta;
				_meshInstance.Scale = new Vector3(_originalScale.X * _currentScale, _originalScale.Y * _currentScale, _originalScale.Z * _currentScale);
			}
			else
			{
				direction++;
			}	
		}
		else if (direction < 2)
		{
			if (_currentScale > MinScale)
			{
				_currentScale -= ExpansionSpeed * (float)delta;
				_meshInstance.Scale = new Vector3(_originalScale.X * _currentScale, _originalScale.Y * _currentScale, _originalScale.Z * _currentScale);
			}
			else
			{
				direction++;
			}
		}
		else
		{
			var tween = CreateTween();
			tween.TweenProperty(_shader, "shader_parameter/sphere_opacity", 0.0, FadeDuration);
			tween.Parallel().TweenProperty(_shader, "shader_parameter/shine_color", new Color(0.0f, 1.0f, 1.0f, 1.0f), FadeDuration);
			tween.TweenCallback(Callable.From(QueueFree));
			SetProcess(false);
		}
	}
}
