using System;
using Data.StatusDatas;

namespace Runtime.Status
{
    [Obsolete("Effects are in a list on the character")]
    public interface IEffectable
    {
        public StatusEntityBase CurrentStatusEntityBase { get; protected set; }

        public void ApplyEffect(StatusData newStatus);
        
        public void RemoveEffect();
    }
}