using ML.Application.Common.Models;
using ML.Application.Media.Queries;
using ML.Domain.Enums;

namespace ML.Application.Common.Interfaces;

public interface ISearchService
{
    Task<Result> AddSearchHistory(string searchId, SearchTypeEnum searchType, CancellationToken cancellationToken);
    Task<Result> DeleteSearchHistory(Guid Id, CancellationToken cancellationToken);
    Task<Result<AudioSearchResult>> GetAudioDetails(string audioId, CancellationToken cancellationToken);
    Task<Result<ImageSearchResult>> GetImageDetails(string imageId, CancellationToken cancellationToken);
    Task<Result<AdminSearchHistoryDto>> GetAdminSearchHistory(
        string? title, 
        string? startDate, 
        string? endDate, 
        string? emailAddress, 
        StatusEnum? status, 
        bool isAscendingSorted, 
        int pageNumber, 
        CancellationToken cancellationToken);
    Task<Result<UsersSearchHistoryDto>> GetUsersSearchHistory(
        string? title,
        string? startDate,
        string? endDate,
        bool isAscendingSorted,
        int pageNumber,
        CancellationToken cancellationToken);
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
