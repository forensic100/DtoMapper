
using DtoMapper.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton(sp =>
{
    var cfg = new MapperConfiguration();
    cfg.AutoRegister<User, UserViewModel>();
    return cfg.Build();
});

await builder.Build().RunAsync();

public class User { public int Id { get; set; } public string? Name { get; set; } }
public class UserViewModel { public int Id { get; set; } public string? Name { get; set; } }
