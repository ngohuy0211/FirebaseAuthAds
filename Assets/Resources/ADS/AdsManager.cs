using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using Firebase.Analytics;
using System;

public class AdsManager : SingletonFreeAlive<AdsManager>
{
    [SerializeField] private AdUnitConfig adUnitConfig;
    //Chỉ 1 loại quảng cáo cura 1 type ược phép hiện trên 1 màn hình
    //Nếu muôn nhiều loại quảng cáo của 1 type hiện thì phải sửa lại dict
    //Tốt nhất là 1 thôi để khng bị phiền
    private Dictionary<AdType, IAdUnit> _adUnits = new Dictionary<AdType, IAdUnit>();
    
    protected override void Awake()
    {
        base.Awake();
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob Initialized");
            RegisterUnits();
        });
    }
    
    private void RegisterUnits()
    {
        AddAdUnit(new BannerAdUnit());
        AddAdUnit(new InterstitialAdUnit());
        AddAdUnit(new RewardedAdUnit());
    }
    
    private void AddAdUnit(IAdUnit adUnit)
    {
        adUnit.Initialize(adUnitConfig);
        adUnit.OnAdClosed += HandleAdClosed;
        _adUnits[adUnit.Type] = adUnit;
    }
    
    public void LoadAd(AdType type, AdPlacement placement)
    {
        if (_adUnits.TryGetValue(type, out var unit)) unit.LoadAd(placement);
    }

    public void ShowAd(AdType type, Action onRewardEarned = null)
    {
        if (_adUnits.TryGetValue(type, out var unit) && unit.IsAdReady()) unit.ShowAd(onRewardEarned);
    }

    public bool IsAdReady(AdType type)
    {
        return _adUnits.TryGetValue(type, out var unit) && unit.IsAdReady();
    }
    
    private void HandleAdClosed(AdType type, AdPlacement placement)
    {
        Debug.Log($"[AdManager] Auto reloading {type} ad for placement {placement}");
        LoadAd(type, placement); // tự động reload sau khi đóng
    }
}
