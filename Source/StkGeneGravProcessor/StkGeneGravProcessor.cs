using Multiplayer.API;
using Verse;

namespace StkGeneGravProcessor;

[StaticConstructorOnStartup]
public static class StkGeneGravProcessor_MP
{
	static StkGeneGravProcessor_MP()
	{
		if (MP.enabled)
			MP.RegisterAll();
	}
}