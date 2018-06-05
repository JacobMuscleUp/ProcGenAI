using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prey : Pathfinder, ITempObj
{
    // BT
    BehaviorTree behaviorTree = new BehaviorTree();
    bool bTargetBlockChosen = false;
    Vector3 vNewLookDir;
    Vector3 vOldLookDir;
    float newDirScale = 0f;
    float currentRotateSpeed;
    //! BT

    [Range(30, 100)]
    public float rotateSpeed;
    [Range(3, 20)]
    public int safeDistance;

    Block attachedBlock;
    public Block AttachedBlock
    {
        get { return attachedBlock; }
        set { attachedBlock = Block = value; }
    }

    protected override void Awake()
    {
        base.Awake();
        BuildBehaviorTree();
    }

    void Start()
    {
        TempObjManager.Instance.tempObjs.Add(this);
		GameEventSignals.DoPreySpawned(this);
    }

    void OnDestroy()
    {
        TempObjManager.Instance.tempObjs.Remove(this);
		GameEventSignals.DoPreyDespawned(this);
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
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
    }

    public override void UpdateWaypoints()
    {
        var pathResult = PathManager.Instance.GetFleeingPath(Wolf.Player.Block, Block, safeDistance);
        if (pathResult != null)
            waypoints = pathResult.Waypoints;
    }

    void BuildBehaviorTree()
    {
        // population phase
        behaviorTree.Add(new SelectorBehavior("Selector_0a"));///////////

        behaviorTree.Add(new SequenceBehavior("Sequence_1a"));
        behaviorTree.Add(new SelectorBehavior("Selector_1a"));
        behaviorTree.Add(new ConditionBehavior("WolfNearby?", () => {//////////////
            return Block.ManhattanDistance(Wolf.Player.Block, Block) < safeDistance;
        }));
        behaviorTree.Add(new ActionBehavior("PlanFleeingPath", () => {
            if (!bTargetBlockChosen) {
                bTargetBlockChosen = true;
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
        behaviorTree.Add(new SequenceBehavior("Sequence_2a"));
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
        behaviorTree.At("Selector_0a").AddChild(behaviorTree.At("Sequence_1a"));
        behaviorTree.At("Selector_0a").AddChild(behaviorTree.At("Selector_1a"));
        // 1
        behaviorTree.At("Sequence_1a").AddChild(behaviorTree.At("WolfNearby?"));
        behaviorTree.At("Sequence_1a").AddChild(behaviorTree.At("PlanFleeingPath"));
        behaviorTree.At("Sequence_1a").AddChild(behaviorTree.At("Navigate"));
        behaviorTree.At("Selector_1a").AddChild(behaviorTree.At("Sequence_2a"));
        behaviorTree.At("Selector_1a").AddChild(behaviorTree.At("ChooseIdleLookDir"));
        // 2
        behaviorTree.At("Sequence_2a").AddChild(behaviorTree.At("IdleLookDirChosen?"));
        behaviorTree.At("Sequence_2a").AddChild(behaviorTree.At("RotateTowardsIdleLookDir"));

        behaviorTree.Root = behaviorTree.At("Selector_0a");
        //! linking phase
    }
}
