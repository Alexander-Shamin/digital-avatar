using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARVRLab.UnityDH_Plugin.Server;
using UnityEditor;
using System.IO;

namespace ARVRLab.UnityDH_Plugin.Editor
{
    public class GeneratePhrase : EditorWindow
    {
        private string serverAdress = "http://127.0.0.1:5000";
        private string dhToken;
        private string ttsToken;
        private string text;
        private string pathToSave;
        private bool generateLipSync;
        private int selectedEngineATL;
        private int selectedEmotionsATL;
        private int selectedVoiceID;
        private int eyesMode;
        private int neckMode;

        [MenuItem("Digital Human/Generate phrase", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<GeneratePhrase>(true, "Generate audio and LipSync");
            window.Show();
        }

        private void Awake()
        {
            ttsToken = EditorPrefs.GetString("TTSKey");
            dhToken = EditorPrefs.GetString("DHKey");
            text = EditorPrefs.GetString("TTS_text");
            serverAdress = EditorPrefs.GetString("TTS_server_adress");
            pathToSave = EditorPrefs.GetString("TTS_path_to_save");
            generateLipSync = EditorPrefs.GetBool("TTS_generateLipSync");
            selectedEngineATL = EditorPrefs.GetInt("TTS_selected_atl_engine");
            selectedEmotionsATL = EditorPrefs.GetInt("TTS_selected_atl_emotions");
            selectedVoiceID = EditorPrefs.GetInt("TTS_selected_voice_id");
            eyesMode = EditorPrefs.GetInt("TTS_selected_eyes_mode");
            neckMode = EditorPrefs.GetInt("TTS_selected_neck_mode");
        }

        private void OnDestroy()
        {
            EditorPrefs.SetString("TTSKey", ttsToken);
            EditorPrefs.SetString("DHKey", dhToken);
            EditorPrefs.SetString("TTS_text", text);
            EditorPrefs.SetString("TTS_server_adress", serverAdress);
            EditorPrefs.SetString("TTS_path_to_save", pathToSave);
            EditorPrefs.SetBool("TTS_generateLipSync", generateLipSync);
            EditorPrefs.SetInt("TTS_selected_atl_engine", selectedEngineATL);
            EditorPrefs.SetInt("TTS_selected_atl_emotions", selectedEmotionsATL);
            EditorPrefs.SetInt("TTS_selected_voice_id", selectedVoiceID);
            EditorPrefs.SetInt("TTS_selected_eyes_mode", eyesMode);
            EditorPrefs.SetInt("TTS_selected_neck_mode", neckMode);
        }

        private void OnGUI()
        {
            GUILayout.Label("Server URL:");
            serverAdress = GUILayout.TextField(serverAdress);

            GUILayout.Label("DH token:");
            dhToken = GUILayout.TextField(dhToken);

            GUILayout.Label("TTS token:");
            ttsToken = GUILayout.TextField(ttsToken);

            GUILayout.Label("Text:");
            text = GUILayout.TextArea(text);

            GUILayout.Label("Voice ID:");
            selectedVoiceID = EditorGUILayout.Popup(selectedVoiceID, ServerAPI.AvaliableVoiceID);

            GUILayout.Space(20);
            generateLipSync = GUILayout.Toggle(generateLipSync, "Generate Lip Sync?");

            if (generateLipSync)
            {
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
            }

            var isReady = !string.IsNullOrEmpty(serverAdress) && !string.IsNullOrEmpty(text);

            GUI.enabled = isReady;
            if (GUILayout.Button("Generate"))
            {
                var tts = new ParamsTTS()
                {
                    ttsToken = ttsToken,
                    text = text,
                    ttsEngine = "latest",
                    voiceID = ServerAPI.AvaliableVoiceID[selectedVoiceID]
                };

                var wav = ServerAPI.GenerateTTS(serverAdress, tts, dhToken);
                if (wav != null)
                {
                    string lipsync = null;
                    if (generateLipSync)
                    {
                        var atlParams = new ParamsATL()
                        {
                            engine = ServerAPI.AvaliableEnginesATL[selectedEngineATL],
                            explicitEmotion = ServerAPI.AvaliableEmotionsATL[selectedEmotionsATL],
                            eyeMovementParameters = new EyeMovementParams() { eyeMovementMode = ServerAPI.EyesMovementModes[eyesMode]},
                            neckMovementParameters = new NeckMovementParams() { neckMovementMode = ServerAPI.NeckMovementModes[neckMode] }
                        };
                        lipsync = ServerAPI.GenerateLipSync(serverAdress, atlParams, wav, dhToken);
                    }

                    pathToSave = EditorUtility.SaveFilePanelInProject("Save file to...", "pharace.wav", "wav", "");
                    if (string.IsNullOrEmpty(pathToSave))
                        return;

                    File.WriteAllBytes(pathToSave, wav);

                    if (lipsync != null)
                    {
                        var jsonAnimPath = Path.ChangeExtension(pathToSave, "json");
                        File.WriteAllText(jsonAnimPath, lipsync);
                    }

                    AssetDatabase.Refresh();
                }
            }
            GUI.enabled = true;
        }
    }
}