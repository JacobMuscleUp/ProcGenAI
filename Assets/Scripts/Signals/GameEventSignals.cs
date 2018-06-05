using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEventSignals 
{
	public delegate void PreySpawnedEventHandler(Prey _prey);
	public static event PreySpawnedEventHandler OnPreySpawned;
	public static void DoPreySpawned(Prey _prey) { if (OnPreySpawned != null) OnPreySpawned(_prey); }

	public delegate void PreyDespawnedEventHandler(Prey _prey);
	public static event PreyDespawnedEventHandler OnPreyDespawned;
	public static void DoPreyDespawned(Prey _prey) { if (OnPreyDespawned != null) OnPreyDespawned(_prey); }
}
