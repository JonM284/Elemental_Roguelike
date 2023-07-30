using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Cleanse Status")]
    public class CleanseStatus: Status
    {
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            //Doesn't do anything. Just applies itself and removes itself
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            
        }
    }
}