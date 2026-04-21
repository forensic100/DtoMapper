using DtoMapper.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DtoMapper.Tests.AutoMapTests
{
    [TestClass]
    public sealed class ReverseFlatteningTests
    {
        // =============================================================
        // Test models
        // =============================================================
        private sealed class Customer
        {
            public string? Name { get; set; }
        }

        private sealed class Order
        {
            public Customer? Customer { get; set; }
        }

        private sealed class OrderFlat
        {
            public string? CustomerName { get; set; }
        }

        // =============================================================
        // 1. Forward flattening is NOT supported
        // =============================================================
        [TestMethod]
        public void Forward_Flattening_Is_Not_Supported()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Order, OrderFlat>();

            var mapper = cfg.Build();

            var result = mapper.Map<Order, OrderFlat>(
                new Order
                {
                    Customer = new Customer
                    {
                        Name = "Alice"
                    }
                });

            // Mapper does not flatten Customer.Name → CustomerName
            Assert.IsNull(result.CustomerName);
        }

        // =============================================================
        // 2. Reverse flattening does NOT create nested objects
        // =============================================================
        [TestMethod]
        public void Reverse_Flattening_Does_Not_Create_Nested_Object()
        {
            var cfg = new MapperConfiguration();
            cfg.AutoRegister<Order, OrderFlat>()
               .ReverseMap();

            var mapper = cfg.Build();

            var result = mapper.Map<OrderFlat, Order>(
                new OrderFlat
                {
                    CustomerName = "Alice"
                });

            // Nested objects are never auto-created
            Assert.IsNull(result.Customer);
        }
    }
}