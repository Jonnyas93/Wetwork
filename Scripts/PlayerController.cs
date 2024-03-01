using Godot;
using System;
using System.Data;

public partial class PlayerController : CharacterBody3D
{
	[Export]
	public const float speed = 5.0f;
	[Export]
	public const float jumpVelocity = 4.5f;

	[Export]
	public float mouseSensitivity = 0.05f;

    [Export]
    public float animDelta = 0.05f;

	private Camera3D camera3D;
	private Node3D cameraPivot;
    private Node3D weaponObject;
    private Node3D dummyPointer;
    private RayCast3D cameraRaycast;
    private RayCast3D weaponRaycast;
    private Marker3D backupAimpoint;
    

    private bool movingToAimpoint;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera3D = GetNode<Camera3D>("CameraPivot/Camera3D");
		cameraPivot = GetNode<Node3D>("CameraPivot");
        Node3D weaponNode = GetNode<Node3D>("CameraPivot/Weapon");
        weaponObject = (Node3D)weaponNode.GetChild(0);
		Input.MouseMode = Input.MouseModeEnum.Captured;
        cameraRaycast = GetNode<RayCast3D>("CameraPivot/CameraRayCast");
        weaponRaycast = weaponObject.GetNode<RayCast3D>("GunComponents/BarrelRayCast");
        backupAimpoint = GetNode<Marker3D>("CameraPivot/BackupAimPoint");
        dummyPointer = GetNode<Node3D>("CameraPivot/DummyPointer");
        movingToAimpoint = true;
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
                velocity.Y = jumpVelocity;
                
            }
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("moveRight", "moveLeft", "moveBackward", "moveForward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}
		
		//  Capturing/Freeing the cursor
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			else
				Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		var aimPoint = cameraRaycast.GetCollisionPoint();
        weaponObject.LookAt(aimPoint);
        if (!cameraRaycast.IsColliding())
        { 
            weaponObject.LookAt(backupAimpoint.GlobalPosition);
        }

		Velocity = velocity;
        MoveAndSlide();
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
			cameraPivot.RotateX(Mathf.DegToRad(mouseEvent.Relative.Y * mouseSensitivity));
			RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * mouseSensitivity));

			Vector3 cameraRot = cameraPivot.RotationDegrees;
			cameraRot.X = Mathf.Clamp(cameraRot.X, -70, 70);
			cameraPivot.RotationDegrees = cameraRot;
            
		}
	}

}
