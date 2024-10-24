
using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using System.Linq;
using System;
using System.Net;
using System.Net.Mail; // Thêm using cho gửi email
using Microsoft.Extensions.Options; // Để sử dụng IOptions

namespace ATMManagementApplication.Controllers
{
    [ApiController]
    [Route("api/atm")]
    public class ATMController : ControllerBase
    {
        private readonly ATMContext _context;
        private readonly EmailSettings _emailSettings; // Thêm EmailSettings

        public ATMController(ATMContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings.Value; // Lấy cấu hình email
        }

        [HttpGet("balance/{customerId}")]
        public IActionResult GetBalance(int customerId)
        {
            var customer = _context.Customers.Find(customerId);
            if (customer == null) return NotFound("Customer not found");

            return Ok(new { balance = customer.Balance });
        }

        [HttpGet("customers")]  // Thêm phương thức này
        public IActionResult GetAllCustomers()
        {
            var customers = _context.Customers.ToList();
            if (customers == null || customers.Count == 0)
                return NotFound("No customers found");

            return Ok(customers);
        }

        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromBody] WithdrawRequest request)
        {
            var customer = _context.Customers.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");

            if (customer.Balance < request.Amount)
                return BadRequest("Insufficient balance");

            customer.Balance -= request.Amount;

            var transaction = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return Ok(new { message = "Withdraw successful", newBalance = customer.Balance });
        }

        [HttpPost("deposit")]
        public IActionResult Deposit([FromBody] DepositRequest request)
        {
            var customer = _context.Customers.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");

            customer.Balance += request.Amount;

            var transaction = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return Ok(new { message = "Deposit successful", newBalance = customer.Balance });
        }

        [HttpGet("transactions/{customerId}")]
        public IActionResult GetTransactionHistory(int customerId)
        {
            var transactions = _context.Transactions
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.Timestamp)
                .ToList();

            if (transactions == null || transactions.Count == 0)
                return NotFound("No transactions found for this customer");

            return Ok(transactions);
        }

        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TransferRequest request)
        {
            var sender = _context.Customers.Find(request.SenderId);
            var receiver = _context.Customers.Find(request.ReceiverId);

            if (sender == null || receiver == null)
                return NotFound("Sender or Receiver not found");

            if (sender.Balance < request.Amount)
                return BadRequest("Insufficient balance");

            sender.Balance -= request.Amount;
            receiver.Balance += request.Amount;

            var transaction = new Transaction
            {
                CustomerId = request.SenderId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true,
                TransferTo = request.ReceiverId
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            // Gửi thông báo qua email
            if (string.IsNullOrEmpty(receiver.Email))
                return BadRequest("Receiver email is not available"); // Kiểm tra email của người nhận

            string subject = "Transfer Successful";
            string body = $"Dear {receiver.Name},\n\n" +
                          $"You have received {request.Amount} from {sender.Name}.\n" +
                          $"Your new balance is {receiver.Balance}.\n\n" +
                          "Thank you for using our service.";

            // Gửi email thông báo đến người nhận
            try
            {
                SendEmail(receiver.Email, subject, body);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi gửi email (ghi log, v.v.)
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }

            return Ok(new { message = "Transfer successful", senderNewBalance = sender.Balance });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_context.Customers.Any(c => c.Name == request.Name))
                return BadRequest("User already exists");

            var newCustomer = new Customer
            {
                Name = request.Name,
                Password = request.Password,
                Balance = 0,
                Email = request.Email // Thêm trường Email
            };

            _context.Customers.Add(newCustomer);
            _context.SaveChanges();

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.Name == request.Name && c.Password == request.Password);

            if (customer == null)
                return Unauthorized("Invalid credentials");

            return Ok(new { message = "Login successful", customerId = customer.CustomerId });
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var customer = _context.Customers.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");

            if (customer.Password != request.OldPassword)
                return BadRequest("Old password is incorrect");

            customer.Password = request.NewPassword;
            _context.SaveChanges();

            return Ok(new { message = "Password changed successfully" });
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587)) // Sử dụng smtp.gmail.com
            {
                client.EnableSsl = true; // Bật SSL
                client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, "idbgvmgqahyiuoaz"); // Lấy thông tin từ cấu hình
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail); // Đây là email người nhận
                client.Send(mailMessage);
            }
        }
    }

    public class EmailSettings // Lớp để cấu hình email
    {
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
    }

    public class WithdrawRequest
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }

    public class DepositRequest
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }

    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; } // Thêm trường Email
    }

    public class LoginRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int CustomerId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class TransferRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }
    }
}
