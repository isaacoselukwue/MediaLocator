using Microsoft.Extensions.Caching.Hybrid;
using ML.Application.Common.Models;
using ML.Application.Media.Queries;
using ML.Domain.Enums;
using ML.Infrastructure.OpenVerse;
using ML.Infrastructure.OpenVerse.DTOs;

namespace ML.Infrastructure.Search;

public class SearchService(HybridCache hybridCache, IMLDbContext mlDbContext, IJwtService jwtService, IOpenVerseService openVerseService) : ISearchService
{
    public async Task<Result> AddSearchHistory(string searchId, SearchTypeEnum searchType, CancellationToken cancellationToken)
    {
        if(searchType == SearchTypeEnum.Audio)
            return await AddAudioSearch(searchId, cancellationToken);
        else if (searchType == SearchTypeEnum.Image)
            return await AddImageSearch(searchId, cancellationToken);
        else
            return Result.Failure("Invalid search type", ["Search type is not valid"]);
    }
    private async Task<Result> AddAudioSearch(string searchId, CancellationToken cancellationToken)
    {
        Result<AudioSearchResult> audioResult = await GetAudioDetails(searchId, cancellationToken);
        if (!audioResult.Succeeded)
            return Result.Failure("Audio details not found", ["Audio could not be retrieved"]);
        Guid userId = jwtService.GetUserId();
        if (await mlDbContext.SearchHistories.AnyAsync(x => x.SearchId == searchId && x.UserId == userId, cancellationToken))
            return Result.Success("Search history already saved!");

        SearchHistories searchHistory = new()
        {
            Attribuition = audioResult.Data?.Attribution,
            Category = audioResult.Data?.Category,
            Created = DateTimeOffset.Now,
            CreatedBy = jwtService.GetEmailAddress(),
            Creator = audioResult.Data?.Creator,
            CreatorUrl = audioResult.Data?.CreatorUrl,
            FileSize = audioResult.Data?.FileSize,
            FileType = audioResult.Data?.FileType,
            ForeignLandingUrl = audioResult.Data?.ForeignLandingUrl,
            Genres = audioResult.Data?.Genres is not null ? string.Join("|", audioResult.Data.Genres) : null,
            IndexedOn = audioResult.Data?.IndexedOn,
            LastModified = DateTimeOffset.Now,
            LastModifiedBy = jwtService.GetEmailAddress(),
            License = audioResult.Data?.License,
            LicenseUrl = audioResult.Data?.LicenseUrl,
            LicenseVersion = audioResult.Data?.LicenseVersion,
            Provider = audioResult.Data?.Provider,
            RelatedUrl = audioResult.Data?.RelatedUrl,
            SearchId = searchId,
            SearchType = SearchTypeEnum.Audio,
            Source = audioResult.Data?.Source,
            Status = StatusEnum.Active,
            Title = audioResult.Data?.Title,
            ThumbNail = audioResult.Data?.Thumbnail,
            Url = audioResult.Data?.Url,
            UserId = userId
        };
        await mlDbContext.SearchHistories.AddAsync(searchHistory, cancellationToken);
        await mlDbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Search history added successfully");
    }
    private async Task<Result> AddImageSearch(string searchId, CancellationToken cancellationToken)
    {
        // Add search history
        Result<ImageSearchResult> imageResult = await GetImageDetails(searchId, cancellationToken);
        if (!imageResult.Succeeded)
            return Result.Failure("Image details not found", ["Image could not be retrieved"]);

        Guid userId = jwtService.GetUserId();
        if (await mlDbContext.SearchHistories.AnyAsync(x => x.SearchId == searchId && x.UserId == userId, cancellationToken))
            return Result.Success("Search history already saved!");

        SearchHistories searchHistory = new()
        {
            Attribuition = imageResult.Data?.Attribution,
            Category = imageResult.Data?.Category,
            Created = DateTimeOffset.Now,
            CreatedBy = jwtService.GetEmailAddress(),
            Creator = imageResult.Data?.Creator,
            CreatorUrl = imageResult.Data?.CreatorUrl,
            FileSize = imageResult.Data?.FileSize,
            FileType = imageResult.Data?.FileType,
            ForeignLandingUrl = imageResult.Data?.ForeignLandingUrl,
            Genres = null,
            IndexedOn = imageResult.Data?.IndexedOn,
            LastModified = DateTimeOffset.Now,
            LastModifiedBy = jwtService.GetEmailAddress(),
            License = imageResult.Data?.License,
            LicenseUrl = imageResult.Data?.LicenseUrl,
            LicenseVersion = imageResult.Data?.LicenseVersion,
            Provider = imageResult.Data?.Provider,
            RelatedUrl = imageResult.Data?.RelatedUrl,
            SearchId = searchId,
            SearchType = SearchTypeEnum.Image,
            Source = imageResult.Data?.Source,
            Status = StatusEnum.Active,
            ThumbNail = imageResult.Data?.ThumbNail,
            Title = imageResult.Data?.Title,
            Url = imageResult.Data?.Url,
            UserId = userId
        };
        await mlDbContext.SearchHistories.AddAsync(searchHistory, cancellationToken);
        await mlDbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Search history added successfully");
    }
    public async Task<Result> DeleteSearchHistory(Guid Id, CancellationToken cancellationToken)
    {
        Guid userId = jwtService.GetUserId();
        SearchHistories? searchHistory = await mlDbContext.SearchHistories.FirstOrDefaultAsync(x => x.Id == Id && x.UserId == userId, cancellationToken);
        if (searchHistory is null || searchHistory.Status != StatusEnum.Active)
            return Result.Failure("Search history not found", ["Search history could not be found"]);

        searchHistory.Status = StatusEnum.Deleted;
        searchHistory.LastModified = DateTimeOffset.Now;
        searchHistory.LastModifiedBy = jwtService.GetEmailAddress();

        int update = await mlDbContext.SaveChangesAsync(cancellationToken);
        return update > 0 ? Result.Success("Search history deleted successfully") : Result.Failure("Search history could not be deleted", ["An unexpected error occurred"]);
    }
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
    public async Task<Result<AdminSearchHistoryDto>> GetAdminSearchHistory(string? title, string? startDate, string? endDate, string? emailAddress, StatusEnum? status, bool isAscendingSorted, int pageNumber, CancellationToken cancellationToken)
    {
        IQueryable<SearchHistories> searchHistories = mlDbContext.SearchHistories;
        if (!string.IsNullOrWhiteSpace(title))
            searchHistories = searchHistories.Where(x => x.Title != null && x.Title.Contains(title));
        if (!string.IsNullOrWhiteSpace(startDate))
            searchHistories = searchHistories.Where(x => x.Created >= DateTimeOffset.Parse(startDate));
        if (!string.IsNullOrWhiteSpace(endDate))
            searchHistories = searchHistories.Where(x => x.Created <= DateTimeOffset.Parse(endDate).AddDays(-1));
        if (!string.IsNullOrWhiteSpace(emailAddress))
            searchHistories = searchHistories.Where(x => x.CreatedBy != null && x.CreatedBy.Contains(emailAddress));
        if (status is not null && status.HasValue)
            searchHistories = searchHistories.Where(x => x.Status == status);
        if (status is null)
            searchHistories = searchHistories.Where(x => x.Status == StatusEnum.Active);
        if (isAscendingSorted)
            searchHistories = searchHistories.OrderBy(x => x.Created);
        else
            searchHistories = searchHistories.OrderByDescending(x => x.Created);
        int totalResults = await searchHistories.CountAsync(cancellationToken);
        int pageSize = 20;
        List<SearchHistories> searchHistoriesList = await searchHistories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        AdminSearchHistoryDto adminSearchHistoryDto = new()
        {
            Page = pageNumber,
            Size = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalResults / pageSize),
            TotalResults = totalResults,
            Results = MapAdminSearchHistoryResult(searchHistoriesList)
        };
        return Result<AdminSearchHistoryDto>.Success("Successfully fetched search history", adminSearchHistoryDto);
    }
    public async Task<Result<UsersSearchHistoryDto>> GetUsersSearchHistory(string? title, string? startDate, string? endDate, bool isAscendingSorted, int pageNumber, CancellationToken cancellationToken)
    {
        IQueryable<SearchHistories> searchHistories = mlDbContext.SearchHistories.Where(x => x.UserId == jwtService.GetUserId() && x.Status == StatusEnum.Active);

        if (!string.IsNullOrWhiteSpace(title))
            searchHistories = searchHistories.Where(x => x.Title != null && x.Title.Contains(title));
        if (!string.IsNullOrWhiteSpace(startDate))
            searchHistories = searchHistories.Where(x => x.Created >= DateTimeOffset.Parse(startDate));
        if (!string.IsNullOrWhiteSpace(endDate))
            searchHistories = searchHistories.Where(x => x.Created <= DateTimeOffset.Parse(endDate).AddDays(-1));
        if(string.IsNullOrWhiteSpace(endDate) && string.IsNullOrWhiteSpace(startDate))
            searchHistories = searchHistories.Where(x => x.Created >= DateTimeOffset.UtcNow.AddDays(-30));
        if (string.IsNullOrWhiteSpace(endDate) && !string.IsNullOrWhiteSpace(startDate))
            searchHistories = searchHistories.Where(x => x.Created <= DateTimeOffset.UtcNow.AddDays(1));
        if (isAscendingSorted)
            searchHistories = searchHistories.OrderBy(x => x.Created);
        else
            searchHistories = searchHistories.OrderByDescending(x => x.Created);

        int totalResults = await searchHistories.CountAsync(cancellationToken);
        int pageSize = 20;

        List<SearchHistories> searchHistoriesList = await searchHistories.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        UsersSearchHistoryDto usersSearchHistoryDto = new()
        {
            Page = pageNumber,
            Size = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalResults / pageSize),
            TotalResults = totalResults,
            Results = MapUserSearchHistoryResult(searchHistoriesList)
        };
        return Result<UsersSearchHistoryDto>.Success("Successfully fetched search history", usersSearchHistoryDto);

    }

    public async Task<Result<DailyMediaDto>> GetDailyMediaAsync(CancellationToken cancellationToken)
    {
        string wordofTheDay = WordOfTheDay();
        string cacheKey = $"daily_media_{wordofTheDay}";

        var result = await hybridCache.GetOrCreateAsync(cacheKey, async token =>
        {
            var audioTask = openVerseService.SearchAudioAsync(wordofTheDay, token, page: 1);
            var imageTask = openVerseService.SearchImagesAsync(wordofTheDay, token, page: 1);

            await Task.WhenAll(audioTask, imageTask);

            AudioSearchResponse audioResult = await audioTask;
            ImageSearchResponse imageResult = await imageTask;

            wordofTheDay = $"Word of day is: {wordofTheDay} {GetEmojiForWord(wordofTheDay)}!";
            return Result<DailyMediaDto>.Success("Successfully fetched daily media", MapDailyMedia(audioResult, imageResult, wordofTheDay));
        }, cancellationToken: cancellationToken);

        return result is null ? Result<DailyMediaDto>.Failure("We could not retrieve daily media at this time", ["An unexpected error occurred"]) : result;
    }

    private static string WordOfTheDay()
    {
        Dictionary<string, string> dayToWord = new()
        {
            { "Monday", "Positive" },
            { "Tuesday", "Drum" },
            { "Wednesday", "Happy" },
            { "Thursday", "Laugh" },
            { "Friday", "Thankful" },
            { "Saturday", "Free" },
            { "Sunday", "Love" }
        };
        return dayToWord[GetDayOfTheWeek()];
    }

    private static string GetEmojiForWord(string word)
    {
        Dictionary<string, string> wordToEmoji = new()
        {
            { "Positive", "😊" },
            { "Drum", "🥁" },
            { "Happy", "😃" },
            { "Laugh", "😂" },
            { "Thankful", "🙏" },
            { "Free", "🆓" },
            { "Love", "❤️" }
        };

        return wordToEmoji.TryGetValue(word, out string? emoji) ? emoji : string.Empty;
    }

    private static string GetDayOfTheWeek()
    {
        return DateTime.Now.DayOfWeek.ToString();
    }

    public async Task<Result<AudioSearchDto>> SearchAudio(string searchQuery, OpenLicenseEnum? license, OpenLicenseTypeEnum? licenseType, OpenAudioCategoryEnum? category, int pageNumber, CancellationToken cancellationToken)
    {
        string licenseString = string.Empty;
        if (license is not null && license.HasValue)
            licenseString = SearchExtensions.GetOpenLicenseEnumString(license.Value);
        string licenseTypeString = string.Empty;
        if (licenseType is not null && licenseType.HasValue)
            licenseTypeString = SearchExtensions.GetOpenLicenseTypeEnumString(licenseType.Value);
        string categoryString = string.Empty;
        if (category is not null && category.HasValue)
            categoryString = SearchExtensions.GetOpenAudioCategoryEnumString(category.Value);

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
            licenseString = SearchExtensions.GetOpenLicenseEnumString(license.Value);
        string licenseTypeString = string.Empty;
        if (licenseType is not null && licenseType.HasValue)
            licenseTypeString = SearchExtensions.GetOpenLicenseTypeEnumString(licenseType.Value);
        string categoryString = string.Empty;
        if (category is not null && category.HasValue)
            categoryString = SearchExtensions.GetOpenImageCategoryEnumString(category.Value);

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
                FileType = x.FileType,
                RelatedUrl = x.RelatedUrl,
                Attribution = x.Attribution,
                Thumbnail = x.Thumbnail
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
            FileType = audioResult.FileType,
            RelatedUrl = audioResult.RelatedUrl,
            Attribution = audioResult.Attribution,
            Thumbnail = audioResult.Thumbnail
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
                ThumbNail = x.Thumbnail,
                Attribution = x.Attribution,
                FileSize = x.FileSize,
                RelatedUrl = x.RelatedUrl
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
            ThumbNail = imageResult.Thumbnail,
            Attribution = imageResult.Attribution,
            FileSize = imageResult.FileSize,
            RelatedUrl = imageResult.RelatedUrl
        };
        return imageSearchResult;
    }
    private static List<UsersSearchHistoryResult> MapUserSearchHistoryResult(List<SearchHistories> searchHistories)
    {
        List<UsersSearchHistoryResult> usersSearchHistoryResults = [.. searchHistories.Select(history => new UsersSearchHistoryResult {
            Id = history.Id,
            Attribuition = history.Attribuition,
            Category = history.Category,
            Creator = history.Creator,
            CreatorUrl = history.CreatorUrl,
            FileSize = history.FileSize,
            FileType = history.FileType,
            ForeignLandingUrl = history.ForeignLandingUrl,
            Genres = history.Genres,
            IndexedOn = history.IndexedOn,
            License = history.License,
            LicenseUrl = history.LicenseUrl,
            LicenseVersion = history.LicenseVersion,
            Provider = history.Provider,
            RelatedUrl = history.RelatedUrl,
            SearchId = history.SearchId,
            SearchDate = history.Created,
            Source = history.Source,
            ThumbNail = history.ThumbNail,
            Title = history.Title,
            Url = history.Url
        })];
        return usersSearchHistoryResults;
    }
    public static List<AdminSearchHistoryResult> MapAdminSearchHistoryResult(List<SearchHistories> searchHistories)
    {
        List<AdminSearchHistoryResult> adminSearchHistoryResults = [.. searchHistories.Select(history => new AdminSearchHistoryResult {
            Id = history.Id,
            SearchId = history.SearchId,
            UserId = history.UserId,
            UsersEmail = history.CreatedBy,
            Status = history.Status,
            Title = history.Title,
            Url = history.Url,
            Creator = history.Creator,
            SearchDate = history.Created,
            License = history.License,
            Provider = history.Provider,
            Attribuition = history.Attribuition,
            RelatedUrl = history.RelatedUrl,
            IndexedOn = history.IndexedOn,
            ForeignLandingUrl = history.ForeignLandingUrl,
            CreatorUrl = history.CreatorUrl,
            LicenseVersion = history.LicenseVersion,
            LicenseUrl = history.LicenseUrl,
            Source = history.Source,
            Category = history.Category,
            Genres = history.Genres,
            FileSize = history.FileSize,
            FileType = history.FileType,
            ThumbNail = history.ThumbNail
        })];
        return adminSearchHistoryResults;
    }
    private static DailyMediaDto MapDailyMedia(AudioSearchResponse audioResult, ImageSearchResponse imageResult, string wordOfTheDay)
    {
        return new DailyMediaDto
        {
            AudioSearchResults = [.. audioResult.Results.Select(x => new AudioSearchResult
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
                    FileType = x.FileType,
                    RelatedUrl = x.RelatedUrl,
                    Attribution = x.Attribution,
                    Thumbnail = x.Thumbnail
                })],
            ImageSearchResults = [.. imageResult.Results.Select(x => new ImageSearchResult
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
                    ThumbNail = x.Thumbnail,
                    Attribution = x.Attribution,
                    FileSize = x.FileSize,
                    RelatedUrl = x.RelatedUrl
                })],
            WordOfTheDay = wordOfTheDay
        };
    }
}
