using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime.Character.AI.EnemyAI
{

    public interface IGoapPlanner
    {
        ActionPlan Plan(GoapAgent _agent, HashSet<AgentGoal> _goals, AgentGoal mostRecentGoal = null);
    }

    public class GoapPlanner : IGoapPlanner
    {
        public ActionPlan Plan(GoapAgent _agent, HashSet<AgentGoal> _goals, AgentGoal mostRecentGoal = null)
        {
            List <AgentGoal> _orderedGoals = _goals
                .Where(g => g.desiredEffects.Any(b => !b.Evaluate()))
                .OrderByDescending(g => g == mostRecentGoal ? g.goalPriority - 0.01 : g.goalPriority)
                .ToList();

            foreach (var _goal in _orderedGoals)
            {
                Node goalNode = new Node(null, null, _goal.desiredEffects, 0);

                if (FindPath(goalNode, _agent.actions))
                {
                    if (goalNode.IsLeafDead)
                    {
                        continue;
                    }

                    Stack<AgentAction> _actionStack = new Stack<AgentAction>();
                    while (goalNode.leaves.Count > 0)
                    {
                        var cheapestLeaf = goalNode.leaves.OrderBy(leaf => leaf.Cost).First();
                        goalNode = cheapestLeaf;
                        _actionStack.Push(cheapestLeaf.agentAction);
                    }

                    return new ActionPlan(_goal, _actionStack, goalNode.Cost);

                }
            }
            
            Debug.Log("NO PLAN FOUND");
            return null;
        }

        public bool FindPath(Node _parent, HashSet<AgentAction> _actions)
        {
            foreach (var _action in _actions)
            {
                var requiredEffects = _parent.requiredEffects;

                requiredEffects.RemoveWhere(b => b.Evaluate());

                if (requiredEffects.Count == 0)
                {
                    return true;
                }

                if (!_action.effects.Any(requiredEffects.Contains))
                {
                    continue; 
                }
                
                var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                newRequiredEffects.ExceptWith(_action.effects);
                newRequiredEffects.UnionWith(_action.preConditions);

                var newAvailableActions = new HashSet<AgentAction>(_actions);
                newAvailableActions.Remove(_action);

                var newNode = new Node(_parent, _action, newRequiredEffects, _parent.Cost + _action.actionCost);

                if (FindPath(newNode, newAvailableActions))
                {
                    _parent.leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.agentAction.preConditions);
                }

                if (newRequiredEffects.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }


    }

    public class Node
    {
        public Node parent { get; }
        public AgentAction agentAction { get; }
        public HashSet<AgentBelief> requiredEffects { get; }
        public List<Node> leaves { get; }
        public float Cost { get; }

        public bool IsLeafDead => leaves.Count == 0 && agentAction == null;

        public Node(Node _parent, AgentAction _action, HashSet<AgentBelief> _effects, float _cost)
        {
            parent = _parent;
            agentAction = _action;
            requiredEffects = _effects;
            Cost = _cost;
        }
    }
    
    public class ActionPlan
    {
        public AgentGoal agentGoal { get; }
        public Stack<AgentAction> actions { get; }
        public float totalCost { get; set; }


        public ActionPlan(AgentGoal _goal, Stack<AgentAction> _actions, float _totalCost)
        {
            agentGoal = _goal;
            actions = _actions;
            totalCost = _totalCost;
        }
        
        
    }
}