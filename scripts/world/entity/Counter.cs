using Godot;
using System;

namespace project1.scripts.world.entity;

public partial class Counter : Node
{
	/// <summary>
	/// If the counter is not active than it supposed to be hidden
	/// and not to be updated, like in the case of a health bar
	/// that is not visible when entity is invincible (no health)
	/// </summary>
	[Export]
	public bool Active { get; set; } = true;
	
	private int cap = 100;
	
	/// <summary>
	/// The total value of the counter that can be reached
	/// </summary>
	[Export]
	public int Cap
	{
		get => cap;
		set
		{
			cap = Mathf.Max(0, value);
			this.value = Mathf.Min(GetTotalCap(), value);
		}
	}
	
	private int cap_adjustment = 0;
	
	/// <summary>
	/// The value that added to counter cap (max value)
	/// due to buffs or debuffs what supposed to be removed
	/// and usually adjustment somehow displayed to the player
	/// </summary>
	[Export]
	public int CapAdjustment
	{
		get => cap_adjustment;
		set
		{
			cap_adjustment = value;
			this.value = Mathf.Min(GetTotalCap(), value);
		}
	}
	
	private int value = 100;
	/// <summary>
	/// The current value of the counter
	/// </summary>
	[Export]
	public int Value
	{
		get => value;
		set
		{
			this.value = Mathf.Max(0, Mathf.Min(GetTotalCap(), value));
		}
	}

	/// <summary>
	/// The value that added to counter current value
	/// </summary>
	public int GetTotalCap()
	{
		return cap + cap_adjustment;
	}

	/// <summary>
	/// Sets the value of the counter to the maximum
	/// </summary>
	public void Fill()
	{
		value = GetTotalCap();
	}
}
