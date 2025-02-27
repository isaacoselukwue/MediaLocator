using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ML.Application.Common.Models;
using ML.Application.Media.Queries;

namespace ML.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class MediaController(ISender sender) : BaseController
    {
        //search for audio
        [HttpGet("audio/search")]
        public async Task<ActionResult<Result<AudioSearchDto>>> SearchAudio([FromQuery] AudioSearchQuery query) //configure swagger to recognise enum values
        {
            var result = await sender.Send(query);
            return Ok(result);
        }
        //search for video

        //add to search history - user

        //fetch search history - user

        //delete search history - user

        //view search history - admin
    }
}
