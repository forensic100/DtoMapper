using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class ConverterFullCoverageTests
    {
        // =============================================================
        // 1. Primitive → primitive using global converter
        // =============================================================
        private class Src1
        {
            public int Value { get; set; }
        }

        private class Dest1
        {
            public string? Value { get; set; }
        }

        [TestMethod]
        public void Converter_Primitive_To_Primitive()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(x => $"#{x}");
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src1, Dest1>(new Src1 { Value = 5 });

            Assert.AreEqual("#5", result.Value);
        }

        // =============================================================
        // 2. Nullable source value
        // =============================================================
        private class Src2
        {
            public int? Value { get; set; }
        }

        private class Dest2
        {
            public string? Value { get; set; }
        }

        [TestMethod]
        public void Converter_Nullable_Source_Value()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int?, string?>(
                x => x == null ? null : $"V{x}");
            cfg.AutoRegister<Src2, Dest2>();

            var mapper = cfg.Build();

            var r1 = mapper.Map<Src2, Dest2>(new Src2 { Value = 3 });
            var r2 = mapper.Map<Src2, Dest2>(new Src2 { Value = null });

            Assert.AreEqual("V3", r1.Value);
            Assert.IsNull(r2.Value);
        }

        // =============================================================
        // 3. Converter inside nested object mapping
        // =============================================================
        private class InnerSrc3
        {
            public int Code { get; set; }
        }

        private class InnerDest3
        {
            public string? Code { get; set; }
        }

        private class Src3
        {
            public InnerSrc3? Inner { get; set; }
        }

        private class Dest3
        {
            public InnerDest3? Inner { get; set; }
        }

        [TestMethod]
        public void Converter_Nested_Object()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(x => $"C{x}");
            cfg.AutoRegister<InnerSrc3, InnerDest3>();
            cfg.AutoRegister<Src3, Dest3>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src3, Dest3>(
                new Src3 { Inner = new InnerSrc3 { Code = 7 } });

            Assert.IsNotNull(result.Inner);
            Assert.AreEqual("C7", result.Inner!.Code);
        }

        // =============================================================
        // 4. Converter inside array elements
        // =============================================================
        private class Src4
        {
            public int[]? Values { get; set; }
        }

        private class Dest4
        {
            public string[]? Values { get; set; }
        }

        [TestMethod]
        public void Converter_Array_Elements()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(x => $"#{x}");
            cfg.AutoRegister<Src4, Dest4>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src4, Dest4>(
                new Src4 { Values = new[] { 1, 2, 3 } });

            CollectionAssert.AreEqual(
                new[] { "#1", "#2", "#3" },
                result.Values!);
        }

        // =============================================================
        // 5. Converter inside list elements
        // =============================================================
        private class Src5
        {
            public List<int>? Values { get; set; }
        }

        private class Dest5
        {
            public List<string>? Values { get; set; }
        }

        [TestMethod]
        public void Converter_List_Elements()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(x => $"v{x}");
            cfg.AutoRegister<Src5, Dest5>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src5, Dest5>(
                new Src5 { Values = new List<int> { 4, 5 } });

            CollectionAssert.AreEqual(
                new List<string> { "v4", "v5" },
                result.Values!);
        }

        // =============================================================
        // 6. Converter precedence over direct assignment
        // =============================================================
        private class Src6
        {
            public int X { get; set; }
        }

        private class Dest6
        {
            public string? X { get; set; }
        }

        [TestMethod]
        public void Converter_Takes_Precedence_Over_Direct_Assign()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(_ => "OVERRIDDEN");
            cfg.AutoRegister<Src6, Dest6>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src6, Dest6>(
                new Src6 { X = 123 });

            Assert.AreEqual("OVERRIDDEN", result.X);
        }

        // =============================================================
        // 7. ReverseMap does NOT auto-reverse converters
        // =============================================================
        private class Src7
        {
            public int Value { get; set; }
        }

        private class Dest7
        {
            public string? Value { get; set; }
        }

        [TestMethod]
        public void Converter_Is_Not_Auto_Reversed()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(x => $"V{x}");
            cfg.AutoRegister<Src7, Dest7>().ReverseMap();

            var mapper = cfg.Build();

            var dest = mapper.Map<Src7, Dest7>(
                new Src7 { Value = 9 });
            var srcBack = mapper.Map<Dest7, Src7>(dest);

            Assert.AreEqual("V9", dest.Value);
            Assert.AreEqual(0, srcBack.Value); // default(int)
        }

        // =============================================================
        // 8. Converter + ReverseMap does not override forward behavior
        // =============================================================
        private class Src8
        {
            public int Id { get; set; }
        }

        private class Dest8
        {
            public string? Id { get; set; }
        }

        [TestMethod]
        public void Converter_With_ReverseMap_Behaves_Predictably()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(x => $"ID{x}");
            cfg.AutoRegister<Src8, Dest8>().ReverseMap();

            var mapper = cfg.Build();

            var dest = mapper.Map<Src8, Dest8>(
                new Src8 { Id = 2 });
            var srcBack = mapper.Map<Dest8, Src8>(dest);

            Assert.AreEqual("ID2", dest.Id);
            Assert.AreEqual(0, srcBack.Id); // no reverse converter
        }
    }
}