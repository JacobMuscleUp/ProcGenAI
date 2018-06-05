using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wanderer : Pathfinder, ITempObj
{
    // BT
    BehaviorTree behaviorTree = new BehaviorTree();
    bool bTargetBlockChosen = false;
    Vector3 vNewLookDir;
    Vector3 vOldLookDir;
    float newDirScale = 0f;
    float currentRotateSpeed;
    //! BT

    [Range(0, 1000)]
    public int idleTime;
    [Range(30f, 100f)]
    public float rotateSpeed;
    public bool patrolMode;

    Block attachedBlock;
    public Block AttachedBlock {
        get { return attachedBlock; }
        set { attachedBlock = Block = value; }
    }
    public Func<Block> GetNextWaypoint { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (patrolMode)
            GetNextWaypoint = () => {
                var randomChunk = ChunkLoader.Instance.chunks
                    [UnityEngine.Random.Range(0, ChunkLoader.Instance.loadChunkDistance)]
                    [UnityEngine.Random.Range(0, ChunkLoader.Instance.loadChunkDistance)];
                var randomBlock = randomChunk.traversableBlocks[UnityEngine.Random.Range(0, randomChunk.traversableBlocks.Count)];
                return randomBlock;
            };
        else
            GetNextWaypoint = () => {
                var randomIndex = UnityEngine.Random.Range(0, 8);
                Block randomBlock = null;
                foreach (var adjacentBlock in Block.AdjacentBlocks())
                    if (randomIndex-- == 0) {
                        randomBlock = adjacentBlock;
                        break;
                    }
                return randomBlock;
            };

        BuildBehaviorTree();
    }

    void Start()
    {
        TempObjManager.Instance.tempObjs.Add(this);
    }

    void OnDestroy()
    {
        TempObjManager.Instance.tempObjs.Remove(this);
    }

    protected override void Update()
    {
        if (behaviorTree.Tick() != Behavior.EStatus.running) {
            behaviorTree.At("Reset").Tick();
        }
    }

    protected override void OnAttachedBlockChanged()
    {
        AttachedBlock = Block;
    }

    Vector3 GetRandomHorizontalDir()
    {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
    }

    void BuildBehaviorTree()
    {
        // population phase
        behaviorTree.Add(new SequenceBehavior("Sequence_0a"));
        behaviorTree.Add(new ActionBehavior("PlanPath", () => {
            if (!bTargetBlockChosen) {
                bTargetBlockChosen = true;

                var potentialTargetBlock = GetNextWaypoint();
                if (potentialTargetBlock == Block)
                    return Behavior.EStatus.failure;
                TargetBlock = potentialTargetBlock;
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
        behaviorTree.Add(new ExtenderBehavior("Extender_1a", idleTime));
        behaviorTree.Add(new SelectorBehavior("Selector_2a"));
        behaviorTree.Add(new SequenceBehavior("Sequence_3a"));
        behaviorTree.Add(new ActionBehavior("ChooseIdleLookDir", () => {
            vNewLookDir = GetRandomHorizontalDir();
            return Behavior.EStatus.success;
        }));
        behaviorTree.Add(new ConditionBehavior("IdleLookDirChosen?", () => {
            return transform.forward != vNewLookDir;
        }));
        behaviorTree.Add(new ActionBehavior("RotateTowardsIdleLookDir", () => {
            if (Mathf.Abs(newDirScale - 0f) < float.Epsilon) {
                vOldLookDir = transform.forward;
                vNewLookDir = GetRandomHorizontalDir();
                currentRotateSpeed = rotateSpeed / Vector3.Angle(vOldLookDir, vNewLookDir);
            }
            newDirScale = ((newDirScale += Time.deltaTime * currentRotateSpeed) > 1f) ? 1f : newDirScale;
            transform.LookAt(transform.position + (newDirScale * vNewLookDir + (1f - newDirScale) * vOldLookDir));
            if (Mathf.Abs(newDirScale - 1f) < float.Epsilon)
                newDirScale = 0f;

            return Behavior.EStatus.success;
        }));

        // isolated
        behaviorTree.Add(new ActionBehavior("Reset", () => {
            bTargetBlockChosen = false;
            return Behavior.EStatus.success;
        }));
        //! population phase

        // linking phase
        // 0
        behaviorTree.At("Sequence_0a").AddChild(behaviorTree.At("PlanPath"));
        behaviorTree.At("Sequence_0a").AddChild(behaviorTree.At("Navigate"));
        behaviorTree.At("Sequence_0a").AddChild(behaviorTree.At("Extender_1a"));
        // 1
        behaviorTree.At("Extender_1a").AddChild(behaviorTree.At("Selector_2a"));
        // 2
        behaviorTree.At("Selector_2a").AddChild(behaviorTree.At("Sequence_3a"));
        behaviorTree.At("Selector_2a").AddChild(behaviorTree.At("ChooseIdleLookDir"));
        // 3
        behaviorTree.At("Sequence_3a").AddChild(behaviorTree.At("IdleLookDirChosen?"));
        behaviorTree.At("Sequence_3a").AddChild(behaviorTree.At("RotateTowardsIdleLookDir"));

        behaviorTree.Root = behaviorTree.At("Sequence_0a");
        //! linking phase
    }
}
