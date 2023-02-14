using System;
using UnityEngine;

namespace Data
{
    [Serializable] 
    [CreateAssetMenu(menuName = "Custom Data/Popup Dialog Data")]
    public class PopupDialogData : ScriptableObject
    {
        
        public string popupTitle = "Title";

        [TextArea(1,3)]
        public string popupDescription = "Description";
        
    }
}