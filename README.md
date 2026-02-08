# GameUp SDK

## Tổng quan dự án

**Tên:** GameUp SDK  

**Mục đích:** Giải pháp Unity all-in-one cho **Quảng cáo** và **Phân tích**:

- **Quảng cáo:** IronSource, AdMob và Unity Ads qua một lớp trung gian duy nhất với cơ chế waterfall (mạng nào có sẵn quảng cáo trước thì hiển thị).
- **Phân tích:** Firebase và AppsFlyer — sự kiện được ghi nhận vào cả hai nơi khi áp dụng được.

---

## Yêu cầu kỹ thuật

| Yêu cầu | Giá trị |
|--------|---------|
| **Phiên bản Unity khuyến nghị** | 2022.3.62f3 (LTS) |
| **Nền tảng** | Android, iOS |
| **Android – Minimum API Level** | 24 (Android 7.0) |
| **Android – Target API Level** | 36 (Android 15) |

Đảm bảo dự án Unity và cài đặt build của bạn đáp ứng các yêu cầu trên trước khi dùng SDK.

---

## Hướng dẫn cài đặt & thiết lập

### Bước 1: Import package qua Git URL

1. Trong Unity, mở **Window > Package Manager**.
2. Bấm nút **+** và chọn **Add package from git URL...**.
3. Nhập Git URL của GameUp SDK (ví dụ: `https://github.com/DuyOhze119/game-up-sdk.git` hoặc URL repo thực tế của bạn).
4. Bấm **Add**. Unity sẽ xử lý và import package.

### Bước 2: Cài đặt các dependency SDK bên ngoài

1. Vào **Tools > GameUp SDK > Install All Dependencies**.
2. Chờ quá trình hoàn tất. Bước này cài đặt hoặc cập nhật các SDK bên ngoài cần cho Quảng cáo và Phân tích (IronSource, AdMob, Unity Ads, Firebase, AppsFlyer, v.v.).

### Bước 3: Cấu hình App ID và Key

1. Vào **Tools > GameUp SDK > Setup**.
2. Trong cửa sổ GameUp SDK Setup, nhập **App ID** và **Key** cho từng mạng bạn dùng (ví dụ: AdMob App ID, IronSource App Key, Unity Ads Game ID, cấu hình Firebase, AppsFlyer Dev Key).
3. Lưu cài đặt.

### Bước 4: Thêm SDK vào scene đầu tiên

1. Mở **scene đầu tiên** của game (thường là scene được load khi khởi động).
2. Tìm **SDK.prefab** trong package (ví dụ: trong `Assets/GameUpSDK/Runtime/Prefab/` hoặc thư mục `Prefab` của package).
3. Kéo **SDK.prefab** vào Hierarchy để nó có mặt khi game chạy.

Prefab chứa bootstrap của SDK (ví dụ: `AdsManager`, analytics và các đối tượng liên quan). Giữ nó trong scene đầu tiên để khởi tạo trước khi bất kỳ quảng cáo hay analytics nào được dùng.

---

## Hướng dẫn lập trình – Quảng cáo (AdsManager)

Quảng cáo được hiển thị qua **Singleton** `AdsManager.Instance`. SDK dùng cơ chế **waterfall**: khi bạn yêu cầu quảng cáo, mạng đã đăng ký đầu tiên có quảng cáo sẵn sẽ hiển thị. Mọi lời gọi nên thực hiện từ **main thread**.

### Hiển thị Interstitial

```csharp
using GameUpSDK;

// Callback tùy chọn
AdsManager.Instance.ShowInterstitial(
    placementName: "between_levels",
    onSuccess: () => { /* đã hiển thị quảng cáo, tiếp tục game */ },
    onFail: () => { /* không có quảng cáo hoặc lỗi, tiếp tục không quảng cáo */ }
);
```

Tham số `placementName` (ví dụ: `"between_levels"`, `"game_over"`) dùng cho theo dõi và báo cáo (Firebase/AppsFlyer).

### Hiển thị Rewarded Video

```csharp
using GameUpSDK;

AdsManager.Instance.ShowRewardedVideo(
    placementName: "extra_life",
    onSuccess: () => { /* người chơi xem hết video, cấp thưởng */ },
    onFail: () => { /* bỏ qua hoặc không có quảng cáo */ }
);
```

`placementName` cũng dùng cho theo dõi.

### Hiển thị Banner

```csharp
using GameUpSDK;

AdsManager.Instance.ShowBanner(placementName: "main_menu");
```

Để ẩn banner:

```csharp
AdsManager.Instance.HideBanner(placementName: "main_menu");
```

Dùng cùng `placementName` khi hiển thị và ẩn để tracking đúng vị trí.

---

## Hướng dẫn lập trình – Analytics (GameUpAnalytics)

**GameUpAnalytics** gửi sự kiện tới **cả Firebase và AppsFlyer** khi áp dụng được (ví dụ: sự kiện level gửi tới Firebase và, với level complete, tới AppsFlyer dưới dạng `af_level_achieved`). Bạn chỉ cần gọi một method cho mỗi sự kiện; SDK xử lý ghi log kép.

### Bắt đầu level (Level Start)

Gọi khi người chơi bắt đầu một level (ví dụ: sau khi bấm "Chơi" hoặc load level).

```csharp
using GameUpSDK;

// level: số thứ tự level (bắt đầu từ 1); index: số lần thử ở level này
GameUpAnalytics.LogLevelStart(level: 5, index: 1);
```

### Hoàn thành level (Level Complete)

Gọi khi người chơi hoàn thành level thành công. Ghi log vào Firebase và AppsFlyer.

```csharp
using GameUpSDK;

// level, index (lần thử), thời gian tính bằng giây, score tùy chọn
GameUpAnalytics.LogLevelComplete(level: 5, index: 1, timeSeconds: 120f, score: 1000);
```

### Thua level (Level Fail)

Gọi khi người chơi thua level.

```csharp
using GameUpSDK;

// level, index (số lần thử), thời gian tính bằng giây từ lúc bắt đầu level đến lúc thua
GameUpAnalytics.LogLevelFail(level: 5, index: 2, timeSeconds: 45f);
```

Các method khác (ví dụ: `LogStartLoading`, `LogCompleteLoading`, `LogButtonClick`, `LogWaveStart`, `LogPurchase`, v.v.) có sẵn cho loading, UI, wave và monetization — dùng theo nhu cầu thiết kế của bạn.

---

## Xử lý sự cố

### Giải quyết dependency / cài đặt lần đầu

- **Đảm bảo kết nối Internet ổn định** khi chạy **Tools > GameUp SDK > Install All Dependencies**. Trình cài đặt tải các SDK bên ngoài và manifest package từ mạng. Kết nối bị gián đoạn hoặc chậm có thể gây cài đặt không đầy đủ hoặc thất bại.
- Nếu cài đặt thất bại, kiểm tra kết nối, thử lại **Install All Dependencies**, và sửa mọi lỗi trên Unity Console (ví dụ: package thiếu hoặc URL không hợp lệ).

### Quảng cáo không hiển thị

- Xác nhận **SDK.prefab** có trong scene đầu tiên và **AdsManager** đã được khởi tạo (ví dụ: sau consent/GDPR nếu cần).
- Gọi `AdsManager.Instance.RequestAll()` sau khi khởi tạo (và sau consent) để các mạng preload interstitial và rewarded video.
- Dùng cùng `placementName` nhất quán khi hiển thị/ẩn và tracking.

### Analytics không xuất hiện

- Đảm bảo Firebase và AppsFlyer đã cấu hình trong **Tools > GameUp SDK > Setup** và prefab SDK có trong scene.
- Trong môi trường phát triển, sự kiện có thể mất thời gian mới hiện trên Firebase DebugView hoặc dashboard AppsFlyer; kiểm tra bộ lọc và app đã chọn.

### Thiếu mục menu

- Nếu **GameUp SDK** không xuất hiện dưới **Tools**, kiểm tra package đã được import đúng và không có lỗi script hoặc assembly definition trong thư mục GameUp SDK. Sửa lỗi biên dịch và mở lại Unity Editor nếu cần.

---

## Ghi chú định dạng

Tài liệu này dùng Markdown chuẩn. Các ví dụ code là C# và có thể copy-paste vào script game; chỉnh tên tham số (ví dụ: `placementName`, `level`, `index`, `timeSeconds`, `score`) cho phù hợp logic game của bạn.
