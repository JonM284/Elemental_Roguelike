﻿using System;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.UI
{
    public class UIPopupDialog: UIBase
    {
        #region Serialized Fields

        [SerializeField] private TMP_Text title;

        [SerializeField] private TMP_Text description;

        #endregion

        #region Accessors

        public PopupDialogData data { get; private set; }

        #endregion
        
        #region UIBase Inherited Methods

        public override void AssignArguments(params object[] _arguments)
        {
            data = _arguments[0] as PopupDialogData;
            title.text = data.popupTitle;
            description.text = data.popupDescription;
            if (_arguments.Length >= 3 && _arguments[2] != null)
            {
                m_confirmAction = _arguments[2] as Action;
            }

            if (_arguments.Length >= 4 && _arguments[3] != null)
            {
                m_closeAction = _arguments[3] as Action;
            }
        }

        #endregion
        
    }
}