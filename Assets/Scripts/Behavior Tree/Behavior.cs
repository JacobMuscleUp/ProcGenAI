using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Behavior
{
	public enum EStatus { success, failure, running, invalid }

	public EStatus Status { get; protected set; }
    public string Name { get; set; }
	protected Behavior(string _name) {
        Status = EStatus.invalid;
        Name = _name;
	}

	protected abstract EStatus Update();
	protected virtual void OnInitialized() {}
	protected virtual void OnTerminated(EStatus _status) {}

	public EStatus Tick()
	{
		if (Status != EStatus.running)
			OnInitialized();
        Status = Update();
		if (Status != EStatus.running)
			OnTerminated(Status);
		return Status;
	}

    public virtual void AddChild(Behavior _child) { }
    public virtual void RemoveChild(Behavior _child) { }
}

public abstract class DecoratorBehavior : Behavior
{
	public Behavior Child { get; protected set; }
	protected DecoratorBehavior(string _name) : base(_name) { }

    public override void AddChild(Behavior _child)
    {
        if (Child != null)
            throw new ApplicationException("DecoratorBehavior cannot overwrite a child");
        Child = _child;
    }

    public override void RemoveChild(Behavior _child)
    {
        if (Child == _child)
            Child = null;
    }
}

public abstract class CompositeBehavior : Behavior
{
	protected List<Behavior> children;

	protected CompositeBehavior(string _name) : base(_name) {
		children = new List<Behavior>();
	}

	public override void AddChild(Behavior _child)
	{
		children.Add(_child);
	}

	public override void RemoveChild(Behavior _child)
	{
		children.Remove(_child);
	}
}

public abstract class LeafBehavior : Behavior
{
    protected LeafBehavior(string _name) : base(_name) { }
}

public class RepeaterBehavior : DecoratorBehavior
{
    public int Count { get; set; }
    public RepeaterBehavior(string _name, int _count) : base(_name) {
        Count = _count;
    }

    protected override EStatus Update()
    {
        for (int count = 0; count < Count; ++count) {
            var childStatus = Child.Tick();
            if (childStatus != EStatus.success && childStatus != EStatus.failure)
                return childStatus;
        }
        return EStatus.success;
    }
}

public class ExtenderBehavior : DecoratorBehavior
{
    public int Count { get; set; }
    int currentCount;
    public ExtenderBehavior(string _name, int _count) : base(_name) {
        Count = _count;
        currentCount = 0;
    }

    protected override EStatus Update()
    {
        if (currentCount < Count) {
            ++currentCount;
            Child.Tick();
            return EStatus.running;
        }
        currentCount = 0;
        return EStatus.success;
    }
}

public class InverterBehavior : DecoratorBehavior
{
    public InverterBehavior(string _name) : base(_name) {}

    protected override EStatus Update()
    {
        var childStatus = Child.Tick();
        return (childStatus == EStatus.success) 
            ? EStatus.failure 
            : ((childStatus == EStatus.failure) 
                ? EStatus.success : childStatus);
    }
}

public class SequenceBehavior : CompositeBehavior
{
    public SequenceBehavior(string _name) : base(_name) { }

    protected override EStatus Update()
	{
		for (int childIndex = 0; childIndex < children.Count; ++childIndex) {
			var childStatus = children[childIndex].Tick();
			if (childStatus != EStatus.success)
				return childStatus;
		}
		return EStatus.success;
	}
}

public class SelectorBehavior : CompositeBehavior
{
    public SelectorBehavior(string _name) : base(_name) {}

    protected override EStatus Update()
	{
        for (int childIndex = 0; childIndex < children.Count; ++childIndex) {
            var childStatus = children[childIndex].Tick();
            if (childStatus != EStatus.failure)
                return childStatus;
        }
        return EStatus.failure;
    }
}

public class ActionBehavior : LeafBehavior
{
    public Func<EStatus> Action { get; set; }
    public ActionBehavior(string _name, Func<EStatus> _action)
        : base(_name) {
        Action = _action;
    }

    protected override EStatus Update()
    {
        return Action();
    }
}

public class ConditionBehavior : LeafBehavior
{
    public Func<bool> Predicate { get; set; }
    public ConditionBehavior(string _name, Func<bool> _predicate)
        : base(_name) {
        Predicate = _predicate;
    }

    protected override EStatus Update()
    {
        return Predicate() ? EStatus.success : EStatus.failure;
    }
}
