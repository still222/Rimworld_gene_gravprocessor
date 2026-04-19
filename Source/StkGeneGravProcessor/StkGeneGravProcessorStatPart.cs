using RimWorld;
using Verse;

namespace StkGeneGravProcessor;

public class StatPart_GravComplexity : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing.TryGetComp<CompPowerLevel>() is { } comp)
		{
			val += comp.ComplexityBonus;
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing.TryGetComp<CompPowerLevel>() is { } comp)
		{
			return $"Grav Processor bonus: +{comp.ComplexityBonus}";
		}
		return null;
	}
}