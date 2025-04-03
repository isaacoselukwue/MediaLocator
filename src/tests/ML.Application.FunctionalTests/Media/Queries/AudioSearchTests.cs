using ML.Application.Media.Queries;

namespace ML.Application.FunctionalTests.Media.Queries;
[TestFixture]
class AudioSearchTests
{
    private Mock<ISearchService> _mockSearchService;
    private AudioSearchQuery emptyQueryCommand;
    private AudioSearchQuery invalidLicenseCommand;
    private AudioSearchQuery invalidLicenseTypeCommand;
    private AudioSearchQuery invalidCategoryCommand;
    private AudioSearchQuery invalidPageNumberCommand;
    private AudioSearchQuery validCommand;
    private AudioSearchValidator _validator;
    private AudioSearchQueryHandler _handler;

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
            Category = OpenAudioCategoryEnum.Music,
            PageNumber = 1
        };

        invalidLicenseCommand = new()
        {
            SearchQuery = "piano",
            License = (OpenLicenseEnum)99,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = OpenAudioCategoryEnum.Music,
            PageNumber = 1
        };

        invalidLicenseTypeCommand = new()
        {
            SearchQuery = "piano",
            License = OpenLicenseEnum.By,
            LicenseType = (OpenLicenseTypeEnum)99,
            Category = OpenAudioCategoryEnum.Music,
            PageNumber = 1
        };

        invalidCategoryCommand = new()
        {
            SearchQuery = "piano",
            License = OpenLicenseEnum.By,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = (OpenAudioCategoryEnum)99,
            PageNumber = 1
        };

        invalidPageNumberCommand = new()
        {
            SearchQuery = "piano",
            License = OpenLicenseEnum.By,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = OpenAudioCategoryEnum.Music,
            PageNumber = 0
        };

        validCommand = new()
        {
            SearchQuery = "piano",
            License = OpenLicenseEnum.By,
            LicenseType = OpenLicenseTypeEnum.Commercial,
            Category = OpenAudioCategoryEnum.Music,
            PageNumber = 1
        };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task AudioSearch_EmptyQuery_FailsValidation()
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
    public async Task AudioSearch_InvalidLicense_FailsValidation()
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
    public async Task AudioSearch_InvalidLicenseType_FailsValidation()
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
    public async Task AudioSearch_InvalidCategory_FailsValidation()
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
    public async Task AudioSearch_InvalidPageNumber_FailsValidation()
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
    public async Task AudioSearch_ValidData_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validCommand);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task AudioSearch_EmptyResults_ReturnsEmptyList()
    {
        var emptyResult = new AudioSearchDto
        {
            Page = 1,
            Size = 21,
            TotalPages = 0,
            TotalResults = 0,
            Results = []
        };

        _mockSearchService.Setup(x => x.SearchAudio(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchDto>.Success("Successfully fetched audio", emptyResult));

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
    public async Task AudioSearch_WithResults_ReturnsCorrectData()
    {
        var audioResults = new List<AudioSearchResult>
        {
            new()
            {
                Id = "audio-123",
                Title = "Piano Sonata",
                Creator = "Test Musician",
                License = "CC BY",
                Provider = "OpenVerse",
                Url = "http://openapi.com/audio.mp3",
                FileType = "mp3",
                FileSize = 1024,
                Thumbnail = "http://openapi.com/thumbnail.jpg"
            },
            new()
            {
                Id = "audio-456",
                Title = "Piano Jazz",
                Creator = "Another Musician",
                License = "CC BY-SA",
                Provider = "OpenVerse",
                Url = "http://openapi.com/audio2.mp3",
                FileType = "mp3",
                FileSize = 2048,
                Thumbnail = "http://openapi.com/thumbnail2.jpg"
            }
        };

        var searchResult = new AudioSearchDto
        {
            Page = 1,
            Size = 21,
            TotalPages = 1,
            TotalResults = 2,
            Results = audioResults
        };

        _mockSearchService.Setup(x => x.SearchAudio(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchDto>.Success("Successfully fetched audio", searchResult));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Results, Has.Count.EqualTo(2));
            Assert.That(result.Data?.TotalResults, Is.EqualTo(2));
            Assert.That(result.Data?.Results[0].Id, Is.EqualTo("audio-123"));
            Assert.That(result.Data?.Results[1].Title, Is.EqualTo("Piano Jazz"));
        });
    }

    [Test]
    public async Task AudioSearch_MultiplePages_ReturnsCorrectPagination()
    {
        var audioResults = new List<AudioSearchResult>
        {
            new() { Id = "audio-201", Title = "Second Page Audio 1" },
            new() { Id = "audio-202", Title = "Second Page Audio 2" }
        };

        var secondPageResult = new AudioSearchDto
        {
            Page = 2,
            Size = 21,
            TotalPages = 5,
            TotalResults = 100,
            Results = audioResults
        };

        validCommand.PageNumber = 2;

        _mockSearchService.Setup(x => x.SearchAudio(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchDto>.Success("Successfully fetched audio", secondPageResult));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.Page, Is.EqualTo(2));
            Assert.That(result.Data?.TotalPages, Is.EqualTo(5));
            Assert.That(result.Data?.TotalResults, Is.EqualTo(100));
            Assert.That(result.Data?.Results[0].Id, Is.EqualTo("audio-201"));
        });
    }

    [Test]
    public async Task AudioSearch_ServiceFailure_ReturnsFailure()
    {
        _mockSearchService.Setup(x => x.SearchAudio(
            validCommand.SearchQuery!,
            validCommand.License,
            validCommand.LicenseType,
            validCommand.Category,
            validCommand.PageNumber,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchDto>.Failure("Failed to retrieve audio", ["API service unavailable"]));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to retrieve audio"));
            Assert.That(result.Errors, Contains.Item("API service unavailable"));
        });
    }

    [Test]
    public async Task AudioSearch_CallsServiceWithCorrectParameters()
    {
        _mockSearchService.Setup(x => x.SearchAudio(
            It.IsAny<string>(),
            It.IsAny<OpenLicenseEnum?>(),
            It.IsAny<OpenLicenseTypeEnum?>(),
            It.IsAny<OpenAudioCategoryEnum?>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchDto>.Success("Successfully fetched audio", new AudioSearchDto()));

        await _handler.Handle(validCommand, CancellationToken.None);

        _mockSearchService.Verify(x => x.SearchAudio(
            "piano",
            OpenLicenseEnum.By,
            OpenLicenseTypeEnum.Commercial,
            OpenAudioCategoryEnum.Music,
            1,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void AudioSearch_ServiceException_PropagatesError()
    {
        _mockSearchService.Setup(x => x.SearchAudio(
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