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
        private const string weaponSubPath = "/Weapon";

        private const string weaponPath = mainMeeplPath + weaponSubPath;
        
        [MenuItem(weaponPath + "/Weapon Generation")]
        private static void CreateAllNewWeapons()
        {
            CreateCustomWindow();
        }

        //[MenuItem(PathName, priority = #)] priority will sort itself in the menu area
        // if the gap is more than 10, it will create a nice gap
        [MenuItem(weaponPath + "/other weapon thing", priority = 11)]
        private static void AddNewWeaponsOfType()
        {
            CreateCustomWindow();
        }


        private static void CreateCustomWindow()
        {
            EditorWindow newWindow = EditorWindow.GetWindow<MeepleWeaponTools>();
            newWindow.titleContent = new GUIContent("Meeple Custom Tools");
        }
        
#endif
    }
}