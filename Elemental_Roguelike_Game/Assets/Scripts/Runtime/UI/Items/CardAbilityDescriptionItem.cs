using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Items
{
    public class CardAbilityDescriptionItem: MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image abilityIcon;

        [SerializeField] private Transform tabParent;
        
        [SerializeField] private GameObject tabPrefab;

        #endregion

        #region Private Fields

        private List<AbilityParamTabItem> activeTabs = new List<AbilityParamTabItem>();

        private List<AbilityParamTabItem> m_cachedTabs = new List<AbilityParamTabItem>();

        private Transform m_tabPool;

        #endregion

        #region Accessors

        private Transform tabPool => CommonUtils.GetRequiredComponent(ref m_tabPool, () =>
        {
            var t = TransformUtils.CreatePool(this.transform, false);
            return t;
        });

        #endregion

        #region Class Implementation

        public void InitializeItem(Sprite _abilityIcon, List<AbilityParametersData> _parameters)
        {
            if (!_abilityIcon.IsNull())
            {
                abilityIcon.sprite = _abilityIcon;
            }

            if (_parameters.Count == 0)
            {
                return;
            }

            foreach (var abilityParam in _parameters)
            {
                GameObject tabGO = null;

                if (m_cachedTabs.Count > 0)
                {
                    tabGO = m_cachedTabs.FirstOrDefault().gameObject;
                    m_cachedTabs.RemoveAt(0);
                }

                if (tabGO.IsNull())
                {
                    tabGO = tabPrefab.Clone(tabParent);
                }
                
                tabGO.TryGetComponent(out AbilityParamTabItem item);
                
                if (item)
                {
                    activeTabs.Add(item);
                    item.InitializeItem(abilityParam.tagColor, abilityParam.displayString);
                }
            }
            
        }

        public void Release()
        {
            foreach (var tab in activeTabs)
            {
                m_cachedTabs.Add(tab);
                tab.transform.ResetTransform(tabPool);
            }
            
            activeTabs.Clear();
        }

        #endregion
    }
}