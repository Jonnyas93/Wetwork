using Godot;
using System;
using System.Data;

public partial class PlayerController : CharacterBody3D
{
	[Export]
	public const float Speed = 5.0f;
	[Export]
	public const float JumpVelocity = 4.5f;

	[Export]
	public float MouseSensitivity = 0.05f;

    [Export]
    public float animDelta = 0.05f;

	private Camera3D camera3D;
	private Node3D CameraPivot;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera3D = GetNode<Camera3D>("CameraPivot/Camera3D");
		CameraPivot = GetNode<Node3D>("CameraPivot");
		Input.MouseMode = Input.MouseModeEnum.Captured;
        
	}

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			{
                velocity.Y = JumpVelocity;
                
            }
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("moveRight", "moveLeft", "moveBackward", "moveForward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}
		
		//  Capturing/Freeing the cursor
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			else
				Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		
		Velocity = velocity;
        MoveAndSlide();
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
			CameraPivot.RotateX(Mathf.DegToRad(mouseEvent.Relative.Y * MouseSensitivity));
			RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * MouseSensitivity));

			Vector3 cameraRot = CameraPivot.RotationDegrees;
			cameraRot.X = Mathf.Clamp(cameraRot.X, -70, 70);
			CameraPivot.RotationDegrees = cameraRot;
		}
	}

}
