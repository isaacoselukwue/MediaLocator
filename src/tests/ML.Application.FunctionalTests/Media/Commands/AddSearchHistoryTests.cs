namespace ML.Application.FunctionalTests.Media.Commands;
[TestFixture]
class AddSearchHistoryTests
{
    private Mock<ISearchService> _mockSearchService;
    private AddSearchHistoryCommand emptyIdCommand;
    private AddSearchHistoryCommand invalidTypeCommand;
    private AddSearchHistoryCommand validAudioCommand;
    private AddSearchHistoryCommand validImageCommand;
    private AddSearchHistoryValidator _validator;
    private AddSearchHistoryCommandValidator _handler;

    [SetUp]
    public void Setup()
    {
        _mockSearchService = new();
        _validator = new();

        emptyIdCommand = new()
        {
            SearchId = "",
            SearchType = SearchTypeEnum.Audio
        };

        invalidTypeCommand = new()
        {
            SearchId = "valid-search-id",
            SearchType = (SearchTypeEnum)999
        };

        validAudioCommand = new()
        {
            SearchId = "audio-123",
            SearchType = SearchTypeEnum.Audio
        };

        validImageCommand = new()
        {
            SearchId = "image-456",
            SearchType = SearchTypeEnum.Image
        };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task AddSearchHistory_EmptySearchId_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(emptyIdCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "SearchId"));
        });
    }

    [Test]
    public async Task AddSearchHistory_InvalidSearchType_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(invalidTypeCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "SearchType"));
        });
    }

    [Test]
    public async Task AddSearchHistory_ValidData_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validAudioCommand);
        Assert.That(validationResult.IsValid, Is.True);

        validationResult = await _validator.ValidateAsync(validImageCommand);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task AddSearchHistory_InvalidSearchType_ReturnsFailed()
    {
        _mockSearchService.Setup(x => x.AddSearchHistory(
            validAudioCommand.SearchId!,
            validAudioCommand.SearchType,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Failure("Invalid search type", ["Search type is not valid"]));

        var result = await _handler.Handle(validAudioCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Contains.Item("Search type is not valid"));
        });
    }

    [Test]
    public async Task AddSearchHistory_AlreadySaved_ReturnsSuccess()
    {
        _mockSearchService.Setup(x => x.AddSearchHistory(
            validAudioCommand.SearchId!,
            validAudioCommand.SearchType,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success("Search history already saved!"));

        var result = await _handler.Handle(validAudioCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Message, Is.EqualTo("Search history already saved!"));
        });
    }

    [Test]
    public async Task AddSearchHistory_AudioType_ReturnsSuccess()
    {
        _mockSearchService.Setup(x => x.AddSearchHistory(
            validAudioCommand.SearchId!,
            validAudioCommand.SearchType,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success("Search history added successfully"));

        var result = await _handler.Handle(validAudioCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Message, Is.EqualTo("Search history added successfully"));
        });

        _mockSearchService.Verify(x => x.AddSearchHistory(
            validAudioCommand.SearchId!,
            SearchTypeEnum.Audio,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task AddSearchHistory_ImageType_ReturnsSuccess()
    {
        _mockSearchService.Setup(x => x.AddSearchHistory(
            validImageCommand.SearchId!,
            validImageCommand.SearchType,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success("Search history added successfully"));

        var result = await _handler.Handle(validImageCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Message, Is.EqualTo("Search history added successfully"));
        });

        _mockSearchService.Verify(x => x.AddSearchHistory(
            validImageCommand.SearchId!,
            SearchTypeEnum.Image,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}