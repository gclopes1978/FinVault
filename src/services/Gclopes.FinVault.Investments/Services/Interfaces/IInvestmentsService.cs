using Gclopes.FinVault.Investments.DTO;

namespace Gclopes.FinVault.Investments.Services.Interfaces;

public interface IInvestmentsService
{
    Task<List<PortfolioDto>> GetPortfolioAsync(CancellationToken cancellationToken = default);
    Task<List<TransactionDto>> GetPortfolioHistoryAsync(CancellationToken cancellationToken = default);
    Task<int> ImportXtbCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<int> ImportTrading212CsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
    
}
