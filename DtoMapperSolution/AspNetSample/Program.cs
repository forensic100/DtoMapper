//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using AspNetSample;
using DtoMapper.Core;

var builder = WebApplication.CreateBuilder(args);

// Mapper configuration
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

