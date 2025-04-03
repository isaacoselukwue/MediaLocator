using ML.Application.Media.Queries;

namespace ML.Application.FunctionalTests.Media.Queries;
[TestFixture]
[TestFixture]
class ImageDetailsTests
{
    private Mock<ISearchService> _mockSearchService;
    private ImageDetailsQuery emptyIdQuery;
    private ImageDetailsQuery validQuery;
    private ImageDetailsValidator _validator;
    private ImageDetailsHandler _handler;

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
            Id = "image-123"
        };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task ImageDetails_EmptyId_FailsValidation()
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
    public async Task ImageDetails_ValidId_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validQuery);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task ImageDetails_NotFound_ReturnsFailed()
    {
        _mockSearchService.Setup(x => x.GetImageDetails(
            validQuery.Id!,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchResult>.Failure("Image details not found", ["Image could not be retrieved"]));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Contains.Item("Image could not be retrieved"));
            Assert.That(result.Message, Is.EqualTo("Image details not found"));
        });
    }

    [Test]
    public async Task ImageDetails_Found_ReturnsSuccess()
    {
        var imageResult = new ImageSearchResult
        {
            Id = "image-123",
            Title = "Test Image",
            Creator = "Test Photographer",
            License = "CC BY",
            Provider = "OpenVerse",
            Url = "http://example.com/image.jpg",
            FileType = "jpg",
            FileSize = 1024,
            ThumbNail = "http://example.com/thumbnail.jpg"
        };

        _mockSearchService.Setup(x => x.GetImageDetails(
            validQuery.Id!,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchResult>.Success("Successfully fetched image", imageResult));

        var result = await _handler.Handle(validQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data?.Id, Is.EqualTo("image-123"));
            Assert.That(result.Data?.Title, Is.EqualTo("Test Image"));
            Assert.That(result.Data?.Creator, Is.EqualTo("Test Photographer"));
            Assert.That(result.Message, Is.EqualTo("Successfully fetched image"));
        });
    }

    [Test]
    public async Task ImageDetails_CallsServiceWithCorrectId()
    {
        _mockSearchService.Setup(x => x.GetImageDetails(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result<ImageSearchResult>.Success("Successfully fetched image", new ImageSearchResult()));

        await _handler.Handle(validQuery, CancellationToken.None);

        _mockSearchService.Verify(x => x.GetImageDetails(
            "image-123",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ImageDetails_ServiceException_PropagatesError()
    {
        _mockSearchService.Setup(x => x.GetImageDetails(
            validQuery.Id!,
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Service exception"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(validQuery, CancellationToken.None));
    }
}