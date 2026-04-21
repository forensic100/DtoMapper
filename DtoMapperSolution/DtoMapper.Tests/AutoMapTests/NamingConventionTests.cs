using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public sealed class NamingConventionTests
    {
        // =============================================================
        // Test models
        // =============================================================
        private sealed class SrcPascal
        {
            public string? FirstName { get; set; }
            public int Age { get; set; }
        }

        private sealed class DestPascal
        {
            public string? FirstName { get; set; }
            public int Age { get; set; }
        }

        private sealed class DestDifferentCase
        {
            public string? firstname { get; set; }
            public int age { get; set; }
        }

        // Nested structure used to guard against flattening
        private sealed class SrcNested
        {
            public Person? Person { get; set; }
        }

        private sealed class Person
        {
            public string? Name { get; set; }
        }

        private sealed class DestFlat
        {
            public string? PersonName { get; set; }
        }

        // =============================================================
        // 1. Direct property names map with default naming convention
        // =============================================================
        [TestMethod]
        public void NamingConvention_Maps_Direct_Property_Names()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcPascal, DestPascal>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcPascal, DestPascal>(
                new SrcPascal
                {
                    FirstName = "Alice",
                    Age = 30
                });

            Assert.AreEqual("Alice", result.FirstName);
            Assert.AreEqual(30, result.Age);
        }

        // =============================================================
        // 2. Naming convention is CASE-INSENSITIVE by default
        // =============================================================
        [TestMethod]
        public void NamingConvention_Is_Case_Insensitive_By_Default()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcPascal, DestDifferentCase>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcPascal, DestDifferentCase>(
                new SrcPascal
                {
                    FirstName = "Alice",
                    Age = 30
                });

            // Case differences do NOT block mapping
            Assert.AreEqual("Alice", result.firstname);
            Assert.AreEqual(30, result.age);
        }

        // =============================================================
        // 3. Naming convention does NOT flatten nested objects
        // =============================================================
        [TestMethod]
        public void NamingConvention_Does_Not_Flatten_Nested_Properties()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcNested, DestFlat>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcNested, DestFlat>(
                new SrcNested
                {
                    Person = new Person
                    {
                        Name = "Alice"
                    }
                });

            // No flattening: Person.Name is NOT mapped to PersonName
            Assert.IsNull(result.PersonName);
        }

        // =============================================================
        // 4. Naming convention does NOT create nested objects
        // =============================================================
        private sealed class SrcFlat
        {
            public string? Name { get; set; }
        }

        private sealed class DestNested
        {
            public Person? Person { get; set; }
        }

        [TestMethod]
        public void NamingConvention_Does_Not_Create_Nested_Objects()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcFlat, DestNested>();

            var mapper = cfg.Build();

            var result = mapper.Map<SrcFlat, DestNested>(
                new SrcFlat
                {
                    Name = "Alice"
                });

            // Naming convention never creates object graphs
            Assert.IsNull(result.Person);
        }

        // =============================================================
        // 5. Naming convention never throws
        // =============================================================
        [TestMethod]
        public void NamingConvention_Never_Throws()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<SrcNested, DestFlat>();

            var mapper = cfg.Build();

            try
            {
                var result = mapper.Map<SrcNested, DestFlat>(
                    new SrcNested
                    {
                        Person = null
                    });

                Assert.IsNotNull(result);
            }
            catch
            {
                Assert.Fail("Naming convention mapping must not throw.");
            }
        }
    }
}