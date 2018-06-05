using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Generator;

public class Chunk : MonoBehaviour
{
    public enum EBiomeType { forest = 0, desert, tundra, count }
    public enum ETempObjType { tempItem = 0, wanderer, patroller, prey, count }

    public EBiomeType BiomeType { get; set; }
    public int XOffset { get; set; }
    public int ZOffset { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }

    int length;
    public int Length {
        get { return length; }
        set { Blocks = new Block[length = value, value]; }
    }
    public Block[,] Blocks { get; private set; }

    public List<Block> traversableBlocks = new List<Block>();

    Perlin elevationNoiseGen = new Perlin();
    float elevationMultiplier = 15.0f;

    public Chunk Generate()
    {
        switch (BiomeType) {
            case EBiomeType.forest:
                for (int j = 0; j < Length; ++j) {
                    for (int i = 0; i < Length; ++i) {
                        var block = Instantiate(ChunkLoader.Instance.prefabForestGrassBlock).GetComponent<Block>();
                        Blocks[i, j] = block;
                        block.Chunk = this;
                        block.Row = i;
                        block.Col = j;
                        block.transform.SetParent(transform);
                        block.transform.localPosition = new Vector3(
                            (j - Length / 2) * ChunkLoader.Instance.BlockLength
                            , 0
                            , (i - Length / 2) * ChunkLoader.Instance.BlockLength);

                        if (!block.NonTraversable)
                            traversableBlocks.Add(block);
                    }
                }
                break;
            case EBiomeType.desert:
                for (int j = 0; j < Length; ++j) {
                    for (int i = 0; i < Length; ++i) {
                        var block = Instantiate(ChunkLoader.Instance.prefabSandBlock).GetComponent<Block>();
                        Blocks[i, j] = block;
                        block.Chunk = this;
                        block.Row = i;
                        block.Col = j;
                        block.transform.SetParent(transform);
                        block.transform.localPosition = new Vector3(
                            (j - Length / 2) * ChunkLoader.Instance.BlockLength
                            , 0
                            , (i - Length / 2) * ChunkLoader.Instance.BlockLength);

                        if (!block.NonTraversable)
                            traversableBlocks.Add(block);
                    }
                }
                break;
            case EBiomeType.tundra:
                for (int j = 0; j < Length; ++j) {
                    for (int i = 0; i < Length; ++i) {
                        var block = Instantiate(ChunkLoader.Instance.prefabTundraGrassBlock).GetComponent<Block>();
                        Blocks[i, j] = block;
                        block.Chunk = this;
                        block.Row = i;
                        block.Col = j;
                        block.transform.SetParent(transform);
                        block.transform.localPosition = new Vector3(
                            (j - Length / 2) * ChunkLoader.Instance.BlockLength
                            , 0
                            , (i - Length / 2) * ChunkLoader.Instance.BlockLength);

                        SpawnTree(block, 1);

                        if (!block.NonTraversable)
                            traversableBlocks.Add(block);
                    }
                }
                break;
            default:
                break;
        }

        var chunkOffset = new Vector2(ZOffset, XOffset);
        if (ChunkLoader.Instance.savedChunks.ContainsKey(chunkOffset)) {
            foreach (var pos2ListPair in ChunkLoader.Instance.savedChunks[chunkOffset]) {
                foreach (var objType in pos2ListPair.Value) {
                    var tempObj = CreateTempObj(objType);
                    if (tempObj != null) {
                        var block = Blocks[(int)pos2ListPair.Key[0], (int)pos2ListPair.Key[1]];
                        block.Occupied = true;
                        tempObj.AttachedBlock = block;
                    }
                }
            }
            ChunkLoader.Instance.savedChunks.Remove(chunkOffset);
        }

        if (!ChunkLoader.Instance.encounteredChunks.Contains(chunkOffset)) {
            var randomBlock = traversableBlocks[UnityEngine.Random.Range(0, traversableBlocks.Count)];
            if (!randomBlock.Occupied) {
                var randomRoll = UnityEngine.Random.Range(0, 100);
                var objType
                    = (randomRoll < 10)
                        ? ETempObjType.patroller
                        : (randomRoll < 30)
                            ? ETempObjType.wanderer
                            : ETempObjType.prey;
                var tempObj = CreateTempObj(objType);
                randomBlock.Occupied = true;
                tempObj.AttachedBlock = randomBlock;
            }
        }

        return this;
    }

    bool SpawnTree(Block _block, int _sparsity)
    {
        int xBlockOffset = _block.Row + XOffset * Length
            , zBlockOffset = _block.Col + ZOffset * Length;

        var maxNoise = double.MinValue;
        for (int x = xBlockOffset - _sparsity; x <= xBlockOffset; ++x) {
            for (int z = zBlockOffset - _sparsity; z <= zBlockOffset; ++z) {
                var currentNoise = elevationNoiseGen.GetValue(x + XOffset * Length, 0.1, z + ZOffset * Length);
                if (maxNoise < currentNoise)
                    maxNoise = currentNoise;
            }
        }
        if (maxNoise == elevationNoiseGen.GetValue(xBlockOffset + XOffset * Length, 0.1, zBlockOffset + ZOffset * Length)) {
            var treeObj = Instantiate(ChunkLoader.Instance.prefabTree);
            treeObj.transform.position = _block.transform.position
                + Vector3.up * (_block.transform.localScale[1] / 2 + transform.localScale[1] / 2);
            treeObj.transform.SetParent(transform);
            _block.NonTraversable = true;
            return true;
        }
        return false;
    }

    public ITempObj CreateTempObj(ETempObjType _objType)
    {
        if (_objType == ETempObjType.tempItem) {
            var tempObj = Instantiate(ChunkLoader.Instance.prefabCarrot).GetComponent<TempItem>();
            tempObj.transform.SetParent(transform);
            return tempObj;
        }
        else if (_objType == ETempObjType.wanderer) {
            var tempObj = Instantiate(ChunkLoader.Instance.prefabWanderer).GetComponent<Wanderer>();
            return tempObj;
        }
        else if (_objType == ETempObjType.patroller) {
            var tempObj = Instantiate(ChunkLoader.Instance.prefabPatroller).GetComponent<Wanderer>();
            return tempObj;
        }
        else if (_objType == ETempObjType.prey) {
            var tempObj = Instantiate(ChunkLoader.Instance.prefabPrey).GetComponent<Prey>();
            return tempObj;
        }
        return null;
    }
}
