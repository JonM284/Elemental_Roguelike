using UnityEngine;

namespace Data
{
    
    [CreateAssetMenu(menuName = "Settings")]
    public class SettingsData: ScriptableObject
    {

        public ColorblindOptions colorblindOptions = ColorblindOptions.NORMAL;
        
        public Color playerSideColor = Color.cyan;

        public Color enemySideColor = Color.red;
        
        public Color neutralSideColor = Color.black;

    }
}