using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace ARVRLab.UnityDH_Plugin
{
    /// <summary>
    /// Скрипт копирует значения блендшейпов с тем же именем
    /// И применяет их каждый кадр
    /// </summary>
    public class CopyShapes : MonoBehaviour
    {
        public SkinnedMeshRenderer referenceMesh;
        private SkinnedMeshRenderer mMesh;

        private Dictionary<int, int> referenceToMine = new Dictionary<int, int>();

        // Start is called before the first frame update
        void Start()
        {
            var refMesh = referenceMesh.sharedMesh;
            var refMeshDict = GenerateDict(refMesh);

            mMesh = GetComponent<SkinnedMeshRenderer>();
            var myMesh = mMesh.sharedMesh;
            var myMeshDict = GenerateDict(myMesh);

            // находим свои блендшейпы в чужом меше
            var referenceToMineSelect = (from id in refMeshDict.Keys
                                         where HasBlendshape(myMeshDict.Keys, id) != null
                                         let value1 = refMeshDict[id]
                                         let value2 = myMeshDict[HasBlendshape(myMeshDict.Keys, id)]
                                         select new { value1, value2 });
            referenceToMine = referenceToMineSelect.ToDictionary(k => k.value1, k => k.value2);

        }

        public string HasBlendshape(IEnumerable<string> keys, string name)
        {
            foreach (var key in keys)
            {
                if (key == name)
                    return key;
                else
                {
                    var lastDotName = name.LastIndexOf('.');
                    if (lastDotName < 0)
                        continue;

                    var nameSubstr = name.Substring(lastDotName);

                    var lastDotKey = key.LastIndexOf('.');
                    if (lastDotKey < 0)
                        continue;

                    var keySubsr = key.Substring(lastDotKey);
                    if (nameSubstr == keySubsr)
                        return key;
                }

            }
            return null;
        }

        public static Dictionary<string, int> GenerateDict(Mesh refMesh)
        {
            var refMeshDict = new Dictionary<string, int>();
            for (int i = 0; i < refMesh.blendShapeCount; i++)
            {
                var name = refMesh.GetBlendShapeName(i);
                var dotIndex = name.IndexOf('.');
                var shortName = name.Substring(dotIndex + 1);

                refMeshDict.Add(shortName, i);
            }

            return refMeshDict;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            foreach (var pair in referenceToMine)
            {
                var blendVal = referenceMesh.GetBlendShapeWeight(pair.Key);
                mMesh.SetBlendShapeWeight(pair.Value, blendVal);
            }
        }
    }
}