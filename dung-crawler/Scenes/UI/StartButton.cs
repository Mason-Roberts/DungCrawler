using Godot;
using System;

public partial class StartButton : Button
{
	public void _on_button_up()
	{
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/StaticDungeon/StaticDungeon.tscn");
	}
}
