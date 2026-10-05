using System;
using System.Globalization;
using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Các hàm hỗ trợ thời gian thực (UTC) dùng cho save/load, cooldown, tính thưởng offline,
    /// hết hạn (expire) và lập lịch. Lưu dưới dạng Unix milliseconds (<see cref="long"/>) nên
    /// có thể lưu thẳng vào save data mà không lo sai lệch múi giờ.
    ///
    /// Thời gian thực (wall-clock) vẫn trôi khi đang xem quảng cáo rewarded/interstitial hoặc
    /// khi hệ điều hành pause app, nên nó phù hợp cho những thứ cần tồn tại kể cả khi tắt app
    /// (cooldown, reset hằng ngày, thưởng offline). Nhưng nó là nguồn SAI nếu dùng cho buff có
    /// thời hạn kiểu "x2 phần thưởng trong 5 phút": nếu mốc hết hạn là một timestamp UTC, mỗi
    /// giây xem quảng cáo sẽ bị trừ thẳng vào buff dù người chơi chưa chơi giây nào. Hãy dùng
    /// <see cref="GameTimeCountdown"/> cho trường hợp đó - nó chỉ giảm khi được tick thủ công
    /// từ gameplay (vd trong Update), nên quảng cáo và việc app bị đẩy xuống nền sẽ không "ăn"
    /// mất thời gian buff.
    ///
    /// Chống cheat: <see cref="SafeNowMs"/> kẹp đồng hồ để không bao giờ bị quan sát thấy chạy
    /// lùi so với giá trị cao nhất mà máy này từng ghi nhận (lưu trong PlayerPrefs), nhờ đó vô
    /// hiệu hóa chiêu cheat kinh điển "vặn đồng hồ hệ thống lùi lại" để reset cooldown hoặc ăn
    /// gian thưởng offline. Nó KHÔNG chống được việc vặn đồng hồ tới (forward); nếu game cần
    /// chặn luôn trường hợp này, hãy gắn <see cref="ExternalTimeProviderMs"/> vào một nguồn thời
    /// gian đáng tin cậy (server/NTP).
    /// </summary>
    public static class UtilityTime
    {
        const string ClockGuardKey = "SketchEngine.UtilityTime.LastObservedUnixMs";
        const long PersistThresholdMs = 5000; // tránh ghi PlayerPrefs ở mỗi lần gọi

        static long _lastObservedUnixMs = -1;
        static long _lastPersistedUnixMs = -1;

        /// <summary>
        /// Nguồn thời gian đáng tin cậy tùy chọn (server/NTP), tính bằng Unix milliseconds.
        /// Khi được gán, nó sẽ thay thế đồng hồ thiết bị cho mọi lần đọc trong class này.
        /// Để null để dùng <see cref="DateTimeOffset.UtcNow"/> mặc định.
        /// </summary>
        public static Func<long> ExternalTimeProviderMs;

        // =====================================================
        // THỜI ĐIỂM HIỆN TẠI
        // =====================================================

        /// <summary>Unix ms thô (đồng hồ thiết bị, hoặc <see cref="ExternalTimeProviderMs"/> nếu có). Không chống bị vặn lùi.</summary>
        public static long NowMs => ExternalTimeProviderMs != null ? ExternalTimeProviderMs() : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>Unix giây thô. Không chống bị vặn lùi.</summary>
        public static long NowSeconds => NowMs / 1000;

        /// <summary>
        /// Unix ms đã được kẹp về giá trị cao nhất từng ghi nhận trên thiết bị này. Dùng cái này
        /// cho MỌI logic cooldown/hết hạn/thưởng offline; chỉ dùng <see cref="NowMs"/> để hiển thị.
        /// </summary>
        public static long SafeNowMs
        {
            get
            {
                EnsureGuardLoaded();
                long now = NowMs;
                if (now > _lastObservedUnixMs)
                {
                    _lastObservedUnixMs = now;
                    if (now - _lastPersistedUnixMs >= PersistThresholdMs)
                        PersistGuard(now);
                }
                return _lastObservedUnixMs;
            }
        }

        static void EnsureGuardLoaded()
        {
            if (_lastObservedUnixMs >= 0) return;
            string saved = PlayerPrefs.GetString(ClockGuardKey, null);
            _lastObservedUnixMs = !string.IsNullOrEmpty(saved) && long.TryParse(saved, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
                ? parsed
                : 0L;
            _lastPersistedUnixMs = _lastObservedUnixMs;
        }

        static void PersistGuard(long unixMs)
        {
            _lastPersistedUnixMs = unixMs;
            PlayerPrefs.SetString(ClockGuardKey, unixMs.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Ép ghi guard chống vặn lùi xuống đĩa ngay lập tức. Gọi từ OnApplicationPause(true) và
        /// OnApplicationQuit để nếu process bị kill, đồng hồ cũng không bị vặn lùi qua khỏi lần ghi gần nhất.
        /// </summary>
        public static void FlushClockGuard()
        {
            EnsureGuardLoaded();
            if (_lastObservedUnixMs != _lastPersistedUnixMs)
                PersistGuard(_lastObservedUnixMs);
            PlayerPrefs.Save();
        }

        // =====================================================
        // CHUYỂN ĐỔI
        // =====================================================

        public static long ToUnixSeconds(DateTime utcDateTime) => new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();
        public static long ToUnixMilliseconds(DateTime utcDateTime) => new DateTimeOffset(utcDateTime).ToUnixTimeMilliseconds();
        public static DateTime FromUnixSeconds(long unixSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
        public static DateTime FromUnixMilliseconds(long unixMilliseconds) => DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;

        // =====================================================
        // THỜI ĐIỂM LƯU (SAVE TIME)
        // =====================================================

        public static long CreateSaveTime() => SafeNowMs;
        public static bool IsValidSaveTime(long savedUnixMs) => savedUnixMs > 0;

        // =====================================================
        // KHOẢNG THỜI GIAN (đã chống vặn lùi)
        // =====================================================

        public static int SecondsSince(long savedUnixMs)
        {
            if (savedUnixMs <= 0) return 0;
            long delta = SafeNowMs - savedUnixMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        public static TimeSpan TimeSince(long savedUnixMs)
        {
            if (savedUnixMs <= 0) return TimeSpan.Zero;
            long delta = SafeNowMs - savedUnixMs;
            return delta <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(delta);
        }

        public static int DurationSeconds(long startUnixMs, long endUnixMs)
        {
            long delta = endUnixMs - startUnixMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        /// <summary>Giới hạn thời gian đã trôi qua, vd để chặn trần thưởng offline tối đa.</summary>
        public static int ClampElapsedSeconds(long savedUnixMs, int maxSeconds) => Mathf.Min(SecondsSince(savedUnixMs), maxSeconds);

        // =====================================================
        // HẾT HẠN / COOLDOWN (đã chống vặn lùi)
        // =====================================================

        public static long CreateExpireAfterSeconds(int seconds) => SafeNowMs + (long)seconds * 1000;
        public static bool IsExpired(long expireUnixMs) => SafeNowMs >= expireUnixMs;

        public static int SecondsLeft(long expireUnixMs)
        {
            long delta = expireUnixMs - SafeNowMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        public static TimeSpan TimeLeft(long expireUnixMs)
        {
            long delta = expireUnixMs - SafeNowMs;
            return delta <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(delta);
        }

        // =====================================================
        // KHOẢNG GIÁ TRỊ / CLAMP
        // =====================================================

        public static bool IsNowInRange(long startUnixMs, long endUnixMs)
        {
            long now = SafeNowMs;
            return now >= startUnixMs && now <= endUnixMs;
        }

        public static long ClampUnix(long unixMs, long min, long max) => unixMs < min ? min : unixMs > max ? max : unixMs;

        /// <summary>Tiến độ từ start đến end, được clamp về 0-1.</summary>
        public static float Progress01(long startUnixMs, long endUnixMs)
        {
            if (endUnixMs <= startUnixMs) return 1f;
            return Mathf.Clamp01((float)(SafeNowMs - startUnixMs) / (endUnixMs - startUnixMs));
        }

        // =====================================================
        // ĐỊNH DẠNG - KHÔNG GC (Span)
        // =====================================================

        /// <summary>Ghi "m:ss" (phút không đệm số 0, có thể vượt quá 2 chữ số). False nếu buffer đích quá nhỏ.</summary>
        public static bool TryFormatMMSS(int totalSeconds, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (totalSeconds < 0) totalSeconds = 0;
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;

            if (!m.TryFormat(destination, out int mLen, default, CultureInfo.InvariantCulture)) return false;
            int cursor = mLen;
            if (cursor >= destination.Length) return false;
            destination[cursor++] = ':';
            if (destination.Length - cursor < 2) return false;
            WriteTwoDigits(s, destination.Slice(cursor));
            charsWritten = cursor + 2;
            return true;
        }

        /// <summary>Ghi "h:mm:ss" (giờ không đệm số 0, có thể vượt quá 2 chữ số). False nếu buffer đích quá nhỏ.</summary>
        public static bool TryFormatHHMMSS(int totalSeconds, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (totalSeconds < 0) totalSeconds = 0;
            int h = totalSeconds / 3600;
            int m = (totalSeconds % 3600) / 60;
            int s = totalSeconds % 60;

            if (!h.TryFormat(destination, out int hLen, default, CultureInfo.InvariantCulture)) return false;
            int cursor = hLen;
            if (cursor >= destination.Length) return false;
            destination[cursor++] = ':';
            if (destination.Length - cursor < 2) return false;
            WriteTwoDigits(m, destination.Slice(cursor));
            cursor += 2;
            if (cursor >= destination.Length) return false;
            destination[cursor++] = ':';
            if (destination.Length - cursor < 2) return false;
            WriteTwoDigits(s, destination.Slice(cursor));
            charsWritten = cursor + 2;
            return true;
        }

        static void WriteTwoDigits(int value, Span<char> destination)
        {
            destination[0] = (char)('0' + (value / 10) % 10);
            destination[1] = (char)('0' + value % 10);
        }

        /// <summary>"h:mm:ss" khi &gt;= 1 giờ, ngược lại "m:ss". Ưu tiên dùng bản TryFormat cho hot path.</summary>
        public static string FormatAuto(int seconds)
        {
            Span<char> buffer = stackalloc char[24];
            bool ok = seconds >= 3600 ? TryFormatHHMMSS(seconds, buffer, out int written) : TryFormatMMSS(seconds, buffer, out written);
            return ok ? new string(buffer.Slice(0, written)) : seconds.ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatMMSS(int seconds)
        {
            Span<char> buffer = stackalloc char[24];
            return TryFormatMMSS(seconds, buffer, out int written) ? new string(buffer.Slice(0, written)) : "0:00";
        }

        public static string FormatHHMMSS(int seconds)
        {
            Span<char> buffer = stackalloc char[24];
            return TryFormatHHMMSS(seconds, buffer, out int written) ? new string(buffer.Slice(0, written)) : "0:00:00";
        }

        public static string FormatTime(TimeSpan time) => FormatAuto((int)time.TotalSeconds);
        public static string FormatTimeLeft(long expireUnixMs) => FormatAuto(SecondsLeft(expireUnixMs));
        public static string FormatTimeSince(long savedUnixMs) => FormatAuto(SecondsSince(savedUnixMs));

        /// <summary>Thời lượng dạng dễ đọc, gọn: "2d 3h", "3h 5m", "5m 2s" hoặc "2s". Chỉ giữ lại 2 đơn vị lớn nhất.</summary>
        public static string FormatHuman(int seconds)
        {
            if (seconds <= 0) return "0s";

            int d = seconds / 86400; seconds %= 86400;
            int h = seconds / 3600; seconds %= 3600;
            int m = seconds / 60;
            int s = seconds % 60;

            Span<char> buffer = stackalloc char[32];
            int cursor = 0;

            if (d > 0) { cursor = AppendUnit(buffer, cursor, d, 'd'); cursor = AppendUnit(buffer, cursor, h, 'h', leadingSpace: true); }
            else if (h > 0) { cursor = AppendUnit(buffer, cursor, h, 'h'); cursor = AppendUnit(buffer, cursor, m, 'm', leadingSpace: true); }
            else if (m > 0) { cursor = AppendUnit(buffer, cursor, m, 'm'); cursor = AppendUnit(buffer, cursor, s, 's', leadingSpace: true); }
            else { cursor = AppendUnit(buffer, cursor, s, 's'); }

            return new string(buffer.Slice(0, cursor));
        }

        static int AppendUnit(Span<char> buffer, int cursor, int value, char unit, bool leadingSpace = false)
        {
            if (leadingSpace) buffer[cursor++] = ' ';
            value.TryFormat(buffer.Slice(cursor), out int written, default, CultureInfo.InvariantCulture);
            cursor += written;
            buffer[cursor++] = unit;
            return cursor;
        }
    }

    /// <summary>
    /// Bộ đếm ngược tính theo thời gian chơi thực tế, không phải thời gian thực (wall-clock).
    /// Nó chỉ giảm khi <see cref="Tick"/> được gọi (vd mỗi frame với Time.unscaledDeltaTime từ
    /// Update của một MonoBehaviour đang chạy). Dùng cái này cho các buff theo phiên chơi như
    /// "x2 phần thưởng trong 5 phút": vì Update không chạy khi quảng cáo rewarded/interstitial
    /// đang hiện hoặc app bị đẩy xuống nền, những khoảng thời gian đó không tốn gì của người
    /// chơi - khác với mốc hết hạn UTC của <see cref="UtilityTime"/>, vốn vẫn đếm ngược liên tục.
    ///
    /// Dùng double để tránh trôi số do sai số float khi phiên chơi kéo dài. Nếu buff cần tồn
    /// tại qua lần khởi động lại app, hãy lưu <see cref="RemainingSeconds"/> (không lưu
    /// timestamp) và khôi phục bằng <see cref="Restore"/>. Không cấp phát (GC) sau khi khởi tạo.
    /// Không an toàn đa luồng; chỉ tick từ một nơi duy nhất.
    /// </summary>
    [Serializable]
    public struct GameTimeCountdown
    {
        public double DurationSeconds { get; private set; }
        public double RemainingSeconds { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFinished => RemainingSeconds <= 0;
        public float Progress01 => DurationSeconds <= 0 ? 1f : (float)(1d - RemainingSeconds / DurationSeconds);

        public GameTimeCountdown(double durationSeconds)
        {
            DurationSeconds = durationSeconds > 0 ? durationSeconds : 0;
            RemainingSeconds = DurationSeconds;
            IsRunning = DurationSeconds > 0;
        }

        /// <summary>Dựng lại bộ đếm từ giá trị remaining-seconds đã lưu trước đó (vd load từ đĩa).</summary>
        public static GameTimeCountdown Restore(double durationSeconds, double remainingSeconds)
        {
            double duration = durationSeconds > 0 ? durationSeconds : 0;
            double remaining = remainingSeconds < 0 ? 0 : remainingSeconds > duration ? duration : remainingSeconds;
            return new GameTimeCountdown { DurationSeconds = duration, RemainingSeconds = remaining, IsRunning = remaining > 0 };
        }

        /// <summary>Giảm bộ đếm theo số giây gameplay thực sự đã trôi qua. Không làm gì khi đang pause hoặc đã kết thúc.</summary>
        public void Tick(double unscaledDeltaSeconds)
        {
            if (!IsRunning || unscaledDeltaSeconds <= 0) return;
            RemainingSeconds -= unscaledDeltaSeconds;
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = 0;
                IsRunning = false;
            }
        }

        public void Pause() => IsRunning = false;
        public void Resume() => IsRunning = RemainingSeconds > 0;

        /// <summary>Cộng thêm thời gian thưởng (vd cộng dồn thêm 1 boost khác), tăng cả thời gian còn lại lẫn tổng thời lượng.</summary>
        public void Extend(double extraSeconds)
        {
            if (extraSeconds <= 0) return;
            RemainingSeconds += extraSeconds;
            DurationSeconds += extraSeconds;
            IsRunning = RemainingSeconds > 0;
        }

        public void Cancel()
        {
            RemainingSeconds = 0;
            IsRunning = false;
        }
    }
}
