using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Runtime.Selection
{
    public class NavigationSelectable: Selectable
    {

        #region Serialized Fields

        [SerializeField] private Transform associatedNewLocation;

        #endregion
        
        #region Accessors

        public CharacterBase activeCharacter => TurnUtils.GetActiveCharacter();

        public bool isInBattle => TurnUtils.isInBattle();

        #endregion

        #region Class Implementation

        public void SelectPathingLocation(Vector3 _pathPosition)
        {
            if (isInBattle && activeCharacter == null)
            {
                return;
            }

            var _newLocation = associatedNewLocation != null
                ? associatedNewLocation.position
                : _pathPosition;
            
            if (activeCharacter.characterMovement.isInBattle)
            {
                var _magToPoint = Vector3.Magnitude(_newLocation.FlattenVector3Y() - activeCharacter.transform.position.FlattenVector3Y());
                if (_magToPoint > activeCharacter.characterMovement.battleMoveDistance)
                {
                    return;
                }    
            }

            var selectedLocation =
                NavMesh.SamplePosition(_newLocation, out NavMeshHit clickedLocation, 100, NavMesh.AllAreas);
            activeCharacter.characterMovement.MoveCharacter(clickedLocation.position);
        }

        #endregion
        
    }
}