using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkEventSignals
{
    public enum EChunkExpandDir { rowBack, rowFront, colBack, colFront }
    public delegate void NewChunkExploredEventHandler(EChunkExpandDir _dir);
    public static event NewChunkExploredEventHandler OnNewChunkExplored;
    public static void DoNewChunkExplored(EChunkExpandDir _dir) { if (OnNewChunkExplored != null) OnNewChunkExplored(_dir); }

    public delegate void ChunkUpdatedEventHandler();
    public static event ChunkUpdatedEventHandler OnChunkUpdated;
    public static void DoChunkUpdated() { if (OnChunkUpdated != null) OnChunkUpdated(); }
}
