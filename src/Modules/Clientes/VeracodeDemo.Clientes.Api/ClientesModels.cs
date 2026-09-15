namespace VeracodeDemo.Clientes.Api;

public sealed record Cliente(
    Guid Id,
    string Nombre,
    string Documento,
    string Segmento);

public sealed record CrearClienteRequest(
    string Nombre,
    string Documento,
    string Segmento);
