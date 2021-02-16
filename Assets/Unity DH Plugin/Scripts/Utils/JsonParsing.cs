
using System;
using System.Linq;

namespace ARVRLab.UnityDH_Plugin
{
    public enum JsonNamingVersion
    {
        LEGACY = 1,
        STANDARD = 2
    }

    public class JsonParsing
    {
        public static string JsonToUnityBlendshapeName(string jsonName, JsonNamingVersion version, 
            string meshPrefix = null, RetargetingProfile profile = null)
        {
            string reformedShapeName = jsonName;
            if (profile != null)
            {
                var retargetNode = profile.retarget.
                    FirstOrDefault((s) => s.jsonName == reformedShapeName);

                if (retargetNode != null)
                {
                    reformedShapeName = retargetNode.unityName;
                }
                else
                {
                    switch (profile.flag)
                    {
                        case RetargetFlag.REPLACE_R_L:
                            reformedShapeName = reformedShapeName.Replace("Right", "_R");
                            reformedShapeName = reformedShapeName.Replace("Left", "_L");
                            break;
                    }
                }
            }

            switch (version)
            {
                case JsonNamingVersion.LEGACY:
                    reformedShapeName = jsonName.Replace('[', '.');
                    reformedShapeName = reformedShapeName.Substring(0, reformedShapeName.Length - 1);
                    break;
                case JsonNamingVersion.STANDARD:
                    if (!string.IsNullOrEmpty(meshPrefix))
                        reformedShapeName = meshPrefix + reformedShapeName;
                    break;
            }

            return reformedShapeName;
        }
    }
}