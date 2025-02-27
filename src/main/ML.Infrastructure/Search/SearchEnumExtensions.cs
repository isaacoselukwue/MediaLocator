using ML.Domain.Enums;

namespace ML.Infrastructure.Search;

public class SearchEnumExtensions
{
    public static string GetOpenLicenseEnumString(OpenLicenseEnum openLicenseEnum)
    {
        return openLicenseEnum switch
        {
            OpenLicenseEnum.By => "by",
            OpenLicenseEnum.ByNc => "by-nc",
            OpenLicenseEnum.ByNcNd => "by-nc-nd",
            OpenLicenseEnum.ByNcSa => "by-nc-sa",
            OpenLicenseEnum.ByNd => "by-nd",
            OpenLicenseEnum.BySa => "by-sa",
            OpenLicenseEnum.Cc0 => "cc0",
            OpenLicenseEnum.Pdm => "pdm",
            OpenLicenseEnum.NcSamplingPlus => "nc-sampling+",
            OpenLicenseEnum.SamplingPlus => "sampling+",
            _ => string.Empty
        };
    }
    public static string GetOpenLicenseTypeEnumString(OpenLicenseTypeEnum openLicenseTypeEnum)
    {
        return openLicenseTypeEnum switch
        {
            OpenLicenseTypeEnum.All => "all",
            OpenLicenseTypeEnum.Commercial => "commercial",
            OpenLicenseTypeEnum.Modification => "modification",
            OpenLicenseTypeEnum.AllCc => "all-cc",
            _ => string.Empty
        };
    }
    public static string GetOpenAudioCategoryEnumString(OpenAudioCategoryEnum openAudioCategoryEnum)
    {
        return openAudioCategoryEnum switch
        {
            OpenAudioCategoryEnum.Audiobook => "audiobook",
            OpenAudioCategoryEnum.Music => "music",
            OpenAudioCategoryEnum.News => "news",
            OpenAudioCategoryEnum.Podcast => "podcast",
            OpenAudioCategoryEnum.Pronunciation => "pronunciation",
            OpenAudioCategoryEnum.SoundEffect => "sound_effect",
            _ => string.Empty
        };
    }
    public static string GetOpenImageCategoryEnumString(OpenImageCategoryEnum openImageCategoryEnum)
    {
        return openImageCategoryEnum switch
        {
            OpenImageCategoryEnum.DigitalisedArtwork => "digitalised-artwork",
            OpenImageCategoryEnum.Illustration => "illustration",
            OpenImageCategoryEnum.Photograph => "photograph",
            _ => string.Empty
        };
    }
}
