using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamedBoneComponent : MonoBehaviour
{
    public string boneName;
    public Vector3 extraRotation;
    public bool mirror;

    public Quaternion ApplyBoneTransformation(Quaternion boneRotation)
    {
        var rot = Quaternion.Euler(extraRotation) * boneRotation;
        if (mirror)
        {
            rot.y = -rot.y;
            rot.z = -rot.z;
        }

        return rot;
    }
    
    public Quaternion BoneWithoutExtraRotation()
    {
        return Quaternion.Inverse(Quaternion.Euler(extraRotation)) 
            * transform.localRotation;
    }
}
