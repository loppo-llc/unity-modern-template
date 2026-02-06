using System;

namespace UnityDotNetSample.Core;

/// <summary>
/// UnityEngine 型に依存しない純粋な数学ユーティリティ。
/// Unity 外で完全にテスト可能。
/// </summary>
public static class MathUtilities
{
    /// <summary>
    /// 値をある範囲から別の範囲へ再マッピングする。
    /// </summary>
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        if (MathF.Abs(fromMax - fromMin) < float.Epsilon)
            throw new ArgumentException("Source range cannot be zero.", nameof(fromMax));

        float normalized = (value - fromMin) / (fromMax - fromMin);
        return toMin + normalized * (toMax - toMin);
    }

    /// <summary>
    /// フレームレート非依存の指数減衰によるスムーズダンピング値を計算する。
    /// </summary>
    public static float ExponentialDecay(float current, float target, float decay, float deltaTime)
    {
        return target + (current - target) * MathF.Exp(-decay * deltaTime);
    }
}
