using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;
using System.Collections.Generic;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AstronautDetailController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AstronautDetailController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAstronautDetails()
        {
            try
            {
                var result = await _mediator.Send(new GetAstronautDetailsQuery());
                return Ok(new
                {
                    Success = true,
                    Message = "Astronaut details retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return this.GetResponse(new BaseResponse
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAstronautDetail([FromBody] CreateAstronautDetail command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                return this.GetResponse(new BaseResponse
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}
