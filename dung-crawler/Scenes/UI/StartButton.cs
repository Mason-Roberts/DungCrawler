using Godot;
using System;

public partial class StartButton : Button
{
	public void _on_button_up()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Test/TestDungeon3.tscn");
	}
}
