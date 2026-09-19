using UnityEngine;

namespace StudyGame.Data
{
    [CreateAssetMenu(fileName = "NewCharacterDNA", menuName = "StudyGame/Character DNA")]
    public class CharacterDNA : ScriptableObject
    {
        public string CharacterId;
        
        [Header("Mesh & Structure")]
        public string HeadMeshId;
        [Range(0.5f, 2f)] public float CapeLengthScale = 1f;
        [Range(0.5f, 2f)] public float SleeveWidthScale = 1f;
        
        [Header("Shader Properties")]
        public Color RimLightColor = Color.white;
        public Color FaceEmissiveColor = Color.yellow;
        
        [Header("Decals")]
        public Texture2D DecalTexture;
    }
}
