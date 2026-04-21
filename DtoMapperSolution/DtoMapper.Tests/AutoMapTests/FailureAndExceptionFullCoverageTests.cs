using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class FailureAndExceptionFullCoverageTests
    {
        // =============================================================
        // Enums used for mismatch tests
        // =============================================================
        private enum EnumA
        {
            One,
            Two
        }

        private enum EnumB
        {
            One
        }

        // =============================================================
        // 1. Enum mismatch does NOT throw (defaults instead)
        // =============================================================
        private class Src1
        {
            public EnumA Value { get; set; }
        }

        private class Dest1
        {
            public EnumB Value { get; set; }
        }

        [TestMethod]
        public void Enum_Name_Mismatch_Maps_To_Default()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src1, Dest1>(
                new Src1 { Value = EnumA.Two });

            Assert.AreEqual(default(EnumB), result.Value);
        }

        // =============================================================
        // 2. Missing converter does NOT throw
        // =============================================================
        private class Src2
        {
            public int Value { get; set; }
        }

        private class Dest2
        {
            public string? Value { get; set; }
        }

        [TestMethod]
        public void Missing_Converter_Does_Not_Throw()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src2, Dest2>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src2, Dest2>(
                new Src2 { Value = 10 });

            Assert.IsNull(result.Value);
        }

        // =============================================================
        // 3. ReverseMap without converters does NOT throw
        // =============================================================
        private class Src3
        {
            public int Value { get; set; }
        }

        private class Dest3
        {
            public string? Value { get; set; }
        }

        [TestMethod]
        public void ReverseMap_Unsupported_Does_Not_Throw()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src3, Dest3>().ReverseMap();

            var mapper = cfg.Build();

            var forward = mapper.Map<Src3, Dest3>(
                new Src3 { Value = 7 });

            var reverse = mapper.Map<Dest3, Src3>(forward);

            Assert.IsNull(forward.Value);
            Assert.AreEqual(0, reverse.Value);
        }

        // =============================================================
        // 4. Mapping null source returns default
        // =============================================================
        [TestMethod]
        public void Mapping_Null_Source_Returns_Default()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src1, Dest1>(null!);

            Assert.AreEqual(default(EnumB), result.Value);
        }

        // =============================================================
        // 5. Missing parameterless constructor throws
        // =============================================================
        private class Src5
        {
            public int X { get; set; }
        }

        private class Dest5
        {
            public int X { get; }

            public Dest5(int x)
            {
                X = x;
            }
        }

        [TestMethod]
        public void Constructor_With_Parameters_Is_Supported()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src5, Dest5>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src5, Dest5>(new Src5 { X = 5 });

            Assert.AreEqual(5, result.X);
        }

        // =============================================================
        // 6. Build() may only be called once
        // =============================================================
        [TestMethod]
        public void Build_Called_Twice_Throws()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            cfg.Build();

            Assert.ThrowsException<System.InvalidOperationException>(() =>
            {
                cfg.Build();
            });
        }

        // =============================================================
        // 7. ReverseMap before AutoRegister throws
        // =============================================================
        [TestMethod]
        public void ReverseMap_Without_AutoRegister_Throws()
        {
            var cfg = new MapperConfiguration();

            Assert.ThrowsException<System.InvalidOperationException>(() =>
            {
                cfg.ReverseMap();
            });
        }
    }
}