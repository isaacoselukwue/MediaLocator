global using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ML.Api.Controllers.v1;
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class BaseController : ControllerBase
{
}
