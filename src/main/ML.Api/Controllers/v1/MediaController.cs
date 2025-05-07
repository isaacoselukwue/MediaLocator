using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Common.Models;
using ML.Application.Media.Commands;
using ML.Application.Media.Queries;

namespace ML.Api.Controllers.v1;

/// <summary>
/// Controller for managing media-related operations including searching for audio and images,
/// retrieving media details, and managing search history.
/// </summary>
[ApiController]
[Authorize]
public class MediaController(ISender sender) : BaseController
{
    /// <summary>
    /// Searches for audio based on specified criteria.
    /// </summary>
    /// <param name="query">
    /// The search query parameters including search terms, license type, category and pagination information.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{AudioSearchDto}"/> with paginated audio search results.
    /// </returns>
    /// <response code="200">Audio search results retrieved successfully.</response>
    /// <response code="400">Search request failed due to validation errors.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user does not have required permissions.</response>
    [HttpGet("audio/search")]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(Result<AudioSearchDto>), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<Result<AudioSearchDto>>> SearchAudio([FromQuery] AudioSearchQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
    /// <summary>
    /// Searches for images based on specified criteria.
    /// </summary>
    /// <param name="query">
    /// The search query parameters including search terms, license type, category and pagination information.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{ImageSearchDto}"/> with paginated image search results.
    /// </returns>
    /// <response code="200">Image search results retrieved successfully.</response>
    /// <response code="400">Search request failed due to validation errors.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user does not have required permissions.</response>
    [HttpGet("image/search")]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(Result<ImageSearchDto>), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<Result<ImageSearchDto>>> SearchImage([FromQuery] ImageSearchQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
    /// <summary>
    /// Retrieves detailed information about a specific audio file by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the audio file.</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{AudioSearchResult}"/> with detailed audio information.
    /// </returns>
    /// <response code="200">Audio details retrieved successfully.</response>
    /// <response code="400">Request failed due to invalid ID.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user does not have required permissions.</response>
    /// <response code="404">Audio file with specified ID not found.</response>
    [HttpGet("audio/{id}")]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(Result<AudioSearchResult>), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(Result), 404)]
    public async Task<ActionResult<Result<AudioSearchDto>>> AudioDetails([FromRoute] string id)
    {
        AudioDetailsQuery query = new() { Id = id };
        var result = await sender.Send(query);
        return Ok(result);
    }
    /// <summary>
    /// Retrieves detailed information about a specific image by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the image.</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{ImageSearchResult}"/> with detailed image information.
    /// </returns>
    /// <response code="200">Image details retrieved successfully.</response>
    /// <response code="400">Request failed due to invalid ID.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user does not have required permissions.</response>
    /// <response code="404">Image with specified ID not found.</response>
    [HttpGet("image/{id}")]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(Result<ImageSearchResult>), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(Result), 404)]
    public async Task<ActionResult<Result<ImageSearchDto>>> SearchImageById([FromRoute] string id)
    {
        ImageDetailsQuery query = new() { Id = id };
        var result = await sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Adds an item to the user's search history.
    /// </summary>
    /// <param name="command">The command containing the search ID and search type.</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result"/> indicating whether the operation was successful.
    /// </returns>
    /// <response code="200">Item added to search history successfully.</response>
    /// <response code="400">Request failed due to validation errors.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user does not have required permissions.</response>
    [HttpPost("search-history")]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<Result>> AddToSearchHistory([FromBody] AddSearchHistoryCommand command)
    {
        var result = await sender.Send(command);
        return Ok(result);
    }
    /// <summary>
    /// Retrieves the authenticated user's search history with optional filtering and sorting.
    /// </summary>
    /// <param name="query">
    /// The query parameters specifying title filter, date range, sorting order and pagination information.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{UsersSearchHistoryDto}"/> with paginated search history data.
    /// </returns>
    /// <response code="200">Search history retrieved successfully.</response>
    /// <response code="400">Request failed due to invalid query parameters.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user does not have required permissions.</response>
    [HttpGet("search-history")]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(Result<UsersSearchHistoryDto>), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<Result<UsersSearchHistoryDto>>> GetSearchHistory([FromQuery] UsersSearchHistoryQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }
    /// <summary>
    /// Deletes a specific item from the user's search history.
    /// </summary>
    /// <param name="id">The unique identifier of the search history item to delete.</param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result"/> indicating whether the deletion was successful.
    /// </returns>
    /// <response code="200">Search history item deleted successfully.</response>
    /// <response code="400">Deletion failed due to invalid ID.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user does not have required permissions.</response>
    /// <response code="404">Search history item with specified ID not found.</response>
    [HttpDelete("search-history/{id}")]
    [Authorize(Policy = "UserPolicy")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(Result), 404)]
    public async Task<ActionResult<Result>> DeleteSearchHistory([FromRoute] Guid id)
    {
        DeleteSearchHistoryCommand command = new() { Id = id };
        var result = await sender.Send(command);
        return Ok(result);
    }
    /// <summary>
    /// Allows administrators to view search history across all users with filtering options.
    /// </summary>
    /// <param name="query">
    /// The query parameters specifying title filter, date range, user email, status, sorting order and pagination information.
    /// </param>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{AdminSearchHistoryDto}"/> with paginated search history data.
    /// </returns>
    /// <response code="200">Admin search history retrieved successfully.</response>
    /// <response code="400">Request failed due to invalid query parameters.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Forbidden, user is not an administrator.</response>
    [HttpGet("admin/search-history")]
    [Authorize(Policy = "AdminPolicy")]
    [ProducesResponseType(typeof(Result<AdminSearchHistoryDto>), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<Result<AdminSearchHistoryDto>>> ViewSearchHistory([FromQuery] AdminSearchHistoryQuery query)
    {
        var result = await sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the media of the day, including a word of the day and curated audio and image results.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing a <see cref="Result{DailyMediaDto}"/> with the daily media content.
    /// </returns>
    /// <response code="200">Daily media retrieved successfully.</response>
    /// <response code="400">Request failed.</response>
    [HttpGet("daily-media")]
    [ProducesResponseType(typeof(Result<DailyMediaDto>), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async ValueTask<ActionResult<Result<DailyMediaDto>>> GetDailyMedia()
    {
        DailyMediaQuery query = new();
        var result = await sender.Send(query);
        return Ok(result);
    }
}
