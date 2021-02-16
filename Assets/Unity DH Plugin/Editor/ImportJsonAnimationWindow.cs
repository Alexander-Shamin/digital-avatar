using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin.Editor
{
    public class ImportJsonAnimation : EditorWindow
    {
        private string jsonPath;
        private string exportPath;
        private Animator selectedAnimator;
        private SkinnedMeshRenderer selectedMesh;
        private string blendshapesPrefix;
        private RetargetingProfile profile;

        [MenuItem("Digital Human/Import json animation")]
        public static void ShowWindow()
        {
            var window = GetWindow<ImportJsonAnimation>(true);
            window.Show();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Select json path"))
            {
                jsonPath = EditorUtility.OpenFilePanel("Import json path", jsonPath, "json");
            }

            if (File.Exists(jsonPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(jsonPath);
                GUILayout.Label(fileName);

                selectedAnimator = EditorGUILayout.ObjectField("Animator", 
                    selectedAnimator, typeof(Animator), true) as Animator;
                selectedMesh = EditorGUILayout.ObjectField("Face mesh",
                    selectedMesh, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

                blendshapesPrefix = EditorGUILayout.TextField("Blendshape prefix", blendshapesPrefix);
                profile = EditorGUILayout.ObjectField("Retargeting", profile, typeof(RetargetingProfile), 
                    allowSceneObjects: false) as RetargetingProfile;

                if (GUILayout.Button("Export directory"))
                {
                    var absPath = EditorUtility.OpenFolderPanel("Select directory to save .anim", exportPath, "");
                    var assetsIndex = absPath.IndexOf("Assets");
                    exportPath = absPath.Substring(assetsIndex);
                }
                if (!string.IsNullOrEmpty(exportPath))
                {
                    GUILayout.Label(exportPath);
                }

                if (selectedAnimator && selectedMesh && exportPath != null)
                {

                    if (GUILayout.Button("Start conversion"))
                    {
                        var json = File.ReadAllText(jsonPath);
                        var clip = JsonAnimationConverter.JsonToAnimationClip(json,
                            selectedAnimator, selectedMesh, blendshapesPrefix, profile);

                        var finalPath = Path.Combine(exportPath, fileName + ".anim");
                        AssetDatabase.CreateAsset(clip, finalPath);
                        AssetDatabase.Refresh();
                    }


                }
            }
        }
    }
}


