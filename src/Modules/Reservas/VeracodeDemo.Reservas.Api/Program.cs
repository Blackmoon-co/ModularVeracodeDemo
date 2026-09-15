using VeracodeDemo.Reservas.Api;
using VeracodeDemo.Shared;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var reservas = new List<Reserva>
{
    new(Guid.Parse("0f0c93f5-0c9b-42e5-8cbd-6729baeb4ee7"), Guid.Parse("29f31b39-5c63-4213-9cf9-72cb94354ab7"), "Sala de reuniones", new DateOnly(2026, 9, 15), "Confirmada"),
    new(Guid.Parse("27518c1b-4c68-46bd-9dbb-ff61e412b1f4"), Guid.Parse("24ac1f2d-3289-4c5d-a6a5-53996b1d6e71"), "Vehiculo ejecutivo", new DateOnly(2026, 9, 20), "Pendiente")
};

app.MapGet("/", () => new ModuleInfo(
    "Reservas",
    "Creacion y seguimiento de reservas",
    ["VeracodeDemo.Shared"]));

app.MapGet("/reservas", () =>
    DomainResult<IReadOnlyCollection<Reserva>>.Ok("Reservas", "Reservas registradas", reservas));

app.MapPost("/reservas", (CrearReservaRequest request) =>
{
    var reserva = new Reserva(Guid.NewGuid(), request.ClienteId, request.Producto, request.Fecha, "Pendiente");
    reservas.Add(reserva);
    return Results.Created($"/reservas/{reserva.Id}", DomainResult<Reserva>.Ok("Reservas", "Reserva creada", reserva));
});

app.Run();
