using UnityEngine;

[CreateAssetMenu(menuName = "BlockBlast/Game Settings", fileName = "GameSettings")]
public sealed class GameSettings : ScriptableObject
{
    [Header("Scoring")]
    [Min(0)] public int scorePerBlockPlaced = 1;
    [Min(0)] public int scorePerLineCleared = 10;
    [Min(0f)] public float comboMultiplierStep = 0.25f;
}
