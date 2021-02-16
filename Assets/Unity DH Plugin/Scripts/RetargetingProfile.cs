using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin
{
    public enum RetargetFlag
    {
        NONE,
        REPLACE_R_L
    }

    [System.Serializable]
    public class RetargetBlendshape
    {
        public string jsonName;
        public string unityName;
    }

    [CreateAssetMenu(menuName = "Digital Human/Retarget Profile")]
    public class RetargetingProfile : ScriptableObject
    {
        public RetargetFlag flag;
        public RetargetBlendshape[] retarget;
    }
}