using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum AdType
{
    Banner,
    Interstitial,
    Rewarded
}

public enum AdPlacement
{
    TestFromGoogle,
    SummonX1,
}

[System.Serializable]
public class AdUnitGroup
{
    public AdType adType;
    public List<AdUnitEntry> entries;
}

[System.Serializable]
public class AdUnitEntry
{
    public AdPlacement placement;
    public string adUnitIdAndroid;
    public string adUnitIdIOS;
}

[CreateAssetMenu(menuName = "Ads/AdUnitConfig")]
public class AdUnitConfig : ScriptableObject
{
    public List<AdUnitGroup> adGroups;

    public string GetUnitId(AdType type, AdPlacement placement)
    {
        var group = adGroups.Find(g => g.adType == type);
        if (group == null) return "";

        var entry = group.entries.Find(e => e.placement == placement);
#if UNITY_ANDROID
        return entry?.adUnitIdAndroid ?? "";
#elif UNITY_IOS
        return entry?.adUnitIdIOS ?? "";
#else
        return entry?.adUnitIdAndroid ?? "";
#endif
    }
}