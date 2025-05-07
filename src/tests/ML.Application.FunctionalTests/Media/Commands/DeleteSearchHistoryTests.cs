namespace ML.Application.FunctionalTests.Media.Commands;
[TestFixture]
class DeleteSearchHistoryTests
{
    private Mock<ISearchService> _mockSearchService;
    private DeleteSearchHistoryCommand emptyIdCommand;
    private DeleteSearchHistoryCommand validCommand;
    private DeleteSearchHistoryValidator _validator;
    private DeleteSearchHistoryCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockSearchService = new();
        _validator = new();

        emptyIdCommand = new()
        {
            Id = Guid.Empty
        };

        validCommand = new()
        {
            Id = Guid.NewGuid()
        };

        _handler = new(_mockSearchService.Object);
    }

    [Test]
    public async Task DeleteSearchHistory_EmptyId_FailsValidation()
    {
        var validationResult = await _validator.ValidateAsync(emptyIdCommand);

        Assert.Multiple(() =>
        {
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.Errors, Has.Some.Matches<FluentValidation.Results.ValidationFailure>(
                f => f.PropertyName == "Id"));
        });
    }

    [Test]
    public async Task DeleteSearchHistory_ValidId_PassesValidation()
    {
        var validationResult = await _validator.ValidateAsync(validCommand);
        Assert.That(validationResult.IsValid, Is.True);
    }

    [Test]
    public async Task DeleteSearchHistory_NotFound_ReturnsFailed()
    {
        _mockSearchService.Setup(x => x.DeleteSearchHistory(
            validCommand.Id,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Failure("Search history not found", ["Search history could not be found"]));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Contains.Item("Search history could not be found"));
            Assert.That(result.Message, Is.EqualTo("Search history not found"));
        });
    }

    [Test]
    public async Task DeleteSearchHistory_UnexpectedError_ReturnsFailed()
    {
        _mockSearchService.Setup(x => x.DeleteSearchHistory(
            validCommand.Id,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Failure("Search history could not be deleted", ["An unexpected error occurred"]));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors, Contains.Item("An unexpected error occurred"));
            Assert.That(result.Message, Is.EqualTo("Search history could not be deleted"));
        });
    }

    [Test]
    public async Task DeleteSearchHistory_ValidId_ReturnsSuccess()
    {
        _mockSearchService.Setup(x => x.DeleteSearchHistory(
            validCommand.Id,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success("Search history deleted successfully"));

        var result = await _handler.Handle(validCommand, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Message, Is.EqualTo("Search history deleted successfully"));
        });

        _mockSearchService.Verify(x => x.DeleteSearchHistory(
            validCommand.Id,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}