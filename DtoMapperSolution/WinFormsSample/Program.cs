using DtoMapper.Core;

namespace WinFormsSample;

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
        Application.Run(new Form1());

    }
}