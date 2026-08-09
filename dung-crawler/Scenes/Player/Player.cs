using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	[Export]
	public float CamSensitivity = 0.002f;

	private Node3D _head;
	private Camera3D _cam;
	private AnimatedSprite2D _hand;
	private bool _lock = false;
	private PulseExpansion _currentExpansion;

	private void ExpansionComplete()
	{
		_currentExpansion = null;
	}

	public override void _Ready() {
		_head = GetNode<Node3D>("Head");
		_cam = GetNode<Camera3D>("Head/Camera3D");
		_hand = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion m) {
			_head.RotateY(-m.Relative.X * CamSensitivity);
			_cam.RotateX(-m.Relative.Y * CamSensitivity);

			Vector3 camRot = _cam.Rotation;
			camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
			_cam.Rotation = camRot;
		} else if (@event is InputEventMouseButton i && i.ButtonIndex == MouseButton.Left && _currentExpansion == null) {
			_hand.Play();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_hand.Frame == 4)
		{
			if (!_lock && _currentExpansion == null)
			{
				_lock = true;

				_currentExpansion = GD.Load<PackedScene>("res://Scenes/PulseExpansion/PulseExpansion.tscn").Instantiate<PulseExpansion>();
				_currentExpansion.MaxScale = 30;
				_currentExpansion.ExpansionSpeed = 10f;
				_currentExpansion.FadeDuration = 1;
				_currentExpansion.OnComplete += () => ExpansionComplete();

				this.AddChild(_currentExpansion);
				_currentExpansion.GlobalPosition = this.GlobalPosition;
			}
		}
		else
		{
			_lock = false;
		}

		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;

			try
			{
				if (_currentExpansion != null && _currentExpansion.direction == 1)
				{
					_currentExpansion.GlobalPosition = this.GlobalPosition;
				}	
			}
			catch (Exception e)
			{
				GD.PrintErr(e);
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
