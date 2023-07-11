using System;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Runtime.Selection
{
    public class NavigationSelectable: Selectable
    {

        #region Actions

        public static event Action<Vector3, bool> SelectPosition;

        #endregion

        #region Serialized Fields

        

        #endregion
        
        #region Accessors

        public CharacterBase activeCharacter => TurnUtils.GetActiveCharacter();

        private bool activeCharacterDoingAction => activeCharacter.characterMovement.isUsingMoveAction ||
                                                   activeCharacter.characterWeaponManager.isUsingWeapon
                                                   || activeCharacter.characterAbilityManager.isUsingAbilityAction ||
                                                   activeCharacter.isSetupThrowBall;

        public bool isInBattle => TurnUtils.isInBattle();

        #endregion

        #region Class Implementation

        public void SelectPathingLocation(Vector3 _pathPosition)
        {
            if (activeCharacter.IsNull())
            {
                Debug.Log($"isInBattle{isInBattle} /// active character null? {activeCharacter == null}");
                return;
            }

            if (!activeCharacterDoingAction)
            {
                Debug.LogError("Active Character Not Doing Action");
                return;
            }

            var setDistance = 100f;
            if (activeCharacter.characterMovement.isUsingMoveAction)
            {
                setDistance = activeCharacter.characterMovement.battleMoveDistance;
            }
            
            var _magToPoint = Vector3.Magnitude(_pathPosition.FlattenVector3Y() - activeCharacter.transform.position.FlattenVector3Y());
            if (_magToPoint > setDistance)
            {
                Debug.LogError("<color=red>Position too far</color>");
                return;
            }    

            
            NavMesh.SamplePosition(_pathPosition, out NavMeshHit clickedLocation, 100, NavMesh.AllAreas);
            
            SelectPosition?.Invoke(clickedLocation.position, false);

        }

        #endregion
        
    }
}