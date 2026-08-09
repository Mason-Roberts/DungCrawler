using Godot;
using System;

public partial class DebugCamera : Camera3D
{
    [Export] public float MoveSpeed { get; set; } = 10.0f;
    [Export] public float LookSensitivity { get; set; } = 0.003f;
    [Export] public float FastMoveMultiplier { get; set; } = 3.0f;

    private Vector3 _rotation = Vector3.Zero;

    public override void _Ready()
    {
        // Capture the mouse cursor for a smooth mouselook experience
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _rotation = Rotation;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Toggle mouse capture mode on pressing Escape
        if (@event.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured 
                ? Input.MouseModeEnum.Visible 
                : Input.MouseModeEnum.Captured;
        }

        // Handle mouse movement for looking around
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _rotation.Y -= mouseMotion.Relative.X * LookSensitivity;
            _rotation.X -= mouseMotion.Relative.Y * LookSensitivity;
            
            // Limit vertical look to prevent the camera flipping upside down
            _rotation.X = Mathf.Clamp(_rotation.X, Mathf.DegToRad(-89f), Mathf.DegToRad(89f));
            
            Rotation = _rotation;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;

        Vector3 inputDirection = Vector3.Zero;

        // Map movement keys to direction vectors
        if (Input.IsKeyPressed(Key.W)) inputDirection += Transform.Basis.Z * -1; // Forward
        if (Input.IsKeyPressed(Key.S)) inputDirection += Transform.Basis.Z;      // Backward
        if (Input.IsKeyPressed(Key.A)) inputDirection += Transform.Basis.X * -1; // Left
        if (Input.IsKeyPressed(Key.D)) inputDirection += Transform.Basis.X;      // Right
        if (Input.IsKeyPressed(Key.E)) inputDirection += Transform.Basis.Y;      // Up
        if (Input.IsKeyPressed(Key.Q)) inputDirection += Transform.Basis.Y * -1; // Down

        if (inputDirection != Vector3.Zero)
        {
            inputDirection = inputDirection.Normalized();
            
            // Apply speed boost if Shift is held down
            float currentSpeed = Input.IsKeyPressed(Key.Shift) 
                ? MoveSpeed * FastMoveMultiplier 
                : MoveSpeed;

            GlobalPosition += inputDirection * currentSpeed * (float)delta;
        }
    }
}
