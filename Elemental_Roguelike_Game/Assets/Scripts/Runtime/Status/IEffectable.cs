namespace Runtime.Status
{
    public interface IEffectable
    {
        public void ApplyEffect(Status _newStatus);
        
        public void RemoveEffect();
    }
}