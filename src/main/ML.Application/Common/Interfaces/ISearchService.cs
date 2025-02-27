using ML.Application.Common.Models;
using ML.Application.Media.Queries;
using ML.Domain.Enums;

namespace ML.Application.Common.Interfaces;

public interface ISearchService
{
    Task<Result<AudioSearchDto>> SearchAudio(
        string searchQuery, 
        OpenLicenseEnum? license, 
        OpenLicenseTypeEnum? licenseType, 
        OpenAudioCategoryEnum? category, 
        int pageNumber,
        CancellationToken cancellationToken);
}
