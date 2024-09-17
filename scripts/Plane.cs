using Godot;
using System;
using Weapon.scripts;

public partial class Plane: CharacterBody2D
{
    [Export]
    private PackedScene _Camera;
    [Export]
    private Node2D _ModelRoot; 
    [Export]
    private Node2D _Muzzle;
    [Export]
    private Timer _ShootTimer;
    [Export]
    private PackedScene _BlueAmmo;
    [Export]
    private Node2D _BlueModel;
    [Export]
    private PackedScene _RedAmmo;
    [Export]
    private Node2D _RedModel;
    [Export]
    private PackedScene _GreenAmmo;
    [Export]
    private Node2D _GreenModel; 
    private float m_Rotation;
    private bool m_CanShoot = true;
    private int m_OwnerId;
    private const float RotDegreeOffset = 90;
    public const float Speed = 300.0f;
    public const float Accleration = 7.5f;
    public const float RotSpeed = 0.075f;
    public const float DashVelocity = 700.0f;
    
    private DamageType DamageType = DamageType.Blue;
    private Camera2D _CamInstance;

    public override void _EnterTree()
    {
        m_OwnerId = int.Parse(Name);
        //Can't call on Ready
        SetMultiplayerAuthority(m_OwnerId);
        if (IsLocalPlayer())
        {
            CallDeferred(nameof(InitCamera));
        }
    }

    private bool IsLocalPlayer()
    {
        return m_OwnerId == Multiplayer.GetUniqueId();
    }

    private void InitCamera()
    {
        _CamInstance = _Camera.Instantiate<Camera2D>();
        GetTree().Root.AddChild(_CamInstance);
    }

    private void _on_shoot_timer_timeout()
    {
        m_CanShoot = true;
        _ShootTimer.Stop();
    }

    public override void _Process(double delta)
    {
        if (IsLocalPlayer())
        {
            _CamInstance.GlobalPosition = GlobalPosition;
        }

        if (IsLocalPlayer())
        {
            DetermineWeapon();
        }

        if (IsLocalPlayer())
        {
            SimpleShoot();
        }
    }

    private void SimpleShoot()
    {
        if (Input.IsActionPressed("action_shoot"))
        {
            Rpc(nameof(_ShootRpc));
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority,CallLocal = true),]
    private void _ShootRpc()
    {
        switch (DamageType)
        {
            case DamageType.Blue:
                _Shoot(_BlueAmmo);
                break;
            case DamageType.Green:
                _Shoot(_GreenAmmo);
                break;
            case DamageType.Red:
                _Shoot(_RedAmmo);
                break;
        }
    }

    private void _Shoot(PackedScene _bullet)
    {
        if (m_CanShoot)
        {
            m_CanShoot = false;
            _ShootTimer.Start();
            var bulletNode = _bullet.Instantiate<Bullet>();
            var dir = _Muzzle.GlobalPosition - GlobalPosition;
            dir = dir.Normalized();
            GetTree().Root.AddChild(bulletNode);
            bulletNode.GlobalPosition = _Muzzle.GlobalPosition;
            bulletNode.SetDirection(dir); 
        }
    }

    private void DetermineWeapon()
    {
        if (Input.IsActionJustPressed("weapon_blue"))
        {
            Rpc(nameof(SetDamageType), (int)DamageType.Blue); 
        }else if (Input.IsActionJustPressed("weapon_red"))
        {
            Rpc(nameof(SetDamageType), (int)DamageType.Red); 
        }else if (Input.IsActionJustPressed("weapon_green"))
        {
            Rpc(nameof(SetDamageType), (int)DamageType.Green); 
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority,CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetDamageType(int damagetType)
    {
        SetDamageType((DamageType)damagetType);
    }
    
    public void SetDamageType(DamageType _damageType)
    {
        if (_damageType == DamageType)
        {
            return;
        }

        switch (_damageType)
        {
            case DamageType.Blue:
                _RedModel.Hide();
                _GreenModel.Hide();
                _BlueModel.Show();
                break;
            case DamageType.Green:
                _RedModel.Hide();
                _GreenModel.Show();
                _BlueModel.Hide();
                break;
            case DamageType.Red:
                _RedModel.Show();
                _GreenModel.Hide();
                _BlueModel.Hide();
                break;
        }
        DamageType = _damageType;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsLocalPlayer())
        {
            return;
        }
        Vector2 velocity = Velocity;
        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        var offsetRad = Mathf.DegToRad(RotDegreeOffset);

        UpdateRotation();

        void UpdateRotation()
        {
            if (direction != Vector2.Zero)
            {
                float targetRadian = direction.Angle() + offsetRad;
                m_Rotation = Mathf.LerpAngle(_ModelRoot.Rotation, targetRadian, RotSpeed);
                _ModelRoot.Rotation = m_Rotation;
            }
        }

        UpdateVelocity();

        void UpdateVelocity()
        {
            var forward = Vector2.FromAngle(m_Rotation - offsetRad);

            if (direction == Vector2.Zero)
            {
                velocity.Y = Mathf.MoveToward(Velocity.Y,0,Accleration);
                velocity.X = Mathf.MoveToward(Velocity.X,0,Accleration);
            }
            else
            {
                velocity.Y = Mathf.MoveToward(Velocity.Y,forward.Y*Speed,Accleration);
                velocity.X = Mathf.MoveToward(Velocity.X,forward.X*Speed,Accleration);
            }

            if (Input.IsActionJustPressed("action_dash"))
            { 
                velocity.X = forward.X * DashVelocity;
                velocity.Y = forward.Y * DashVelocity;
            }

            Velocity = velocity;
        }

        MoveAndSlide();
    }
}