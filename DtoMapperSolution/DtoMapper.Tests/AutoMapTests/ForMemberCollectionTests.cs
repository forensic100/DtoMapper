using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public sealed class ForMemberCollectionTests
    {
        // =============================================================
        // Test models
        // =============================================================

        private sealed class ChildSource
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        private sealed class ChildDest
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        private sealed class ParentSource
        {
            public ParentSource()
            {
                Children = new List<ChildSource>();
            }

            public List<ChildSource> Children { get; set; }
        }

        private sealed class ParentDest
        {
            public ParentDest()
            {
                ChildModels = new List<ChildDest>();
            }

            public List<ChildDest> ChildModels { get; set; }
        }

        // =============================================================
        // 1. ForMember maps renamed child collection
        // =============================================================
        [TestMethod]
        public void ForMember_MapFromCollection_Maps_Child_Collections()
        {
            var cfg = new MapperConfiguration();

            cfg.AutoRegister<ChildSource, ChildDest>();

            cfg.AutoRegister<ParentSource, ParentDest>()
               .ForMember(
                   d => d.ChildModels,
                   o => o.MapFromCollection(s => s.Children));

            var mapper = cfg.Build();

            var result = mapper.Map<ParentSource, ParentDest>(
                new ParentSource
                {
                    Children =
                    {
                        new ChildSource { Id = 1, Value = "A" },
                        new ChildSource { Id = 2, Value = "B" }
                    }
                });

            Assert.IsNotNull(result.ChildModels);
            Assert.AreEqual(2, result.ChildModels.Count);
            Assert.AreEqual(1, result.ChildModels[0].Id);
            Assert.AreEqual("A", result.ChildModels[0].Value);
            Assert.AreEqual(2, result.ChildModels[1].Id);
            Assert.AreEqual("B", result.ChildModels[1].Value);
        }

        // =============================================================
        // 2. Empty collection maps correctly
        // =============================================================
        [TestMethod]
        public void ForMember_MapFromCollection_Maps_Empty_Collection()
        {
            var cfg = new MapperConfiguration();

            cfg.AutoRegister<ChildSource, ChildDest>();
            cfg.AutoRegister<ParentSource, ParentDest>()
               .ForMember(
                   d => d.ChildModels,
                   o => o.MapFromCollection(s => s.Children));

            var mapper = cfg.Build();

            var result = mapper.Map<ParentSource, ParentDest>(
                new ParentSource());

            Assert.IsNotNull(result.ChildModels);
            Assert.AreEqual(0, result.ChildModels.Count);
        }

        // =============================================================
        // 3. ReverseMap works with ForMember collections
        // =============================================================
        [TestMethod]
        public void ForMember_MapFromCollection_ReverseMap_Does_Not_Infer_Reverse()
        {
            var cfg = new MapperConfiguration();

            cfg.AutoRegister<ChildSource, ChildDest>();
            cfg.AutoRegister<ParentSource, ParentDest>()
                .ForMember(
                    d => d.ChildModels,
                    o => o.MapFromCollection(s => s.Children))
                .ReverseMap();

            var mapper = cfg.Build();

            var forward = mapper.Map<ParentSource, ParentDest>(
                new ParentSource
                {
                    Children =
                    {
                        new ChildSource { Id = 9, Value = "X" }
                    }
                });

            var reverse = mapper.Map<ParentDest, ParentSource>(forward);

            // ✅ forward works
            Assert.AreEqual(1, forward.ChildModels.Count);

            // ✅ reverse does NOT infer collection mapping
            Assert.IsNotNull(reverse.Children);
            Assert.AreEqual(0, reverse.Children.Count);
        }


        // =============================================================
        // 4. ForMember collection does not affect other members
        // =============================================================
        private sealed class ParentWithExtra
        {
            public ParentWithExtra()
            {
                Children = new List<ChildSource>();
            }

            public List<ChildSource> Children { get; set; }
            public string Name { get; set; }
        }

        private sealed class ParentWithExtraModel
        {
            public ParentWithExtraModel()
            {
                ChildModels = new List<ChildDest>();
            }

            public List<ChildDest> ChildModels { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void ForMember_MapFromCollection_Does_Not_Break_Other_Members()
        {
            var cfg = new MapperConfiguration();

            cfg.AutoRegister<ChildSource, ChildDest>();
            cfg.AutoRegister<ParentWithExtra, ParentWithExtraModel>()
               .ForMember(
                   d => d.ChildModels,
                   o => o.MapFromCollection(s => s.Children));

            var mapper = cfg.Build();

            var result = mapper.Map<ParentWithExtra, ParentWithExtraModel>(
                new ParentWithExtra
                {
                    Name = "Parent",
                    Children =
                    {
                        new ChildSource { Id = 1, Value = "OK" }
                    }
                });

            Assert.AreEqual("Parent", result.Name);
            Assert.AreEqual(1, result.ChildModels.Count);
        }
    }
}