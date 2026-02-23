using System;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Gameplay.Sensors
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerDetectionSensor: MonoBehaviour
    {

        [SerializeField] private SphereCollider collider;
        
        public event Action<CharacterBase> OnCharacterEnter;
        public event Action<CharacterBase> OnCharacterExit;

        public void SetColliderRadius(float newRadius)
        {
            collider.radius = newRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CharacterBase characterBase)) return;
            OnCharacterEnter?.Invoke(characterBase);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out CharacterBase characterBase)) return;
            OnCharacterExit?.Invoke(characterBase);
        }
    }
}