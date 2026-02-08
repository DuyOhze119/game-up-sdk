using UnityEngine;

namespace GameUpSDK
{
    /// <summary>
    /// Runtime config for GameUp SDK (AppsFlyer, IronSource, AdMob, UnityAds).
    /// When SDK is installed from package (read-only), Setup window saves here instead of prefabs.
    /// Loaded from Resources at runtime and applied to ad components.
    /// </summary>
    public class GameUpSDKConfig : ScriptableObject
    {
        [Header("AppsFlyer")]
        public string appsFlyerDevKey = "";
        public string appsFlyerAppId = "";

        [Header("IronSource (LevelPlay)")]
        public string ironSourceAppKey = "";
        public string ironSourceBannerId = "";
        public string ironSourceInterstitialId = "";
        public string ironSourceRewardedId = "";

        [Header("AdMob")]
        public string admobBannerId = "";
        public string admobInterstitialId = "";
        public string admobRewardedId = "";
        public string admobAppOpenId = "";

        [Header("UnityAds")]
        public string unityAdsAppKey = "";
        public string unityAdsBannerId = "";
        public string unityAdsInterstitialId = "";
        public string unityAdsRewardedId = "";
    }
}
