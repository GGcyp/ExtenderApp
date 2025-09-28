

namespace ExtenderApp.Repositoryies
{
    public interface IDbContextProvider
    {
        IDbContext GetDbContext();
    }
}
