using System;
using System.Reflection;
using UnityEngine;

namespace GameUpSDK
{
    /// <summary>
    /// Applies GameUpSDKConfig from Resources to ad components in the scene when SDK is installed from package.
    /// Runs after scene load so config saved in Setup window (to Assets/Resources/GameUpSDKConfig.asset) is applied.
    /// </summary>
    public static class GameUpSDKConfigApplier
    {
        private const string ConfigResourceName = "GameUpSDKConfig";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyConfig()
        {
            var config = Resources.Load<GameUpSDKConfig>(ConfigResourceName);
            if (config == null) return;

            ApplyToIronSource(config);
            ApplyToUnityAds(config);
            ApplyToAdmob(config);
            ApplyToAppsFlyer(config);
        }

        private static void ApplyToIronSource(GameUpSDKConfig c)
        {
            var list = UnityEngine.Object.FindObjectsOfType<IronSourceAds>();
            foreach (var ad in list)
                ad.SetLevelPlayConfig(c.ironSourceAppKey, c.ironSourceBannerId, c.ironSourceInterstitialId, c.ironSourceRewardedId);
        }

        private static void ApplyToUnityAds(GameUpSDKConfig c)
        {
            var list = UnityEngine.Object.FindObjectsOfType<UnityAds>();
            foreach (var ad in list)
                ad.SetLevelPlayConfig(c.unityAdsAppKey, c.unityAdsBannerId, c.unityAdsInterstitialId, c.unityAdsRewardedId);
        }

        private static void ApplyToAdmob(GameUpSDKConfig c)
        {
            var list = UnityEngine.Object.FindObjectsOfType<AdmobAds>();
            foreach (var ad in list)
                ad.SetAdUnitIds(c.admobBannerId, c.admobInterstitialId, c.admobRewardedId, c.admobAppOpenId);
        }

        private static void ApplyToAppsFlyer(GameUpSDKConfig c)
        {
            if (string.IsNullOrEmpty(c.appsFlyerDevKey)) return;
            var type = Type.GetType("AppsFlyerObjectScript, AppsFlyer");
            if (type == null) return;
            var objects = UnityEngine.Object.FindObjectsOfType(type);
            foreach (var obj in objects)
            {
                try
                {
                    var t = obj.GetType();
                    SetField(t, obj, "devKey", c.appsFlyerDevKey);
                    SetField(t, obj, "appID", c.appsFlyerAppId);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[GameUpSDK] AppsFlyer config apply: " + e.Message);
                }
            }
        }

        private static void SetField(Type type, object target, string fieldName, string value)
        {
            var f = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null)
                f.SetValue(target, value ?? "");
        }
    }
}
