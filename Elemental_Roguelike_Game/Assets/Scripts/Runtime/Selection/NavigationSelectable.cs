using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.Selection
{
    public class NavigationSelectable: Selectable
    {

        #region Accessors

        public CharacterBase activeCharacter => TurnUtils.GetActiveCharacter();

        #endregion

        #region Class Implementation

        public void SelectPathingLocation(Vector3 _pathPosition)
        {
            if (activeCharacter == null)
            {
                return;
            }

            var magToPoint = Vector3.Magnitude(_pathPosition.FlattenVector3Y() - activeCharacter.transform.position.FlattenVector3Y());
            if (magToPoint > activeCharacter.characterMovement.battleMoveDistance)
            {
                return;
            }
            
            activeCharacter.characterMovement.MoveCharacterAgent(_pathPosition);
        }

        #endregion
        
    }
}