using UnityEngine;

namespace UnityDotNetSample.Components;

/// <summary>
/// GameObject を継続的に回転させるサンプル MonoBehaviour。
/// DLL から Unity API を直接利用できることを示すデモ。
/// </summary>
public class SampleRotator : MonoBehaviour
{
    [SerializeField]
    private Vector3 _rotationSpeed = new(0f, 90f, 0f);

    [SerializeField]
    private Space _rotationSpace = Space.Self;

    private void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime, _rotationSpace);
    }

    /// <summary>
    /// 回転速度を設定する。外部からの設定用に公開。
    /// </summary>
    public void SetRotationSpeed(Vector3 speed)
    {
        _rotationSpeed = speed;
    }
}
