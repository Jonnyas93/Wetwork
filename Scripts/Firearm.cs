using Godot;
using System;
using System.Diagnostics;
using System.Text;

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
    [Export]
    public float range = 100f; //Maximum Range of the weapon
    [Export]
    public PackedScene BulletImpact {get; set;} //sprite for when the bullets from the gun hit something
    [Export]
    public string Caliber = "7.62 x 39mm";
    [Export]
    public float vRecoil = 5f;
    [Export]
    public float hRecoil = 5f;
    [Export]
    public float recoilTimer = 0.0f;


    //Object 
    public MuzzleFlash muzzleFlash; //Muzzle flash component
    public Timer firerateTimer; //timer for how quickly the bullets shoot (min of 0.05 sec)
    public Timer reloadTimer; //timer for reloading
    public AudioStreamPlayer3D shootAudio; //audio source for the bullets firing
    public AudioStreamPlayer3D reloadAudio; //audio source for reloading
    public RayCast3D barrelRayCast; 
    


    //Operation Variables
    public bool ableToShoot; //bool for if the weapon is able to shoot

    public int roundsInMag; //number of current rounds in the magazine

    public Node3D startPosition;
    
    

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        muzzleFlash = GetNode<MuzzleFlash>("GunComponents/MuzzleFlash");
        firerateTimer = GetNode<Timer>("GunComponents/FirerateTimer");
        reloadTimer = GetNode<Timer>("GunComponents/ReloadTimer");
        reloadAudio = GetNode<AudioStreamPlayer3D>("GunComponents/ReloadAudio");
        shootAudio = GetNode<AudioStreamPlayer3D>("GunComponents/ShootAudio");
        barrelRayCast = GetNode<RayCast3D>("GunComponents/BarrelRayCast");
        startPosition = this;
        firerateTimer.WaitTime = 1/(firerate/60);
        roundsInMag = magazineSize;
        ableToShoot = true;
        barrelRayCast.ScaleObjectLocal(new Vector3(1,range,1));
	}

	public override void _PhysicsProcess(double delta)
	{
        
	}

    public void Shoot(double delta)
    {
        hitObject();
        Recoil(delta);
        muzzleFlash.Shoot();
        firerateTimer.Start();
        shootAudio.Play();
        roundsInMag--;
    }

    private void hitObject()
    {
        if(barrelRayCast.IsColliding())
        {
            Vector3 impactPoint = barrelRayCast.GetCollisionPoint();
            Vector3 impactNormal = barrelRayCast.GetCollisionNormal();
            //GD.Print(impactNormal);
            var impact = BulletImpact.Instantiate<Node3D>();
            GetTree().CurrentScene.AddChild(impact);
            impact.Position = impactPoint + (impactNormal * 0.01f);
            if (impactNormal != Vector3.Up)
            {
                impact.LookAt(impactPoint + impactNormal, Vector3.Up);
                impact.Transform = impact.Transform.RotatedLocal(Vector3.Left, (float)(Math.PI/2.0));
            }
            impact.Rotate(impactNormal, (float)GD.RandRange(0,MathF.PI*2));
        }
    }

    public void Recoil(double delta)
    {
        recoilTimer += (float)delta;
        double horizontalRecoil = GD.RandRange(-hRecoil,hRecoil) * (MathF.PI/180);
        double verticalRecoil = -GD.RandRange(1.2,1.5) * (MathF.PI/180);
        Node3D unRecoiledGun = this;
        Node3D recoiledGun = this;
        recoiledGun.RotateX((float)verticalRecoil);
        recoiledGun.RotateY((float)horizontalRecoil);
        Transform = unRecoiledGun.Transform.InterpolateWith(recoiledGun.Transform, recoilTimer);
    }

    public void OnFirerateTimerTimeout()
    {
        ableToShoot = true;
    }

    public void OnReloadTimerTimeout()
    {
        roundsInMag = magazineSize;
        ableToShoot = true;
    }
}
