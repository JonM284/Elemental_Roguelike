using Runtime.Status;
using Runtime.VFX;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.StatusDatas
{
    [CreateAssetMenu(menuName = "Maulball/Status/Status Data")]
    public class StatusData: ScriptableObject
    {

        [Header("Visuals / Description")] 
        public string statusIdentifierGUID;
        
        [Space(15)]
        [Header("Status Common Fields")]
        public string statusName = "==== Ability Name ====";
        public string statusDescription = "//// Ability Description ////";

        public StatusType statusType = StatusType.Neutral;
        
        public int amountOfTurns;
        
        [Header("Icons")]
        public Sprite statusIcon, statusIconLow;
        
        [Header("VFX")]
        
        public VFXPlayer statusStayVFX;
        
        public VFXPlayer statusOneTimeVFX;

        public bool playVFXOnTrigger;
        
        [Header("Reference")]
        public GameObject statusGameObject;
        
        [ContextMenu("Make Identifier")]
        public void CreateGUID()
        {
            statusIdentifierGUID = System.Guid.NewGuid().ToString();
        }
        
    }
}