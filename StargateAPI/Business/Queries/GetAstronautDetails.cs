using MediatR;
using StargateAPI.Business.Data;
using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDetailsQuery : IRequest<List<AstronautDetail>>
    {
    }

    public class GetAstronautDetailsHandler : IRequestHandler<GetAstronautDetailsQuery, List<AstronautDetail>>
    {
        private readonly StargateContext _context;

        public GetAstronautDetailsHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<List<AstronautDetail>> Handle(GetAstronautDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _context.AstronautDetails.ToListAsync(cancellationToken);
        }
    }
}
