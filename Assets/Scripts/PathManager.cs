using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }

    Graph<Block> graph = new Graph<Block>() {
        Heuristic = (_startNode, _goalNode) => {
            return Block.ManhattanDistance(_startNode.Value, _goalNode.Value);
        }
        , PredicateTestAdjacency = (_node) => {
            return !_node.Value.Occupied;
        }
    };
    Dictionary<Block, GraphNode<Block>> block2NodeMap = new Dictionary<Block, GraphNode<Block>>();

    public int maxPathfinderCountPerFrame;
    public Queue<Pathfinder> pathfinderQueue = new Queue<Pathfinder>();

    void Awake()
    {
        Instance = this;

        ChunkEventSignals.OnChunkUpdated += OnChunkUpdated;
    }

    void OnDestroy()
    {
        ChunkEventSignals.OnChunkUpdated -= OnChunkUpdated;
    }

    void Update()
    {
        if (pathfinderQueue.Count > 0)
            for (int count = maxPathfinderCountPerFrame; pathfinderQueue.Count > 0 && count > 0; --count, pathfinderQueue.Dequeue())
                pathfinderQueue.Peek().UpdateWaypoints();
    }
    
    void OnChunkUpdated()
    {
        block2NodeMap.Clear();
        for (int row = 0; row < ChunkLoader.Instance.loadChunkDistance; ++row) {
            for (int col = 0; col < ChunkLoader.Instance.loadChunkDistance; ++col) {
                var chunk = ChunkLoader.Instance.chunks[row][col];
                foreach (var block in chunk.Blocks) {
                    var currentNode = (block2NodeMap.ContainsKey(block) 
                        ? block2NodeMap[block] : (block2NodeMap[block] = graph.AddVertex(block)));
                    if (block.NonTraversable) continue;

                    if (block.Row != 0) {
                        var adjacentBlock = block.Chunk.Blocks[block.Row - 1, block.Col];
                        graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock) 
                            ? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
                    }
                    else {
                        if (row != 0) {
                            var adjacentBlock = ChunkLoader.Instance.chunks[block.Chunk.Row - 1][block.Chunk.Col]
                                .Blocks[block.Chunk.Length - 1, block.Col];
                            graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock)
                                ? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
                        }
                    }
                    if (block.Row != block.Chunk.Length - 1) {
                        var adjacentBlock = block.Chunk.Blocks[block.Row + 1, block.Col];
                        graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock)
                            ? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
                    }
                    else {
                        if (row != ChunkLoader.Instance.loadChunkDistance - 1) {
                            var adjacentBlock = ChunkLoader.Instance.chunks[block.Chunk.Row + 1][block.Chunk.Col].Blocks[0, block.Col];
                            graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock)
                                ? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
                        }
                    }

                    if (block.Col != 0) {
						var adjacentBlock = block.Chunk.Blocks[block.Row, block.Col - 1];
						graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock) 
							? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
                    }
                    else {
						if (col != 0) {
							var adjacentBlock = ChunkLoader.Instance.chunks[block.Chunk.Row][block.Chunk.Col - 1].Blocks[block.Row, block.Chunk.Length - 1];
							graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock) 
								? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
						}
                    }
                    if (block.Col != block.Chunk.Length - 1) {
						var adjacentBlock = block.Chunk.Blocks[block.Row, block.Col + 1];
						graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock) 
							? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
                    }
                    else {
						if (col != ChunkLoader.Instance.loadChunkDistance - 1) {
							var adjacentBlock = ChunkLoader.Instance.chunks[block.Chunk.Row][block.Chunk.Col + 1].Blocks[block.Row, 0];
							graph.SetEdge(currentNode, block2NodeMap.ContainsKey(adjacentBlock) 
								? block2NodeMap[adjacentBlock] : graph.AddVertex(adjacentBlock), adjacentBlock.NonTraversable ? -1 : 1);
						}
                    }
                }
            }
        }
    }

    public PathResult<Block> GetPath(Block _start, Block _goal)
    {
        //Debug.Log(string.Format("[{0}, {1}], {2}", _start.Row, _start.Col, _start.Chunk));
        //Debug.Log(string.Format("[{0}, {1}], {2}", _goal.Row, _goal.Col, _goal.Chunk));
        if (_start == null || _goal == null || !block2NodeMap.ContainsKey(_goal) || !block2NodeMap.ContainsKey(_start)) return null;
        return graph.AstarSearch(block2NodeMap[_start], block2NodeMap[_goal], (_node) => {
            /*Debug.Log(string.Format("Chunk[{0}, {1}], Block[{2}, {3}]"
                , _node.Value.Chunk.Row, _node.Value.Chunk.Col
                , _node.Value.Row, _node.Value.Col));*/
        });
    }

    public PathResult<Block> GetChasingPath(Block _start, Block _goal)
    {
        if (_start == null || _goal == null || !block2NodeMap.ContainsKey(_goal) || !block2NodeMap.ContainsKey(_start)) return null;
        return graph.AstarSearchChase(block2NodeMap[_start], block2NodeMap[_goal]);
    }

    public PathResult<Block> GetFleeingPath(Block _start, Block _goal, double _safeGCost)
    {
        if (_start == null || _goal == null || !block2NodeMap.ContainsKey(_goal) || !block2NodeMap.ContainsKey(_start)) return null;
        return graph.AstarSearchFlee(block2NodeMap[_start], block2NodeMap[_goal], _safeGCost);
    }
}
