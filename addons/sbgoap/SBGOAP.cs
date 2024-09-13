#if TOOLS
using Godot;
using System;
using project1.addons.sbgoap;
using project1.addons.sbgoap.ai;
using project1.addons.sbgoap.ai.schedule;
using Brain = project1.addons.sbgoap.ai.Brain;

[Tool]
public partial class SBGOAP : EditorPlugin
{
	public static Func<Brain, ulong> GameTimeProvider = brain => Time.GetTicksMsec();
	public static Func<Brain, ulong> DayTimeProvider = brain => Time.GetTicksMsec() % 24000;
	

	public override void _EnterTree()
	{
		// Initialize the registry
		InitializeRegistry();

		// Add the custom types
		AddCustomType("Brain", "Node", 
			GD.Load<Script>("res://addons/sbgoap/ai/Brain.cs"), null);
	}


	public override void _ExitTree()
	{
		RemoveCustomType("Brain");
	}

	private void InitializeRegistry()
	{
		GOAPRegistry.RegisterSchedule("Empty", new Schedule());
		GOAPRegistry.RegisterActivity("Idle");
	}
}
#endif
