using System.Collections.Generic;

namespace Runtime.Character.AI.EnemyAI
{
    public class AgentGoal
    {
        public string goalName { get; }
        public float goalPriority { get; private set; }

        public HashSet<AgentBelief> desiredEffects { get; } = new();

        AgentGoal(string _goalName)
        {
            goalName = _goalName;
        }
        
        
        public class Builder
        {
            private readonly AgentGoal m_agentGoal;

            public Builder(string _name)
            {
                m_agentGoal = new AgentGoal(_name);
            }


            public Builder WithPriority(float _priority)
            {
                m_agentGoal.goalPriority = _priority;
                return this;
            }

            public Builder WithDesiredEffect(AgentBelief _effect)
            {
                m_agentGoal.desiredEffects.Add(_effect);
                return this;
            }

            public AgentGoal Build()
            {
                return m_agentGoal;
            }
            
        }
    }
}