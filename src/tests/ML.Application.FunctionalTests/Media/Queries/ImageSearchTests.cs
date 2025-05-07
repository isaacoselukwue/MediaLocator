using ML.Application.Media.Queries;

namespace ML.Application.FunctionalTests.Media.Queries;
[TestFixture]
class ImageSearchTests
{
    private Mock<ISearchService> _mockSearchService;
    private ImageSearchQuery emptyQueryCommand;
    private ImageSearchQuery invalidLicenseCommand;
    private ImageSearchQuery invalidLicenseTypeCommand;
    private ImageSearchQuery invalidCategoryCommand;
    private ImageSearchQuery invalidPageNumberCommand;
    private ImageSearchQuery validCommand;
    private ImageSearchValidator _validator;
    private ImageSearchQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockSearchService = new();
        _validator = new();

        emptyQueryCommand = new()
        {
            SearchQuery = "",
            License = OpenLicenseEnum.By,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = OpenImageCategoryEnum.Photograph,
            PageNumber = 1
        };

        invalidLicenseCommand = new()
        {
            SearchQuery = "sunset",
            License = (OpenLicenseEnum)99,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = OpenImageCategoryEnum.Photograph,
            PageNumber = 1
        };

        invalidLicenseTypeCommand = new()
        {
            SearchQuery = "sunset",
            License = OpenLicenseEnum.By,
            LicenseType = (OpenLicenseTypeEnum)99,
            Category = OpenImageCategoryEnum.Photograph,
            PageNumber = 1
        };

        invalidCategoryCommand = new()
        {
            SearchQuery = "sunset",
            License = OpenLicenseEnum.By,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = (OpenImageCategoryEnum)99,
            PageNumber = 1
        };

        invalidPageNumberCommand = new()
        {
            SearchQuery = "sunset",
            License = OpenLicenseEnum.By,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = OpenImageCategoryEnum.Photograph,
            PageNumber = 0
        };

        validCommand = new()
        {
            SearchQuery = "sunset",
            License = OpenLicenseEnum.By,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = OpenImageCategoryEnum.Photograph,
            PageNumber = 1
        };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task ImageSearch_EmptyQuery_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(emptyQueryCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "SearchQuery"));
        });
    }

    [Test]
    public async Task ImageSearch_InvalidLicense_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(invalidLicenseCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "License"));
        });
    }

    [Test]
    public async Task ImageSearch_InvalidLicenseType_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(invalidLicenseTypeCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "LicenseType"));
        });
    }

    [Test]
    public async Task ImageSearch_InvalidCategory_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(invalidCategoryCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "Category"));
        });
    }

    [Test]
    public async Task ImageSearch_InvalidPageNumber_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(invalidPageNumberCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "PageNumber"));
        });
    }

    [Test]
    public async Task ImageSearch_ValidData_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validCommand);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task ImageSearch_EmptyResults_ReturnsEmptyList()
    {
        var emptyResult = new ImageSearchDto
        {
            Page = 1,
            Size = 21,
            TotalPages = 0,
            TotalResults = 0,
            Results = []
        };

        _mockSearchService.Setup(x => x.SearchImage(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchDto>.Success("Successfully fetched images", emptyResult));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Results, Is.Empty);
            Assert.That(result.Data?.TotalResults, Is.EqualTo(0));
            Assert.That(result.Data?.TotalPages, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ImageSearch_WithResults_ReturnsCorrectData()
    {
        var imageResults = new List<ImageSearchResult>
        {
            new()
            {
                Id = "image-123",
                Title = "Beautiful Sunset",
                Creator = "Nature Photographer",
                License = "CC BY",
                Provider = "OpenVerse",
                Url = "http://openapi.com/image.jpg",
                FileType = "jpg",
                FileSize = 1024,
                ThumbNail = "http://openapi.com/thumbnail.jpg"
            },
            new()
            {
                Id = "image-456",
                Title = "Mountain Sunset",
                Creator = "Another Photographer",
                License = "CC BY-SA",
                Provider = "OpenVerse",
                Url = "http://openapi.com/image2.jpg",
                FileType = "jpg",
                FileSize = 2048,
                ThumbNail = "http://openapi.com/thumbnail2.jpg"
            }
        };

        var searchResult = new ImageSearchDto
        {
            Page = 1,
            Size = 21,
            TotalPages = 1,
            TotalResults = 2,
            Results = imageResults
        };

        _mockSearchService.Setup(x => x.SearchImage(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchDto>.Success("Successfully fetched images", searchResult));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Results, Has.Count.EqualTo(2));
            Assert.That(result.Data?.TotalResults, Is.EqualTo(2));
            Assert.That(result.Data?.Results[0].Id, Is.EqualTo("image-123"));
            Assert.That(result.Data?.Results[1].Title, Is.EqualTo("Mountain Sunset"));
        });
    }

    [Test]
    public async Task ImageSearch_MultiplePages_ReturnsCorrectPagination()
    {
        var imageResults = new List<ImageSearchResult>
        {
            new() { Id = "image-201", Title = "Second Page Image 1" },
            new() { Id = "image-202", Title = "Second Page Image 2" }
        };

        var secondPageResult = new ImageSearchDto
        {
            Page = 2,
            Size = 21,
            TotalPages = 5,
            TotalResults = 100,
            Results = imageResults
        };

        validCommand.PageNumber = 2;

        _mockSearchService.Setup(x => x.SearchImage(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchDto>.Success("Successfully fetched images", secondPageResult));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Page, Is.EqualTo(2));
            Assert.That(result.Data?.TotalPages, Is.EqualTo(5));
            Assert.That(result.Data?.TotalResults, Is.EqualTo(100));
            Assert.That(result.Data?.Results[0].Id, Is.EqualTo("image-201"));
        });
    }

    [Test]
    public async Task ImageSearch_ServiceFailure_ReturnsFailure()
    {
        _mockSearchService.Setup(x => x.SearchImage(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchDto>.Failure("Failed to retrieve images", ["API service unavailable"]));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to retrieve images"));
            Assert.That(result.Errors, Contains.Item("API service unavailable"));
        });
    }

    [Test]
    public async Task ImageSearch_CallsServiceWithCorrectParameters()
    {
        _mockSearchService.Setup(x => x.SearchImage(
            It.IsAny<string>(),
            It.IsAny<OpenLicenseEnum?>(),
            It.IsAny<OpenLicenseTypeEnum?>(),
            It.IsAny<OpenImageCategoryEnum?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchDto>.Success("Successfully fetched images", new ImageSearchDto()));

        await _handler.Handle(validCommand, CancellationToken.None);

        _mockSearchService.Verify(x => x.SearchImage(
            "sunset",
            OpenLicenseEnum.By,
            OpenLicenseTypeEnum.Commercial,
            OpenImageCategoryEnum.Photograph,
            1,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ImageSearch_ServiceException_PropagatesError()
    {
        _mockSearchService.Setup(x => x.SearchImage(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Service exception"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(validCommand, CancellationToken.None));
    }
}