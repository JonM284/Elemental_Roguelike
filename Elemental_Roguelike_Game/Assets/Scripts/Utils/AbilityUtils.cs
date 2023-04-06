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
        
        public static Ability GetRandomAbility()
        {
            return abilityController.GetRandomAbility();
        }
        
        public static Ability GetRandomAbilityByType(ElementTyping _type)
        {
            if (_type == null)
            {
                return default;
            }
            
            return abilityController.GetRandomAbilityByType(_type);
        }

        public static Ability GetRandomAbilityByType(string guid)
        {
            if (guid == string.Empty)
            {
                return default;
            }

            return abilityController.GetRandomAbilityByType(guid);
        }

        public static Ability GetAbilityByGUID(string _guid)
        {
            if (_guid == string.Empty)
            {
                return default;
            }

            return abilityController.GetAbilityByGUID(_guid);
        }
        
        public static Ability GetRandomAbilityByType(ElementTyping _type, Ability _excludingAbility)
        {
            if (_type == null || _excludingAbility == null)
            {
                return default;
            }
            
            return abilityController.GetRandomAbilityByType(_type, _excludingAbility);
        }

        #endregion

    }
}