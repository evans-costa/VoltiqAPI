using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Voltiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    [field: AllowNull, MaybeNull]
    protected ISender Sender =>
        field ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
