using UnityDotNetSample.Components;
using Xunit;

namespace UnityDotNetSample.Tests.Components;

/// <summary>
/// Unity コンポーネントのテスト境界を示すサンプル。
/// MonoBehaviour 派生クラスは Unity 外ではインスタンス化できないため、
/// ここではリフレクションで API サーフェスを検証する。
/// 結合テストには Unity Test Framework (PlayMode/EditMode テスト) を使用すること。
/// </summary>
public class SampleRotatorTests
{
    [Fact]
    public void SampleRotator_ClassExists_AndIsPublic()
    {
        var type = typeof(SampleRotator);

        Assert.True(type.IsPublic);
        Assert.Equal("MonoBehaviour", type.BaseType?.Name);
    }

    [Fact]
    public void SampleRotator_HasSetRotationSpeedMethod()
    {
        var method = typeof(SampleRotator).GetMethod("SetRotationSpeed");

        Assert.NotNull(method);
        Assert.True(method.IsPublic);
    }
}
