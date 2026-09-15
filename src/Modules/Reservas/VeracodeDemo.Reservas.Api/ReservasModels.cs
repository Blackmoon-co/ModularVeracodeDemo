namespace VeracodeDemo.Reservas.Api;

public sealed record Reserva(
    Guid Id,
    Guid ClienteId,
    string Producto,
    DateOnly Fecha,
    string Estado);

public sealed record CrearReservaRequest(
    Guid ClienteId,
    string Producto,
    DateOnly Fecha);
