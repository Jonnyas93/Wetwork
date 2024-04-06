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
    [Export(PropertyHint.Range, "0.5,1,or_less")]
    public float durability = 1f; //Weapon condition and how prone is the weapon to jam
    [Export]
    public float range = 100f; //Maximum Range of the weapon
    [Export]
    public PackedScene BulletImpact {get; set;} //sprite for when the bullets from the gun hit something
    [Export]
    public string Caliber = "7.62 x 39mm"; //Text description of caliber
    [Export]
    public float vRecoil = 5f; //Vertical recoil of weapon
    [Export]
    public float hRecoil = 5f; //Horizontal recoil of weapon
    [Export(PropertyHint.Range, "0,1")]
    public float recoilTimer = 0.0f;


    //Objects
    public MuzzleFlash muzzleFlash; //Muzzle flash component
    public Timer firerateTimer; //timer for how quickly the bullets shoot (min of 0.05 sec)
    public Timer reloadTimer; //timer for reloading
    private Timer cycleTimer; //Timer for cycling
    private Timer unjamTimer; //Timer for unjamming
    public AudioStreamPlayer3D shootAudio; //audio source for the bullets firing
    public AudioStreamPlayer3D reloadAudio; //audio source for reloading
    public RayCast3D barrelRayCast; //raycast for bullets to hit
    public AnimationPlayer animPlayer; //animation player
    public Node3D startPosition; //Default position of weapon to return to after shooting


    //Operation Variables
    public bool ableToShoot; //bool for if the weapon is able to shoot
    public int roundsInMag; //number of current rounds in the magazine
    public JamsState jammed; //tracks if the weapon is currently jammed
    



    //Enumerations

    public enum JamsState
    {
        ProperOperation,
        FeedFailure,
        Misfire,
        EjectFailure,
    }
    
    

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        muzzleFlash = GetNode<MuzzleFlash>("GunComponents/MuzzleFlash");
        firerateTimer = GetNode<Timer>("GunComponents/FirerateTimer");
        reloadTimer = GetNode<Timer>("GunComponents/ReloadTimer");
        cycleTimer = GetNode<Timer>("GunComponents/CycleTimer");
        unjamTimer = GetNode<Timer>("GunComponents/UnjamTimer");
        reloadAudio = GetNode<AudioStreamPlayer3D>("GunComponents/ReloadAudio");
        shootAudio = GetNode<AudioStreamPlayer3D>("GunComponents/ShootAudio");
        barrelRayCast = GetNode<RayCast3D>("GunComponents/BarrelRayCast");
        animPlayer = GetNode<AnimationPlayer>("GunComponents/AnimationPlayer");
        startPosition = this;
        firerateTimer.WaitTime = 1/(firerate/60);
        roundsInMag = magazineSize;
        ableToShoot = true;
        barrelRayCast.ScaleObjectLocal(new Vector3(1,range,1));
        reloadTimer.WaitTime = reloadSpeed;
        jammed = JamsState.ProperOperation;
	}

	public override void _PhysicsProcess(double delta)
	{
        
	}

    public void Shoot(double delta)
    {
        Jam();
        if (jammed == JamsState.ProperOperation || jammed == JamsState.EjectFailure) 
        {
            hitObject();
            Recoil(delta);
            muzzleFlash.Shoot();
            firerateTimer.Start();
            shootAudio.Play();
            roundsInMag--;
        }
    }

    public void Reload()
    {
        GD.Print(reloadTimer.WaitTime);
        animPlayer.Play("Global/reload",-1,1/reloadSpeed,false);
        ableToShoot = false;
        reloadTimer.Start(reloadSpeed);
        reloadAudio.Play();
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
        double verticalRecoil = GD.RandRange(1.2,1.5) * (MathF.PI/180);
        Node3D unRecoiledGun = this;
        Node3D recoiledGun = this;
        recoiledGun.RotateX((float)verticalRecoil);
        recoiledGun.RotateY((float)horizontalRecoil);
        Transform = unRecoiledGun.Transform.InterpolateWith(recoiledGun.Transform, recoilTimer);
    }

    public void ReturnToCenter(double delta)
    {
        
    }

    public void OnFirerateTimerTimeout()
    {
        if (jammed == JamsState.ProperOperation)
        {
            ableToShoot = true;
        }
    }

    public void OnReloadTimerTimeout()
    {
        roundsInMag = magazineSize;
        ableToShoot = true;
        GD.Print("Reload");
    }

    public void Jam()
    {
        float randomFloat = GD.Randf();

        if (randomFloat > durability)
        {
            ableToShoot = false;
            int jamType = GD.RandRange(1,(Enum.GetNames(typeof(JamsState)).Length-1));
            jammed = (JamsState)jamType;
            //play jammed effects
            if (jammed == JamsState.EjectFailure)
            {
                //Display stuck cartridge
            }
            GD.Print(jammed);
        }
    }

    public void Unjam()
    {
        ableToShoot = false;
        unjamTimer.Start(chamberSpeed);
        reloadAudio.Play();
    }

    public void OnUnjamTimerTimeout()
    {
        switch (jammed)
            {
                case JamsState.FeedFailure:
                    //Play cycle animation (NO CASE EJECTION)
                    break;
                case JamsState.Misfire:
                    //Play cycle animation (CASE EJECTION)
                    roundsInMag--;
                    break;
                case JamsState.EjectFailure:
                    //Play cycle animation (CASE EJECTION)
                    break;
            }
        ableToShoot = true;
        jammed = JamsState.ProperOperation;
    }

    public void Cycle()
    {
        ableToShoot = false;
        cycleTimer.Start(chamberSpeed);
        reloadAudio.Play();
    }

    public void OnCycleTimerTimeout()
    {
        if(roundsInMag > 0)
        {
            if(!chambered)
            {
                chambered = true;
                roundsInMag--;
                //Play cycle animation
            }
            else
            {
                roundsInMag--;
                //Play cycle animation with case ejection
            }
        }
        GD.Print("Round in magazine: " + roundsInMag);
        ableToShoot = true;
    }

    
}
