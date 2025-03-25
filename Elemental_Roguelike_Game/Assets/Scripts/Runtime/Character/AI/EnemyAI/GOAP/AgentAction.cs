using System.Collections.Generic;

namespace Runtime.Character.AI.EnemyAI
{
    public class AgentAction
    {
        public string actionName { get; }
        public float actionCost { get; private set; }

        public HashSet<AgentBelief> preConditions { get; } = new();
        public HashSet<AgentBelief> effects { get; } = new();

        private IActionStrategy m_strategy;
        public bool complete => m_strategy.complete;

        AgentAction(string _name)
        {
            actionName = _name;
        }

        public void Start() => m_strategy.Start();

        public void Update(float _deltaTime)
        {
            if (!m_strategy.canPerform)
            {
                return;
            }

            m_strategy.Update(_deltaTime);

            if (!m_strategy.complete)
            {
                return;
            }

            foreach (var _effect in effects)
            {
                _effect.Evaluate();
            }
            
        }

        public void Stop() => m_strategy.Stop();
        
        
        public class Builder
        {
            private readonly AgentAction m_agentAction;

            public Builder(string _name)
            {
                m_agentAction = new AgentAction(_name)
                {
                    actionCost = 1f
                };
            }


            public Builder WithCost(float _cost)
            {
                m_agentAction.actionCost = _cost;
                return this;
            }

            public Builder WithStrategy(IActionStrategy _strategy)
            {
                m_agentAction.m_strategy = _strategy;
                return this;
            }

            public Builder AddPrecondition(AgentBelief _precondition)
            {
                m_agentAction.preConditions.Add(_precondition);
                return this;
            }

            public Builder AddEffect(AgentBelief _effect)
            {
                m_agentAction.effects.Add(_effect);
                return this;
            }

            public AgentAction Build()
            {
                return m_agentAction;
            }
            

        }
    }
}