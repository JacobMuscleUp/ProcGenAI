using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BehaviorTree
{
    Behavior root;
    public Behavior Root {
        get { return root; }
        set { root = currentBehavior = value; }
    }
    public Dictionary<string, Behavior> Behaviors { get; private set; }
    Behavior currentBehavior;

    public BehaviorTree() {
        Root = null;
        Behaviors = new Dictionary<string, Behavior>();
    }

    public void Add(Behavior _behavior)
    {
        if (!Behaviors.ContainsKey(_behavior.Name))
            Behaviors[_behavior.Name] = _behavior;
        else
            throw new ApplicationException("Duplicate behavior names are not permitted");
    }

    public Behavior At(string _name)
    {
        if (!Behaviors.ContainsKey(_name))
            throw new ApplicationException(string.Format("{0} doesn't exist in BehaviorTree", _name));
        return Behaviors[_name];
    }

    public Behavior.EStatus Tick()
    {
        return currentBehavior.Tick();
    }
}
