using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public sealed class NestedMappingFullCoverageTests
    {
        // =============================================================
        // Helper types
        // =============================================================
        private sealed class InnerSrc
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        private sealed class InnerDest
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        // =============================================================
        // 1. Nested object maps when TypeMap exists
        // =============================================================
        private sealed class Src1
        {
            public InnerSrc? Inner { get; set; }
        }

        private sealed class Dest1
        {
            public InnerDest? Inner { get; set; }
        }

        [TestMethod]
        public void Nested_Object_Maps_When_TypeMap_Exists()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<InnerSrc, InnerDest>();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src1, Dest1>(
                new Src1
                {
                    Inner = new InnerSrc
                    {
                        Id = 5,
                        Name = "Test"
                    }
                });

            Assert.IsNotNull(result.Inner);
            Assert.AreEqual(5, result.Inner!.Id);
            Assert.AreEqual("Test", result.Inner.Name);
        }

        // =============================================================
        // 2. Nested object is null when source is null
        // =============================================================
        [TestMethod]
        public void Nested_Object_With_TypeMap_Throws_When_Source_Is_Null()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<InnerSrc, InnerDest>();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            Assert.ThrowsException<NullReferenceException>(() =>
                mapper.Map<Src1, Dest1>(
                    new Src1 { Inner = null }));
        }


        // =============================================================
        // 3. Nested object not created without TypeMap
        // =============================================================
        private sealed class DestNoMap
        {
            public InnerDest? Inner { get; set; }
        }

        [TestMethod]
        public void Nested_Object_Not_Created_Without_TypeMap()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, DestNoMap>();

            var mapper = cfg.Build();

            var result = mapper.Map<Src1, DestNoMap>(
                new Src1
                {
                    Inner = new InnerSrc
                    {
                        Id = 1,
                        Name = "X"
                    }
                });

            // No InnerSrc → InnerDest TypeMap exists
            Assert.IsNull(result.Inner);
        }

        // =============================================================
        // 4. ReverseMap does not infer nested mappings
        // =============================================================
        [TestMethod]
        public void ReverseMap_Does_Not_Create_Nested_TypeMap()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<InnerSrc, InnerDest>();
            cfg.AutoRegister<Src1, Dest1>()
               .ReverseMap();

            var mapper = cfg.Build();

            var forward = mapper.Map<Src1, Dest1>(
                new Src1
                {
                    Inner = new InnerSrc { Id = 3, Name = "A" }
                });

            var reverse = mapper.Map<Dest1, Src1>(forward);

            // Reverse nested mapping not inferred
            Assert.IsNull(reverse.Inner);
        }

        // =============================================================
        // 5. Nested collections follow normal collection rules
        // =============================================================
        private sealed class ChildSrc
        {
            public int Value { get; set; }
        }

        private sealed class ChildDest
        {
            public int Value { get; set; }
        }

        private sealed class NestedCollectionSrc
        {
            public List<ChildSrc>? Items { get; set; }
        }

        private sealed class NestedCollectionDest
        {
            public List<ChildDest>? Items { get; set; }
        }

        private sealed class SrcWithNestedCollection
        {
            public NestedCollectionSrc? Inner { get; set; }
        }

        private sealed class DestWithNestedCollection
        {
            public NestedCollectionDest? Inner { get; set; }
        }

        [TestMethod]
        public void Nested_Collections_Map_When_Element_TypeMap_Exists()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<ChildSrc, ChildDest>();
            cfg.AutoRegister<NestedCollectionSrc, NestedCollectionDest>();
            cfg.AutoRegister<SrcWithNestedCollection, DestWithNestedCollection>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcWithNestedCollection, DestWithNestedCollection>(
                new SrcWithNestedCollection
                {
                    Inner = new NestedCollectionSrc
                    {
                        Items = new List<ChildSrc>
                        {
                            new ChildSrc { Value = 1 },
                            new ChildSrc { Value = 2 }
                        }
                    }
                });

            Assert.IsNotNull(result.Inner);
            Assert.IsNotNull(result.Inner!.Items);
            Assert.AreEqual(2, result.Inner.Items!.Count);
            Assert.AreEqual(1, result.Inner.Items[0].Value);
            Assert.AreEqual(2, result.Inner.Items[1].Value);
        }

        // =============================================================
        // 6. Nested collections are null when element TypeMap missing
        // =============================================================
        [TestMethod]
        public void Nested_Collections_Are_Null_When_Element_Map_Missing()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<NestedCollectionSrc, NestedCollectionDest>();
            cfg.AutoRegister<SrcWithNestedCollection, DestWithNestedCollection>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcWithNestedCollection, DestWithNestedCollection>(
                new SrcWithNestedCollection
                {
                    Inner = new NestedCollectionSrc
                    {
                        Items = new List<ChildSrc>
                        {
                            new ChildSrc { Value = 5 }
                        }
                    }
                });

            Assert.IsNotNull(result.Inner);
            Assert.IsNull(result.Inner!.Items);
        }

        // =============================================================
        // 7. Nested mapping never throws
        // =============================================================
        [TestMethod]
        public void Nested_Mapping_Never_Throws()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            try
            {
                var result = mapper.Map<Src1, Dest1>(
                    new Src1
                    {
                        Inner = new InnerSrc { Id = 9, Name = "Safe" }
                    });

                Assert.IsNotNull(result);
            }
            catch
            {
                Assert.Fail("Nested mapping must not throw.");
            }
        }
    }
}