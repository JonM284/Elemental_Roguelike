namespace Runtime.Character.AI.EnemyAI
{
    public interface IActionStrategy
    {
        bool canPerform { get; }
        bool complete { get; }

        void Start()
        {
            
        }

        void Update(float deltaTime)
        {
            
        }

        void Stop()
        {
            
        }
    }
}