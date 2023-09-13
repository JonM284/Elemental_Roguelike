using UnityEngine;

namespace Runtime.GameplayEvents
{
    public abstract class GameplayEventType : ScriptableObject
    {

        #region Serialized Fields

        [SerializeField] private Texture m_eventTexture;

        #endregion

        #region Public Fields

        public string eventGUID;

        #endregion
        
        #region Accessors

        public Texture eventTexture => m_eventTexture;

        #endregion

        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            if (eventGUID != string.Empty)
            {
                return;
            }
            
            eventGUID = System.Guid.NewGuid().ToString();
        }

    }
}