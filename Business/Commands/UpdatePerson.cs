using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<BaseResponse>
    {
        public string Name { get; set; }
        public string NewName { get; set; }

        public class Handler : IRequestHandler<UpdatePerson, BaseResponse>
        {
            private readonly StargateContext _context;

            public Handler(StargateContext context)
            {
                _context = context;
            }

            public async Task<BaseResponse> Handle(UpdatePerson request, CancellationToken cancellationToken)
            {
                var person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.Name);
                if (person == null)
                {
                    return new BaseResponse { Message = "Person not found", Success = false, ResponseCode = (int)HttpStatusCode.NotFound };
                }

                person.Name = request.NewName ?? person.Name;
                _context.People.Update(person);
                await _context.SaveChangesAsync();

                return new BaseResponse { Success = true, Message = "Person updated successfully" };
            }
        }
    }
}
