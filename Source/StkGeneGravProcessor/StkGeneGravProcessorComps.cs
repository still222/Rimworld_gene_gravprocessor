using System.Collections.Generic;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace StkGeneGravProcessor;

public class CompPowerLevel : ThingComp
{
	private CompPowerTrader powerComp;
	public int PowerLevel = 1;
	public CompProperties_PowerLevel Props => (CompProperties_PowerLevel)props;
	public virtual float PowerUsage => Props.firstLevelConsumption * PowerLevel * PowerScaling;
	public virtual float PowerScaling => Props.ScalingEnabled ? Mathf.Pow(1.025f, PowerLevel - 1) : 1f;
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
			powerComp.PowerOutput = -PowerUsage;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		// Only show gizmo on the first selected stand with this comp
		if (!IsFirstSelectedProcessor())
			yield break;

		// Collect all selected gravprocessors with this comp
		var comps = new List<CompPowerLevel>();

		foreach (var obj in Find.Selector.SelectedObjects)
		{
			if (obj is Building building)
			{
				var comp = building.GetComp<CompPowerLevel>();
				if (comp != null)
				{
					comps.Add(comp);
				}
			}
		}

		if (powerComp != null && comps.Count > 0)
		{
			yield return new Command_Action
			{
				action = () => DropLevel(comps),
				defaultLabel = "stkLowerPowerLevel".Translate(),
				defaultDesc = "stkLowerPowerLevelDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower")
			};

			yield return new Command_Action
			{
				action = () => RaiseLevel(comps),
				defaultLabel = "stkRaisePowerLevel".Translate(),
				defaultDesc = "stkRaisePowerLevelDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise")
			};

		}
	
		yield break;
	}

	private bool IsFirstSelectedProcessor()
	{
		foreach (var selected in Find.Selector.SelectedObjects)
			if (selected is Building building && building.GetComp<CompPowerLevel>() != null)
				return building == parent; // true if this is the first

		return false;
	}

	protected void ThrowCurrentOverclockText()
	{
		MoteMaker.ThrowText(
			parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f),
			parent.Map,
			ComplexityBonus.ToString("+0;-0;0"),
			Color.white
		);

	}

	[SyncMethod]
	public static void RaiseLevel(List<CompPowerLevel> list)
	{
		SoundDefOf.DragSlider.PlayOneShotOnCamera();
		foreach (var comp in list)
		{
			if (comp.PowerLevel < comp.Props.PowerLevels)
			{
				comp.PowerLevel++;
				comp.UpdatePower();
			}
			
			comp.ThrowCurrentOverclockText();
		}

	}

	[SyncMethod]
	public static void DropLevel(List<CompPowerLevel> list)
	{
		SoundDefOf.DragSlider.PlayOneShotOnCamera();
		foreach (var comp in list)
		{
			if (comp.PowerLevel > 1)
			{
				comp.PowerLevel--;
				comp.UpdatePower();
			}
			
			comp.ThrowCurrentOverclockText();
		}

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
