using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public interface IPersonService
    {
        Task AddAstronautDetail(int personId, AstronautDetail detail);
    }
}