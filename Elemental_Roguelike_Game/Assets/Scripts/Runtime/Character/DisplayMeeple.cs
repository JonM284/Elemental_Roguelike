using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    public class DisplayMeeple: MonoBehaviour
    {

        #region Private Fields

        private CharacterVisuals m_characterVisuals;

        #endregion

        #region Accessors

        private CharacterVisuals characterVisuals => CommonUtils.GetRequiredComponent(ref m_characterVisuals, () =>
        {
            var cv = GetComponent<CharacterVisuals>();
            return cv;
        });

        public CharacterStatsData assignedData { get; private set; }

        #endregion


        #region Class Implementation

        public void InitializeDisplay(CharacterStatsData _data)
        {
            if (_data.IsNull())
            {
                Debug.LogError("No Character Data to work with");
                return;
            }
            
            var elementType = ElementUtils.GetElementTypeByGUID(_data.meepleElementTypeRef);
            characterVisuals.InitializeMeepleCharacterVisuals(elementType);

            assignedData = _data;
        }

        #endregion


    }
}