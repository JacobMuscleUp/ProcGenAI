using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathResult<T>
{
    public PathResult()
    {
        Index = 0;
    }

    public IList<GraphNode<T>> Waypoints { get; set; }
    public int Index { get; set; }
}

public class GraphNode<T>
{
    public Dictionary<GraphNode<T>, double> AdjacencyMap { get; private set; }
    public T Value { get; set; }

    public GraphNode(T _value)
    {
        AdjacencyMap = new Dictionary<GraphNode<T>, double>();
        Value = _value;
    }
}

public class Graph<T>
{
    public LinkedList<GraphNode<T>> Nodes { get; private set; }
    public Func<GraphNode<T>, GraphNode<T>, double> Heuristic { get; set; }
    public Func<GraphNode<T>, bool> PredicateTestAdjacency { get; set; }

    public Graph()
    {
        Nodes = new LinkedList<GraphNode<T>>();
        Heuristic = (_startNode, _goalNode) => { return 0; };
        PredicateTestAdjacency = (_node) => { return true; };
    }

    public GraphNode<T> AddVertex(T _value)
    {
        var node = new GraphNode<T>(_value);
        Nodes.AddLast(node);
        return node;
    }

    public void SetEdge(GraphNode<T> _head, GraphNode<T> _tail, int _weight, bool _bBidir = true)
    {
        _head.AdjacencyMap[_tail] = _weight;
        if (_bBidir) { _tail.AdjacencyMap[_head] = _weight; }
    }

    public bool IsEdge(GraphNode<T> _head, GraphNode<T> _tail)
    {
        return _head.AdjacencyMap[_tail] != -1;
    }

    public PathResult<T> AstarSearch(GraphNode<T> _start, GraphNode<T> _goal, Action<GraphNode<T>> _action)
    {
        var parents = new Dictionary<GraphNode<T>, GraphNode<T>>();
        var costs = new Dictionary<GraphNode<T>, double>();
        var openList = new PriorityQueue<double, GraphNode<T>>();
        var closedList = new HashSet<GraphNode<T>>();

        parents[_start] = _start;
        openList.Enqueue(Heuristic(_start, _goal), _start);
        costs[_start] = 0;
        
        while (openList.Count > 0) {
            var current = openList.Dequeue().Value;
            _action(current);
            if (current == _goal)
                return TracePath(_start, _goal, parents);
            closedList.Add(current);
            
            foreach (var next in GetNeighbors(current, null)) {
                if (closedList.Contains(next)) continue;
                var cost = costs[current] + current.AdjacencyMap[next];
                if (!costs.ContainsKey(next) || cost < costs[next]) {
                    costs[next] = cost;
                    openList.Enqueue(cost + Heuristic(next, _goal), next);
                    parents[next] = current;
                }
            }
        }
        return null;
    }

    public PathResult<T> AstarSearchChase(GraphNode<T> _start, GraphNode<T> _goal)
    {
        var parents = new Dictionary<GraphNode<T>, GraphNode<T>>();
        var costs = new Dictionary<GraphNode<T>, double>();
        var openList = new PriorityQueue<double, GraphNode<T>>();
        var closedList = new HashSet<GraphNode<T>>();
        var alwaysTraversableNodes = new List<GraphNode<T>>() { _goal };

        parents[_start] = _start;
        openList.Enqueue(Heuristic(_start, _goal), _start);
        costs[_start] = 0;

        while (openList.Count > 0) {
            var current = openList.Dequeue().Value;

            if (current == _goal)
                return TracePath(_start, parents[_goal], parents);
            closedList.Add(current);

            foreach (var next in GetNeighbors(current, alwaysTraversableNodes)) {
                if (closedList.Contains(next)) continue;
                var cost = costs[current] + current.AdjacencyMap[next];
                if (!costs.ContainsKey(next) || cost < costs[next]) {
                    costs[next] = cost;
                    openList.Enqueue(cost + Heuristic(next, _goal), next);
                    parents[next] = current;
                }
            }
        }
        return null;
    }

    public PathResult<T> AstarSearchFlee(GraphNode<T> _undesirable, GraphNode<T> _start, double _safeGCost)
    {
        var parents = new Dictionary<GraphNode<T>, GraphNode<T>>();
        var costs = new Dictionary<GraphNode<T>, double>();
        var openList = new PriorityQueue<double, GraphNode<T>>();
        var closedList = new HashSet<GraphNode<T>>();
        var alwaysTraversableNodes = new List<GraphNode<T>>() { _start };
        var bMustFlee = false;

        parents[_undesirable] = _undesirable;
        openList.Enqueue(Heuristic(_undesirable, _start), _undesirable);
        costs[_undesirable] = 0;

        while (openList.Count > 0) {
            var current = openList.Dequeue().Value;
            
            if (costs[current] >= _safeGCost) {
                if (bMustFlee)
                    return TracePath(_start, current, parents);
                else
                    break;
            }
            if (current == _start) {
                openList.Clear();
                alwaysTraversableNodes = null;
                bMustFlee = true;
            }
            closedList.Add(current);
            
            foreach (var next in GetNeighbors(current, alwaysTraversableNodes)) {
                if (closedList.Contains(next)) continue;
                var cost = costs[current] + current.AdjacencyMap[next];
                if (!costs.ContainsKey(next) || cost < costs[next]) {
                    costs[next] = cost;
                    openList.Enqueue(bMustFlee ? cost : (cost + Heuristic(next, _start)), next);
                    parents[next] = current;
                }
            }
        }
        return null;
    }

    PathResult<T> TracePath(GraphNode<T> _start, GraphNode<T> _goal, Dictionary<GraphNode<T>, GraphNode<T>> _parents)
    {
        var list = new List<GraphNode<T>>();
        for (; _goal != _start && _goal != _parents[_goal]; list.Insert(0, _goal), _goal = _parents[_goal]) { }
        return new PathResult<T>() { Waypoints = list };
    }

    IEnumerable<GraphNode<T>> GetNeighbors(GraphNode<T> _node, List<GraphNode<T>> _alwaysTraversableNodes)
    {
        foreach (var elem in _node.AdjacencyMap) {
            if (IsEdge(_node, elem.Key)) {
                if (_alwaysTraversableNodes != null) {
                    if (!_alwaysTraversableNodes.Contains(elem.Key) && !PredicateTestAdjacency(elem.Key))
                        continue;
                }
                else if (!PredicateTestAdjacency(elem.Key))
                    continue;
            }
            else
                continue;
            /*if (!IsEdge(_node, elem.Key) || !PredicateTestAdjacency(elem.Key)) continue;*/
            yield return elem.Key;
        }
    }
}
