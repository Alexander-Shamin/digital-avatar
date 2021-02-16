using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin.Server
{
    public class Request<T>
    {
        public Data<T> data { get; set; }
    }

    public class Data<T>
    {
        public string id { get; set; }
        public string type { get; set; }
        public T attributes { get; set; }
    }

    public class AttributesATL
    {
        public ParamsATL atl;
    }

    public class ParamsATL
    {
        public ParamsATL()
        {
            // always unity
            platform = "unity";
        }

        public string platform { get; set; }
        public string rigVersion { get; set; }
        public string characterID { get; set; }
        public string engine { get; set; }
        public string answerFormat { get; set; }
        public string explicitEmotion { get; set; }

        public EyeMovementParams eyeMovementParameters { get; set; }
        public NeckMovementParams neckMovementParameters { get; set; }
    }

    public class EyeMovementParams
    {
        public string eyeMovementMode { get; set; }
    }

    public class NeckMovementParams
    {
        public string neckMovementMode { get; set; }
    }

    public class AttributesTTS
    {
        public ParamsTTS tts;
    }

    public class ParamsTTS
    {
        public string ttsToken { get; set; }
        public string ttsEngine { get; set; }
        public string voiceID { get; set; }
        public string text { get; set; }
        public string textType { get; set; }
    }

}