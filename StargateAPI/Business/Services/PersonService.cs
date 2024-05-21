using StargateAPI.Business.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace StargateAPI.Business.Services
{
    public class PersonService : IPersonService
    {
        private readonly StargateContext _context;

        public PersonService(StargateContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Restrict creation of AstronautDetail when no AstronautDuty is found
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AddAstronautDetail(int personId, AstronautDetail detail)
        {
            var person = await _context.People
                .Include(p => p.AstronautDuties)
                .FirstOrDefaultAsync(p => p.Id == personId);

            if (person == null)
            {
                throw new Exception("Person not found");
            }

            if (!person.AstronautDuties.Any())
            {
                throw new Exception("A person must have an AstronautDuty assignment ebfore adding AstronautDetail");
            }

            detail.PersonId = personId;
            _context.AstronautDetails.Add(detail);
            await _context.SaveChangesAsync();
        }
    }
}
