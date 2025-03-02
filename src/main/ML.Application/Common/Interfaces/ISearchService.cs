using ML.Application.Common.Models;
using ML.Application.Media.Queries;
using ML.Domain.Enums;

namespace ML.Application.Common.Interfaces;

public interface ISearchService
{
    Task<Result<AudioSearchResult>> GetAudioDetails(string audioId, CancellationToken cancellationToken);
    Task<Result<ImageSearchResult>> GetImageDetails(string imageId, CancellationToken cancellationToken);
    Task<Result<AudioSearchDto>> SearchAudio(
        string searchQuery, 
        OpenLicenseEnum? license, 
        OpenLicenseTypeEnum? licenseType, 
        OpenAudioCategoryEnum? category, 
        int pageNumber,
        CancellationToken cancellationToken);
    Task<Result<ImageSearchDto>> SearchImage(
        string searchQuery,
        OpenLicenseEnum? license,
        OpenLicenseTypeEnum? licenseType,
        OpenImageCategoryEnum? category,
        int pageNumber,
        CancellationToken cancellationToken);
}
