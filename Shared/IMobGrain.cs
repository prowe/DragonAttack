
using System;
using System.Threading.Tasks;
using Orleans;

namespace Dragon.Shared
{
    public interface IMobGrain : IGrainWithStringKey
    {
        Task<GameCharacterStatus> GetStatus();

        Task BeAttacked(Guid attackerId);
    }
}