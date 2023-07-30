using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Ability/Params/Ability Parameters")]
    public class AbilityParametersData: ScriptableObject
    {
        public Color tagColor;
        public string displayString;
    }
}