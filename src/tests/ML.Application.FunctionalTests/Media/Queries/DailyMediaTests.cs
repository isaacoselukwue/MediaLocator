using ML.Application.Media.Queries;
using Moq;

namespace ML.Application.FunctionalTests.Media.Queries;
[TestFixture]
class DailyMediaTests
{
    private Mock<ISearchService> _mockSearchService;
    private DailyMediaQuery _query;
    private DailyMediaQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockSearchService = new();
        _query = new DailyMediaQuery();
        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task DailyMedia_Success_ReturnsAudioAndImageResults()
    {
        var dailyMediaDto = new DailyMediaDto
        {
            WordOfTheDay = "Word of day is: Happy 😃!",
            AudioSearchResults = [
                new AudioSearchResult
                {
                    Id = "audio-123",
                    Title = "Happy Song",
                    Creator = "Happy Artist",
                    License = "CC BY",
                    Provider = "OpenVerse"
                },
                new AudioSearchResult
                {
                    Id = "audio-456",
                    Title = "Happy Music",
                    Creator = "Another Artist",
                    License = "CC BY-SA",
                    Provider = "OpenVerse"
                }
            ],
            ImageSearchResults = [
                new ImageSearchResult
                {
                    Id = "image-123",
                    Title = "Happy Photo",
                    Creator = "Happy Photographer",
                    License = "CC BY",
                    Provider = "OpenVerse"
                },
                new ImageSearchResult
                {
                    Id = "image-456",
                    Title = "Happy Picture",
                    Creator = "Another Photographer",
                    License = "CC BY-SA",
                    Provider = "OpenVerse"
                }
            ]
        };

        _mockSearchService.Setup(x => x.GetDailyMediaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DailyMediaDto>.Success("Successfully fetched daily media", dailyMediaDto));

        var result = await _handler.Handle(_query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data?.WordOfTheDay, Is.EqualTo("Word of day is: Happy 😃!"));
            Assert.That(result.Data?.AudioSearchResults, Has.Count.EqualTo(2));
            Assert.That(result.Data?.ImageSearchResults, Has.Count.EqualTo(2));
            Assert.That(result.Data?.AudioSearchResults[0].Title, Is.EqualTo("Happy Song"));
            Assert.That(result.Data?.ImageSearchResults[0].Title, Is.EqualTo("Happy Photo"));
            Assert.That(result.Message, Is.EqualTo("Successfully fetched daily media"));
        });
    }

    [Test]
    public async Task DailyMedia_EmptyResults_ReturnsEmptyLists()
    {
        var emptyResult = new DailyMediaDto
        {
            WordOfTheDay = "Word of day is: Positive 😊!",
            AudioSearchResults = [],
            ImageSearchResults = []
        };

        _mockSearchService.Setup(x => x.GetDailyMediaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DailyMediaDto>.Success("Successfully fetched daily media", emptyResult));

        var result = await _handler.Handle(_query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data?.WordOfTheDay, Is.Not.Empty);
            Assert.That(result.Data?.AudioSearchResults, Is.Empty);
            Assert.That(result.Data?.ImageSearchResults, Is.Empty);
        });
    }

    [Test]
    public async Task DailyMedia_ServiceFailure_ReturnsFailure()
    {
        _mockSearchService.Setup(x => x.GetDailyMediaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DailyMediaDto>.Failure("Failed to retrieve daily media", ["API service unavailable"]));

        var result = await _handler.Handle(_query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to retrieve daily media"));
            Assert.That(result.Errors, Contains.Item("API service unavailable"));
        });
    }

    [Test]
    public async Task DailyMedia_CallsServiceCorrectly()
    {
        _mockSearchService.Setup(x => x.GetDailyMediaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DailyMediaDto>.Success("Successfully fetched daily media", new DailyMediaDto()));

        await _handler.Handle(_query, CancellationToken.None);

        _mockSearchService.Verify(x => x.GetDailyMediaAsync(
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void DailyMedia_ServiceException_PropagatesError()
    {
        _mockSearchService.Setup(x => x.GetDailyMediaAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service exception"));

        Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(_query, CancellationToken.None));
    }

    [Test]
    public async Task DailyMedia_VerifyWordOfDay_ContainsExpectedFormat()
    {
        var dailyMediaDto = new DailyMediaDto
        {
            WordOfTheDay = "Word of day is: Happy 😃!",
            AudioSearchResults = [],
            ImageSearchResults = []
        };

        _mockSearchService.Setup(x => x.GetDailyMediaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DailyMediaDto>.Success("Successfully fetched daily media", dailyMediaDto));

        var result = await _handler.Handle(_query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Data?.WordOfTheDay, Does.StartWith("Word of day is:"));
            Assert.That(result.Data?.WordOfTheDay, Does.EndWith("!"));
            Assert.That(result.Data?.WordOfTheDay, Does.Contain("😃"));
        });
    }
}