using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wolf : Pathfinder
{
    public static Wolf Player { get; private set; }
	bool bFirstChunkUpdate = true;

	public int waitingTime;
	public float memoryTime;

    public bool Auto { get; private set; }
	public Func<bool> GetPath { get; private set; }

    List<GameObject> seenPreyList = new List<GameObject>();
    bool seeMode = true;
	Dictionary<Prey, float> prey2MemoryDuration = new Dictionary<Prey, float>();

    BehaviorTree behaviorTree = new BehaviorTree();
    public Pathfinder Target { get; private set; }
    bool bTargetBlockChosen = false;

    protected override void Awake()
    {
        base.Awake();

        if (CompareTag("Player")) { Player = this; }
        BuildBehaviorTree();

        ChunkEventSignals.OnChunkUpdated += OnChunkUpdated;
		GameEventSignals.OnPreySpawned += OnPreySpawned;
		GameEventSignals.OnPreyDespawned += OnPreyDespawned;
    }

    void Start()
    {
        Auto = true;
        Target = null;
    }

    void OnDestroy()
    {
        ChunkEventSignals.OnChunkUpdated -= OnChunkUpdated;
    }

    protected override void Update()
    {
        if (this == Player) {
            if (Input.GetKeyDown(KeyCode.T))
                Auto = !Auto;
            if (Input.GetKeyDown(KeyCode.Y))
                if (!(seeMode = !seeMode))
                    foreach (var preyObj in seenPreyList)
                        if (preyObj != null)
                            Utils.ModifyAlpha(preyObj.GetComponent<Renderer>(), 1f);
            if (seeMode)
                LookForPreys();
        }

        if (Auto) {
            if (behaviorTree.Tick() != Behavior.EStatus.running) {
                behaviorTree.At("Reset").Tick();
            }
        }
        else
            base.Update();
    }

    void OnChunkUpdated()
    {
        if (this != Player) return;

        if (bFirstChunkUpdate) {
            bFirstChunkUpdate = false;

            var centerIndex = ChunkLoader.Instance.loadChunkDistance / 2;
            for (int row = 0; row < ChunkLoader.Instance.chunkSize; ++row) {
                for (int col = 0; col < ChunkLoader.Instance.chunkSize; ++col) {
                    var candidateBlock = ChunkLoader.Instance.chunks[centerIndex][centerIndex].Blocks[row, col];
                    if (!candidateBlock.NonTraversable) {
                        Block = candidateBlock;
                        goto Proc0;
                    }
                }
                continue;
            Proc0:
                break;
            }
        }
    }

	void OnPreySpawned(Prey _prey)
	{
		prey2MemoryDuration[_prey] = 0f;
	}

	void OnPreyDespawned(Prey _prey)
	{
		prey2MemoryDuration.Remove(_prey);
	}

    public override void UpdateWaypoints()
    {
        if (!Auto)
            base.UpdateWaypoints();
        else 
			GetPath();
    }

    bool GetChasingPath()
    {
        if (Target == null)
            return true;

        var pathResult = PathManager.Instance.GetChasingPath(Block, Target.Block);
        if (pathResult != null)
            waypoints = pathResult.Waypoints;
        return true;
    }

    bool GetFleeingPath()
    {
        if (Target == null)
            return true;

        var pathResult = PathManager.Instance.GetFleeingPath(Target.Block, Block, 8);
        if (pathResult != null)
            waypoints = pathResult.Waypoints;
        return true;
    }

    protected override void ExploreNewChunks(Block currentBlock, Block nextBlock)
    {
        if (this != Player) return;

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
    }

    void LookForPreys()
    {
        var preyList = new List<GameObject>();
		foreach (var pathfinder in All)
			if (pathfinder != null && pathfinder is Prey) {
				preyList.Add(pathfinder.gameObject);
				var prey = (Prey)pathfinder;
				if (prey2MemoryDuration.ContainsKey(prey))
					prey2MemoryDuration[prey] -= Time.deltaTime;
			}

        foreach (var preyObj in seenPreyList)
            if (preyObj != null)
                Utils.ModifyAlpha(preyObj.GetComponent<Renderer>(), 1f);
        seenPreyList = SensorySystem.Sight2(float.MaxValue, transform.position, transform.forward, 90f, preyList);
		foreach (var preyObj in seenPreyList) {
			Utils.ModifyAlpha(preyObj.GetComponent<Renderer>(), 0.2f);
			var prey = preyObj.GetComponent<Prey>();
			if (prey2MemoryDuration.ContainsKey(prey))
				prey2MemoryDuration[prey] = memoryTime;
		}
    }

    void BuildBehaviorTree()
    {
        // population phase
		behaviorTree.Add(new SelectorBehavior("Selector_0a"));
		behaviorTree.Add(new SequenceBehavior("Sequence_1a"));
		behaviorTree.Add(new SequenceBehavior("Sequence_1b"));
		behaviorTree.Add(new ActionBehavior("PlanFleeingPath", () => {
			if (!bTargetBlockChosen) {
				bTargetBlockChosen = true;
				TargetBlock = Target.Block;
				GetPath = () => { return GetFleeingPath(); };
				PathManager.Instance.pathfinderQueue.Enqueue(this);
			}
			if (PathManager.Instance.pathfinderQueue.Contains(this))
				return Behavior.EStatus.running;
			return Behavior.EStatus.success;
		}));

		behaviorTree.Add(new ConditionBehavior("FeelNoThreat?", () => {
			int adjacentPreyCount = 0;
			foreach (var pathfinder in All)
				if (pathfinder is Prey && Block.ManhattanDistance(pathfinder.Block, Block) < 4)
					++adjacentPreyCount;
			return Target == null || adjacentPreyCount < 2;
		}));
        behaviorTree.Add(new SelectorBehavior("Selector_2a"));
        behaviorTree.Add(new SequenceBehavior("Sequence_3a"));
        behaviorTree.Add(new ActionBehavior("FindTarget", () => {
            Target = null;
            foreach (var pathfinder in All)
                if (pathfinder is Prey)
                    if (Target == null 
						|| (Block.ManhattanDistance(pathfinder.Block, Block) < Block.ManhattanDistance(Target.Block, Block)
							&& prey2MemoryDuration.ContainsKey((Prey)pathfinder)
							&& prey2MemoryDuration[(Prey)pathfinder] > 0f))
                        Target = pathfinder;
            return Behavior.EStatus.running;
        }));
        behaviorTree.Add(new ConditionBehavior("HasTarget?", () => {
            return Target != null;
        }));
        behaviorTree.Add(new ActionBehavior("PlanChasingPath", () => {
            if (!bTargetBlockChosen) {
                bTargetBlockChosen = true;
                TargetBlock = Target.Block;
                GetPath = () => { return GetChasingPath(); };
                PathManager.Instance.pathfinderQueue.Enqueue(this);
            }
            if (PathManager.Instance.pathfinderQueue.Contains(this))
                return Behavior.EStatus.running;
            return Behavior.EStatus.success;
        }));
        behaviorTree.Add(new ActionBehavior("Navigate", () => {
            Navigate();
            return (waypoints == null) ? Behavior.EStatus.success : Behavior.EStatus.running;
        }));
		behaviorTree.Add(new ExtenderBehavior("Extender_4a", waitingTime));
		behaviorTree.Add(new ActionBehavior("Idle", () => {
			return Behavior.EStatus.success;
		}));

        // isolated
        behaviorTree.Add(new ActionBehavior("Reset", () => {
            bTargetBlockChosen = false;
			Target = null;
            return Behavior.EStatus.success;
        }));
        //! population phase

        // linking phase
		// 0
		behaviorTree.At("Selector_0a").AddChild(behaviorTree.At("Sequence_1a"));
		behaviorTree.At("Selector_0a").AddChild(behaviorTree.At("Sequence_1b"));
		// 1
		behaviorTree.At("Sequence_1a").AddChild(behaviorTree.At("FeelNoThreat?"));
		behaviorTree.At("Sequence_1a").AddChild(behaviorTree.At("Selector_2a"));
		behaviorTree.At("Sequence_1b").AddChild(behaviorTree.At("PlanFleeingPath"));
		behaviorTree.At("Sequence_1b").AddChild(behaviorTree.At("Navigate"));
		// 2
		behaviorTree.At("Selector_2a").AddChild(behaviorTree.At("Sequence_3a"));
		behaviorTree.At("Selector_2a").AddChild(behaviorTree.At("FindTarget"));
		// 3
		behaviorTree.At("Sequence_3a").AddChild(behaviorTree.At("HasTarget?"));
		behaviorTree.At("Sequence_3a").AddChild(behaviorTree.At("PlanChasingPath"));
		behaviorTree.At("Sequence_3a").AddChild(behaviorTree.At("Navigate"));
		behaviorTree.At("Sequence_3a").AddChild(behaviorTree.At("Extender_4a"));
		// 4
		behaviorTree.At("Extender_4a").AddChild(behaviorTree.At("Idle"));

		behaviorTree.Root = behaviorTree.At("Selector_0a");
        //! linking phase
    }
}
