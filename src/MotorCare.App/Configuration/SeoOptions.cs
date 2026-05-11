namespace MotorCare.App.Configuration;

public sealed class SeoOptions
{
    public const string SectionName = "Seo";

    public string SiteName { get; set; } = "GarajPass";

    public string SiteBaseUrl { get; set; } = "http://46.225.166.254";

    public string DefaultTitle { get; set; } =
        "GarajPass | Servis, Ekspertiz ve Arac Gecmisi Yonetim Platformu";

    public string DefaultDescription { get; set; } =
        "GarajPass; servis isletmeleri icin musteri, arac, randevu, servis emri, ekspertiz ve arac gecmisi kayitlarini tek panelden yoneten modern servis yonetim platformudur.";

    public string OgImageUrl { get; set; } = "/images/og-garajpass.svg";

    public bool RobotsIndexEnabled { get; set; }
}
