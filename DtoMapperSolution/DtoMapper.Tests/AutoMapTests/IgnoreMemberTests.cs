using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public sealed class IgnoreMemberTests
    {
        private sealed class Source
        {
            public string Name { get; set; }
            public int InternalId { get; set; }
        }

        private sealed class Destination
        {
            public string Name { get; set; }
            public int InternalId { get; set; }
        }

        // =============================================================
        // 1. Ignored property is not mapped
        // =============================================================
        [TestMethod]
        public void IgnoreMember_Does_Not_Map_Destination_Property()
        {
            var cfg = new MapperConfiguration();

            cfg.AutoRegister<Source, Destination>()
               .ForMember(d => d.InternalId, o => o.Ignore());

            var mapper = cfg.Build();

            var result = mapper.Map<Source, Destination>(
                new Source
                {
                    Name = "John",
                    InternalId = 42
                });

            Assert.AreEqual("John", result.Name);
            Assert.AreEqual(0, result.InternalId); // default(int)
        }

        // =============================================================
        // 2. Ignored property does not interfere with others
        // =============================================================
        [TestMethod]
        public void IgnoreMember_Does_Not_Affect_Other_Members()
        {
            var cfg = new MapperConfiguration();

            cfg.AutoRegister<Source, Destination>()
               .ForMember(d => d.InternalId, o => o.Ignore());

            var mapper = cfg.Build();

            var result = mapper.Map<Source, Destination>(
                new Source
                {
                    Name = "Alice",
                    InternalId = 100
                });

            Assert.AreEqual("Alice", result.Name);
        }

        // =============================================================
        // 3. Ignore only applies in one direction
        // =============================================================
        [TestMethod]
        public void IgnoreMember_Is_Not_Implicitly_Reversed()
        {
            var cfg = new MapperConfiguration();

            cfg.AutoRegister<Source, Destination>()
               .ForMember(d => d.InternalId, o => o.Ignore())
               .ReverseMap();

            var mapper = cfg.Build();

            var dest = mapper.Map<Source, Destination>(
                new Source { Name = "Bob", InternalId = 7 });

            var back = mapper.Map<Destination, Source>(dest);

            Assert.AreEqual("Bob", back.Name);
            Assert.AreEqual(0, dest.InternalId);
            Assert.AreEqual(0, back.InternalId);
        }
    }
}