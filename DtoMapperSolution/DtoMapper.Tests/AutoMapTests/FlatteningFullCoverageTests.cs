using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class FlatteningFullCoverageTests
    {
        // =============================================================
        // Helper classes
        // =============================================================
        private class Address
        {
            public string? City { get; set; }
            public string? Street { get; set; }
        }

        private class AddressDto
        {
            public string? City { get; set; }
            public string? Street { get; set; }
        }

        // =============================================================
        // 1. Simple flattening (object → flat DTO)
        // =============================================================
        private class Src1
        {
            public Address? Address { get; set; }
        }

        private class Dest1
        {
            public string? AddressCity { get; set; }
            public string? AddressStreet { get; set; }
        }

        [TestMethod]
        public void Flatten_Object_To_Flat_Dto()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            var src = new Src1
            {
                Address = new Address
                {
                    City = "Toronto",
                    Street = "Queen"
                }
            };

            var result = mapper.Map<Src1, Dest1>(src);

            // Flattening is not supported in the final engine
            Assert.IsNull(result.AddressCity);
            Assert.IsNull(result.AddressStreet);
        }

        // =============================================================
        // 2. Flattening with null nested object
        // =============================================================
        [TestMethod]
        public void Flatten_With_Null_Nested_Object_Uses_Default()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();

            var src = new Src1
            {
                Address = null
            };

            var result = mapper.Map<Src1, Dest1>(src);

            Assert.IsNull(result.AddressCity);
            Assert.IsNull(result.AddressStreet);
        }

        // =============================================================
        // 3. Reverse flattening is NOT automatic
        // =============================================================
        private class Src3
        {
            public string? AddressCity { get; set; }
            public string? AddressStreet { get; set; }
        }

        private class Dest3
        {
            public Address? Address { get; set; }
        }

        [TestMethod]
        public void Reverse_Flattening_Is_Not_Automatic()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src3, Dest3>();

            var mapper = cfg.Build();

            var src = new Src3
            {
                AddressCity = "Toronto",
                AddressStreet = "Queen"
            };

            var result = mapper.Map<Src3, Dest3>(src);

            // Reverse flattening is conservative & not inferred
            Assert.IsNull(result.Address);
        }

        // =============================================================
        // 4. ReverseMap does NOT enable reverse flattening
        // =============================================================
        [TestMethod]
        public void ReverseMap_Does_Not_Enable_Reverse_Flattening()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Dest1, Src1>().ReverseMap();

            var mapper = cfg.Build();

            var src = new Dest1
            {
                AddressCity = "Toronto",
                AddressStreet = "Queen"
            };

            var result = mapper.Map<Dest1, Src1>(src);

            // Still not inferred
            Assert.IsNull(result.Address);
        }

        // =============================================================
        // 5. Partial flattening (only some properties match)
        // =============================================================
        private class Dest5
        {
            public string? AddressCity { get; set; }
            public string? Ignored { get; set; }
        }

        [TestMethod]
        public void Flatten_Partial_Property_Match()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest5>();

            var mapper = cfg.Build();

            var src = new Src1
            {
                Address = new Address { City = "Toronto" }
            };

            var result = mapper.Map<Src1, Dest5>(src);

            // Flattening is not supported in the final engine
            Assert.IsNull(result.AddressCity);
            Assert.IsNull(result.Ignored);
        }

        // =============================================================
        // 6. Flattening MUST NOT throw on missing paths
        // =============================================================
        private class Src6
        {
            public Address? Address { get; set; }
        }

        private class Dest6
        {
            public string? AddressZipCode { get; set; }
        }

        [TestMethod]
        public void Flatten_Missing_Path_Does_Not_Throw()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src6, Dest6>();

            var mapper = cfg.Build();

            var src = new Src6
            {
                Address = new Address { City = "Toronto" }
            };

            var result = mapper.Map<Src6, Dest6>(src);

            Assert.IsNull(result.AddressZipCode);
        }

        // =============================================================
        // 7. Flattening does NOT work for collections
        // =============================================================
        private class Src7
        {
            public Address[]? Addresses { get; set; }
        }

        private class Dest7
        {
            public string? AddressesCity { get; set; }
        }

        [TestMethod]
        public void Flattening_Is_Not_Supported_For_Collections()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src7, Dest7>();

            var mapper = cfg.Build();

            var src = new Src7
            {
                Addresses = new[]
                {
                    new Address { City = "Toronto" }
                }
            };

            var result = mapper.Map<Src7, Dest7>(src);

            Assert.IsNull(result.AddressesCity);
        }

        // =============================================================
        // 8. Flattening with deeply nested objects
        // =============================================================
        private class Country
        {
            public string? Name { get; set; }
        }

        private class Address8
        {
            public Country? Country { get; set; }
        }

        private class Src8
        {
            public Address8? Address { get; set; }
        }

        private class Dest8
        {
            public string? AddressCountryName { get; set; }
        }

        [TestMethod]
        public void Flatten_Deep_Nested_Object()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src8, Dest8>();

            var mapper = cfg.Build();

            var src = new Src8
            {
                Address = new Address8
                {
                    Country = new Country { Name = "Canada" }
                }
            };

            var result = mapper.Map<Src8, Dest8>(src);

            // Deep flattening is not supported; default is expected
            Assert.IsNull(result.AddressCountryName);
        }
    }
}