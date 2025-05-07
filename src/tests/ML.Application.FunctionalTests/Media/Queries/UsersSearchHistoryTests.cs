using ML.Application.Media.Queries;

namespace ML.Application.FunctionalTests.Media.Queries;
[TestFixture]
class UsersSearchHistoryTests
{
    private Mock<ISearchService> _mockSearchService;
    private UsersSearchHistoryQuery invalidPageNumberQuery;
    private UsersSearchHistoryQuery onlyEndDateQuery;
    private UsersSearchHistoryQuery onlyStartDateQuery;
    private UsersSearchHistoryQuery validQuery;
    private UsersSearchHistoryQuery validQueryWithDates;
    private UsersSearchHistoryValidator _validator;
    private UsersSearchHistoryQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockSearchService = new();
        _validator = new();

        invalidPageNumberQuery = new() { PageNumber = 0, IsAscendingSorted = false };
        onlyEndDateQuery = new() { EndDate = "2023-01-01", PageNumber = 1 };
        onlyStartDateQuery = new() { StartDate = "2023-01-01", PageNumber = 1 };
        validQuery = new() { PageNumber = 1 };
        validQueryWithDates = new()
        {
            PageNumber = 1,
            StartDate = "2023-01-01",
            EndDate = "2023-01-31",
            Title = "Test Title",
            IsAscendingSorted = true
        };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task UsersSearchHistory_InvalidPageNumber_FailsValidation()
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
    public async Task UsersSearchHistory_OnlyEndDate_FailsValidation()
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
    public async Task UsersSearchHistory_OnlyStartDate_FailsValidation()
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
    public async Task UsersSearchHistory_ValidQuery_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validQuery);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task UsersSearchHistory_ValidQueryWithDates_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validQueryWithDates);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task UsersSearchHistory_EmptyResults_ReturnsEmptyList()
    {
        var emptyResult = new UsersSearchHistoryDto
        {
            Page = 1,
            Size = 20,
            TotalPages = 0,
            TotalResults = 0,
            Results = []
        };

        _mockSearchService.Setup(x => x.GetUsersSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<UsersSearchHistoryDto>.Success("Successfully fetched search history", emptyResult));

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
    public async Task UsersSearchHistory_SinglePage_ReturnsCorrectPagination()
    {
        var searchHistory = GetTestSearchHistory(5);

        var singlePageResult = new UsersSearchHistoryDto
        {
            Page = 1,
            Size = 20,
            TotalPages = 1,
            TotalResults = 5,
            Results = searchHistory
        };

        _mockSearchService.Setup(x => x.GetUsersSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<UsersSearchHistoryDto>.Success("Successfully fetched search history", singlePageResult));

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
    public async Task UsersSearchHistory_MultiplePages_ReturnsCorrectPage()
    {
        var searchHistory = GetTestSearchHistory(10);

        var multiPageResult = new UsersSearchHistoryDto
        {
            Page = 2,
            Size = 20,
            TotalPages = 3,
            TotalResults = 45,
            Results = searchHistory
        };

        validQuery.PageNumber = 2;

        _mockSearchService.Setup(x => x.GetUsersSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<UsersSearchHistoryDto>.Success("Successfully fetched search history", multiPageResult));

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
    public async Task UsersSearchHistory_WithFilters_CallsServiceWithCorrectParameters()
    {
        _mockSearchService.Setup(x => x.GetUsersSearchHistory(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<UsersSearchHistoryDto>.Success("Successfully fetched search history", new UsersSearchHistoryDto()));

        await _handler.Handle(validQueryWithDates, CancellationToken.None);

        _mockSearchService.Verify(x => x.GetUsersSearchHistory(
            "Test Title",
            "2023-01-01",
            "2023-01-31",
            true,
            1,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task UsersSearchHistory_ServiceFailure_ReturnsFailure()
    {
        _mockSearchService.Setup(x => x.GetUsersSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<UsersSearchHistoryDto>.Failure("Failed to fetch search history", ["Database error"]));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to fetch search history"));
            Assert.That(result.Errors, Contains.Item("Database error"));
        });
    }

    [Test]
    public void UsersSearchHistory_ServiceException_PropagatesError()
    {
        _mockSearchService.Setup(x => x.GetUsersSearchHistory(
            validQuery.Title,
            validQuery.StartDate,
            validQuery.EndDate,
            validQuery.IsAscendingSorted,
            validQuery.PageNumber,
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Service exception"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(validQuery, CancellationToken.None));
    }

    private static List<UsersSearchHistoryResult> GetTestSearchHistory(int count)
    {
        List<UsersSearchHistoryResult> results = [];

        for (int i = 0; i < count; i++)
        {
            results.Add(new UsersSearchHistoryResult
            {
                Id = Guid.NewGuid(),
                SearchId = $"search-{i}",
                Title = $"Search Result {i}",
                Url = $"http://example.com/media/{i}",
                Creator = $"Creator {i}",
                License = "CC BY",
                Provider = "OpenVerse",
                SearchDate = DateTimeOffset.UtcNow.AddDays(-i),
                Category = i % 2 == 0 ? "Music" : "Image",
                FileType = i % 2 == 0 ? "mp3" : "jpg",
                ThumbNail = $"http://example.com/thumbnail/{i}.jpg"
            });
        }

        return results;
    }
}