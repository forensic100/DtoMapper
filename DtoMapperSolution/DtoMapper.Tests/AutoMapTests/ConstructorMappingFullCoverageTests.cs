using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public class ConstructorMappingFullCoverageTests
    {
        // =============================================================
        // 1. Simple constructor mapping
        // =============================================================
        private class Src1
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        private class Dest1
        {
            public int Id { get; }
            public string? Name { get; }

            public Dest1(int id, string? name)
            {
                Id = id;
                Name = name;
            }
        }

        [TestMethod]
        public void Constructor_Simple_Mapping()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src1, Dest1>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src1, Dest1>(new Src1 { Id = 5, Name = "A" });

            Assert.AreEqual(5, result.Id);
            Assert.AreEqual("A", result.Name);
        }

        // =============================================================
        // 2. Constructor with reordered parameters
        // =============================================================
        private class Src2
        {
            public string? Name { get; set; }
            public int Age { get; set; }
        }

        private class Dest2
        {
            public int Age { get; }
            public string? Name { get; }

            public Dest2(int age, string? name)
            {
                Age = age;
                Name = name;
            }
        }

        [TestMethod]
        public void Constructor_Parameter_Order_Does_Not_Matter()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src2, Dest2>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src2, Dest2>(new Src2 { Name = "Bob", Age = 30 });

            Assert.AreEqual("Bob", result.Name);
            Assert.AreEqual(30, result.Age);
        }

        // =============================================================
        // 3. Fallback to parameterless constructor
        // =============================================================
        private class Src3
        {
            public int X { get; set; }
        }

        private class Dest3
        {
            public int X { get; set; }
            public Dest3() { }
        }

        [TestMethod]
        public void Parameterless_Constructor_Fallback()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src3, Dest3>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src3, Dest3>(new Src3 { X = 7 });

            Assert.AreEqual(7, result.X);
        }

        // =============================================================
        // 4. Constructor + additional property mapping
        // =============================================================
        private class Src4
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Extra { get; set; }
        }

        private class Dest4
        {
            public int Id { get; }
            public string? Name { get; }
            public string? Extra { get; set; }

            public Dest4(int id, string? name)
            {
                Id = id;
                Name = name;
            }
        }

        [TestMethod]
        public void Constructor_And_Property_Mapping_Together()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src4, Dest4>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src4, Dest4>(
                new Src4 { Id = 9, Name = "Z", Extra = "E" });

            Assert.AreEqual(9, result.Id);
            Assert.AreEqual("Z", result.Name);
            Assert.AreEqual("E", result.Extra);
        }

        // =============================================================
        // 5. Nested constructor argument mapping
        // =============================================================
        private class InnerSrc
        {
            public string? Code { get; set; }
        }

        private class InnerDest
        {
            public string? Code { get; }
            public InnerDest(string? code)
            {
                Code = code;
            }
        }

        private class Src5
        {
            public InnerSrc? Inner { get; set; }
        }

        private class Dest5
        {
            public InnerDest? Inner { get; }
            public Dest5(InnerDest? inner)
            {
                Inner = inner;
            }
        }

        [TestMethod]
        public void Constructor_Nested_Object_Mapping()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<InnerSrc, InnerDest>();
            cfg.AutoRegister<Src5, Dest5>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src5, Dest5>(
                new Src5 { Inner = new InnerSrc { Code = "X1" } });

            Assert.IsNotNull(result.Inner);
            Assert.AreEqual("X1", result.Inner!.Code);
        }

        // =============================================================
        // 6. Constructor mapping with value types
        // =============================================================
        private class Src6
        {
            public int A { get; set; }
            public int B { get; set; }
        }

        private class Dest6
        {
            public int Sum { get; }
            public Dest6(int a, int b)
            {
                Sum = a + b;
            }
        }

        [TestMethod]
        public void Constructor_Value_Type_Arguments()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src6, Dest6>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src6, Dest6>(new Src6 { A = 3, B = 4 });

            Assert.AreEqual(7, result.Sum);
        }

        // =============================================================
        // 7. Constructor does not interfere with array mapping
        // =============================================================
        private class Src7
        {
            public int[]? Values { get; set; }
        }

        private class Dest7
        {
            public int[]? Values { get; }

            public Dest7(int[]? values)
            {
                Values = values;
            }
        }

        [TestMethod]
        public void Constructor_Array_Argument_Mapping()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Src7, Dest7>();

            var mapper = cfg.Build();
            var result = mapper.Map<Src7, Dest7>(
                new Src7 { Values = new[] { 1, 2, 3 } });

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result.Values!);
        }
    }
}