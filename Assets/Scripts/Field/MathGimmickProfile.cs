using UnityEngine;

[CreateAssetMenu(fileName = "MathGimmickProfile", menuName = "Math/MathGimmickProfile")]
public class MathGimmickProfile : ScriptableObject
{
    public string profileName = "New Math Profile";
    public string inputExpression = "x > 0";
    public string operationType = "Logarithm";
    public string effectType = "ScaleTransform";
}
