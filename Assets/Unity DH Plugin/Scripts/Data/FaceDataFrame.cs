using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin
{
    [System.Serializable]
    public class BoneTransform
    {
        public float[] pos { get; set; }
        public float[] eul { get; set; }

        public Vector3 Position
        {
            get
            {
                return pos != null ? new Vector3(pos[0], pos[1], pos[2]) : Vector3.zero;
            }
        }

        public Vector3 Euler
        {
            get
            {
                return eul != null ? new Vector3(eul[0], eul[1], eul[2]) : Vector3.zero;
            }
            set
            {
                eul = new float[] { value.x, value.y, value.z }; 
            }
        }

        public Pose Transform
        {
            get
            {
                return new Pose(Position, Quaternion.Euler(Euler));
            }
        }
    }

    [System.Serializable]
    public class FaceDataFrame
    {
        public long timestamp { get; set; }
        public float[] blendshapes { get; set; }
        public float[] points { get; set; }
        public BoneTransform[] bones { get; set; }

        public float[] cameraTransform { get; set; }
        public float[] faceTransform { get; set; }
        public float[] rightEyeTransform { get; set; }
        public float[] leftEyeTransform { get; set; }

        public Vector3[] PointsV3()
        {
            var list = new List<Vector3>();
            for (int i = 0; i < points.Length; i += 3)
            {
                var vec = new Vector3(points[i], points[i + 1], points[i + 2]);
                list.Add(vec);
            }

            return list.ToArray();
        }

        public static float[] ToFloatArray(Vector3[] arr)
        {
            var floatList = new List<float>();
            for (int i = 0; i < arr.Length; i++)
            {
                floatList.Add(arr[i].x);
                floatList.Add(arr[i].y);
                floatList.Add(arr[i].z);
            }

            return floatList.ToArray();
        }
    }
}