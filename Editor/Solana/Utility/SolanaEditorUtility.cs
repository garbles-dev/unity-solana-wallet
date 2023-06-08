using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Solana.Unity.SDK.Editor 
{
    public static class SolanaEditorUtility 
    { 

        #region Styling Constants

        public static readonly RectOffset standardMargin = new(5, 5, 2, 2);
        public static readonly RectOffset standardPadding = new(10, 10, 2, 2);
        public static readonly RectOffset scrollViewPadding = new(15, 15, 2, 2);

        public static readonly int headingFontSize = 16;

        #endregion
        
        #region GUIStyles

        public static readonly GUIStyle propLabelStyle = new(GUI.skin.label) {
            stretchWidth = false,
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            padding = standardPadding,
            margin = standardMargin
        };

        public static readonly GUIStyle headingLabelStyle = new(GUI.skin.label) {
            stretchWidth = true,
            fontStyle = FontStyle.Bold,
            fontSize = headingFontSize,
            padding = standardPadding,
            margin = standardMargin,
            wordWrap = true
        };

        public static readonly GUIStyle selectButtonStyle = new(GUI.skin.button) {
            stretchWidth = false,
            padding = standardPadding,
            margin = standardMargin
        };

        public static readonly GUIStyle fileSelectPathStyle = new(GUI.skin.box) {
            stretchWidth = true,
            wordWrap = false,
            alignment = TextAnchor.MiddleLeft,
            padding = standardPadding,
            margin = standardMargin,
            clipping = TextClipping.Clip
        };

        public static readonly GUIStyle textFieldStyle = new(GUI.skin.textField) {
            stretchWidth = true,
            alignment = TextAnchor.MiddleLeft,
            padding = standardPadding,
            margin = standardMargin
        };

        public static readonly GUIStyle scrollViewStyle = new(GUI.skin.scrollView) {
            padding = scrollViewPadding,
            stretchHeight = false,
        };

        #endregion

        #region Controls

        public static string RPCField(string currentRPC) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("RPC URL", propLabelStyle);
            var newRPC = EditorGUILayout.TextField(currentRPC, textFieldStyle);
            EditorGUILayout.EndHorizontal();
            return newRPC;
        }

        public static string FileSelectField(
            string labelText,
            string currentPath,
            bool inProjectResources,
            string explorerTitle = null,
            string extension = null
        ) {
            // Layout:
            var basePath = Path.GetDirectoryName(Application.dataPath + "/Resources/");
            string newPath = null;
            StaticTextProperty(labelText, currentPath, "Select", delegate {
                if (extension == null) 
                {
                    newPath = EditorUtility.OpenFolderPanel(explorerTitle, basePath, "");
                }
                else 
                {
                    newPath = EditorUtility.OpenFilePanel(explorerTitle, basePath, extension);
                }
            });
            if (newPath == null)
            {
                return currentPath;
            } 
            else if (inProjectResources && Path.GetRelativePath(basePath, newPath).StartsWith(".."))
            {
                Debug.LogError("Path must be inside the Resources folder.");
                return currentPath;
            }
            return inProjectResources ? Path.GetRelativePath(Application.dataPath, newPath) : newPath;
        }

        public static void StaticTextProperty(
            string label, 
            string value,
            string buttonText = null,
            Action buttonAction = null
        ) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, propLabelStyle);
            if (buttonText != null && buttonAction != null) {
                if (GUILayout.Button(buttonText, selectButtonStyle)) {
                    buttonAction?.Invoke();
                }
            }
            GUILayout.Box(value, fileSelectPathStyle);
            EditorGUILayout.EndHorizontal();
        }

        public static void Heading(string text, TextAnchor alignment)
        {
            var style = headingLabelStyle;
            style.alignment = alignment;
            GUILayout.Label(text, style);
        }

        #endregion
    }
}
