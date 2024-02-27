using Godot;
using System;
using System.Diagnostics;

public partial class MuzzleFlash : Node3D
{

    [Export(PropertyHint.Range, "0.05,0.1")]
    public float muzzleFlashTime = 0.1f;

	public MeshInstance3D mesh;
	//public OmniLight3D omniLight;
	public Timer shootTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mesh = GetNode<MeshInstance3D>("MuzzleFlashMesh");
		//omniLight = GetNode<OmniLight3D>("MuzzleFlashMesh/OmniLight3D");
		shootTimer = GetNode<Timer>("MuzzleTimer");
        mesh.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void Shoot()
    {
        mesh.Show();
        shootTimer.Start(muzzleFlashTime);    
    }


	public void OnMuzzleTimerTimeout()
	{
        mesh.Hide();
	}
}
