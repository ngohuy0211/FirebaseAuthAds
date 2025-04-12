using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using UnityEngine;

public class RewardedAdUnit : IAdUnit
{
    public AdType Type => AdType.Rewarded;
    public event Action<AdType, AdPlacement> OnAdClosed;

    private RewardedAd _rewardedAd;
    private AdUnitConfig _config;
    private AdPlacement _currentPlacement;

    public void Initialize(AdUnitConfig config)
    {
        _config = config;
    }

    public void ShowAd(Action onRewardEarned = null)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            Debug.Log("[Ad] Showing RewardedAd");
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"[Ad] User earned reward: {reward.Type}, amount: {reward.Amount}");
                FirebaseAnalytics.LogEvent("rewarded_earned", new Parameter[]
                {
                    new Parameter("reward_type", reward.Type),
                    new Parameter("reward_amount", reward.Amount)
                });

                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("[Ad] RewardedAd not ready.");
            FirebaseAnalytics.LogEvent("rewarded_show_skipped");
        }
    }

    public bool IsAdReady()
    {
        return _rewardedAd != null && _rewardedAd.CanShowAd();
    }

    public void LoadAd(AdPlacement placement)
    {
        _currentPlacement = placement;
        string adUnitId = _config.GetUnitId(Type, placement);

        RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error != null)
            {
                Debug.LogError("Rewarded load failed: " + error.GetMessage());
                return;
            }

            _rewardedAd = ad;
            FirebaseAnalytics.LogEvent("rewarded_loaded");

            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("RewardedAd closed");
                FirebaseAnalytics.LogEvent("rewarded_closed");
                OnAdClosed?.Invoke(Type, _currentPlacement); // Reload lại sau khi xem xong
            };

            _rewardedAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded opened");
                FirebaseAnalytics.LogEvent("rewarded_opened");
            };

            _rewardedAd.OnAdFullScreenContentFailed += (adError) =>
            {
                Debug.Log("Rewarded failed to show: " + adError);
                FirebaseAnalytics.LogEvent("rewarded_show_failed", new Parameter("message", adError.GetMessage()));
            };
        });
    }
}