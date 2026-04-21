using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public sealed class NullHandlingTests
    {
        // =============================================================
        // Simple models
        // =============================================================
        private sealed class SrcSimple
        {
            public string? Name { get; set; }
            public int Value { get; set; }
        }

        private sealed class DestSimple
        {
            public string? Name { get; set; }
            public int Value { get; set; }
        }

        // =============================================================
        // 1. Null source object creates default destination instance
        // =============================================================
        [TestMethod]
        public void Mapping_Null_Source_Object_Creates_Default_Destination()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcSimple, DestSimple>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcSimple, DestSimple>(null!);

            // ✅ Root-level null source creates destination with defaults
            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
            Assert.AreEqual(0, result.Value);
        }

        // =============================================================
        // 2. Null properties propagate safely
        // =============================================================
        [TestMethod]
        public void Null_Properties_Are_Mapped_Safely()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcSimple, DestSimple>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcSimple, DestSimple>(
                new SrcSimple
                {
                    Name = null,
                    Value = 5
                });

            Assert.IsNull(result.Name);
            Assert.AreEqual(5, result.Value);
        }

        // =============================================================
        // Nested models
        // =============================================================
        private sealed class InnerSrc
        {
            public string? Data { get; set; }
        }

        private sealed class InnerDest
        {
            public string? Data { get; set; }
        }

        private sealed class SrcNested
        {
            public InnerSrc? Inner { get; set; }
        }

        private sealed class DestNested
        {
            public InnerDest? Inner { get; set; }
        }

        // =============================================================
        // 3. Nested null throws when TypeMap exists
        // =============================================================
        [TestMethod]
        public void Nested_Null_With_TypeMap_Throws()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<InnerSrc, InnerDest>();
            cfg.AutoRegister<SrcNested, DestNested>();

            var mapper = cfg.Build();

            Assert.ThrowsException<System.NullReferenceException>(() =>
                mapper.Map<SrcNested, DestNested>(
                    new SrcNested { Inner = null }));
        }

        // =============================================================
        // 4. Nested null without TypeMap remains null
        // =============================================================
        private sealed class DestNestedNoMap
        {
            public InnerDest? Inner { get; set; }
        }

        [TestMethod]
        public void Nested_Null_Without_TypeMap_Remains_Null()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcNested, DestNestedNoMap>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcNested, DestNestedNoMap>(
                new SrcNested { Inner = null });

            Assert.IsNull(result.Inner);
        }

        // =============================================================
        // Collection models
        // =============================================================
        private sealed class ItemSrc
        {
            public int Value { get; set; }
        }

        private sealed class ItemDest
        {
            public int Value { get; set; }
        }

        private sealed class SrcCollection
        {
            public List<ItemSrc>? Items { get; set; }
        }

        private sealed class DestCollection
        {
            public List<ItemDest>? Items { get; set; }
        }

        // =============================================================
        // 5. Null collection propagates to destination
        // =============================================================
        [TestMethod]
        public void Null_Collection_Maps_To_Null()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcCollection, DestCollection>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcCollection, DestCollection>(
                new SrcCollection { Items = null });

            Assert.IsNull(result.Items);
        }

        // =============================================================
        // 6. Collection maps when element TypeMap exists
        // =============================================================
        [TestMethod]
        public void Collection_Maps_When_Element_TypeMap_Exists()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<ItemSrc, ItemDest>();
            cfg.AutoRegister<SrcCollection, DestCollection>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcCollection, DestCollection>(
                new SrcCollection
                {
                    Items = new List<ItemSrc>
                    {
                        new ItemSrc { Value = 1 },
                        new ItemSrc { Value = 2 }
                    }
                });

            Assert.IsNotNull(result.Items);
            Assert.AreEqual(2, result.Items!.Count);
            Assert.AreEqual(1, result.Items[0].Value);
            Assert.AreEqual(2, result.Items[1].Value);
        }

        // =============================================================
        // 7. Collection becomes null when element TypeMap missing
        // =============================================================
        [TestMethod]
        public void Collection_Becomes_Null_When_Element_Map_Missing()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcCollection, DestCollection>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcCollection, DestCollection>(
                new SrcCollection
                {
                    Items = new List<ItemSrc>
                    {
                        new ItemSrc { Value = 9 }
                    }
                });

            Assert.IsNull(result.Items);
        }
    }
}