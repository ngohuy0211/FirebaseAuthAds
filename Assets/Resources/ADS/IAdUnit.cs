using System;

public interface IAdUnit
{
    AdType Type { get; }
    void Initialize(AdUnitConfig config);
    void LoadAd(AdPlacement placement);
    void ShowAd(Action onRewardEarned = null);
    bool IsAdReady();
    event System.Action<AdType, AdPlacement> OnAdClosed;
}
