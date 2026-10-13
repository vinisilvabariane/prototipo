using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/workflow")]
public class WorkflowController(
    
    ) : ControllerBase
{
    public async Task<IActionResult> Create() { 
        
    }
}
