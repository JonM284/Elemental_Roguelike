using System;
using System.Collections.Generic;
using System.Linq;
using Data.Elements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MeepleEditor.CustomTools
{
    public class MeepleWeaponTools: EditorWindow
    {
        public void CreateGUI()
        {
            rootVisualElement.Add(new Label("All Element Types"));
            var elementTypesGUIDs = AssetDatabase.FindAssets("t:ElementTyping");
            var allElementTypes = new List<ElementTyping>();
            foreach (var guid in elementTypesGUIDs)
            {
                allElementTypes.Add(AssetDatabase.LoadAssetAtPath<ElementTyping>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            var splitView = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);
            
            rootVisualElement.Add(splitView);

            var leftPane = new ListView();
            splitView.Add(leftPane);
            var rightPane = new VisualElement();
            splitView.Add(rightPane);
            
            leftPane.makeItem = () => new Label();
            leftPane.bindItem = (item, index) => { ((Label)item).text = allElementTypes[index].name; };
            leftPane.itemsSource = allElementTypes;
        }
    }
}