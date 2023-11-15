namespace Runtime.Status
{
    public interface IEffectable
    {
        public Status currentStatus { get; protected set; }

        public void ApplyEffect(Status _newStatus);
        
        public void RemoveEffect();
    }
}