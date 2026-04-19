using System;
using System.Collections.Generic;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace StkGeneGravProcessor;

public class CompPowerLevel : ThingComp
{
	public CompProperties_PowerLevel Props => (CompProperties_PowerLevel)props;
	public int PowerLevel = 1;
	private CompPowerTrader powerComp;
	public virtual float PowerUsage => Props.firstLevelConsumption * PowerLevel * PowerScaling;
	public virtual float PowerScaling => Props.ScalingEnabled ? (float)Math.Pow(1.025, PowerLevel - 1) : 1f;
	public int ComplexityBonus => Props.ComplexityPerLevel * PowerLevel;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		powerComp = parent.GetComp<CompPowerTrader>();
		UpdatePower();
	}
	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref PowerLevel, "PowerLevel", 1, false);
		UpdatePower();
	}

	private void UpdatePower()
	{
		if (powerComp != null)
		{
			powerComp.PowerOutput = -PowerUsage;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		// Only show gizmo on the first selected stand with this comp
		if (!IsFirstSelectedProcessor())
			yield break;

		if (powerComp != null)
		{
			yield return new Command_Action
			{
				action = DropLevel,
				defaultLabel = "stkLowerPowerLevel".Translate(),
				defaultDesc = "stkLowerPowerLevelDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower")
			};
			yield return new Command_Action
			{
				action = RaiseLevel,
				defaultLabel = "stkRaisePowerLevel".Translate(),
				defaultDesc = "stkRaisePowerLevelDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise")
			};
		}
		yield break;
	}

	[SyncMethod(SyncContext.None)]
	public void RaiseLevel()
	{
		if (PowerLevel < Props.PowerLevels)
		{
			PowerLevel++;
			UpdatePower();
		}
	}
	[SyncMethod(SyncContext.None)]
	public void DropLevel()
	{
		if (PowerLevel > 1)
		{
			PowerLevel--;
			UpdatePower();
		}
	}
	private bool IsFirstSelectedProcessor()
	{
		foreach (var selected in Find.Selector.SelectedObjects)
		{
			if (selected is Building building && building.GetComp<CompPowerLevel>() != null)
				return building == parent; // true if this is the first
		}
		return false;
	}
}

public class CompProperties_PowerLevel : CompProperties
{
	public int PowerLevels = 25;
	public int ComplexityPerLevel = 2;
	public float firstLevelConsumption = 50f;
	public bool ScalingEnabled = false;

	public CompProperties_PowerLevel()
	{
		compClass = typeof(CompPowerLevel);
	}
}