using VeracodeDemo.Clientes.Api;
using VeracodeDemo.Shared;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var clientes = new List<Cliente>
{
    new(Guid.Parse("29f31b39-5c63-4213-9cf9-72cb94354ab7"), "Ana Torres", "CC-10001", "Empresarial"),
    new(Guid.Parse("24ac1f2d-3289-4c5d-a6a5-53996b1d6e71"), "Luis Mora", "CC-10002", "Preferencial")
};

app.MapGet("/", () => new ModuleInfo(
    "Clientes",
    "Registro y consulta de clientes",
    ["VeracodeDemo.Shared"]));

app.MapGet("/clientes", () =>
    DomainResult<IReadOnlyCollection<Cliente>>.Ok("Clientes", "Clientes activos", clientes));

app.MapGet("/clientes/{id:guid}", (Guid id) =>
{
    var cliente = clientes.SingleOrDefault(item => item.Id == id);
    return cliente is null
        ? Results.NotFound(DomainResult<Cliente>.Fail("Clientes", "Cliente no encontrado"))
        : Results.Ok(DomainResult<Cliente>.Ok("Clientes", "Cliente encontrado", cliente));
});

app.MapPost("/clientes", (CrearClienteRequest request) =>
{
    var cliente = new Cliente(Guid.NewGuid(), request.Nombre, request.Documento, request.Segmento);
    clientes.Add(cliente);
    return Results.Created($"/clientes/{cliente.Id}", DomainResult<Cliente>.Ok("Clientes", "Cliente creado", cliente));
});

app.Run();
