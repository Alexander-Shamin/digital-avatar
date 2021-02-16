using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ARVRLab.UnityDH_Plugin
{
    public class PoseCurve
    {
        public AnimationCurve[] rotation;
    }

    public class JsonAnimationConverter
    {
        private static float DefaultFPS = 25f;

        public static FaceData JsonToData(string json)
        {
            return JsonConvert.DeserializeObject<FaceData>(json);
        }

        public static AnimationCurve[] GenerateBlendshapeCurves(FaceData parsedData)
        {
            float fps = parsedData.fps != 0f ? parsedData.fps : DefaultFPS;
            var blendshapeNames = parsedData.blendShapesNames;
            var curves = new AnimationCurve[blendshapeNames.Length];
            var frames = parsedData.frames;

            for (int i = 0; i < curves.Length; i++)
            {
                var animationCurve = new AnimationCurve();

                for (int j = 0; j < frames.Length; j++)
                {
                    var frame = frames[j];
                    float time = j / fps;
                    float value = frame.blendshapes[i];

                    // TODO: add support for custom curve interpolation
                    var keyframe = new Keyframe(time, value);
                    animationCurve.AddKey(keyframe);
                }

                curves[i] = animationCurve;
            }

            return curves;
        }

        public static PoseCurve[] GenerateBonesCurves(FaceData parsedData, Animator targetAnim)
        {
            float fps = parsedData.fps != 0f ? parsedData.fps : DefaultFPS;
            var bonesNames = parsedData.bonesNames;
            var curves = new List<PoseCurve>();
            var frames = parsedData.frames;

            for (int i = 0; i < bonesNames.Length; i++)
            {
                var poseCurve = new PoseCurve();
                var rotCurves = new List<AnimationCurve>();

                var boneName = bonesNames[i];
                var targetBone = FindBone(targetAnim.transform, boneName);
                if (targetBone == null)
                {
                    Debug.LogWarning($"Can't find {boneName} bone. " +
                        $"Add NamedBoneComponet to {boneName} GameObject");
                    continue;
                }

                // record each roation as 4 curves
                // in quaternion xyzw order
                for (int ax = 0; ax < 4; ax++)
                {
                    var animationCurve = new AnimationCurve();

                    for (int j = 0; j < frames.Length; j++)
                    {
                        var frame = frames[j];
                        var rot = frame.bones[i].Transform.rotation;
                        var fRot = targetBone.ApplyBoneTransformation(rot);

                        float time = j / fps;
                        var value = fRot[ax];

                        // TODO: add support for custom curve interpolation
                        var keyframe = new Keyframe(time, value);
                        animationCurve.AddKey(keyframe);
                    }

                    rotCurves.Add(animationCurve);
                }

                poseCurve.rotation = rotCurves.ToArray();
                curves.Add(poseCurve);
            }

            return curves.ToArray();

        }

        public static AnimationClip JsonToAnimationClip(string json, Animator targetAnim, 
            SkinnedMeshRenderer targetMesh, string blendshapesPrefix, RetargetingProfile profile)
        {
            var parsedData = JsonToData(json);

            var animationClip = new AnimationClip();
            float fps = parsedData.fps != 0f ? parsedData.fps : DefaultFPS;
            animationClip.frameRate = DefaultFPS;

            var frames = parsedData.frames;

            var bsCurves = GenerateBlendshapeCurves(parsedData);
            var bsNames = parsedData.blendShapesNames;
            for (int i = 0; i < bsNames.Length; i++)
            {
                // check if this blendshape exist
                var shapeName = bsNames[i];
                var reformedShapeName = JsonParsing.JsonToUnityBlendshapeName(shapeName, JsonNamingVersion.STANDARD, blendshapesPrefix, profile);
                var index = targetMesh.sharedMesh.GetBlendShapeIndex(reformedShapeName);
                if (index == -1)
                {
                    Debug.LogWarning($"Can't find blendshape {reformedShapeName} in {targetMesh} GameObject. " +
                        $"Check blendshape prefix or change model blendshape name");
                    continue;
                }

                // check if atl used this blendshape
                if (!HasAnimationFrames(frames, i))
                {
                    continue;
                }

                var animationCurve = bsCurves[i];
                var unityShapeName = "blendShape." + reformedShapeName;

                var objPath = TraceParent(targetMesh.transform, targetAnim.transform);
                animationClip.SetCurve(objPath, typeof(SkinnedMeshRenderer), unityShapeName, animationCurve);
            }

            var boneCurves = GenerateBonesCurves(parsedData, targetAnim);
            var boneNames = parsedData.bonesNames;
            for (int i = 0; i < boneCurves.Length; i++)
            {
                var boneName = boneNames[i];
                var boneCurve = boneCurves[i];

                var targetBone = FindBone(targetAnim.transform, boneName);
                if (targetBone == null)
                {
                    continue;
                }

                var objPath = TraceParent(targetBone.transform, targetAnim.transform);

                animationClip.SetCurve(objPath, typeof(Transform), "localRotation.x", boneCurve.rotation[0]);
                animationClip.SetCurve(objPath, typeof(Transform), "localRotation.y", boneCurve.rotation[1]);
                animationClip.SetCurve(objPath, typeof(Transform), "localRotation.z", boneCurve.rotation[2]);
                animationClip.SetCurve(objPath, typeof(Transform), "localRotation.w", boneCurve.rotation[3]);
            }

            animationClip.EnsureQuaternionContinuity();
            return animationClip;
        }

        private static bool HasAnimationFrames(FaceDataFrame[] allFrames, int index)
        {
            foreach (var frame in allFrames)
            {
                if (frame.blendshapes[index] > 0.1f)
                {
                    return true;
                }
            }

            return false;
        }

        private static string TraceParent(Transform child, Transform finalParent)
        {
            var queue = new Queue<Transform>();
            var curElement = child;

            while (curElement != finalParent)
            {
                queue.Enqueue(curElement);
                curElement = curElement.parent;
                if (curElement == null)
                    throw new Exception("Child doesn't belong to parent!");
            }

            var pathTr = queue.ToArray().Reverse();
            var path = "";
            foreach (var tr in pathTr)
                path += tr.name + '/';

            if (path.Length > 0)
                path = path.Remove(path.Length - 1);
            return path;
        }

        private static NamedBoneComponent FindBone(Transform parent, string name)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                var bone = c.GetComponent<NamedBoneComponent>();

                if (bone != null && bone.boneName == name)
                    return bone;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }
    }
}