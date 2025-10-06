using Data;
using Data.AbilityDatas;
using Data.CharacterData;
using Data.Elements;
using Runtime.Abilities;
using Runtime.GameControllers;

namespace Utils
{
    public static class AbilityUtils
    {

        #region Private Fields

        private static AbilityController m_abilityController;

        #endregion

        #region Accessors

        private static AbilityController abilityController => GameControllerUtils.GetGameController(ref m_abilityController);

        #endregion

        #region Class Implementation

        public static AbilityData GetRandomAbilityByType(ElementTyping _type, CharacterClassData _class)
        {
            if (_type == null)
            {
                return default;
            }
            
            return abilityController.GetRandomAbilityByTypeAndClass(_type, _class);
        }

        public static AbilityData GetRandomAbilityByType(string guid, CharacterClassData _class)
        {
            if (guid == string.Empty)
            {
                return default;
            }

            return abilityController.GetRandomAbilityByTypeAndClass(guid, _class);
        }
        
        public static AbilityData GetRandomAbilityByType(string guid, string classGUID)
        {
            if (guid == string.Empty)
            {
                return default;
            }

            return abilityController.GetRandomAbilityByTypeAndClass(guid, classGUID);
        }

        public static AbilityData GetRandomAbilityByType(ElementTyping _type, CharacterClassData _class ,AbilityData _excludingAbility)
        {
            if (_type == null || _excludingAbility == null)
            {
                return default;
            }
            
            return abilityController.GetRandomAbilityByTypeAndClass(_type, _class, _excludingAbility);
        }

        #endregion

    }
}