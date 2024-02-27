using Godot;
using System;
using System.Diagnostics;

public partial class Firearm : Node3D
{
    [ExportGroup("Gun Properties")] 
    [Export]
    public string weaponName = "weaponName"; //What the weapon is called
    [Export]
    public float damage = 1f; //How much damage the weapon does on hit
    [Export]
    public float firerate = 300f;//How quickly the weapon attacks
    [Export]
    public float length = 1f; //how long the weapon is for wall raising
    [Export]
    public float reloadSpeed = 5f; //time it takes to reload
    [Export]
    public int magazineSize = 30; //The number of bullets the gun can shoot before reloading
    [Export]
    public bool chambered = true; //Does the gun have a round in the chamber
    [Export]
    public float chamberSpeed = 2f; //how long it takes to chamber a round
    [Export(PropertyHint.Range, "0,1")]
    public float jamingFactor = 1f; //How prone is the weapon to jam

    //Object 
    public MuzzleFlash muzzleFlash; //Muzzle flash component
    public Timer firerateTimer; //timer for how quickly the bullets shoot (min of 0.05 sec)
    public AudioStreamPlayer3D shootAudio; //audio source for the bullets firing


    //Operation Variables
    public bool isShooting; 
    

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        muzzleFlash = GetNode<MuzzleFlash>("GunComponents/MuzzleFlash");
        firerateTimer = GetNode<Timer>("GunComponents/FirerateTimer");
        shootAudio = GetNode<AudioStreamPlayer3D>("GunComponents/ShootAudio");
        firerateTimer.WaitTime = 1/(firerate/60);
        GD.Print(1/(firerate/60));
        isShooting = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if(Input.IsActionPressed("shoot") && !isShooting)
        {
            Shoot();
            isShooting = true;
        }
	}

    public void Shoot()
    {
        muzzleFlash.Shoot();
        firerateTimer.Start();
        shootAudio.Play();
    }

    public void OnFirerateTimerTimeout()
    {
        isShooting = false;
    }
}
