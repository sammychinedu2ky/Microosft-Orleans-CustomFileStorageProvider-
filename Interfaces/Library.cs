using Orleans.Concurrency;

namespace Interfaces
{
    public interface IPersonGrain : IGrainWithStringKey
    {
        //[AlwaysInterleave]
        Task AddName(string name);
        Task<string> GetName();
    }
}
