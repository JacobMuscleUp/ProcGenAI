using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    static List<Pathfinder> all = new List<Pathfinder>();
    public static List<Pathfinder> All { get { return all; } }

    public float moveSpeed;
    [Range(.15f, 1f)]
    public float steeringSpeed;

    Block block;
    public Block Block {
        get { return block; }
        set {
            if (block == null)
                currentWaypoint = nextWaypoint = new GraphNode<Block>(value);
            else
                block.Occupied = false;
            transform.position = StandingPos(block = value);
            block.Occupied = true;
        }
    }

    public Block TargetBlock { get; set; }
    protected IList<GraphNode<Block>> waypoints;
    protected GraphNode<Block> currentWaypoint, nextWaypoint;

    protected virtual void Awake()
    {
        All.Add(this);
    }

    protected virtual void Update()
    {
        Navigate();
    }

    protected void Navigate()
    {
        if (waypoints != null) {
            if (waypoints.Count > 0) {
                if (currentWaypoint == nextWaypoint)
                    nextWaypoint = waypoints[0];
                if (nextWaypoint.Value == null) {
                    nextWaypoint = currentWaypoint;
                    waypoints = null;
                    return;
                }
                else if (nextWaypoint.Value.Occupied) {
                    nextWaypoint = currentWaypoint;
                    waypoints = null;
                    PathManager.Instance.pathfinderQueue.Enqueue(this);
                    return;
                }
                
                Steer();

                if (Vector3.Dot(
                        nextWaypoint.Value.transform.position - currentWaypoint.Value.transform.position
                        , StandingPos(nextWaypoint.Value) - transform.position
                    ) < 0
                   && Vector3.Distance(StandingPos(nextWaypoint.Value), transform.position) < .3f) { 
                //if (Vector3.Distance(nextWaypoint.Value.transform.position, currentWaypoint.Value.transform.position) 
                //    < Vector3.Distance(transform.position, StandingPos(currentWaypoint.Value))) {
                    var nextBlock = (currentWaypoint = nextWaypoint).Value;
                    var currentBlock = Block;
                    Block = nextBlock;
                    ExploreNewChunks(currentBlock, nextBlock);
                    OnAttachedBlockChanged();

                    waypoints.RemoveAt(0);
                }
            }
            else {
                waypoints = null;
                TargetBlock = null;
            }
        }
    }

    protected virtual void ExploreNewChunks(Block currentBlock, Block nextBlock)
    {}
    /*protected virtual void ExploreNewChunks(Block currentBlock, Block nextBlock)
    {
        if (nextBlock.Chunk != currentBlock.Chunk) {
            if (nextBlock.Chunk.Row == currentBlock.Chunk.Row + 1)
                ChunkEventSignals.DoNewChunkExplored(ChunkEventSignals.EChunkExpandDir.rowBack);
            else if (nextBlock.Chunk.Row == currentBlock.Chunk.Row - 1)
                ChunkEventSignals.DoNewChunkExplored(ChunkEventSignals.EChunkExpandDir.rowFront);
            else if (nextBlock.Chunk.Col == currentBlock.Chunk.Col + 1)
                ChunkEventSignals.DoNewChunkExplored(ChunkEventSignals.EChunkExpandDir.colBack);
            else if (nextBlock.Chunk.Col == currentBlock.Chunk.Col - 1)
                ChunkEventSignals.DoNewChunkExplored(ChunkEventSignals.EChunkExpandDir.colFront);
        }
    }*/

    protected virtual void Steer()
    {
        var repulsiveForceSum = Vector3.zero;
        foreach (var adjacentBlock in Block.AdjacentBlocks()) {
            if (!adjacentBlock.Occupied && !adjacentBlock.NonTraversable) continue;
            var repulsiveForce = transform.position - StandingPos(adjacentBlock);
            repulsiveForceSum += repulsiveForce.normalized / repulsiveForce.sqrMagnitude;
        }
        var desiredDir = (StandingPos(nextWaypoint.Value) - transform.position).normalized;
        if (repulsiveForceSum.sqrMagnitude > 1f) 
            (desiredDir += repulsiveForceSum.normalized).Normalize();
        
        //var desiredDir = (StandingPos(nextWaypoint.Value) - transform.position).normalized;
        var moveDir = (-desiredDir != transform.forward) ?
            (steeringSpeed * desiredDir + (1 - steeringSpeed) * transform.forward).normalized
            : (steeringSpeed * new Vector3(-desiredDir[2], desiredDir[1], desiredDir[0]) + (1 - steeringSpeed) * transform.forward).normalized;
        transform.LookAt(transform.position + moveDir);
        transform.position += transform.forward * (moveSpeed * Time.deltaTime);
    }

    protected virtual void OnAttachedBlockChanged()
    {}

    public virtual void UpdateWaypoints()
    {
        var pathResult = PathManager.Instance.GetPath(Block, TargetBlock);
        if (pathResult != null)
            waypoints = pathResult.Waypoints;
    }

    public Vector3 StandingPos(Block _block)
    {
        return _block.transform.position
            + Vector3.up * (_block.transform.localScale[1] / 2 + transform.localScale[1] / 2);
    }
}
