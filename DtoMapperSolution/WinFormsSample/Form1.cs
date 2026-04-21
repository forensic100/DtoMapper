namespace WinFormsSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();

            var user = new UserModel
            {
                Id = 1,
                Name = "Alice"
            };

            var vm = Program.Mapper
                .Map<UserModel, UserViewModel>(user);

            labelName.Text = vm.Name;

        }
    }
}
