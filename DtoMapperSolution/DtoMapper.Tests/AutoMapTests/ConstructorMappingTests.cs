using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class ConstructorMappingTests
    {
        private class UserDto { public string? Name { get; set; } }

        private class User
        {
            public string Name { get; }
            public User(string name) { Name = name; }
        }

        [TestMethod]
        public void Constructor_Injection_Mapping_Works()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<UserDto, User>();
            var mapper = cfg.Build();

            var dto = new UserDto { Name = "Alice" };
            var user = mapper.Map<UserDto, User>(dto);

            Assert.AreEqual("Alice", user.Name);
        }
    }
}