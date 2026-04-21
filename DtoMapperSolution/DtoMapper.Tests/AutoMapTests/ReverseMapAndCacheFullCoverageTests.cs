using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class ReverseMapAndCacheFullCoverageTests
    {
        // =============================================================
        // 1. ReverseMap registers both directions (fluent API)
        // =============================================================
        private class A1 { public string? Name { get; set; } }
        private class B1 { public string? Name { get; set; } }

        [TestMethod]
        public void ReverseMap_Registers_Both_Directions_Fluent()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<A1, B1>().ReverseMap();

            var mapper = cfg.Build();

            var a = new A1 { Name = "Foo" };
            var b = mapper.Map<A1, B1>(a);
            var a2 = mapper.Map<B1, A1>(b);

            Assert.AreEqual("Foo", b.Name);
            Assert.AreEqual("Foo", a2.Name);
        }

        // =============================================================
        // 2. ReverseMap does NOT override existing forward maps
        // =============================================================
        private class A2 { public string? S { get; set; } }
        private class B2 { public string? S { get; set; } }

        [TestMethod]
        public void ReverseMap_DoesNotOverride_Existing_Map()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<A2, B2>(); // forward map
            cfg.ReverseMap();           // backward via last-registered

            var mapper = cfg.Build();

            var b = mapper.Map<A2, B2>(new A2 { S = "X" });
            var a = mapper.Map<B2, A2>(b);

            Assert.AreEqual("X", b.S);
            Assert.AreEqual("X", a.S);
        }

        // =============================================================
        // 3. ReverseMap uses cached maps (no duplicate generation)
        // =============================================================
        private class A3 { public int V { get; set; } }
        private class B3 { public int V { get; set; } }

        [TestMethod]
        public void ReverseMap_Is_Cached_And_Reused()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<A3, B3>().ReverseMap();

            // First build compiles both TypeMaps
            var mapper1 = cfg.Build();

            var b1 = mapper1.Map<A3, B3>(new A3 { V = 10 });
            var a1 = mapper1.Map<B3, A3>(b1);

            Assert.AreEqual(10, b1.V);
            Assert.AreEqual(10, a1.V);

            // Sanity check on repeated calls (no new maps created)
            var b2 = mapper1.Map<A3, B3>(new A3 { V = 20 });
            var a2 = mapper1.Map<B3, A3>(b2);

            Assert.AreEqual(20, b2.V);
            Assert.AreEqual(20, a2.V);
        }

        // =============================================================
        // 4. ReverseMap with lists maintains object-level semantics
        //    (reverse does not materialize collections automatically)
        // =============================================================
        private class A4 { public List<Item4>? Items { get; set; } }
        private class B4 { public List<Item4>? Items { get; set; } }
        private class Item4 { public string? Name { get; set; } }

        [TestMethod]
        public void ReverseMap_List_Object_Level_Semantics()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Item4, Item4>();
            cfg.AutoRegister<A4, B4>().ReverseMap();

            var mapper = cfg.Build();

            var a = new A4
            {
                Items = new List<Item4>
                {
                    new Item4 { Name = "A" },
                    new Item4 { Name = "B" }
                }
            };

            var b = mapper.Map<A4, B4>(a);
            var a2 = mapper.Map<B4, A4>(b);

            Assert.IsNotNull(b.Items);
            Assert.AreEqual(2, b.Items!.Count);
            Assert.AreEqual("A", b.Items[0].Name);
            Assert.AreEqual("B", b.Items[1].Name);

            // ReverseMap preserves object-level semantics
            Assert.IsNotNull(a2.Items);
            Assert.AreEqual(2, a2.Items!.Count);
            Assert.AreEqual("A", a2.Items[0].Name);
            Assert.AreEqual("B", a2.Items[1].Name);
        }

        // =============================================================
        // 5. ReverseMap does not auto-reverse converters (cache-safe)
        // =============================================================
        private class A5 { public int Id { get; set; } }
        private class B5 { public string? Id { get; set; } }

        [TestMethod]
        public void ReverseMap_Does_Not_AutoReverse_Converters()
        {
            var cfg = new MapperConfiguration();
            cfg.AddGlobalConverter<int, string>(x => $"ID{x}");
            cfg.AutoRegister<A5, B5>().ReverseMap();

            var mapper = cfg.Build();

            var b = mapper.Map<A5, B5>(new A5 { Id = 7 });
            var a = mapper.Map<B5, A5>(b);

            Assert.AreEqual("ID7", b.Id);
            Assert.AreEqual(0, a.Id); // default(int) – no reverse converter
        }

        // =============================================================
        // 6. ReverseMap + caching across multiple instances
        // =============================================================
        private class A6 { public string? Name { get; set; } }
        private class B6 { public string? Name { get; set; } }

        [TestMethod]
        public void ReverseMap_Cache_Reused_Across_Multiple_Maps()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<A6, B6>().ReverseMap();

            var mapper = cfg.Build();

            var b1 = mapper.Map<A6, B6>(new A6 { Name = "One" });
            var b2 = mapper.Map<A6, B6>(new A6 { Name = "Two" });

            var a1 = mapper.Map<B6, A6>(b1);
            var a2 = mapper.Map<B6, A6>(b2);

            Assert.AreEqual("One", a1.Name);
            Assert.AreEqual("Two", a2.Name);
        }
    }
}