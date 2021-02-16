using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ARVRLab.UnityDH_Plugin.Server
{
    public class ServerAPI
    {
        public static string[] AvaliableEnginesATL
        {
            get
            {
                return new string[]
                {
                    "latest",
                    "elenasb_nvpattexp1-5955-52b68123",
                    "sdfanet_6192_lr01_wd00001_b64_e60_l1_lapa1_lapb1_ds1",
                    "sdfanetb_6199_lr02_wd00001_b64_e60_lh_lapa1_lapb1_naug",
                    "sdfanetd_4525_lr02_wd00001_b128_e120_l1_lapa1_lapb25_naug_ds8_bsw_ea6bd9c6",
                    "sdfanetb39ru_4369_lr02_wd00001_b64_e60_l1_lapa1_lapb2_naug_ds1_bsw_2ed8ff6c"
                };
            }
        }

        public static string[] AvaliableEmotionsATL
        {
            get
            {
                return new string[]
                {
                    "angry",
                    "calm",
                    "curious",
                    "disgust",
                    "doubt",
                    "dreamy",
                    "embarrassment",
                    "fear",
                    "flirt",
                    "happy",
                    "playful",
                    "positive",
                    "resentment",
                    "shame",
                    "sorrow",
                    "strict",
                    "supplication",
                    "surprise",
                    "wrath"
                };
            }
        }

        public static string[] EyesMovementModes
        {
            get
            {
                return new string[]
                {
                    "none",
                    "mocap"
                };
            }
        }

        public static string[] NeckMovementModes
        {
            get
            {
                return new string[]
                {
                    "none",
                    "mocap"
                };
            }
        }

        public static string[] AvaliableVoiceID
        {
            get
            {
                return new string[]
                {
                    "default",
                    "Che_HQ",
                    "Erm_HQ",
                    "She_HQ"
                };
            }
        }

        public static byte[] GenerateTTS(string serverUrl, ParamsTTS tts, string serverToken = null)
        {
            var request = GenerateTTSRequest(tts);
            var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            Uri baseUri = new Uri(serverUrl);
            Uri reqUri = new Uri(baseUri, "/dh/api/v1/tts");

            var webRequest = UnityWebRequest.Put(reqUri, json);
            if (!string.IsNullOrEmpty(serverToken))
                webRequest.SetRequestHeader("Authorization", "Bearer " + serverToken);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.method = "POST";

            var webRequestHandler = webRequest.SendWebRequest();
            while (!webRequestHandler.isDone) { }

            if (webRequest.isNetworkError)
            {
                Debug.LogError("TTS failed with network error: " + webRequest.error);
                return null;
            }

            if (webRequest.isHttpError)
            {
                var text = webRequest.downloadHandler.text;
                var errCode = webRequest.responseCode;

                Debug.LogError($"TTS failed with response code {errCode}. " +
                    $"Server returned message: {text}");
                return null;
            }

            var data = webRequest.downloadHandler.data;
            return data;
        }

        public static string GenerateLipSync(string serverUrl, ParamsATL atl, byte[] wavFile, string serverToken = null)
        {
            var request = GenerateATLRequest(atl);
            var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            Uri baseUri = new Uri(serverUrl);
            Uri reqUri = new Uri(baseUri, "/dh/api/v1/atl");

            var form = new WWWForm();
            form.AddBinaryData("audio", wavFile);
            form.AddField("json", json);

            var webRequest = UnityWebRequest.Post(reqUri, form);
            if (!string.IsNullOrEmpty(serverToken))
                webRequest.SetRequestHeader("Authorization", "Bearer " + serverToken);
            var webRequestHandler = webRequest.SendWebRequest();
            while (!webRequestHandler.isDone) { }

            if (webRequest.isNetworkError)
            {
                Debug.LogError("ATL failed with network error: " + webRequest.error);
                return null;
            }

            if (webRequest.isHttpError)
            {
                var text = webRequest.downloadHandler.text;
                var errCode = webRequest.responseCode;

                Debug.LogError($"TTS failed with response code {errCode}. " +
                    $"Server returned message: {text}");
                return null;
            }

            var data = webRequest.downloadHandler.text;
            return data;
        }

        public static object GenerateATLRequest(ParamsATL atl)
        {
            var request = new Request<AttributesATL>
            {
                data = new Data<AttributesATL>
                {
                    id = Guid.NewGuid().ToString(),
                    type = "audioToLipSync",
                    attributes = new AttributesATL()
                    {
                        atl = atl
                    }
                }
            };

            return request;
        }

        public static Request<AttributesTTS> GenerateTTSRequest(ParamsTTS paramsTTS)
        {
            var request = new Request<AttributesTTS>
            {
                data = new Data<AttributesTTS>
                {
                    id = Guid.NewGuid().ToString(),
                    type = "textToSpeech",
                    attributes = new AttributesTTS()
                    {
                        tts = paramsTTS
                    }
                }
            };

            return request;
        }
    }
}
