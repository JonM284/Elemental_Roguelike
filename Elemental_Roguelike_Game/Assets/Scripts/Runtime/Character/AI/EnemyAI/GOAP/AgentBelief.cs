using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Character.AI.EnemyAI
{

    public class BeliefFactor
    {
        private readonly GoapAgent agent;

        private readonly Dictionary<string, AgentBelief> beliefs;

        public BeliefFactor(GoapAgent _agent, Dictionary<string, AgentBelief> _beliefs)
        {
            this.agent = _agent;
            this.beliefs = _beliefs;
        }

        public void AddBelief(string _key, Func<bool> _condition)
        {
            beliefs.Add(_key, new AgentBelief.Builder(_key)
                .WithCondition(_condition)
                .Build());
        }

        public void AddSensorBelief(string _key, Sensor _sensor)
        {
            beliefs.Add(_key, new AgentBelief.Builder(_key)
                .WithCondition(() => _sensor.isTargetInRange)
                .WithLocation(() => _sensor.targetPosition)
                .Build());
        }

        public void AddLocationBelief(string _key, float _distance, Transform _locationCondition)
        {
            AddLocationBelief(_key, _distance, _locationCondition.position);
        }

        
        public void AddLocationBelief(string _key, float _distance, Vector3 _locationCondition)
        {
            beliefs.Add(_key, new AgentBelief.Builder(_key)
                .WithCondition(() => InRangeOf(_locationCondition, _distance))
                .WithLocation(() => _locationCondition)
                .Build());
        }

        bool InRangeOf(Vector3 _checkPos, float _range) => Vector3.Distance(agent.transform.position, _checkPos) < _range;

        

    }
    
    public class AgentBelief
    {
        //GOAP created with the help of: https://www.youtube.com/watch?v=T_sBYgP7_2k&t=291s

        public string name { get; }
    
        private Func<bool> condition = () => false;

        private Func<Vector3> observedLocation = () => Vector3.zero;

        #region Accessors

        public Vector3 location => observedLocation();
        
        #endregion

        #region Constructor

        public AgentBelief(string _name)
        {
            name = _name;
        }

        #endregion

        #region Class Implementation

        public bool Evaluate() => condition();

        #endregion

        #region Nested Classes

        public class Builder
        {
            private readonly AgentBelief belief;

            public Builder(string _name)
            {
                belief = new AgentBelief(_name);
            }

            public Builder WithCondition(Func<bool> _condition)
            {
                belief.condition = _condition;
                return this;
            }

            public Builder WithLocation(Func<Vector3> _observedLocation)
            {
                belief.observedLocation = _observedLocation;
                return this;
            }

            public AgentBelief Build()
            {
                return belief;
            }
            
        }

        #endregion
        
        
    }
}