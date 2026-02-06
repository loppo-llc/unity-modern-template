using UnityEngine;

namespace UnityDotNetSample.ScriptableObjects;

/// <summary>
/// データ駆動設計を示すサンプル ScriptableObject。
/// Assets &gt; Create &gt; UnityDotNetSample &gt; Game Settings から作成可能。
/// </summary>
[CreateAssetMenu(fileName = "GameSettings", menuName = "UnityDotNetSample/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Player")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;

    [Header("Game")]
    [SerializeField] private int _maxLives = 3;

    public float MoveSpeed => _moveSpeed;
    public float JumpForce => _jumpForce;
    public int MaxLives => _maxLives;
}
