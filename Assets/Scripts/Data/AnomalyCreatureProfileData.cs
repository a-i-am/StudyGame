using System;
using UnityEngine;

namespace StudyGame.Data
{
    public enum AnomalyArchetype
    {
        Polyhedron,
        HumanoidShadow,
        OrbitalSphere,
        SwarmCloud,
        LinearChain
    }

    [Serializable]
    public class AnomalyCreatureProfileData
    {
        public string id;
        public string conceptTitle;
        public AnomalyArchetype archetype;
        public string primarySymbolText;
        public string hexColor;
        public float glitchFrequency = 2.0f;
        public string visualDescription;
        public string clueKeyword;

        public Color GetParsedColor()
        {
            if (string.IsNullOrEmpty(hexColor)) return Color.cyan;
            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                return color;
            }
            return Color.cyan;
        }
    }

    [Serializable]
    public class AnomalyProfileListWrapper
    {
        public AnomalyCreatureProfileData[] profiles;
    }
}
