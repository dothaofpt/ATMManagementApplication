using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using System.Linq;
using System;

namespace ATMManagementApplication.Controllers{
    
    [ApiController]
    [Route("api/atm")]
    public class ATMController : ControllerBase{

        private readonly ATMContext _context;
        public ATMController(ATMContext context){
            _context = context;
        }
        [HttpGet("balance/{customerId}")]
        public IActionResult GetBalance(int customerId){
            var customer = _context.Customers.Find(customerId);
            if(customer == null) return NotFound("Customer not found");

            return Ok(new {balance = customer.Balance});

        }
        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromBody] WithdrawRequest request){
            var customer = _context.Customers.Find(request.CustomerId);
            if(customer==null) 
                return NotFound("Customer not found");

            if(customer.Balance < request.Amount)
                return BadRequest("Insufficient balance");
            
            customer.Balance -= request.Amount; 

            var transaction = new Transaction{
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return Ok(new {message ="Withdraw successful",newBalance = customer.Balance});

        }

    } 
    public class WithdrawRequest{
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }
}