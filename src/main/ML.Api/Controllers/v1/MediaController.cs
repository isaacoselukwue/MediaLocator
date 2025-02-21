using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ML.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class MediaController : BaseController
    {
        //search for audio

        //search for video

        //search
    }
}
