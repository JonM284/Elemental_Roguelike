using Project.Scripts.Utils;
using Runtime.Gameplay;
using UnityEngine;

namespace Runtime.Character
{
    public interface IBallInteractable
    {
        
        public BallBehavior heldBall { get; protected set; }

        public bool canPickupBall { get; protected set; }

        public bool hasBall => !heldBall.IsNull();
        
        public void PickUpBall(BallBehavior ball);

        public void KnockBallAway(Transform attacker);
        
        public void ThrowBall(Vector3 direction, bool _isShot);
        
    }
}