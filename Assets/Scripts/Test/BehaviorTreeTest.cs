using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeTest : MonoBehaviour
{
    BehaviorTree behaviorTree = new BehaviorTree();
    int a = 0, b = 0, c = 0, d = 0;
    bool flag = true;

    void Awake()
    {
        behaviorTree.Add(new SelectorBehavior("Selector0"));

        behaviorTree.Add(new SequenceBehavior("Sequence0"));
        behaviorTree.Add(new ActionBehavior("Run", () => {
            if (d == 2)
                return Behavior.EStatus.success;
            ++d;
            Debug.Log("run");
            return Behavior.EStatus.running;
        }));

        behaviorTree.Add(new ActionBehavior("Eat", () => {
            if (a == 5)
                return Behavior.EStatus.success;
            ++a;
            Debug.Log("eat");
            return Behavior.EStatus.running;
        }));
        behaviorTree.Add(new InverterBehavior("Inverter0"));
        behaviorTree.Add(new ActionBehavior("Program", () => {
            if (c == 4)
                return Behavior.EStatus.success;
            ++c;
            Debug.Log("program");
            return Behavior.EStatus.running;
        }));

        behaviorTree.Add(new ActionBehavior("Sleep", () => {
            if (b == 3)
                return Behavior.EStatus.success;
            ++b;
            Debug.Log("sleep");
            return Behavior.EStatus.running;
        }));

        behaviorTree.At("Selector0").AddChild(behaviorTree.At("Sequence0"));
        behaviorTree.At("Selector0").AddChild(behaviorTree.At("Run"));
        behaviorTree.At("Sequence0").AddChild(behaviorTree.At("Eat"));
        behaviorTree.At("Sequence0").AddChild(behaviorTree.At("Inverter0"));
        behaviorTree.At("Sequence0").AddChild(behaviorTree.At("Program"));
        behaviorTree.At("Inverter0").AddChild(behaviorTree.At("Sleep"));

        behaviorTree.Root = behaviorTree.At("Selector0");
    }

    void Update()
    {
        if (flag) {
            var status = behaviorTree.Tick();
            if (status == Behavior.EStatus.success) {
                a = b = c = d = 0;
                flag = false;
            }
        }
    }
}
