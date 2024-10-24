using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using System.Linq;

namespace ATMManagementApplication.Controllers{
    
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase{
        
        private readonly ATMContext _context;
        public AuthController(ATMContext context){
            _context = context;
            
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Customer login){
            var customer = _context.Customers
                .FirstOrDefault(c => c.Name == login.Name 
                                && c.Password == login.Password);
            if(customer == null){
                return Unauthorized("Invalid credentials");
            }

            return Ok(new {message = "Login successful", customerId = customer.CustomerId});



        }

    }
}
