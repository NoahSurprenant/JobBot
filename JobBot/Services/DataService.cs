using JobBot.Database;
using Microsoft.EntityFrameworkCore;

namespace JobBot.Services;
public class DataService
{
    private readonly IDbContextFactory<DataContext> _factory;
    public DataService(IDbContextFactory<DataContext> factory)
    {
        _factory = factory;
    }
}
