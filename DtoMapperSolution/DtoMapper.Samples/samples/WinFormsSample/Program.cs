
using DtoMapper.Core;

static class Program
{
    public static Mapper Mapper = null!;

    [STAThread]
    static void Main()
    {
        var cfg = new MapperConfiguration();
        cfg.AutoRegister<UserModel, UserViewModel>();
        Mapper = cfg.Build();

        ApplicationConfiguration.Initialize();
        Application.Run(new Form());
    }
}

public class UserModel { public int Id { get; set; } public string? Name { get; set; } }
public class UserViewModel { public int Id { get; set; } public string? Name { get; set; } }
