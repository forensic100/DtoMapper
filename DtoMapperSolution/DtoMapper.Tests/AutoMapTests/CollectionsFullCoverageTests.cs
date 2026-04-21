using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class CollectionsFullCoverageTests
    {
        // =============================================================
        // 1. Array → Array (primitive)
        // =============================================================
        private class SrcArr1 { public int[]? Numbers { get; set; } }
        private class DestArr1 { public int[]? Numbers { get; set; } }

        [TestMethod]
        public void Array_To_Array_Primitive()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcArr1, DestArr1>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcArr1, DestArr1>(new SrcArr1
            {
                Numbers = new[] { 1, 2, 3 }
            });

            Assert.IsNotNull(result.Numbers);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.Numbers!);
        }

        // =============================================================
        // 2. Array → Array (nested elements)
        // =============================================================
        private class Item2 { public string? Name { get; set; } }
        private class Item2D { public string? Name { get; set; } }

        private class SrcArr2 { public Item2[]? Items { get; set; } }
        private class DestArr2 { public Item2D[]? Items { get; set; } }

        [TestMethod]
        public void Array_To_Array_Nested_Elements()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Item2, Item2D>();
            cfg.AutoRegister<SrcArr2, DestArr2>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcArr2, DestArr2>(new SrcArr2
            {
                Items = new[]
                {
                    new Item2 { Name = "A" },
                    new Item2 { Name = "B" }
                }
            });

            Assert.IsNotNull(result.Items);
            Assert.AreEqual("A", result.Items![0].Name);
            Assert.AreEqual("B", result.Items![1].Name);
        }

        // =============================================================
        // 3. List<T> → List<T> (leaf)
        // =============================================================
        private class SrcList3 { public List<string>? Tags { get; set; } }
        private class DestList3 { public List<string>? Tags { get; set; } }

        [TestMethod]
        public void List_To_List_Simple()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcList3, DestList3>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcList3, DestList3>(new SrcList3
            {
                Tags = new List<string> { "X", "Y", "Z" }
            });

            Assert.IsNotNull(result.Tags);
            CollectionAssert.AreEqual(
                new List<string> { "X", "Y", "Z" },
                result.Tags!);
        }

        // =============================================================
        // 4. Array with null elements
        // =============================================================
        private class N4 { public string? V { get; set; } }
        private class N4D { public string? V { get; set; } }

        private class SrcArr4 { public N4?[]? Items { get; set; } }
        private class DestArr4 { public N4D?[]? Items { get; set; } }

        [TestMethod]
        public void Array_With_Null_Elements()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<N4, N4D>();
            cfg.AutoRegister<SrcArr4, DestArr4>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcArr4, DestArr4>(new SrcArr4
            {
                Items = new N4?[]
                {
                    new N4 { V = "A" },
                    null,
                    new N4 { V = "C" }
                }
            });

            Assert.IsNotNull(result.Items);
            Assert.AreEqual("A", result.Items![0]!.V);
            Assert.IsNull(result.Items![1]);
            Assert.AreEqual("C", result.Items![2]!.V);
        }

        // =============================================================
        // 5. Empty array
        // =============================================================
        private class SrcArr5 { public string[]? Names { get; set; } }
        private class DestArr5 { public string[]? Names { get; set; } }

        [TestMethod]
        public void Empty_Array_Roundtrip()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcArr5, DestArr5>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcArr5, DestArr5>(new SrcArr5
            {
                Names = new string[0]
            });

            Assert.IsNotNull(result.Names);
            Assert.AreEqual(0, result.Names!.Length);
        }

        // =============================================================
        // 6. Null array
        // =============================================================
        private class SrcArr6 { public int[]? Data { get; set; } }
        private class DestArr6 { public int[]? Data { get; set; } }

        [TestMethod]
        public void Null_Array_Roundtrip()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcArr6, DestArr6>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcArr6, DestArr6>(new SrcArr6
            {
                Data = null
            });

            Assert.IsNull(result.Data);
        }

        // =============================================================
        // 7. Flattening inside arrays is NOT supported
        // =============================================================
        private class Addr7 { public string? City { get; set; } }
        private class Cust7 { public Addr7? Address { get; set; } }

        private class SrcArr7 { public Cust7[]? People { get; set; } }
        private class DestArr7 { public string[]? City { get; set; } }

        [TestMethod]
        public void Flattening_Inside_Array_Is_Not_Supported()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcArr7, DestArr7>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcArr7, DestArr7>(new SrcArr7
            {
                People = new[]
                {
                    new Cust7 { Address = new Addr7 { City = "NY" } },
                    new Cust7 { Address = new Addr7 { City = "LA" } }
                }
            });

            // Collection flattening is intentionally unsupported
            Assert.IsNull(result.City);
        }
    }
}