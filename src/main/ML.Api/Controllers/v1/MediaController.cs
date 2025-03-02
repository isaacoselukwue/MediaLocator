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
        public async Task<ActionResult<Result<AudioSearchDto>>> SearchAudio([FromQuery] AudioSearchQuery query)
        {
            var result = await sender.Send(query);
            return Ok(result);
        }
        //search for image
        [HttpGet("image/search")]
        public async Task<ActionResult<Result<ImageSearchDto>>> SearchVideo([FromQuery] ImageSearchQuery query)
        {
            var result = await sender.Send(query);
            return Ok(result);
        }
        //search for audio by id
        [HttpGet("audio/{id}")]
        public async Task<ActionResult<Result<AudioSearchDto>>> AudioDetails([FromRoute] string id)
        {
            AudioDetailsQuery query = new() { Id = id };
            var result = await sender.Send(query);
            return Ok(result);
        }
        //search for image by id
        [HttpGet("image/{id}")]
        public async Task<ActionResult<Result<ImageSearchDto>>> SearchImageById([FromRoute] string id)
        {
            ImageDetailsQuery query = new() { Id = id };
            var result = await sender.Send(query);
            return Ok(result);
        }

        //add to search history - user

        //fetch search history - user

        //delete search history - user

        //view search history - admin
    }
}
