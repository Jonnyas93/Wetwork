using Godot;
using System;

public partial class BulletImpact : Node3D
{

    public GpuParticles3D dustParticles;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        dustParticles = GetNode<GpuParticles3D>("DustParticles");
        dustParticles.Emitting = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
	}
}
