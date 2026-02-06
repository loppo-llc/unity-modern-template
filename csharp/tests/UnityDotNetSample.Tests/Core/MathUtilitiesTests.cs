using System;
using UnityDotNetSample.Core;
using Xunit;

namespace UnityDotNetSample.Tests.Core;

public class MathUtilitiesTests
{
    [Theory]
    [InlineData(0.5f, 0f, 1f, 0f, 100f, 50f)]
    [InlineData(0f, 0f, 1f, 0f, 100f, 0f)]
    [InlineData(1f, 0f, 1f, 0f, 100f, 100f)]
    [InlineData(5f, 0f, 10f, -1f, 1f, 0f)]
    public void Remap_ShouldMapValueCorrectly(
        float value, float fromMin, float fromMax,
        float toMin, float toMax, float expected)
    {
        float result = MathUtilities.Remap(value, fromMin, fromMax, toMin, toMax);

        Assert.Equal(expected, result, precision: 4);
    }

    [Fact]
    public void Remap_ZeroRange_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            MathUtilities.Remap(5f, 3f, 3f, 0f, 100f));
    }

    [Fact]
    public void ExponentialDecay_ShouldApproachTarget()
    {
        float current = 100f;
        float target = 0f;

        // 大きな deltaTime と高い decay で、ターゲットに非常に近づくはず
        float result = MathUtilities.ExponentialDecay(current, target, decay: 10f, deltaTime: 1f);

        Assert.True(result < 1f, $"Expected near-zero, got {result}");
    }

    [Fact]
    public void ExponentialDecay_ZeroDeltaTime_ShouldReturnCurrent()
    {
        float current = 50f;
        float target = 100f;

        float result = MathUtilities.ExponentialDecay(current, target, decay: 5f, deltaTime: 0f);

        Assert.Equal(current, result, precision: 5);
    }
}
