using Godot;
using System;

public partial class Menu : Node
{
	[Export]
	public Control UI;
	[Export]
	public Node LevelContainer; 
	[Export]
	public PackedScene LevelScene; 
	[Export]
	public LineEdit LineEdit;
	[Export]
	private Label StatusLabel; 
	[Export]
	private HBoxContainer HostHBox;
	[Export]
	private HBoxContainer NotConnectedHBox;

	public override void _Ready()
	{
		Multiplayer.ConnectionFailed += _OnConnectionFailed;
		Multiplayer.ConnectedToServer += _OnConnectedToServer;
	}

	private void _OnConnectedToServer()
	{
		StatusLabel.Text = "Connected!";
	}
	
	private void _OnConnectionFailed()
	{
		StatusLabel.Text = "连接失败";
	}

	public void ChangeLevel(PackedScene _scene)
	{
		foreach (var child in LevelContainer.GetChildren())
		{
			LevelContainer.RemoveChild(child);
			child.QueueFree();
		}

		LevelContainer.AddChild(_scene.Instantiate());
	}
	
	private void _on_start_btn_pressed()
	{
		Rpc(nameof(HideMenu));
		CallDeferred( nameof(ChangeLevel), LevelScene);
	}

	private void _on_host_btn_pressed()
	{
		Lobby.Instance.CreateGame(Int32.Parse(LineEdit.Text.Split(':')[1]));
		NotConnectedHBox.Hide();
		HostHBox.Show();
		StatusLabel.Text = "房主中";
	}

	private void _on_join_btn_pressed()
	{
		HostHBox.Hide();
		NotConnectedHBox.Hide();
		var strings = LineEdit.Text.Split(':');
		Lobby.Instance.JoinGame(strings[0],int.Parse(strings[1]));
		StatusLabel.Text = "加入中";
	}

	[Rpc(MultiplayerApi.RpcMode.Authority,CallLocal = true,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void HideMenu()
	{
		UI.Hide();
	}
}
