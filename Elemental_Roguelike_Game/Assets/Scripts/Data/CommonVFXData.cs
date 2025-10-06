using Runtime.VFX;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Maulball/VFX/Common VFX")]
    public class CommonVFXData: ScriptableObject
    {
        public VFXPlayer buffVFXPrefab;

        public VFXPlayer debuffVFXPrefab;

        public VFXPlayer damageVFXPrefab;

        public VFXPlayer deathVFXPrefab;
    }
}