using MeepleEditor.CustomTools;
using UnityEditor;
using UnityEngine;

namespace MeepleEditor.MenuBar
{
    public class MeepleTools
    {
//required: if this statement is not place, the build will compile with this
#if UNITY_EDITOR

        private const string mainMeeplPath = "Meeple Tools";
        private const string scriptableToolSubPath = "/Scriptable Object Tool";
        private const string csvToolSubPath = "/CSV Tool";

        private const string scriptablePath = mainMeeplPath + scriptableToolSubPath;
        private const string csvPath = mainMeeplPath + csvToolSubPath;
        
        
        [MenuItem(scriptablePath + "/Scriptable Object Generation")]
        private static void CreateScriptableObjectWindow()
        {
            EditorWindow newWindow = EditorWindow.GetWindow<ScriptableObjectCreation>();
            newWindow.titleContent = new GUIContent("Scriptable Object Creation Tools");
        }

        //[MenuItem(PathName, priority = #)] priority will sort itself in the menu area
        // if the gap is more than 10, it will create a nice gap
        [MenuItem(csvPath + "/CSV Converter", priority = 11)]
        private static void AddNewWeaponsOfType()
        {
            //TODO: change to csv converter
            CreateCustomWindow();
        }


        private static void CreateScriptableObjectCreationWindow()
        {
            EditorWindow newWindow = EditorWindow.GetWindow<ScriptableObjectCreation>();
            newWindow.titleContent = new GUIContent("Scriptable Object Creation Tools");
        }

        private static void CreateCustomWindow()
        {
            EditorWindow newWindow = EditorWindow.GetWindow<MeepleWeaponTools>();
            newWindow.titleContent = new GUIContent("Meeple Custom Tools");
        }
        
#endif
    }
}