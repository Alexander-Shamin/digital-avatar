using ARVRLab.UnityDH_Plugin.Server;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin.Editor
{
    public class GenerateLipSync : EditorWindow
    {
        private string serverAdress = "http://127.0.0.1:5000";
        private string dhToken;
        private string wavPath;
        private int selectedEngineATL;
        private int selectedEmotionsATL;
        private int eyesMode;
        private int neckMode;

        private void Awake()
        {
            serverAdress = EditorPrefs.GetString("TTS_server_adress");
            dhToken = EditorPrefs.GetString("DHKey");
            eyesMode = EditorPrefs.GetInt("TTS_selected_eyes_mode");
            neckMode = EditorPrefs.GetInt("TTS_selected_neck_mode");
        }

        private void OnDestroy()
        {
            EditorPrefs.SetString("TTS_server_adress", serverAdress);
            EditorPrefs.SetString("DHKey", dhToken);
            EditorPrefs.SetInt("TTS_selected_eyes_mode", eyesMode);
            EditorPrefs.SetInt("TTS_selected_neck_mode", neckMode);
        }

        [MenuItem("Digital Human/Generate lip sync")]
        public static void ShowWindow()
        {
            var window = GetWindow<GenerateLipSync>(true, "Generate LipSync for audio");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Server URL:");
            serverAdress = GUILayout.TextField(serverAdress);

            GUILayout.Label("DH token:");
            dhToken = GUILayout.TextField(dhToken);

            GUILayout.Space(10);
            GUILayout.Label("Emotion:");
            selectedEmotionsATL = EditorGUILayout.Popup(selectedEmotionsATL, ServerAPI.AvaliableEmotionsATL);

            GUILayout.Label("Engine ID:");
            selectedEngineATL = EditorGUILayout.Popup(selectedEngineATL, ServerAPI.AvaliableEnginesATL);

            GUILayout.Space(5);

            GUILayout.Label("Eyes movement:");
            eyesMode = EditorGUILayout.Popup(eyesMode, ServerAPI.EyesMovementModes);

            GUILayout.Label("Neck movement:");
            neckMode = EditorGUILayout.Popup(neckMode, ServerAPI.NeckMovementModes);

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Lip Sync"))
            {
                wavPath = EditorUtility.OpenFilePanel("Select audio file", wavPath, "wav");
                if (!string.IsNullOrEmpty(wavPath))
                {
                    var wav = File.ReadAllBytes(wavPath);

                    var atlParams = new ParamsATL()
                    {
                        engine = ServerAPI.AvaliableEnginesATL[selectedEngineATL],
                        explicitEmotion = ServerAPI.AvaliableEmotionsATL[selectedEmotionsATL],
                        eyeMovementParameters = new EyeMovementParams() { eyeMovementMode = ServerAPI.EyesMovementModes[eyesMode] },
                        neckMovementParameters = new NeckMovementParams() { neckMovementMode = ServerAPI.NeckMovementModes[neckMode] }
                    };

                    var lipsync = ServerAPI.GenerateLipSync(serverAdress, atlParams, wav, dhToken);
                    if (lipsync != null)
                    {
                        var jsonAnimPath = Path.ChangeExtension(wavPath, "json");
                        if (string.IsNullOrEmpty(jsonAnimPath))
                            return;

                        File.WriteAllText(jsonAnimPath, lipsync);
                        AssetDatabase.Refresh();
                    }
                }
            }

        }
    }
}