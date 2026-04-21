using DtoMapper.Core;

var cfg = new MapperConfiguration();
cfg.AutoRegister<UserEntity, UserDto>();

var mapper = cfg.Build();

var user = new UserEntity
{
    Id = 1,
    Name = "Alice"
};

var dto = mapper.Map<UserEntity, UserDto>(user);

Console.WriteLine($"{dto.Id} - {dto.Name}");