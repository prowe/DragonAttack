using System.Threading.Tasks;
using Orleans;

namespace Backend.Controllers
{
    public interface IValues : IGrainWithIntegerKey
    {
        Task<int> Increment();
    }
}