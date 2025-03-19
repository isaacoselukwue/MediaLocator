using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Common.Models;
using ML.Application.Media.Commands;
using ML.Application.Media.Queries;

namespace ML.Api.Controllers.v1
{
    [ApiController]
    [Authorize]
    public class MediaController(ISender sender) : BaseController
    {
        //search for audio
        [HttpGet("audio/search")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<Result<AudioSearchDto>>> SearchAudio([FromQuery] AudioSearchQuery query)
        {
            var result = await sender.Send(query);
            return Ok(result);
        }
        //search for image
        [HttpGet("image/search")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<Result<ImageSearchDto>>> SearchVideo([FromQuery] ImageSearchQuery query)
        {
            var result = await sender.Send(query);
            return Ok(result);
        }
        //search for audio by id
        [HttpGet("audio/{id}")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<Result<AudioSearchDto>>> AudioDetails([FromRoute] string id)
        {
            AudioDetailsQuery query = new() { Id = id };
            var result = await sender.Send(query);
            return Ok(result);
        }
        //search for image by id
        [HttpGet("image/{id}")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<Result<ImageSearchDto>>> SearchImageById([FromRoute] string id)
        {
            ImageDetailsQuery query = new() { Id = id };
            var result = await sender.Send(query);
            return Ok(result);
        }

        //add to search history - user
        [HttpPost("search-history")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<Result>> AddToSearchHistory([FromBody] AddSearchHistoryCommand command)
        {
            var result = await sender.Send(command);
            return Ok(result);
        }
        //fetch search history - user, date filter, isAscending (isDescending)
        [HttpGet("search-history")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<Result<UsersSearchHistoryDto>>> GetSearchHistory([FromQuery] UsersSearchHistoryQuery query)
        {
            var result = await sender.Send(query);
            return Ok(result);
        }
        //delete search history - user
        [HttpDelete("search-history/{id}")]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<Result>> DeleteSearchHistory([FromRoute] Guid id)
        {
            DeleteSearchHistoryCommand command = new() { Id = id };
            var result = await sender.Send(command);
            return Ok(result);
        }
        //view search history - admin (allow them to pass user email as well, date filter)
        [HttpGet("admin/search-history")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<Result<AdminSearchHistoryDto>>> ViewSearchHistory([FromQuery] AdminSearchHistoryQuery query)
        {
            var result = await sender.Send(query);
            return Ok(result);
        }

        [HttpGet("daily-media")]
        public async ValueTask<ActionResult<Result<DailyMediaDto>>> GetDailyMedia()
        {
            var query = new DailyMediaQuery();
            var result = await sender.Send(query);
            return Ok(result);
        }
    }
}
