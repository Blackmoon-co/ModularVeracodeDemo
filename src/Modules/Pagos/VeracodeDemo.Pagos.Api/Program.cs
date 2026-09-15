using VeracodeDemo.Pagos.Api;
using VeracodeDemo.Shared;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var pagos = new List<Pago>
{
    new(Guid.Parse("afe99c49-f0a9-4a87-a652-176601af6f64"), Guid.Parse("0f0c93f5-0c9b-42e5-8cbd-6729baeb4ee7"), 780000m, "COP", "Aprobado")
};

app.MapGet("/", () => new ModuleInfo(
    "Pagos",
    "Registro y conciliacion de pagos",
    ["VeracodeDemo.Shared"]));

app.MapGet("/pagos", () =>
    DomainResult<IReadOnlyCollection<Pago>>.Ok("Pagos", "Pagos registrados", pagos));

app.MapPost("/pagos", (RegistrarPagoRequest request) =>
{
    var pago = new Pago(Guid.NewGuid(), request.ReservaId, request.Valor, request.Moneda, "Recibido");
    pagos.Add(pago);
    return Results.Created($"/pagos/{pago.Id}", DomainResult<Pago>.Ok("Pagos", "Pago registrado", pago));
});

app.Run();
