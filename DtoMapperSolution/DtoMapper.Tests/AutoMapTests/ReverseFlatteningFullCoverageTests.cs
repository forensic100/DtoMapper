using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class ReverseFlatteningFullCoverageTests
    {
        // =============================================================
        // Helper types
        // =============================================================
        private class Address
        {
            public string? City { get; set; }
            public string? Street { get; set; }
        }

        // =============================================================
        // 1. Reverse flattening is NOT inferred
        // =============================================================
        private class FlatSrc1
        {
            public string? AddressCity { get; set; }
            public string? AddressStreet { get; set; }
        }

        private class NestedDest1
        {
            public Address? Address { get; set; }
        }

        [TestMethod]
        public void Reverse_Flattening_Is_Not_Inferred()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<FlatSrc1, NestedDest1>();

            var mapper = cfg.Build();

            var src = new FlatSrc1
            {
                AddressCity = "Toronto",
                AddressStreet = "Queen"
            };

            var result = mapper.Map<FlatSrc1, NestedDest1>(src);

            // Reverse flattening is not supported
            Assert.IsNull(result.Address);
        }

        // =============================================================
        // 2. ReverseMap does NOT enable reverse flattening
        // =============================================================
        [TestMethod]
        public void ReverseMap_Does_Not_Enable_Reverse_Flattening()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<FlatSrc1, NestedDest1>()
               .ReverseMap();

            var mapper = cfg.Build();

            var src = new FlatSrc1
            {
                AddressCity = "Toronto",
                AddressStreet = "Queen"
            };

            var result = mapper.Map<FlatSrc1, NestedDest1>(src);

            // Still not inferred, even with ReverseMap
            Assert.IsNull(result.Address);
        }

        // =============================================================
        // 3. Reverse flattening never throws
        // =============================================================
        [TestMethod]
        public void Reverse_Flattening_Does_Not_Throw()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<FlatSrc1, NestedDest1>();

            var mapper = cfg.Build();

            var src = new FlatSrc1
            {
                AddressCity = "Toronto",
                AddressStreet = "Queen"
            };

            NestedDest1 result = null!;

            try
            {
                result = mapper.Map<FlatSrc1, NestedDest1>(src);
            }
            catch
            {
                Assert.Fail("Reverse flattening must not throw.");
            }

            Assert.IsNotNull(result);
            Assert.IsNull(result.Address);
        }

        // =============================================================
        // 4. Reverse flattening with partial data still defaults
        // =============================================================
        private class FlatSrc4
        {
            public string? AddressCity { get; set; }
        }

        private class NestedDest4
        {
            public Address? Address { get; set; }
        }

        [TestMethod]
        public void Reverse_Flattening_With_Partial_Data_Defaults()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<FlatSrc4, NestedDest4>();

            var mapper = cfg.Build();

            var src = new FlatSrc4
            {
                AddressCity = "Toronto"
            };

            var result = mapper.Map<FlatSrc4, NestedDest4>(src);

            // No inference even with partial data
            Assert.IsNull(result.Address);
        }

        // =============================================================
        // 5. Reverse flattening does not create nested objects
        // =============================================================
        private class FlatSrc5
        {
            public string? AddressCity { get; set; }
            public string? AddressStreet { get; set; }
            public string? Other { get; set; }
        }

        private class NestedDest5
        {
            public Address? Address { get; set; }
            public string? Other { get; set; }
        }

        [TestMethod]
        public void Reverse_Flattening_Does_Not_Create_Nested_Object()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<FlatSrc5, NestedDest5>();

            var mapper = cfg.Build();

            var src = new FlatSrc5
            {
                AddressCity = "Toronto",
                AddressStreet = "Queen",
                Other = "X"
            };

            var result = mapper.Map<FlatSrc5, NestedDest5>(src);

            // Other direct mapping works
            Assert.AreEqual("X", result.Other);

            // Nested object is NOT inferred
            Assert.IsNull(result.Address);
        }
    }
}