using Godot;
using System;

public partial class Level : Node2D
{
	[Export]
	private Camera2D m_Camera2D;
	
	[Export]
	public Node2D PlayerContainer;

	[Export]
	public PackedScene PlayerScene;

	[Export]
	public CollisionShape2D SpawnArea;

	public override void _Ready()
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}

		foreach (var peer in Multiplayer.GetPeers())
		{
			AddPlayer(id: peer);
		}
		AddPlayer(1);
		Multiplayer.PeerDisconnected += DeletePlayer;
		Multiplayer.PeerConnected += AddPlayer;
	}

	public override void _ExitTree()
	{
		if (!Multiplayer.IsServer())
		{
			return;
		}
		
		Multiplayer.PeerDisconnected -= DeletePlayer;
		Multiplayer.PeerConnected -= AddPlayer;
	}

	private void AddPlayer(long id)
	{
		var plane = PlayerScene.Instantiate<Plane>();
		plane.Name = id.ToString();
		plane.GlobalPosition = GetSpawnPoint();
		PlayerContainer.AddChild(plane);
		// var info = Lobby.Instance.Players[id];
	}

	public Vector2 GetSpawnPoint()
	{
		var size = SpawnArea.Shape.GetRect();
		var x = GD.RandRange(size.Position.X, size.End.X);
		var y = GD.RandRange(size.Position.Y, size.End.Y);
	
		return new Vector2((float)x, (float)y)+SpawnArea.GlobalPosition;
	}
	
	private void DeletePlayer(long id)
	{
		if (!PlayerContainer.HasNode(id.ToString()))
		{
			return;
		}
		PlayerContainer.GetNode(id.ToString()).QueueFree();
	}
}
