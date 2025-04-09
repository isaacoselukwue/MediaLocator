using ML.Application.Media.Queries;

namespace ML.Application.FunctionalTests.Media.Queries;
[TestFixture]
class AdminSearchHistoryTests
{
    private Mock<ISearchService> _mockSearchService;
    private AdminSearchHistoryQuery invalidPageNumberQuery;
    private AdminSearchHistoryQuery onlyEndDateQuery;
    private AdminSearchHistoryQuery onlyStartDateQuery;
    private AdminSearchHistoryQuery invalidStatusQuery;
    private AdminSearchHistoryQuery validQuery;
    private AdminSearchHistoryValidator _validator;
    private AdminSearchHistoryQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockSearchService = new();
        _validator = new();

        invalidPageNumberQuery = new() { PageNumber = 0, IsAscendingSorted = false };
        onlyEndDateQuery = new() { EndDate = "2023-01-01", PageNumber = 1 };
        onlyStartDateQuery = new() { StartDate = "2023-01-01", PageNumber = 1 };
        invalidStatusQuery = new() { Status = (StatusEnum)99, PageNumber = 1 };
        validQuery = new() { PageNumber = 1, StartDate = "2023-01-01", EndDate = "2023-01-31" };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task AdminSearchHistory_InvalidPageNumber_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(invalidPageNumberQuery);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "PageNumber"));
        });
    }

    [Test]
    public async Task AdminSearchHistory_OnlyEndDate_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(onlyEndDateQuery);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "StartDate"));
        });
    }

    [Test]
    public async Task AdminSearchHistory_OnlyStartDate_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(onlyStartDateQuery);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "EndDate"));
        });
    }

    [Test]
    public async Task AdminSearchHistory_InvalidStatus_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(invalidStatusQuery);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "Status"));
        });
    }

    [Test]
    public async Task AdminSearchHistory_ValidQuery_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validQuery);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task AdminSearchHistory_EmptyResults_ReturnsEmptyList()
    {
        var emptyResult = new AdminSearchHistoryDto
        {
            Page = 1,
            Size = 20,
            TotalPages = 0,
            TotalResults = 0,
            Results = []
        };

        _mockSearchService.Setup(x => x.GetAdminSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.EmailAddress,
            validQuery.Status,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AdminSearchHistoryDto>.Success("Successfully fetched search history", emptyResult));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Results, Is.Empty);
            Assert.That(result.Data?.TotalResults, Is.EqualTo(0));
            Assert.That(result.Data?.TotalPages, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task AdminSearchHistory_SinglePage_ReturnsCorrectPagination()
    {
        var searchHistory = GetTestSearchHistory(5);

        var singlePageResult = new AdminSearchHistoryDto
        {
            Page = 1,
            Size = 20,
            TotalPages = 1,
            TotalResults = 5,
            Results = searchHistory
        };

        _mockSearchService.Setup(x => x.GetAdminSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.EmailAddress,
            validQuery.Status,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AdminSearchHistoryDto>.Success("Successfully fetched search history", singlePageResult));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Results, Has.Count.EqualTo(5));
            Assert.That(result.Data?.TotalResults, Is.EqualTo(5));
            Assert.That(result.Data?.TotalPages, Is.EqualTo(1));
            Assert.That(result.Data?.Page, Is.EqualTo(1));
            Assert.That(result.Data?.Size, Is.EqualTo(20));
        });
    }

    [Test]
    public async Task AdminSearchHistory_MultiplePages_ReturnsCorrectPage()
    {
        var searchHistory = GetTestSearchHistory(10);

        var multiPageResult = new AdminSearchHistoryDto
        {
            Page = 2,
            Size = 20,
            TotalPages = 3,
            TotalResults = 45,
            Results = searchHistory
        };

        validQuery.PageNumber = 2;

        _mockSearchService.Setup(x => x.GetAdminSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.EmailAddress,
            validQuery.Status,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AdminSearchHistoryDto>.Success("Successfully fetched search history", multiPageResult));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Results, Has.Count.EqualTo(10));
            Assert.That(result.Data?.TotalResults, Is.EqualTo(45));
            Assert.That(result.Data?.TotalPages, Is.EqualTo(3));
            Assert.That(result.Data?.Page, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task AdminSearchHistory_WithFilters_CallsServiceWithCorrectParameters()
    {
        AdminSearchHistoryQuery filteredQuery = new()
        {
            Title = "Test Title",
            StartDate = "2023-01-01",
            EndDate = "2023-01-31",
            EmailAddress = "test@example.com",
            Status = StatusEnum.Active,
            IsAscendingSorted = true,
            PageNumber = 1
        };

        var result = new AdminSearchHistoryDto
        {
            Page = 1,
            Size = 20,
            TotalPages = 1,
            TotalResults = 5,
            Results = GetTestSearchHistory(5)
        };

        _mockSearchService.Setup(x => x.GetAdminSearchHistory(
            filteredQuery.Title,
            filteredQuery.StartDate,
            filteredQuery.EndDate,
            filteredQuery.EmailAddress,
            filteredQuery.Status,
            filteredQuery.IsAscendingSorted,
            filteredQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AdminSearchHistoryDto>.Success("Successfully fetched search history", result));

        await _handler.Handle(filteredQuery, CancellationToken.None);

        _mockSearchService.Verify(x => x.GetAdminSearchHistory(
            "Test Title",
            "2023-01-01",
            "2023-01-31",
            "test@example.com",
            StatusEnum.Active,
            true,
            1,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static List<AdminSearchHistoryResult> GetTestSearchHistory(int count)
    {
        List<AdminSearchHistoryResult> results = [];

        for (int i = 0; i < count; i++)
        {
            results.Add(new AdminSearchHistoryResult
            {
                Id = Guid.NewGuid(),
                SearchId = $"search-{i}",
                UserId = Guid.NewGuid(),
                UsersEmail = $"user{i}@example.com",
                Status = StatusEnum.Active,
                Title = $"Search Result {i}",
                Url = $"http://openapi.com/media/{i}",
                Creator = $"Creator {i}",
                License = "CC BY",
                Provider = "OpenVerse",
                SearchDate = DateTimeOffset.UtcNow.AddDays(-i),
                Category = i % 2 == 0 ? "Music" : "Image",
                FileType = i % 2 == 0 ? "mp3" : "jpg",
                ThumbNail = $"http://openapi.com/thumbnail/{i}.jpg"
            });
        }

        return results;
    }
}