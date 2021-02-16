using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin
{
    [System.Serializable]
    public class FaceData
    {
        public string version { get; set; }
        public string platform { get; set; }
        public float fps { get; set; }

        public string[] bonesNames { get; set; }
        public string[] blendShapesNames { get; set; }

        public FaceDataFrame[] frames { get; set; }
        public int[] triangles { get; set; }
    }
}