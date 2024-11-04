using Domain.Contract;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PuansonController : ControllerBase
{
    private new IPuansonService Service { get; }

    public PuansonController(IPuansonService service) : base()
    {
        Service = service;
    }

    [HttpGet("fill")]
    public Task Fill()
    {
        
        return Task.CompletedTask;
    }
}