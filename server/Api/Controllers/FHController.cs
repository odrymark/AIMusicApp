using FHHelper;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/featurehub")]
public class FhController(IFeatureStateProvider stateProvider) : ControllerBase
{
    [HttpPost("getFeatureState")]
    public ActionResult GetFeatureStatus([FromBody] string feature)
    {
        var res = stateProvider.IsEnabled(feature);
        return Ok(res);
    }
}