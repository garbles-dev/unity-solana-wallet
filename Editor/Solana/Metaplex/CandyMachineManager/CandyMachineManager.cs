using UnityEditor;
using UnityEngine;

namespace Solana.Unity.SDK.Editor 
{
    public class CandyMachineManager : EditorWindow 
    {

        #region Properties

        string configLocationPath;
        string rpc;
        Vector2 scrollViewPosition = Vector2.zero;
        bool showCandyMachines = false;

        #endregion

        #region MenuItem

        [MenuItem("Solana/Metaplex/Candy Machine")]
        public static void ShowWindow() {
            GetWindow(typeof(CandyMachineManager), false, "Candy Machine Manager");
        }

        #endregion

        #region Unity Messages

        private void OnGUI() {
            SolanaEditorUtility.FileSelectField(
                "Keypair",
                "",
                false,
                "Select a valid Keypair",
                "json"
            );
            configLocationPath = SolanaEditorUtility.FileSelectField(
                "Config Location",
                configLocationPath,
                true,
                "Select a config folder"
            );
            rpc = SolanaEditorUtility.RPCField(rpc);
            CandyMachineScrollView();
            if (GUILayout.Button("Create new Candy Machine")) {
                CandyMachineSetupWizard.OpenNew(configLocationPath);
            }
            if (GUILayout.Button("Import Candy Machine")) {
                Debug.Log("Launch finder and copy config.");
                Close();
            }
        }

        #endregion

        #region Private

        private void CandyMachineScrollView() {
            showCandyMachines = EditorGUILayout.BeginFoldoutHeaderGroup(showCandyMachines, "Existing Candy Machines");
            if (showCandyMachines) {
                scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition, SolanaEditorUtility.scrollViewStyle);
                MetaplexEditorUtility.CandyMachineField("");
                MetaplexEditorUtility.CandyMachineField("");
                MetaplexEditorUtility.CandyMachineField("");
                MetaplexEditorUtility.CandyMachineField("");
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        #endregion
    }
}