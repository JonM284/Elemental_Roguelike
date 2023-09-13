using System;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    public abstract class GameplayEventBase: MonoBehaviour
    {

        public static event Action GameplayEventEnded;

        public void OnConfirmEventEnd()
        {
            GameplayEventEnded?.Invoke();
        }
        
        
    }
}