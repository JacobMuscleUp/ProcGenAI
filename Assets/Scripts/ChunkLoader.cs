using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Generator;

public class ChunkLoader : MonoBehaviour
{
    public static ChunkLoader Instance { get; set; }

    public GameObject prefabForestGrassBlock;
    public GameObject prefabSandBlock;
    public GameObject prefabTundraGrassBlock;
    public GameObject prefabTree;
    public GameObject prefabCarrot;
    public GameObject prefabWanderer;
    public GameObject prefabPatroller;
    public GameObject prefabPrey;

    public int loadChunkDistance;
    public int chunkSize;
    public float blockSeparation;

    public List<List<Chunk>> chunks = new List<List<Chunk>>();
    public Dictionary<Vector2, Dictionary<Vector2, List<Chunk.ETempObjType>>> savedChunks
        = new Dictionary<Vector2, Dictionary<Vector2, List<Chunk.ETempObjType>>>();
    public HashSet<Vector2> encounteredChunks = new HashSet<Vector2>();
    public float BlockLength { get; private set; }

    Perlin elevationNoiseGen = new Perlin();
    Perlin moistureNoiseGen = new Perlin();

    void Awake()
    {
        Instance = this;
        
        ChunkEventSignals.OnNewChunkExplored += OnNewChunkExplored;
    }

	void Start()
	{
        moistureNoiseGen.Seed = (elevationNoiseGen.Seed = Random.Range(0, int.MaxValue)) + 1;

		BlockLength = prefabForestGrassBlock.GetComponent<Renderer>().bounds.extents.x * 2;
		BlockLength += blockSeparation;    

		for (int row = 0; row < loadChunkDistance; ++row) {
			var rowChunks = new List<Chunk>();
			for (int col = 0; col < loadChunkDistance; ++col) {
				var chunk = new GameObject().AddComponent<Chunk>();

                var chunkOffset = new Vector2(
                    chunk.ZOffset = chunk.Row = row
                    , chunk.XOffset = chunk.Col = col);
				chunk.BiomeType = GetChunkType(
                    elevationNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1])
                    , moistureNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1]));
                //chunk.type = (Chunk.EType)System.Math.Round(
                //    (elevationNoiseGen.GetValue(col, 0.1, row) / 2 + 0.5) * ((int)Chunk.EType.count - 1));

                chunk.Length = chunkSize;
				chunk.transform.position = new Vector3(
					col * chunkSize * BlockLength
					, 0
					, row * chunkSize * BlockLength);
				chunk.name = string.Format("Chunk[{0}][{1}]", row, col);

				rowChunks.Add(chunk.Generate());
                encounteredChunks.Add(chunkOffset);
            }
			chunks.Add(rowChunks);
		}
		ChunkEventSignals.DoChunkUpdated();
	}

    void OnDestroy()
    {
        ChunkEventSignals.OnNewChunkExplored -= OnNewChunkExplored;
    }

    void OnNewChunkExplored(ChunkEventSignals.EChunkExpandDir _dir)
    {
        Debug.Log(string.Format("chunk[{0}, {1}]", Wolf.Player.Block.Chunk.ZOffset, Wolf.Player.Block.Chunk.XOffset));
        switch (_dir) {
            case ChunkEventSignals.EChunkExpandDir.rowBack:
                PushRowBack();
                break;
            case ChunkEventSignals.EChunkExpandDir.rowFront:
                PushRowFront();
                break;
            case ChunkEventSignals.EChunkExpandDir.colBack:
                PushColumnBack();
                break;
            case ChunkEventSignals.EChunkExpandDir.colFront:
                PushColumnFront();
                break;
            default:
                break;
        }
        ChunkEventSignals.DoChunkUpdated();
    }

    void PushRowFront()
    {
        SaveAndDestroyTempObjs(RowBackEnumerator());
        
        foreach (var chunk in chunks[loadChunkDistance - 1])
            Destroy(chunk.gameObject);
        for (int row = loadChunkDistance - 1; row > 0; --row) {
            chunks[row] = chunks[row - 1];
            foreach (var chunk in chunks[row]) {
                chunk.Row = chunk.Row + 1;
                chunk.name = string.Format("Chunk[{0}][{1}]", chunk.Row, chunk.Col);
            }
        }

        var rowChunks = new List<Chunk>();
        foreach (var oldChunk in chunks[0]) {
            var newChunk = new GameObject().AddComponent<Chunk>();

            var chunkOffset = new Vector2(
                newChunk.ZOffset = oldChunk.ZOffset - 1
                , newChunk.XOffset = oldChunk.XOffset);
            newChunk.BiomeType = GetChunkType(
                elevationNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1])
                , moistureNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1]));

            newChunk.Row = 0;
            newChunk.Col = oldChunk.Col;
            newChunk.Length = oldChunk.Length;
            newChunk.transform.position = new Vector3(
                oldChunk.transform.position[0]
                , oldChunk.transform.position[1]
                , oldChunk.transform.position[2] - chunkSize * BlockLength);
            newChunk.name = string.Format("Chunk[{0}][{1}]", newChunk.Row, newChunk.Col);

            rowChunks.Add(newChunk.Generate());
            encounteredChunks.Add(chunkOffset);
        }

        chunks[0] = rowChunks;
    }

    void PushRowBack()
    {
        SaveAndDestroyTempObjs(RowFrontEnumerator());

        foreach (var chunk in chunks[0])
            Destroy(chunk.gameObject);
        for (int row = 1; row < loadChunkDistance; ++row) {
            chunks[row - 1] = chunks[row];
            foreach (var chunk in chunks[row]) {
                chunk.Row = chunk.Row - 1;
                chunk.name = string.Format("Chunk[{0}][{1}]", chunk.Row, chunk.Col);
            }
        }

        var rowChunks = new List<Chunk>();
        foreach (var oldChunk in chunks[loadChunkDistance - 1]) {
            var newChunk = new GameObject().AddComponent<Chunk>();

            var chunkOffset = new Vector2(
                newChunk.ZOffset = oldChunk.ZOffset + 1
                , newChunk.XOffset = oldChunk.XOffset);
            newChunk.BiomeType = GetChunkType(
                elevationNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1])
                , moistureNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1]));

            newChunk.Row = loadChunkDistance - 1;
            newChunk.Col = oldChunk.Col;
            newChunk.Length = oldChunk.Length;
            newChunk.transform.position = new Vector3(
                oldChunk.transform.position[0]
                , oldChunk.transform.position[1]
                , oldChunk.transform.position[2] + chunkSize * BlockLength);
            newChunk.name = string.Format("Chunk[{0}][{1}]", newChunk.Row, newChunk.Col);

            rowChunks.Add(newChunk.Generate());
            encounteredChunks.Add(chunkOffset);
        }

        chunks[loadChunkDistance - 1] = rowChunks;
    }

    void PushColumnFront()
    {
        SaveAndDestroyTempObjs(ColumnBackEnumerator());

        foreach (var rowChunk in chunks) {
            Destroy(rowChunk[loadChunkDistance - 1].gameObject);
            for (int col = loadChunkDistance - 1; col > 0; --col) {
                var chunk = rowChunk[col] = rowChunk[col - 1];
                chunk.Col = chunk.Col + 1;
                chunk.name = string.Format("Chunk[{0}][{1}]", chunk.Row, chunk.Col);
            }

            var newChunk = new GameObject().AddComponent<Chunk>();

            var chunkOffset = new Vector2(
                newChunk.ZOffset = rowChunk[0].ZOffset
                , newChunk.XOffset = rowChunk[0].XOffset - 1);
            newChunk.BiomeType = GetChunkType(
                elevationNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1])
                , moistureNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1]));

            newChunk.Row = rowChunk[0].Row;
            newChunk.Col = 0;
            newChunk.Length = chunkSize;
            newChunk.transform.position = new Vector3(
                rowChunk[newChunk.Col].transform.position[0] - chunkSize * BlockLength
                , rowChunk[newChunk.Col].transform.position[1]
                , rowChunk[newChunk.Col].transform.position[2]);
            newChunk.name = string.Format("Chunk[{0}][{1}]", newChunk.Row, newChunk.Col);

            rowChunk[newChunk.Col] = newChunk.Generate();
            encounteredChunks.Add(chunkOffset);
        }
    }

    void PushColumnBack()
    {
        SaveAndDestroyTempObjs(ColumnFrontEnumerator());

        foreach (var rowChunk in chunks) {
            Destroy(rowChunk[0].gameObject);
            for (int col = 1; col < loadChunkDistance; ++col) {
                var chunk = rowChunk[col - 1] = rowChunk[col];
                chunk.Col = chunk.Col - 1;
                chunk.name = string.Format("Chunk[{0}][{1}]", chunk.Row, chunk.Col);
            }

            var newChunk = new GameObject().AddComponent<Chunk>();

            var chunkOffset = new Vector2(
                newChunk.ZOffset = rowChunk[0].ZOffset
                , newChunk.XOffset = rowChunk[loadChunkDistance - 1].XOffset + 1);
            newChunk.BiomeType = GetChunkType(
                elevationNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1])
                , moistureNoiseGen.GetValue(chunkOffset[0], 0.1, chunkOffset[1]));

            newChunk.Row = rowChunk[0].Row;
            newChunk.Col = loadChunkDistance - 1;
            newChunk.Length = chunkSize;
            newChunk.transform.position = new Vector3(
                rowChunk[newChunk.Col].transform.position[0] + chunkSize * BlockLength
                , rowChunk[newChunk.Col].transform.position[1]
                , rowChunk[newChunk.Col].transform.position[2]);
            newChunk.name = string.Format("Chunk[{0}][{1}]", newChunk.Row, newChunk.Col);

            rowChunk[newChunk.Col] = newChunk.Generate();
            encounteredChunks.Add(chunkOffset);
        }
    }

    Chunk.EBiomeType GetChunkType(double _elevation, double _moisture)
    {
        if (_elevation < 0) {
            if (_moisture < 0) return Chunk.EBiomeType.desert;
            return Chunk.EBiomeType.forest;
        }
        return Chunk.EBiomeType.tundra;
    }

    void SaveAndDestroyTempObjs(IEnumerable<Chunk> _chunkIter)
    {
        foreach (var tempObj in TempObjManager.Instance.tempObjs) {
            foreach (var chunk in _chunkIter) {
                if (chunk == tempObj.AttachedBlock.Chunk) {
                    var chunkOffset = new Vector2(tempObj.AttachedBlock.Chunk.ZOffset, tempObj.AttachedBlock.Chunk.XOffset);
                    var blockOffset = new Vector2(tempObj.AttachedBlock.Row, tempObj.AttachedBlock.Col);

                    if (!savedChunks.ContainsKey(chunkOffset))
                        savedChunks[chunkOffset] = new Dictionary<Vector2, List<Chunk.ETempObjType>>();
                    if (!savedChunks[chunkOffset].ContainsKey(blockOffset))
                        savedChunks[chunkOffset][blockOffset] = new List<Chunk.ETempObjType>();

                    if (tempObj is Wanderer)
                        savedChunks[chunkOffset][blockOffset].Add(!((Wanderer)tempObj).patrolMode 
                            ? Chunk.ETempObjType.wanderer : Chunk.ETempObjType.patroller);
                    else if (tempObj is TempItem)
                        savedChunks[chunkOffset][blockOffset].Add(Chunk.ETempObjType.tempItem);
                    else if (tempObj is Prey)
                        savedChunks[chunkOffset][blockOffset].Add(Chunk.ETempObjType.prey);

                    Destroy(((MonoBehaviour)tempObj).gameObject);
                }
            }
        }
    }

    IEnumerable<Chunk> RowFrontEnumerator()
    {
        foreach (var chunk in chunks[0])
            yield return chunk;
    }
    IEnumerable<Chunk> RowBackEnumerator()
    {
        foreach (var chunk in chunks[loadChunkDistance - 1])
            yield return chunk;
    }
    IEnumerable<Chunk> ColumnFrontEnumerator()
    {
        foreach (var rowChunk in chunks)
            yield return rowChunk[0];
    }
    IEnumerable<Chunk> ColumnBackEnumerator()
    {
        foreach (var rowChunk in chunks)
            yield return rowChunk[loadChunkDistance - 1];
    }
}
