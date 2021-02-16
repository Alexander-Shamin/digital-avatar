using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARVRLab.UnityDH_Plugin
{
    public class JsonPlayer : MonoBehaviour
    {
        public static bool Verbose = false;

        public UniversalPlayer player;
        public bool useInterpolation = true;

        [Header("Rig")]
        public SkinnedMeshRenderer target;
        public string meshBlendshapePrefix;
        public RetargetingProfile profile;
        public bool animateBones = true;

        [Header("Json Data")]
        public JsonNamingVersion namingVersion = JsonNamingVersion.STANDARD;
        [SerializeField]
        private TextAsset initialText = null;
        [SerializeField]
        private bool isLoop = false;

        private List<int> jsonToUnityBlendMap = null;
        private List<int> jsonToUnityBoneMap = null;

        private FaceData currentData = null;
        private NamedBoneComponent[] bones;

        public event System.Action OnStartPlaying, OnFinishPlaying;

        public int CurrentFrame { get; private set; }

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            bones = GetComponentsInChildren<NamedBoneComponent>();
        }

        private void Start()
        {
            if (initialText)
            {
                SetJsonToPlay(initialText.text, player, loop: isLoop);
            }
        }

        private void PrepareBlendshapesMapping()
        {
            if (currentData.blendShapesNames != null)
            {
                jsonToUnityBlendMap = new List<int>();

                for (int i = 0; i < currentData.blendShapesNames.Length; i++)
                {
                    var shapeName = currentData.blendShapesNames[i];
                    var reformedShapeName = JsonParsing.JsonToUnityBlendshapeName(shapeName, namingVersion, meshBlendshapePrefix, profile);

                    var index = target.sharedMesh.GetBlendShapeIndex(reformedShapeName);
                    if (index == -1 && Verbose)
                    {
                        Debug.LogWarning($"Can't find blendshape {reformedShapeName} in {target.gameObject.name} GameObject. " +
                            $"Check blendshape prefix or change model blendshape name");
                    }

                    jsonToUnityBlendMap.Add(index);
                }
            }
        }

        private void PrepareBonesMapping()
        {
            if (animateBones && currentData.bonesNames != null)
            {
                jsonToUnityBoneMap = new List<int>();
                for (int i = 0; i < currentData.bonesNames.Length; i++)
                {
                    var jsonBoneName = currentData.bonesNames[i];
                    var unityBoneIndex = Array.FindIndex(bones, bone => bone.boneName == jsonBoneName);
                    if (unityBoneIndex == -1 && Verbose)
                    {
                        Debug.LogWarning($"Can't find {jsonBoneName} bone. " +
                            $"Add NamedBoneComponet to {jsonBoneName} GameObject");
                    }

                    jsonToUnityBoneMap.Add(unityBoneIndex);
                }
            }
        }

        private void UpdateBones(FaceDataFrame curFrame, FaceDataFrame nextFrame, float frameDif)
        {
            if (!animateBones || currentData.bonesNames == null)
                return;

            for (int i = 0; i < currentData.bonesNames.Length; i++)
            {
                int index = jsonToUnityBoneMap[i];
                if (index == -1)
                {
                    continue;
                }

                Quaternion boneRotation;
                if (!useInterpolation)
                {
                    boneRotation = curFrame.bones[i].Transform.rotation;
                }
                else
                {
                    var curBoneRotation = curFrame.bones[i].Transform.rotation;
                    var nextBoneRotation = nextFrame.bones[i].Transform.rotation;

                    boneRotation = Quaternion.Lerp(curBoneRotation, nextBoneRotation, frameDif);
                }

                var bone = bones[index];
                bone.transform.localRotation = bone.ApplyBoneTransformation(boneRotation);
            }
        }

        private void UpdateBlendshapes(FaceDataFrame curFrame, FaceDataFrame nextFrame, float frameDif)
        {
            for (int i = 0; i < currentData.blendShapesNames.Length; i++)
            {
                int index = jsonToUnityBlendMap[i];
                if (index == -1)
                {
                    continue;
                }

                float blendshapeVal;
                if (!useInterpolation)
                {
                    blendshapeVal = curFrame.blendshapes[i];
                }
                else
                {
                    var curVal = curFrame.blendshapes[i];
                    var nextVal = nextFrame.blendshapes[i];

                    blendshapeVal = Mathf.Lerp(curVal, nextVal, frameDif);
                }

                target.SetBlendShapeWeight(index, blendshapeVal);
            }
        }

        public void SetJsonToPlay(string json, UniversalPlayer linkedPlayer = null, float timeToSwitch = 0f, bool loop = false)
        {
            SetAnimationToPlay(JsonAnimationConverter.JsonToData(json), linkedPlayer, timeToSwitch, loop);
        }

        public void SetAnimationToPlay(FaceData currentData, UniversalPlayer linkedPlayer = null, float timeToSwitch = 0f, bool loop = false)
        {
            IsPlaying = true;

            this.player = linkedPlayer;
            this.currentData = currentData;
            this.isLoop = loop;

            if (jsonToUnityBlendMap == null)
                PrepareBlendshapesMapping();
            if (jsonToUnityBoneMap == null)
                PrepareBonesMapping();

            StopAllCoroutines();
            StartCoroutine(PlayingRoutine(timeToSwitch));
        }

        #region Blending Routines
        private FaceDataFrame GenerateCurrentFrame()
        {
            float[] curBS = null;
            if (jsonToUnityBlendMap != null)
            {
                curBS = new float[jsonToUnityBlendMap.Count];

                for (int i = 0; i < curBS.Length; i++)
                {
                    var bsUnityIndex = jsonToUnityBlendMap[i];
                    curBS[i] = target.GetBlendShapeWeight(bsUnityIndex);
                }

            }

            BoneTransform[] curBones = null;
            if (jsonToUnityBoneMap != null)
            {
                curBones = new BoneTransform[jsonToUnityBoneMap.Count];
                for (int i = 0; i < curBones.Length; i++)
                {
                    var boneUnityIndex = jsonToUnityBoneMap[i];
                    if (boneUnityIndex == -1)
                        continue;

                    curBones[i] = new BoneTransform()
                    {
                        Euler = bones[boneUnityIndex].BoneWithoutExtraRotation().eulerAngles
                    };
                }

            }

            return new FaceDataFrame()
            {
                blendshapes = curBS,
                bones = curBones
            };
        }

        private FaceDataFrame GenerateIdleFrame()
        {
            float[] curBS = null;
            if (jsonToUnityBlendMap != null)
                curBS = Enumerable.Repeat(0f, jsonToUnityBlendMap.Count).ToArray();
            BoneTransform[] curBones = null;
            if (jsonToUnityBoneMap != null)
                curBones = Enumerable.Repeat(new BoneTransform(), jsonToUnityBoneMap.Count).ToArray();

            return new FaceDataFrame()
            {
                blendshapes = curBS,
                bones = curBones
            };
        }

        private IEnumerator BlendTo(float blendingTime, FaceDataFrame targetFrame)
        {
            var initFrame = GenerateCurrentFrame();

            float curTime = 0f;
            while (curTime < blendingTime)
            {
                float frameDif = curTime / blendingTime;
                UpdateBlendshapes(initFrame, targetFrame, frameDif);
                UpdateBones(initFrame, targetFrame, frameDif);

                curTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator PlayAnimation()
        {
            var startTime = Time.time;

            while (true)
            {
                // calculate current frame
                float frameT;
                if (player != null)
                    frameT = currentData.frames.Length * player.percentTime;
                else
                {
                    var timePassed = Time.time - startTime;
                    frameT = timePassed * currentData.fps;
                }

                // check if frame index is valid
                var frameIndex = Mathf.FloorToInt(frameT);
                if (frameIndex < 0 || frameIndex >= currentData.frames.Length)
                    yield break;
                var curFrame = currentData.frames[frameIndex];

                // calculate current percent between frames
                var frameDif = frameT - Mathf.Floor(frameT);

                // calculate next frame index
                int nextIndex;
                if (frameIndex != currentData.frames.Length - 1)
                    nextIndex = frameIndex + 1;
                else
                    nextIndex = frameIndex;
                var nextFrame = currentData.frames[nextIndex];

                // update all blendshapes
                UpdateBlendshapes(curFrame, nextFrame, frameDif);
                UpdateBones(curFrame, nextFrame, frameDif);

                yield return null;
            }
        }

        private IEnumerator PlayingRoutine(float blendingTime)
        {
            // blend to the new animation first frame
            var firstFrame = currentData.frames.FirstOrDefault();
            if (firstFrame == null)
                yield break;

            yield return BlendTo(blendingTime, firstFrame);

            // blending complete - start playing sound
            player?.Play();
            OnStartPlaying?.Invoke();

            // play animation
            do
            {
                yield return PlayAnimation();
            }
            while (isLoop);

            // move to idle frame
            var idleFrame = GenerateIdleFrame();
            yield return BlendTo(blendingTime, idleFrame);

            // animation complete
            OnFinishPlaying?.Invoke();
        }
        #endregion
    }
}