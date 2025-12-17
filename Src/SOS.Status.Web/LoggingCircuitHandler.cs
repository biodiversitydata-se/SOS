using Microsoft.AspNetCore.Components.Server.Circuits;

namespace SOS.Status.Web;

public class LoggingCircuitHandler : CircuitHandler
{
    private readonly ILogger<LoggingCircuitHandler> _logger;

    public LoggingCircuitHandler(ILogger<LoggingCircuitHandler> logger)
    {
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🟢 Circuit opened: {CircuitId}", circuit.Id);

        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🔌 Connection UP: {CircuitId}", circuit.Id);

        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "⚠️ Connection DOWN: {CircuitId}", circuit.Id);

        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "🔴 Circuit CLOSED: {CircuitId}", circuit.Id);

        return Task.CompletedTask;
    }
}
