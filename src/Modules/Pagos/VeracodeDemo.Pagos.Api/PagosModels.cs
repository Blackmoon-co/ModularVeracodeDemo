namespace VeracodeDemo.Pagos.Api;

public sealed record Pago(
    Guid Id,
    Guid ReservaId,
    decimal Valor,
    string Moneda,
    string Estado);

public sealed record RegistrarPagoRequest(
    Guid ReservaId,
    decimal Valor,
    string Moneda);
