using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;

namespace Runtime.Character.AI.EnemyAI.BehaviourTrees
{
    public class BehaviourTree : Node
    {
        public BehaviourTree(string _name) : base(_name) { }

        public override Status Process()
        {
            while (currentChild < children.Count)
            {
                Status _status = children[currentChild].Process();
                if (_status != Status.Success)
                {
                    return _status;
                }

                currentChild = (currentChild + 1) % children.Count;
            }

            return Status.Success;
        }

    }

    public class RandomSelector : PrioritySelector
    {
        protected override List<Node> SortChildren() => children.Shuffle().ToList();

        public RandomSelector(string _name, int _priority = 0): base(_name, _priority) { }
    }

    public class PrioritySelector : Node
    {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= SortChildren();
        
        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();
        
        public PrioritySelector(string name, int priority = 0) : base(name, priority) { }
        
        public override void Reset() {
            base.Reset();
            sortedChildren = null;
        }
        
        public override Status Process() {
            foreach (var child in SortedChildren) {
                switch (child.Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        continue;
                }
            }

            Reset();
            return Status.Failure;
        }
        
        
    }
    
    
    public class Selector : Node
    {
        public Selector(string _name, int _priority = 0) : base(_name, _priority) { }
        
        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentChild++;
                        return Status.Running;
                }
            }
            
            Reset();
            return Status.Failure;
        }
    }

    public class Sequence : Node
    {

        public Sequence(string _name, int _priority = 0) : base(_name, _priority) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        currentChild = 0;
                        return Status.Failure;
                    default:
                        currentChild++;
                        return currentChild == children.Count ? Status.Success : Status.Running;
                }
            }
            
            Reset();
            return Status.Success;
        }
        
    }

public class Leaf : Node
    {
        private readonly IStrategy strategy;

        public Leaf(string _name, IStrategy _strategy, int _priority = 0) : base(_name, _priority)
        {
            this.strategy = _strategy;
        }

        public override Node.Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }
    
    public class Node
    {
        public enum Status { Success, Failure, Running }

        public readonly string nodeName;
        public readonly int priority;

        protected readonly List<Node> children = new List<Node>();

        protected int currentChild;

        /// <summary>
        /// Setup node
        /// </summary>
        /// <param name="_name">Name of Node</param>
        /// <param name="_priority">Priority => larger # means MORE priority</param>
        public Node(string _name = "Node", int _priority = 0)
        {
            nodeName = _name;
            priority = _priority;
        }

        public void AddChild(Node _child) => children.Add(_child);

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset()
        {
            currentChild = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }


    }
}