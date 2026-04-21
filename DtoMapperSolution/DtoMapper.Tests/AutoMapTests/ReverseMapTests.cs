using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class ReverseMapTests
    {
        private class A { public string? Name { get; set; } }
        private class B { public string? Name { get; set; } }

        [TestMethod]
        public void ReverseMap_Registers_Both_Directions()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<A, B>().ReverseMap();
            var mapper = cfg.Build();

            var a = new A { Name = "Foo" };
            var b = mapper.Map<A, B>(a);
            var a2 = mapper.Map<B, A>(b);

            Assert.AreEqual("Foo", a2.Name);
        }
    }
}