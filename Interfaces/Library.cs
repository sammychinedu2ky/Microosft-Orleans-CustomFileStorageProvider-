namespace Interfaces
{
    public interface IPersonGrain : IGrainWithStringKey
    {
        Task AddName(string name);
        Task<string> GetName();
    }
}
