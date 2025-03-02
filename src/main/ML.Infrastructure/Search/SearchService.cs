using Microsoft.Extensions.Caching.Hybrid;
using ML.Application.Common.Models;
using ML.Application.Media.Queries;
using ML.Domain.Enums;
using ML.Infrastructure.OpenVerse;
using ML.Infrastructure.OpenVerse.DTOs;

namespace ML.Infrastructure.Search;

public class SearchService(HybridCache hybridCache, IOpenVerseService openVerseService) : ISearchService
{
    public async Task<Result<AudioSearchResult>> GetAudioDetails(string audioId, CancellationToken cancellationToken)
    {
        string cacheKey = $"audio_detail_{audioId}";
        var result = await hybridCache.GetOrCreateAsync(cacheKey, async token =>
        {
            var audio = await openVerseService.GetAudioDetailAsync(audioId, token);
            return audio is not null ? Result<AudioSearchResult>.Success("Successfully fetched audio", MapAudioDetailResponse(audio)) 
                : Result<AudioSearchResult>.Failure("Audio details not found", ["Audio could not be retrieved"]);
        }, cancellationToken: cancellationToken);
        return result is null ? Result<AudioSearchResult>.Failure("We could not retrieve audio at this time", ["An unexpected error occurred"]) : result;
    }
    public async Task<Result<ImageSearchResult>> GetImageDetails(string imageId, CancellationToken cancellationToken)
    {
        string cacheKey = $"image_detail_{imageId}";
        var result = await hybridCache.GetOrCreateAsync(cacheKey, async token =>
        {
            var image = await openVerseService.GetImageDetailAsync(imageId, token);
            return image is not null ? Result<ImageSearchResult>.Success("Successfully fetched image", MapImageDetailResponse(image))
                : Result<ImageSearchResult>.Failure("Image details not found", ["Image could not be retrieved"]);
        }, cancellationToken: cancellationToken);
        return result is null ? Result<ImageSearchResult>.Failure("We could not retrieve image at this time", ["An unexpected error occurred"]) : result;
    }
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

    public async Task<Result<ImageSearchDto>> SearchImage(string searchQuery, OpenLicenseEnum? license, OpenLicenseTypeEnum? licenseType, OpenImageCategoryEnum? category, int pageNumber, CancellationToken cancellationToken)
    {
        string licenseString = string.Empty;
        if (license is not null && license.HasValue)
            licenseString = SearchEnumExtensions.GetOpenLicenseEnumString(license.Value);
        string licenseTypeString = string.Empty;
        if (licenseType is not null && licenseType.HasValue)
            licenseTypeString = SearchEnumExtensions.GetOpenLicenseTypeEnumString(licenseType.Value);
        string categoryString = string.Empty;
        if (category is not null && category.HasValue)
            categoryString = SearchEnumExtensions.GetOpenImageCategoryEnumString(category.Value);

        string cacheKey = $"image_search_{searchQuery}_{licenseString}_{licenseTypeString}_{categoryString}_{pageNumber}";

        // Call search API
        var result = await hybridCache.GetOrCreateAsync(cacheKey, async token =>
        {
            var searchResult = await openVerseService.SearchImagesAsync(searchQuery, token, licenseString, licenseTypeString, categoryString, page: pageNumber);
            return Result<ImageSearchDto>.Success("Successfully fetched images", MapImageResponse(searchResult));
        }, cancellationToken: cancellationToken);
        return result is null ? Result<ImageSearchDto>.Failure("We could not retrieve image at this time", ["An unexpected error occurred"]) : result;
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
    private static AudioSearchResult MapAudioDetailResponse(AudioResult audioResult)
    {
        AudioSearchResult audioSearchResult = new()
        {
            Id = audioResult.Id,
            Title = audioResult.Title,
            IndexedOn = audioResult.IndexedOn,
            ForeignLandingUrl = audioResult.ForeignLandingUrl,
            Url = audioResult.Url,
            Creator = audioResult.Creator,
            CreatorUrl = audioResult.CreatorUrl,
            License = audioResult.License,
            LicenseVersion = audioResult.LicenseVersion,
            LicenseUrl = audioResult.LicenseUrl,
            Provider = audioResult.Provider,
            Source = audioResult.Source,
            Category = audioResult.Category,
            Genres = audioResult.Genres,
            FileSize = audioResult.FileSize,
            FileType = audioResult.FileType
        };
        return audioSearchResult;
    }
    private static ImageSearchDto MapImageResponse(ImageSearchResponse imageSearchResponse)
    {
        ImageSearchDto audioSearchDto = new()
        {
            Page = imageSearchResponse.Page,
            Size = imageSearchResponse.PageSize,
            TotalPages = imageSearchResponse.PageCount,
            TotalResults = imageSearchResponse.ResultCount,
            Results = [.. imageSearchResponse.Results.Select(x => new ImageSearchResult
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
                FileType = x.FileType,
                ThumbNail = x.Thumbnail
            })]
        };
        return audioSearchDto;
    }
    private static ImageSearchResult MapImageDetailResponse(ImageResult imageResult)
    {
        ImageSearchResult imageSearchResult = new()
        {
            Id = imageResult.Id,
            Title = imageResult.Title,
            IndexedOn = imageResult.IndexedOn,
            ForeignLandingUrl = imageResult.ForeignLandingUrl,
            Url = imageResult.Url,
            Creator = imageResult.Creator,
            CreatorUrl = imageResult.CreatorUrl,
            License = imageResult.License,
            LicenseVersion = imageResult.LicenseVersion,
            LicenseUrl = imageResult.LicenseUrl,
            Provider = imageResult.Provider,
            Source = imageResult.Source,
            Category = imageResult.Category,
            FileType = imageResult.FileType,
            ThumbNail = imageResult.Thumbnail
        };
        return imageSearchResult;
    }
}
