using Microsoft.VisualStudio.TestTools.UnitTesting;
using DtoMapper;
using DtoMapper.Core;

namespace DtoMapper.Tests.AutoMapTests
{
    // ================================================================
    // 1. DIRECT PROPERTY MAPPING
    // ================================================================
    [TestClass]
    public class DirectMappingTests
    {
        private class SrcA
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        private class DestA
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        [TestMethod]
        public void Direct_Properties_Map_Correctly()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcA, DestA>();
            var mapper = cfg.Build();

            var src = new SrcA { Id = 11, Name = "Alice" };
            var dest = mapper.Map<SrcA, DestA>(src);

            Assert.AreEqual(11, dest.Id);
            Assert.AreEqual("Alice", dest.Name);
        }

        // ----------------------------------------------------------

        private class SrcB
        {
            public string? Title { get; set; }
        }

        private class DestB
        {
            public string? title { get; set; } // lower-case destination
        }

        [TestMethod]
        public void Direct_Mapping_Ignores_Case()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcB, DestB>();
            var mapper = cfg.Build();

            var src = new SrcB { Title = "Engineer" };
            var dest = mapper.Map<SrcB, DestB>(src);

            Assert.AreEqual("Engineer", dest.title);
        }
    }

}