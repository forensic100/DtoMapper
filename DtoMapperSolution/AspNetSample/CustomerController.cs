using DtoMapper.Core;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSample;

[ApiController]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly Mapper _mapper;

    public CustomerController(Mapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public ActionResult<CustomerDto> Get(int id)
    {
        var customer = new Customer
        {
            Id = id,
            Name = "Alice"
        };

        var dto = _mapper.Map<Customer, CustomerDto>(customer);
        return Ok(dto);
    }
}