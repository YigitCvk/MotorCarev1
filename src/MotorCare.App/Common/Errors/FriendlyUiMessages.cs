namespace MotorCare.App.Common.Errors;

/// <summary>
/// Central catalog of Turkish user-facing error messages for Blazor UI.
/// Use these instead of ex.Message when a more contextual override is needed.
/// </summary>
public static class FriendlyUiMessages
{
    public const string UnexpectedError = "Şu anda sistemsel bir sorun oluştu. Lütfen daha sonra tekrar deneyin.";
    public const string NetworkError = "Sunucuya ulaşılamıyor. Lütfen internet bağlantınızı kontrol edin.";

    public const string LoginFailed = "İşletme kodu, e-posta veya şifre hatalı.";
    public const string EmailNotVerified = "E-posta adresinizi doğrulamanız gerekiyor.";
    public const string UserInactive = "Hesabınız şu anda aktif değil.";
    public const string TenantInactive = "İşletmeniz şu anda aktif değil. Lütfen destek ile iletişime geçin.";
    public const string TooManyAttempts = "Çok fazla başarısız deneme yapıldı. Lütfen daha sonra tekrar deneyin.";
    public const string SessionExpired = "Oturumunuzun süresi dolmuş. Lütfen tekrar giriş yapın.";

    public const string Forbidden = "Bu işlemi yapma yetkiniz bulunmuyor.";
    public const string NotFound = "İstenen kayıt bulunamadı.";
    public const string Conflict = "Bu işlem mevcut kayıtlarla çakışıyor. Lütfen bilgileri kontrol edin.";
    public const string ValidationError = "Lütfen zorunlu alanları doğru şekilde doldurun.";

    public const string InviteInvalid = "Davet bağlantısı geçersiz veya süresi dolmuş.";
    public const string PasswordResetFailed = "Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.";
    public const string EmailVerificationFailed = "Doğrulama kodu hatalı veya süresi dolmuş.";
}
