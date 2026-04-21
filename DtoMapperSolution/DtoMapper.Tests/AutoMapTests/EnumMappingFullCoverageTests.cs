using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public sealed class EnumMappingFullCoverageTests
    {
        // =============================================================
        // Test enums
        // =============================================================
        private enum StatusA
        {
            Unknown = 0,
            Active = 1,
            Disabled = 2
        }

        private enum StatusB
        {
            Unknown = 0,
            Active = 10,
            Disabled = 20
        }

        private enum StatusC
        {
            Unknown = 0,
            Active = 1
        }

        // =============================================================
        // 1. Enum → Enum defaults when numeric value missing
        // =============================================================
        private sealed class Src1
        {
            public StatusA Status { get; set; }
        }

        private sealed class Dest1
        {
            public StatusB Status { get; set; }
        }

        [TestMethod]
        public void Enum_To_Enum_Defaults_To_Unknown_When_Value_Missing()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src1, Dest1>(
                new Src1 { Status = StatusA.Active });

            Assert.AreEqual(StatusB.Unknown, result.Status);
        }

        // =============================================================
        // 2. Enum mismatch defaults to Unknown
        // =============================================================
        private sealed class Src2
        {
            public StatusA Status { get; set; }
        }

        private sealed class Dest2
        {
            public StatusC Status { get; set; }
        }

        [TestMethod]
        public void Enum_Mismatch_Defaults_To_Unknown()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src2, Dest2>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src2, Dest2>(
                new Src2 { Status = StatusA.Disabled });

            Assert.AreEqual(StatusC.Unknown, result.Status);
        }

        // =============================================================
        // 3. ReverseMap always defaults enums to Unknown
        // =============================================================
        [TestMethod]
        public void Enum_ReverseMap_Defaults_To_Unknown()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>()
               .ReverseMap();

            var mapper = cfg.Build();

            var forward = mapper.Map<Src1, Dest1>(
                new Src1 { Status = StatusA.Active });

            var reverse = mapper.Map<Dest1, Src1>(forward);

            Assert.AreEqual(StatusB.Unknown, forward.Status);
            Assert.AreEqual(StatusA.Unknown, reverse.Status);
        }

        // =============================================================
        // 4. Enum mapping in ARRAYS is numeric
        // =============================================================
        private sealed class SrcArr
        {
            public StatusA[]? Values { get; set; }
        }

        private sealed class DestArr
        {
            public StatusC[]? Values { get; set; }
        }

        [TestMethod]
        public void Enum_Mapping_In_Array_Elements_Is_Numeric()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcArr, DestArr>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcArr, DestArr>(
                new SrcArr
                {
                    Values = new[]
                    {
                        StatusA.Active,
                        StatusA.Disabled
                    }
                });

            Assert.IsNotNull(result.Values);
            Assert.AreEqual(2, result.Values!.Length);
            Assert.AreEqual((StatusC)1, result.Values[0]);
            Assert.AreEqual((StatusC)2, result.Values[1]);
        }

        // =============================================================
        // 5. Enum mapping in LISTS is unsupported → null
        // =============================================================
        private sealed class SrcList
        {
            public List<StatusA>? Values { get; set; }
        }

        private sealed class DestList
        {
            public List<StatusC>? Values { get; set; }
        }

        [TestMethod]
        public void Enum_Mapping_In_List_Elements_Is_Null()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcList, DestList>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcList, DestList>(
                new SrcList
                {
                    Values = new List<StatusA>
                    {
                        StatusA.Active,
                        StatusA.Disabled
                    }
                });

            Assert.IsNull(result.Values);
        }
    }
}