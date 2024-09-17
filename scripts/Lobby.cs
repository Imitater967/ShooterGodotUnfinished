using Godot;
using System;
using System.Collections.Generic;
using Weapon.scripts;

public partial class Lobby : Node
{
	public static Lobby Instance { get; private set; }
	
	[Signal]
	public delegate void OnPlayerConnectedEventHandler(int peer_id, PlayerInfo player_info);

	[Signal]
	public delegate void OnPlayerDisconnectedEventHandler(int peer_id);

	[Signal]
	public delegate void OnServerDisconnectedEventHandler();

	public Dictionary<long,PlayerInfo> Players = new Dictionary<long, PlayerInfo>();

	private PlayerInfo m_PlayerInfo = new PlayerInfo()
	{
		Name = "Imitater967"
	};


	public override void _Ready()
	{
		Instance = this;
		Multiplayer.PeerConnected += _OnPlayerConnected;
		Multiplayer.ConnectedToServer += _OnConnectedToServer;
		Multiplayer.PeerDisconnected += _OnPlayerDisconnected;
		Multiplayer.ServerDisconnected += _OnServerDisconnected;
		Multiplayer.ConnectionFailed += _OnConnectionFail;
	}

	private void _OnConnectedToServer()
	{
		var player_id = Multiplayer.GetUniqueId();
		Players[player_id] = m_PlayerInfo;
		EmitSignal("OnPlayerConnected", player_id, m_PlayerInfo);
	}

	private void _OnPlayerDisconnected(long _id)
	{
		Players.Remove(_id);
		EmitSignal("OnPlayerDisconnected", _id);
	}

	private void _OnServerDisconnected()
	{
		Multiplayer.SetMultiplayerPeer(null);
		EmitSignal("OnServerDisconnected");
		Players.Clear();
	}

	private void _OnConnectionFail()
	{
		Multiplayer.SetMultiplayerPeer(null);
	}

	private void _OnPlayerConnected(long _id)
	{
		RpcId(_id, "_RegisterPlayer", m_PlayerInfo);
	}

	
	/// <summary>
	/// Client send 2 server
	/// </summary>
	/// <param name="_newInfo"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void _RegisterPlayer(string _playerName)
	{
		var newPlayerId = Multiplayer.GetRemoteSenderId();
		var playerInfo = new PlayerInfo();
		playerInfo.Name = _playerName;
		Players[newPlayerId] = playerInfo;
		EmitSignal("OnPlayerConnected", newPlayerId, playerInfo);
	}

	public Error JoinGame(string address,int port)
	{
		var peer = new ENetMultiplayerPeer();
		var error = peer.CreateClient(address, port);
		if (error != Error.Ok)
		{
			return error;
		}
		Multiplayer.SetMultiplayerPeer(peer);
		return Error.Ok;
	}
	public Error CreateGame(int port)
	{
		var peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port);
		if (error != Error.Ok)
		{
			return error;
		}
		Multiplayer.SetMultiplayerPeer(peer);
		Players.Clear();
		Players.Add(1,m_PlayerInfo);
		EmitSignal("OnPlayerConnected", 1, m_PlayerInfo);
		
		return Error.Ok;
	}
}
