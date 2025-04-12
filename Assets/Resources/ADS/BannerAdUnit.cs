using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using UnityEngine;

public class BannerAdUnit : IAdUnit
{
    public AdType Type => AdType.Banner;
    public event Action<AdType, AdPlacement> OnAdClosed;
    
    private BannerView _bannerView;
    private AdUnitConfig _config;
    
    public void LoadAd(AdPlacement placement)
    {
        string adUnitId = _config.GetUnitId(Type, placement);

        _bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        _bannerView.LoadAd(new AdRequest());

        FirebaseAnalytics.LogEvent("banner_loaded");
    }
    
    public void Initialize(AdUnitConfig config)
    {
        _config = config;
    }

    public void ShowAd(Action onRewardEarned = null)
    {
        _bannerView?.Show();
        Debug.Log("Banner shown");
        FirebaseAnalytics.LogEvent("banner_shown");
    }

    public bool IsAdReady()
    {
        return _bannerView != null;
    }
}
