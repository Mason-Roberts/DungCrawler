using Godot;
using System;

public partial class PulseExpansion : Area3D
{

	[Export]
	public double MaxScale { get; set; }

	[Export]
	public double ExpansionSpeed { get; set; }

	[Export]
	public double FadeDuration { get; set; }

	private MeshInstance3D _meshInstance { get; set; }
	private ShaderMaterial _shader { get; set; }

	private double _currentScale { get; set; } = 0.1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_meshInstance = GetNode<MeshInstance3D>("PulseMeshInstance");
		_shader = _meshInstance.GetActiveMaterial(0) as ShaderMaterial;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_currentScale < MaxScale)
		{
			_currentScale += ExpansionSpeed * delta;
		}
		else
		{
			var tween = CreateTween();
			tween.TweenProperty(_shader, "shader_parameter/sphere_opacity", 0.0, FadeDuration);
			tween.Parallel().TweenProperty(_shader, "shader_parameter/shine_color", new Color(0, 0, 0, 0), FadeDuration);
			tween.TweenCallback(Callable.From(QueueFree));
			SetProcess(false);
		}
	}
}
