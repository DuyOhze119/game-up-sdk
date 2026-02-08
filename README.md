# GameUp SDK

## Project Overview

**Name:** GameUp SDK  

**Purpose:** An all-in-one Unity solution for **Ads** and **Analytics**:

- **Ads:** IronSource, AdMob, and Unity Ads via a single mediator with waterfall (first available network wins).
- **Analytics:** Firebase and AppsFlyer — events are logged to both where applicable.

---

## Prerequisites (Technical Requirements)

| Requirement | Value |
|-------------|--------|
| **Recommended Unity Version** | 2022.3.62f3 (LTS) |
| **Platforms** | Android, iOS |
| **Android – Minimum API Level** | 24 (Android 7.0) |
| **Android – Target API Level** | 36 (Android 15) |

Ensure your Unity project and build settings meet these before using the SDK.

---

## Installation & Setup Guide

### Step 1: Import the package via Git URL

1. In Unity, open **Window > Package Manager**.
2. Click the **+** button and choose **Add package from git URL...**.
3. Enter the GameUp SDK Git URL (e.g. `https://github.com/DuyOhze119/game-up-sdk.git` or your actual repo URL).
4. Click **Add**. Unity will resolve and import the package.

### Step 2: Install external SDK dependencies

1. Go to **Tools > GameUp SDK > Install All Dependencies**.
2. Wait for the process to finish. This installs or updates the external SDKs required for Ads and Analytics (IronSource, AdMob, Unity Ads, Firebase, AppsFlyer, etc.).

### Step 3: Configure App IDs and keys

1. Go to **Tools > GameUp SDK > Setup**.
2. In the GameUp SDK Setup window, enter your **App IDs** and **Keys** for each network you use (e.g. AdMob App ID, IronSource App Key, Unity Ads Game ID, Firebase config, AppsFlyer Dev Key).
3. Save the settings.

### Step 4: Add the SDK to your first scene

1. Open the **first scene** of your game (typically the one that loads at startup).
2. Locate **SDK.prefab** in the package (e.g. under `Assets/GameUpSDK/Runtime/Prefab/` or the package’s `Prefab` folder).
3. Drag **SDK.prefab** into the Hierarchy so it is present when the game runs.

The prefab contains the SDK bootstrap (e.g. `AdsManager`, analytics, and related objects). Keep it in the first scene so it initializes before any ads or analytics are used.

---

## Coding Guide – Ads (AdsManager)

Ads are shown through the **Singleton** `AdsManager.Instance`. The SDK uses a **waterfall**: when you request an ad, the first registered network that has an ad available will show it. All calls should be made from the **main thread**.

### Show Interstitial

```csharp
using GameUpSDK;

// Optional callbacks
AdsManager.Instance.ShowInterstitial(
    placementName: "between_levels",
    onSuccess: () => { /* ad shown, continue game */ },
    onFail: () => { /* no ad or error, continue without ad */ }
);
```

The `placementName` (e.g. `"between_levels"`, `"game_over"`) is used for tracking and reporting (Firebase/AppsFlyer).

### Show Rewarded Video

```csharp
using GameUpSDK;

AdsManager.Instance.ShowRewardedVideo(
    placementName: "extra_life",
    onSuccess: () => { /* user watched full video, grant reward */ },
    onFail: () => { /* skipped or not available */ }
);
```

Again, `placementName` is used for tracking.

### Show Banner

```csharp
using GameUpSDK;

AdsManager.Instance.ShowBanner(placementName: "main_menu");
```

To hide the banner:

```csharp
AdsManager.Instance.HideBanner(placementName: "main_menu");
```

Use the same `placementName` when showing and hiding so the correct placement is tracked.

---

## Coding Guide – Analytics (GameUpAnalytics)

**GameUpAnalytics** sends events to **both Firebase and AppsFlyer** where applicable (e.g. level events go to Firebase and, for level complete, to AppsFlyer as `af_level_achieved`). You only call one method per event; the SDK handles dual logging.

### Level Start

Call when the player starts a level (e.g. after “Play” or level load).

```csharp
using GameUpSDK;

// level: level number (1-based); index: attempt number for this level
GameUpAnalytics.LogLevelStart(level: 5, index: 1);
```

### Level Complete

Call when the player finishes the level successfully. Logs to Firebase and AppsFlyer.

```csharp
using GameUpSDK;

// level, index (attempt), time in seconds, optional score
GameUpAnalytics.LogLevelComplete(level: 5, index: 1, timeSeconds: 120f, score: 1000);
```

### Level Fail

Call when the player fails the level.

```csharp
using GameUpSDK;

// level, index (attempt number), time in seconds from level start to fail
GameUpAnalytics.LogLevelFail(level: 5, index: 2, timeSeconds: 45f);
```

Other methods (e.g. `LogStartLoading`, `LogCompleteLoading`, `LogButtonClick`, `LogWaveStart`, `LogPurchase`, etc.) are available for loading, UI, waves, and monetization — use them as needed for your design.

---

## Troubleshooting

### Dependency resolution / first install

- **Ensure you have a stable Internet connection** when running **Tools > GameUp SDK > Install All Dependencies**. The installer fetches external SDKs and package manifests from the network. Interrupted or slow connections can cause incomplete or failed installs.
- If the installer fails, check your connection, retry **Install All Dependencies**, and fix any Unity Console errors (e.g. missing packages or invalid URLs).

### Ads not showing

- Confirm **SDK.prefab** is in the first scene and that **AdsManager** has initialized (e.g. after consent/GDPR if required).
- Call `AdsManager.Instance.RequestAll()` after initialization (and after consent) so networks can preload interstitials and rewarded videos.
- Use the same `placementName` consistently for show/hide and tracking.

### Analytics not appearing

- Ensure Firebase and AppsFlyer are configured in **Tools > GameUp SDK > Setup** and that the SDK prefab is present in the scene.
- In development, events can take time to appear in Firebase DebugView or AppsFlyer dashboards; check filters and app selection.

### Menu items missing

- If **GameUp SDK** does not appear under **Tools**, ensure the package is imported correctly and that there are no script or assembly definition errors in the GameUp SDK folders. Fix any compile errors and re-open the Unity Editor if needed.

---

## Formatting note

This document uses standard Markdown. Code examples are C# and are intended to be copy-pasted into your game scripts; adjust names (e.g. `placementName`, `level`, `index`, `timeSeconds`, `score`) to match your game logic.
