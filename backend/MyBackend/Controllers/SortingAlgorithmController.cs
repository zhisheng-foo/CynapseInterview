using Microsoft.AspNetCore.Mvc;
using MyBackend.Models;
using MyBackend.Services;
using System.Linq;

namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SortAlgorithmController : ControllerBase
    {
        private readonly SortAlgorithmService _service;

        // Constructor with Dependency Injection
        public SortAlgorithmController(SortAlgorithmService service)
        {
            _service = service;
        }

        [HttpPost("allocate")]
        public IActionResult Allocate([FromBody] AllocationRequest request)
        {
            if (request == null || request.TotalAmount <= 0 || request.NumberOfParticipants <= 0)
                return BadRequest("Invalid input.");

            try
            {
                var results = _service.Allocate(request.TotalAmount, request.NumberOfParticipants);

                var response = results.Select(r => new AllocationResponse
                {
                    Participant = r.Key,
                    Amount = r.Value
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("allocations")]
        public IActionResult GetAllAllocations()
        {
            try
            {
                var allocations = _service.GetAllAllocations();
                return Ok(allocations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
