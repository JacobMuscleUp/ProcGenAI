using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Chunk Chunk { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public bool Occupied { get; set; }
    public bool NonTraversable { get; set; }

    void Awake()
    {
        Occupied = false;
        NonTraversable = false;
    }

    public static int ManhattanDistance(Block _block0, Block _block1)
    {
        var chunkColDiff = _block0.Chunk.Col - _block1.Chunk.Col;
        var chunkRowDiff = _block0.Chunk.Row - _block1.Chunk.Row;
        int colDiff = 0, rowDiff = 0;

        if (chunkColDiff != 0) {
            colDiff += (Mathf.Abs(chunkColDiff) - 1) * ChunkLoader.Instance.chunkSize;
            if (chunkColDiff < 0)
                colDiff += _block0.Chunk.Length - _block0.Col + _block1.Col;
            else if (chunkColDiff > 0)
                colDiff += _block1.Chunk.Length - _block1.Col + _block0.Col;
        }
        else
            colDiff += Mathf.Abs(_block0.Col - _block1.Col);

        if (chunkRowDiff != 0) {
            rowDiff += (Mathf.Abs(chunkRowDiff) - 1) * ChunkLoader.Instance.chunkSize;
            if (chunkRowDiff < 0)
                rowDiff += _block0.Chunk.Length - _block0.Row + _block1.Row;
            else if (chunkRowDiff > 0)
                rowDiff += _block1.Chunk.Length - _block1.Row + _block0.Row;
        }
        else
            rowDiff += Mathf.Abs(_block0.Row - _block1.Row);

        return colDiff + rowDiff;
    }

    public IEnumerable<Block> AdjacentBlocks()
    {
        var bRowMax = (Row == Chunk.Length - 1);
        var bColMax = (Col == Chunk.Length - 1);
        var bRowMin = (Row == 0);
        var bColMin = (Col == 0);

        var bChunkRowMax = (Chunk.Row == ChunkLoader.Instance.loadChunkDistance - 1);
        var bChunkColMax = (Chunk.Col == ChunkLoader.Instance.loadChunkDistance - 1);
        var bChunkRowMin = (Chunk.Row == 0);
        var bChunkColMin = (Chunk.Col == 0);

        if (bChunkRowMax)
            ;
        else if (!bRowMax) {
            if (bChunkColMax)
                ;
            else if (!bColMax)
                yield return Chunk.Blocks[Row + 1, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col + 1].Blocks[Row + 1, 0];

            yield return Chunk.Blocks[Row + 1, Col];

            if (bChunkColMin)
                ;
            else if (!bColMin)
                yield return Chunk.Blocks[Row + 1, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col - 1].Blocks[Row + 1, Chunk.Length - 1];
        }
        else {
            if (bChunkColMax)
                ;
            else if (!bColMax)
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col].Blocks[0, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col + 1].Blocks[0, 0];

            yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col].Blocks[0, Col];

            if (bChunkColMin)
                ;
            else if (!bColMin)
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col].Blocks[0, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col - 1].Blocks[0, Chunk.Length - 1];
        }

        if (bChunkColMin)
            ;
        else if (!bColMin)
            yield return Chunk.Blocks[Row, Col - 1];
        else
            yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col - 1].Blocks[Row, Chunk.Length - 1];

        if (bChunkRowMin)
            ;
        else if (!bRowMin) {
            if (bChunkColMin)
                ;
            else if (!bColMin)
                yield return Chunk.Blocks[Row - 1, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col - 1].Blocks[Row - 1, Chunk.Length - 1];

            yield return Chunk.Blocks[Row - 1, Col];

            if (bChunkColMax)
                ;
            else if (!bColMax)
                yield return Chunk.Blocks[Row - 1, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col + 1].Blocks[Row - 1, 0];
        }
        else {
            if (bChunkColMin)
                ;
            else if (!bColMin)
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col].Blocks[Chunk.Length - 1, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col - 1].Blocks[Chunk.Length - 1, Chunk.Length - 1];

            yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col].Blocks[Chunk.Length - 1, Col];

            if (bChunkColMax)
                ;
            else if (!bColMax)
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col].Blocks[Chunk.Length - 1, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col + 1].Blocks[Chunk.Length - 1, 0];
        }

        if (bChunkColMax)
            ;
        else if (!bColMax)
            yield return Chunk.Blocks[Row, Col + 1];
        else
            yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col + 1].Blocks[Row, 0];

        
        /*
        if (!bRowMax)  {
            if (!bColMax)
                yield return Chunk.Blocks[Row + 1, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col + 1].Blocks[Row + 1, 0];
            yield return Chunk.Blocks[Row + 1, Col];
            if (!bColMin)
                yield return Chunk.Blocks[Row + 1, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col - 1].Blocks[Row + 1, Chunk.Length - 1];
        }
        else {
            if (!bColMax)
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col].Blocks[0, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col + 1].Blocks[0, 0];
            yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col].Blocks[0, Col];
            if (!bColMin)
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col].Blocks[0, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row + 1][Chunk.Col - 1].Blocks[0, Chunk.Length - 1];
        }

        if (!bColMin)
            yield return Chunk.Blocks[Row, Col - 1];
        else
            yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col - 1].Blocks[Row, Chunk.Length - 1];

        if (!bRowMin) {
            if (!bColMin)
                yield return Chunk.Blocks[Row - 1, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col - 1].Blocks[Row - 1, Chunk.Length - 1];
            yield return Chunk.Blocks[Row - 1, Col];
            if (!bColMax)
                yield return Chunk.Blocks[Row - 1, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col + 1].Blocks[Row - 1, 0];
        }
        else {
            if (!bColMin)
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col].Blocks[Chunk.Length - 1, Col - 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col - 1].Blocks[Chunk.Length - 1, Chunk.Length - 1];
            yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col].Blocks[Chunk.Length - 1, Col];
            if (!bColMax)
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col].Blocks[Chunk.Length - 1, Col + 1];
            else
                yield return ChunkLoader.Instance.chunks[Chunk.Row - 1][Chunk.Col + 1].Blocks[Chunk.Length - 1, 0];
        }

        if (!bColMax)
            yield return Chunk.Blocks[Row, Col + 1];
        else
            yield return ChunkLoader.Instance.chunks[Chunk.Row][Chunk.Col + 1].Blocks[Row, 0];
        */
    }
}
