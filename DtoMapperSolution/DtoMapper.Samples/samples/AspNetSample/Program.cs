
using DtoMapper.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
{
    var cfg = new MapperConfiguration();
    cfg.AutoRegister<Customer, CustomerDto>();
    return cfg.Build();
});

builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();

public record Customer(int Id, string Name);
public record CustomerDto(int Id, string Name);
