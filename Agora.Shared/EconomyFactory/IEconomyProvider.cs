namespace Agora.Shared.EconomyFactory;

public interface IEconomyProvider
{
    string EconomyType { get; }
    IEconomy CreateEconomy(IServiceProvider serviceProvider);
}
