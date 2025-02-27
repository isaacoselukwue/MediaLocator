using Microsoft.Extensions.Caching.Hybrid;
using ML.Application.Common.Models;
using ML.Application.Media.Queries;
using ML.Domain.Enums;
using ML.Infrastructure.OpenVerse;
using ML.Infrastructure.OpenVerse.DTOs;

namespace ML.Infrastructure.Search;

public class SearchService(HybridCache hybridCache, IOpenVerseService openVerseService) : ISearchService
{

    public async Task<Result<AudioSearchDto>> SearchAudio(string searchQuery, OpenLicenseEnum? license, OpenLicenseTypeEnum? licenseType, OpenAudioCategoryEnum? category, int pageNumber, CancellationToken cancellationToken)
    {
        string licenseString = string.Empty;
        if (license is not null && license.HasValue)
            licenseString = SearchEnumExtensions.GetOpenLicenseEnumString(license.Value);
        string licenseTypeString = string.Empty;
        if (licenseType is not null && licenseType.HasValue)
            licenseTypeString = SearchEnumExtensions.GetOpenLicenseTypeEnumString(licenseType.Value);
        string categoryString = string.Empty;
        if (category is not null && category.HasValue)
            categoryString = SearchEnumExtensions.GetOpenAudioCategoryEnumString(category.Value);

        string cacheKey = $"audio_search_{searchQuery}_{licenseString}_{licenseTypeString}_{categoryString}_{pageNumber}";

        // Call search API
        var result = await hybridCache.GetOrCreateAsync(cacheKey, async token =>
        {
            var searchResult = await openVerseService.SearchAudioAsync(searchQuery, token, licenseString, licenseTypeString, categoryString, page: pageNumber);
            return Result<AudioSearchDto>.Success("Successfully fetched images", MapAudioResponse(searchResult));
        }, cancellationToken: cancellationToken);
        return result is null ? Result<AudioSearchDto>.Failure("We could not retrieve audio at this time", ["An unexpected error occurred"]) : result;
    }
    private static AudioSearchDto MapAudioResponse(AudioSearchResponse audioSearchResponse)
    {
        AudioSearchDto audioSearchDto = new()
        {
            Page = audioSearchResponse.Page,
            Size = audioSearchResponse.PageSize,
            TotalPages = audioSearchResponse.PageCount,
            TotalResults = audioSearchResponse.ResultCount,
            Results = [.. audioSearchResponse.Results.Select(x => new AudioSearchResult
            {
                Id = x.Id,
                Title = x.Title,
                IndexedOn = x.IndexedOn,
                ForeignLandingUrl = x.ForeignLandingUrl,
                Url = x.Url,
                Creator = x.Creator,
                CreatorUrl = x.CreatorUrl,
                License = x.License,
                LicenseVersion = x.LicenseVersion,
                LicenseUrl = x.LicenseUrl,
                Provider = x.Provider,
                Source = x.Source,
                Category = x.Category,
                Genres = x.Genres,
                FileSize = x.FileSize,
                FileType = x.FileType
            })]
        };
        return audioSearchDto;
    }
}
