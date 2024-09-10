#if TOOLS
using Godot;
using System;

[Tool]
public partial class SBGOAP : EditorPlugin
{
	public override void _EnterTree()
	{
		// Brain Root Node
		var script = GD.Load<Script>("res://addons/sbgoap/ai/Brain.cs");
		var texture = GD.Load<Texture2D>("res://addons/sbgoap/icons/brain.svg");
		AddCustomType("Brain", "Node", script, texture);
		
		// Schedules Node
		script = GD.Load<Script>("res://addons/sbgoap/ai/schedule/Schedules.cs");
		texture = GD.Load<Texture2D>("res://addons/sbgoap/icons/schedule.svg");
		AddCustomType("Schedules", "Node", script, texture);
		
		// Sensors Node
		script = GD.Load<Script>("res://addons/sbgoap/ai/sensor/Sensors.cs");
		texture = GD.Load<Texture2D>("res://addons/sbgoap/icons/ringing-bell.svg");
		AddCustomType("Sensors", "Node", script, texture);
		
		// Memories Node
		script = GD.Load<Script>("res://addons/sbgoap/ai/memory/Memories.cs");
		texture = GD.Load<Texture2D>("res://addons/sbgoap/icons/bookshelf.svg");
		AddCustomType("Memories", "Node", script, texture);
		
		// Memory Node
		script = GD.Load<Script>("res://addons/sbgoap/ai/memory/MemoryNode.cs");
		texture = GD.Load<Texture2D>("res://addons/sbgoap/icons/book.svg");
		AddCustomType("Memory", "Node", script, texture);
		
		// Expirable Memory Node
		script = GD.Load<Script>("res://addons/sbgoap/ai/memory/ExpirableMemoryNode.cs");
		texture = GD.Load<Texture2D>("res://addons/sbgoap/icons/book.svg");
		AddCustomType("Expirable Memory", "Node", script, texture);
		
		// Behaviors Node
		script = GD.Load<Script>("res://addons/sbgoap/ai/behavior/Behaviors.cs");
		texture = GD.Load<Texture2D>("res://addons/sbgoap/icons/man-standing.svg");
		AddCustomType("Behaviors", "Node", script, texture);
	}

	public override void _ExitTree()
	{
		RemoveCustomType("Brain");
		RemoveCustomType("Schedules");
		RemoveCustomType("Sensors");
		RemoveCustomType("Memory");
		RemoveCustomType("Expirable Memory");
		RemoveCustomType("Memories");
		RemoveCustomType("Behaviors");
	}
}
#endif
