
using System;
using System.Threading.Tasks;
using Orleans;

namespace Dragon.Shared
{
    public interface IPlayerGrain : IGrainWithGuidKey
    {
        Task<GameCharacterStatus> GetStatus();

        Task BeAttacked(Guid attackerId);
    }
}