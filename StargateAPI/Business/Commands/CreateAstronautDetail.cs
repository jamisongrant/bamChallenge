using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDetail : IRequest<BaseResponse>
    {
        public int PersonId { get; set; }
        public string CurrentDutyTitle { get; set; }
        public string CurrentRank { get; set; }
        public DateTime CareerStartDate { get; set; }
        public DateTime? CareerEndDate { get; set; }
    }

    public class CreateAstronautDetailHandler : IRequestHandler<CreateAstronautDetail, BaseResponse>
    {
        private readonly StargateContext _context;

        public CreateAstronautDetailHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse> Handle(CreateAstronautDetail request, CancellationToken cancellationToken)
        {
            // Check if the person exists
            var person = await _context.People.FindAsync(request.PersonId);
            if (person == null)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "Person not found"
                };
            }

            // Check if the person has at least one AstronautDuty
            var hasDuty = await _context.AstronautDuties.AnyAsync(ad => ad.PersonId == request.PersonId, cancellationToken);
            if (!hasDuty)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = "Person must have at least one AstronautDuty before adding AstronautDetail"
                };
            }

            // Create and save the AstronautDetail
            var astronautDetail = new AstronautDetail
            {
                PersonId = request.PersonId,
                CurrentDutyTitle = request.CurrentDutyTitle,
                CurrentRank = request.CurrentRank,
                CareerStartDate = request.CareerStartDate,
                CareerEndDate = request.CareerEndDate
            };

            _context.AstronautDetails.Add(astronautDetail);
            await _context.SaveChangesAsync(cancellationToken);

            return new BaseResponse
            {
                Success = true,
                Message = "AstronautDetail created successfully"
            };
        }
    }
}
