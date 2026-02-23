using System;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
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

        private bool activeCharacterDoingAction => activeCharacter.isDoingAction;

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
                setDistance = activeCharacter.characterMovement.currentMoveDistance;
            }
            
            var _correctPivot = !activeCharacter.isGoalieCharacter ? activeCharacter.transform.position.FlattenVector3Y() : activeCharacter.characterMovement.pivotTransform.position.FlattenVector3Y();
            var _magToPoint = Vector3.Magnitude(_pathPosition.FlattenVector3Y() - _correctPivot);
            if (_magToPoint > setDistance)
            {
                Debug.LogError("<color=red>Position too far</color>");
                UIController.Instance.CreateFloatingTextAtCursor("Too Far", Color.red);
                return;
            }    

            
            NavMesh.SamplePosition(_pathPosition, out NavMeshHit clickedLocation, 100, NavMesh.AllAreas);

            if (activeCharacter.IsNull())
            {
                SelectPosition?.Invoke(clickedLocation.position, false);
            }
            else
            {
                activeCharacter.CheckAllAction(clickedLocation.position, false);
            }
        }

        #endregion
        
    }
}