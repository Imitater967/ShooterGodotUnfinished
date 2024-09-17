using Godot;
using System;
using Weapon.scripts;

public partial class Bullet : Area2D
{ 

	[Export]
	private DamageType _DamageType = DamageType.Red;

	private Vector2 _Direction;
	private float _Speed = 800;
	private const float RotOffset = 90;

	public override async void _Ready()
	{
		await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout); 
		QueueFree();
	}

	public void SetDirection(Vector2 dir)
	{
		_Direction = dir;
		Rotation =GlobalPosition.AngleToPoint(GlobalPosition + _Direction) + Mathf.DegToRad(RotOffset);
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += _Direction * _Speed * (float)delta;
	}

	private void _on_body_entered(Node2D body)
	{ 
	}
}
