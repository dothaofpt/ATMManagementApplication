
using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using System;

namespace ATMManagementApplication.Controllers
{
    [ApiController]
    [Route("api/atm")]
    public class ATMController : ControllerBase
    {
        private readonly ATMContext _context;

        public ATMController(ATMContext context)
        {
            _context = context;
        }

        // API để lấy số dư tài khoản của khách hàng theo ID
        [HttpGet("balance/{customerId}")]
        public IActionResult GetBalance(int customerId)
        {
            var customer = _context.Customers.Find(customerId);

            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            return Ok(new { balance = customer.Balance });
        }

        // API để thực hiện rút tiền
        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromBody] WithdrawRequest request)
        {
            var customer = _context.Customers.Find(request.CustomerId);

            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            if (customer.Balance < request.Amount)
            {
                return BadRequest("Insufficient balance");
            }

            // Trừ số tiền rút từ số dư hiện tại
            customer.Balance -= request.Amount;

            // Tạo một giao dịch
            var transaction = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

           
            _context.Transactions.Add(transaction);
            _context.SaveChanges();

           
            return Ok(new { message = "Withdraw successful" });
        }

        // API để thực hiện nạp tiền
        [HttpPost("deposit")]
        public IActionResult Deposit([FromBody] DepositRequest request)
        {
            var customer = _context.Customers.Find(request.CustomerId);

            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            
            customer.Balance += request.Amount;

            // Tạo một giao dịch nạp tiền
            var transaction = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

            // Thêm giao dịch vào cơ sở dữ liệu
            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            // Trả về thông báo thành công với thông tin chi tiết
            return Ok(new 
            { 
                message = "Deposit successful",
                customerName = customer.Name,
                customerId = customer.CustomerId,
                amountDeposited = request.Amount,
                newBalance = customer.Balance,
                transactionId = transaction.TransactionId,
                transactionTimestamp = transaction.Timestamp 
            });
        }
    }

    // Lớp để nhận thông tin rút tiền
    public class WithdrawRequest
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }

    // Lớp để nhận thông tin nạp tiền
    public class DepositRequest
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }
}
