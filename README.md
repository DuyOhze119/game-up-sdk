# GameUp SDK

## Tổng quan dự án

**Tên:** GameUp SDK  
**Mục đích:** Giải pháp Unity All-in-One cho **Quảng cáo** và **Phân tích**, giúp tối ưu hóa quy trình tích hợp và vận hành game.

- **Quảng cáo (Ads):** Tích hợp IronSource (LevelPlay), AdMob và Unity Ads qua một lớp trung gian duy nhất. Hỗ trợ cơ chế waterfall thông minh (mạng nào có quảng cáo trước thì hiển thị).
- **Phân tích (Analytics):** Tích hợp Firebase và AppsFlyer. Sự kiện được tự động ghi nhận vào cả hai hệ thống để đối soát dữ liệu chính xác.

---

## Yêu cầu kỹ thuật (Prerequisites)

Đảm bảo dự án Unity và cấu hình build của bạn đáp ứng các yêu cầu sau trước khi tích hợp:

| Yêu cầu | Giá trị |
|--------|---------|
| **Phiên bản Unity khuyến nghị** | 2022.3.62f3 (LTS) |
| **Nền tảng hỗ trợ** | Android, iOS |
| **Android – Minimum API Level** | 24 (Android 7.0) |
| **Android – Target API Level** | 36 (Android 15) |

---

## Hướng dẫn Cài đặt (Installation)

### Bước 1: Cấu hình .gitignore (Quan trọng)
Để tránh đẩy các file thư viện C++ nặng của Firebase lên Git gây lỗi hoặc phình to repo, hãy thêm đoạn sau vào cuối file `.gitignore` của dự án:

```
# --- Firebase Ignored Files (Auto match versions) ---
Assets/Firebase/Plugins/x86_64/FirebaseCppAnalytics*.so
Assets/Firebase/Plugins/x86_64/FirebaseCppAnalytics*.so.meta
Assets/Firebase/Plugins/x86_64/FirebaseCppApp*.so
Assets/Firebase/Plugins/x86_64/FirebaseCppApp*.so.meta
Assets/Firebase/Plugins/x86_64/FirebaseCppApp*.bundle.meta
Assets/Firebase/Plugins/x86_64/FirebaseCppApp*.bundle
Assets/Firebase/Plugins/x86_64/FirebaseCppApp*.dll
Assets/Firebase/Plugins/x86_64/FirebaseCppApp*.dll.meta
```

### Bước 2: Cài đặt các SDK nền tảng (Dependencies)

Bạn có 2 cách để cài đặt các thư viện lõi (IronSource, AdMob, Firebase, AppsFlyer):

**Cách 1: Cài thủ công (Manual - Khuyên dùng nếu muốn kiểm soát version)**
Tải và import các gói sau trước khi cài GameUp SDK:
- **IronSource (LevelPlay):** https://drive.google.com/file/d/1kCBcKoNa1HqmJ8DNYHs8L33IJVcRmWDw/view?usp=drive_link
- **Google Mobile Ads (AdMob):** https://drive.google.com/file/d/1rveiwzLxUXN4uQIju5-mE7m3E9NEZ29k/view?usp=sharing
- **Firebase (Analytics & App):** https://drive.google.com/drive/folders/10F_6GM68T809iY1fpj5jYYGjF_ntUkch?usp=sharing
- **AppsFlyer:** https://drive.google.com/file/d/1GcIQf1jDWbt3nGYWbRF8KQ9b0zT6sbOB/view?usp=sharing

**Cách 2: Dùng Tool tự động (Sau khi import GameUp SDK)**
Xem hướng dẫn ở Bước 3 bên dưới.

### Bước 3: Import GameUp SDK

1. Trong Unity, mở **Window > Package Manager**.
2. Bấm nút **+** và chọn **Add package from git URL...**.
3. Dán đường dẫn Git sau:
   https://github.com/DuyOhze119/game-up-sdk.git
4. Bấm **Add**.

### Bước 4: Hoàn tất cài đặt Dependencies & Mediation

**Cài đặt Ads Mediation (Bắt buộc):**
   - Vào menu **Ads Mediation > Integration Manager**.
   - Tại tab **SDKs**:
     - Tìm **UnityAds**: Nhấn **Install**.
     - Tìm **AdMob**: Nhấn **Install** (để IronSource nhận diện AdMob).

---

## Hướng dẫn Thiết lập (Setup)

### 1. Cấu hình App ID và Key
1. Vào menu **Tools > GameUp SDK > Setup**.
2. Nhập đầy đủ thông tin:
   - **AdMob App ID** (Android/iOS).
   - **IronSource App Key**.
   - **AppsFlyer Dev Key** & **App ID** (iOS).
3. Nhấn **Save Configuration**.

### 2. Thêm SDK vào Scene
1. Mở **scene đầu tiên** của game (Splash/Loading/Login).
2. Tìm file **SDK.prefab** trong thư mục Packages (đường dẫn: `Packages/GameUp Base SDK/Prefabs` hoặc `Assets/GameUpSDK/Prefab`).
3. Kéo **SDK.prefab** vào Hierarchy.
   *(Lưu ý: Prefab này chứa `AdsManager` và `DontDestroyOnLoad`, chỉ cần có mặt ở scene đầu tiên).*

---

## Hướng dẫn Lập trình – Quảng cáo (AdsManager)

Quảng cáo được gọi thông qua Singleton `AdsManager.Instance`. Mọi lời gọi hàm nên được thực hiện từ **Main Thread**.

### Hiển thị Interstitial (Quảng cáo chuyển cảnh)
```csharp
using GameUpSDK;

AdsManager.Instance.ShowInterstitial(
    placementName: "Level_End", // Tên vị trí để tracking
    onSuccess: () => {
        // Đã hiển thị quảng cáo xong, tiếp tục game
        Debug.Log("Interstitial Closed");
    },
    onFail: () => {
        // Lỗi load, không có quảng cáo, hoặc lỗi mạng -> Tiếp tục game
        Debug.Log("Interstitial Failed or Not Ready");
    }
);
```

### Hiển thị Rewarded Video (Quảng cáo trả thưởng)

```csharp
using GameUpSDK;

AdsManager.Instance.ShowRewardedVideo(
    placementName: "Shop_Free_Gold",
    onSuccess: () => {
        // Người chơi đã xem hết video -> Trả thưởng
        GiveGold(100);
    },
    onFail: () => {
        // Người chơi tắt sớm hoặc không có quảng cáo -> Không trả thưởng
        ShowMessage("Ad not available");
    }
);
```

### Hiển thị Banner

```csharp
using GameUpSDK;

// Hiển thị
AdsManager.Instance.ShowBanner(placementName: "Main_Menu");

// Ẩn
AdsManager.Instance.HideBanner(placementName: "Main_Menu");
```

---

## Hướng dẫn Lập trình – Analytics (GameUpAnalytics)

Hệ thống tự động log sự kiện song song lên **Firebase** và **AppsFlyer**.

### Bắt đầu màn chơi (Level Start)

```csharp
using GameUpSDK;

// level: Level hiện tại (int)
// index: Số lần chơi lại level này (int)
GameUpAnalytics.LogLevelStart(level: 5, index: 1);
```

### Hoàn thành màn chơi (Level Complete)

```csharp
using GameUpSDK;

// timeSeconds: Thời gian chơi (float)
// score: Điểm số đạt được (int)
GameUpAnalytics.LogLevelComplete(level: 5, index: 1, timeSeconds: 120f, score: 1000);
```

### Thua màn chơi (Level Fail)

```csharp
using GameUpSDK;

GameUpAnalytics.LogLevelFail(level: 5, index: 2, timeSeconds: 45f);
```

---

## Xử lý sự cố (Troubleshooting)

### 1. Quảng cáo không hiển thị (No Fill / Not Ready)

* Đảm bảo **SDK.prefab** đã có trong scene.
* Kiểm tra internet trên thiết bị.
* Gọi `AdsManager.Instance.RequestAll()` thủ công nếu cần preload lại.
* Kiểm tra log Unity với từ khóa `[GameUpSDK]` hoặc `IronSource`.

### 2. Lỗi Build Android (Duplicate Class / Dependencies)

* Vào **Assets > External Dependency Manager > Android Resolver > Force Resolve**.
* Nếu lỗi liên quan đến Firebase `x86_64`, kiểm tra lại cấu hình `.gitignore` và xóa folder `Library` để build lại.

### 3. Analytics không thấy dữ liệu

* Firebase: Dữ liệu realtime có thể trễ, hãy dùng **DebugView** để test.
* AppsFlyer: Kiểm tra **Dev Key** đã chính xác trong phần Setup chưa.

### 4. Không thấy menu "Tools > GameUp SDK"

* Kiểm tra lại console xem có lỗi biên dịch (Compile Error) nào không.
* Đảm bảo đã import đủ các dependencies (AdMob, Firebase...).

---

*Documentation maintained by GameUp Team.*
