using MediatR;
using Microsoft.AspNetCore.Mvc;
using MotorCare.Application.Vehicles.Commands.CreateVehicle;
using MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

namespace MotorCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateVehicleCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByPlate), new { plate = command.Plate }, id);
    }

    [HttpGet("{plate}")]
    public async Task<ActionResult<VehicleDto>> GetByPlate(string plate)
    {
        var vehicle = await _mediator.Send(new GetVehicleByPlateQuery(plate));

        if (vehicle == null)
            return NotFound();

        return Ok(vehicle);
    }
}
