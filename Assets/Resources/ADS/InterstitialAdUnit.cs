using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using UnityEngine;

public class InterstitialAdUnit : IAdUnit
{
    public AdType Type => AdType.Interstitial;
    public event Action<AdType, AdPlacement> OnAdClosed;
    
    private InterstitialAd _interstitialAd;
    private AdUnitConfig _config;
    private AdPlacement _currentPlacement;

    public void Initialize(AdUnitConfig config)
    {
        _config = config;
    }

    public void ShowAd(Action onRewardEarned = null)
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
            FirebaseAnalytics.LogEvent("interstitial_shown");
        }
        else
        {
            Debug.Log("Interstitial not ready");
            FirebaseAnalytics.LogEvent("interstitial_skipped");
        }
    }
    
    public bool IsAdReady()
    {
        return _interstitialAd != null && _interstitialAd.CanShowAd();
    }
    
    public void LoadAd(AdPlacement placement)
    {
        _currentPlacement = placement;
        string adUnitId = _config.GetUnitId(Type, placement);

        InterstitialAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"Interstitial load failed: {error?.GetMessage()}");
                FirebaseAnalytics.LogEvent("interstitial_load_failed", new Parameter("error", error?.GetMessage() ?? "null"));
                return;
            }

            _interstitialAd = ad;
            FirebaseAnalytics.LogEvent("interstitial_loaded");

            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                FirebaseAnalytics.LogEvent("interstitial_closed");
                OnAdClosed?.Invoke(Type, _currentPlacement); // Reload lại sau khi xem xong
            };
            
            _interstitialAd.OnAdFullScreenContentFailed += (err) =>
            {
                Debug.Log("Interstitial failed to show: " + err);
                FirebaseAnalytics.LogEvent("interstitial_show_failed", new Parameter("message", err.GetMessage()));
            };

            _interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial opened");
                FirebaseAnalytics.LogEvent("interstitial_opened");
            };
        });
    }
}
