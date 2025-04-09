using ML.Application.Media.Queries;

namespace ML.Application.FunctionalTests.Media.Queries;
[TestFixture]
class AudioDetailsTests
{
    private Mock<ISearchService> _mockSearchService;
    private AudioDetailsQuery emptyIdQuery;
    private AudioDetailsQuery validQuery;
    private AudioDetailsValidator _validator;
    private AudioDetailsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockSearchService = new();
        _validator = new();

        emptyIdQuery = new()
        {
            Id = ""
        };

        validQuery = new()
        {
            Id = "audio-123"
        };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task AudioDetails_EmptyId_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(emptyIdQuery);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "Id"));
        });
    }

    [Test]
    public async Task AudioDetails_ValidId_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validQuery);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task AudioDetails_NotFound_ReturnsFailed()
    {
        _mockSearchService.Setup(x => x.GetAudioDetails(
            validQuery.Id!,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchResult>.Failure("Audio details not found", ["Audio could not be retrieved"]));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Contains.Item("Audio could not be retrieved"));
            Assert.That(result.Message, Is.EqualTo("Audio details not found"));
        });
    }

    [Test]
    public async Task AudioDetails_Found_ReturnsSuccess()
    {
        var audioResult = new AudioSearchResult
        {
            Id = "audio-123",
            Title = "Test Audio",
            Creator = "Test Creator",
            License = "CC BY",
            Provider = "OpenVerse",
            Url = "http://example.com/audio.mp3",
            FileType = "mp3",
            FileSize = 1024,
            Thumbnail = "http://example.com/thumbnail.jpg"
        };

        _mockSearchService.Setup(x => x.GetAudioDetails(
            validQuery.Id!,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchResult>.Success("Successfully fetched audio", audioResult));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data?.Id, Is.EqualTo("audio-123"));
            Assert.That(result.Data?.Title, Is.EqualTo("Test Audio"));
            Assert.That(result.Data?.Creator, Is.EqualTo("Test Creator"));
            Assert.That(result.Message, Is.EqualTo("Successfully fetched audio"));
        });
    }

    [Test]
    public async Task AudioDetails_CallsServiceWithCorrectId()
    {
        _mockSearchService.Setup(x => x.GetAudioDetails(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<AudioSearchResult>.Success("Successfully fetched audio", new AudioSearchResult()));

        await _handler.Handle(validQuery, CancellationToken.None);

        _mockSearchService.Verify(x => x.GetAudioDetails(
            "audio-123",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void AudioDetails_ServiceException_PropagatesError()
    {
        _mockSearchService.Setup(x => x.GetAudioDetails(
            validQuery.Id!,
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Service exception"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(validQuery, CancellationToken.None));
    }
}