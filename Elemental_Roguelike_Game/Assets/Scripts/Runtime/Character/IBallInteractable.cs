using Runtime.Gameplay;
using UnityEngine;

namespace Runtime.Character
{
    public interface IBallInteractable
    {
        public void PickUpBall(BallBehavior ball);

        public void KnockBallAway(Transform attacker);
        
        public void ThrowBall(Vector3 direction, bool _isShot);
        
    }
}