
using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using System;
using System.Linq;
using System.Net.Mail;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

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

        // API để đăng ký người dùng mới
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            var existingCustomer = _context.Customers.FirstOrDefault(c => c.Name == request.Name);
            if (existingCustomer != null)
            {
                return BadRequest("Customer already exists.");
            }

            // Băm mật khẩu
            string hashedPassword = HashPassword(request.Password);

            // Tạo người dùng mới
            var newCustomer = new Customer
            {
                Name = request.Name,
                Password = hashedPassword,
                Balance = 0 // Mặc định số dư là 0
            };

            _context.Customers.Add(newCustomer);
            _context.SaveChanges();

            return Ok(new { message = "Registration successful", customerId = newCustomer.CustomerId });
        }

        // API để đăng nhập
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Name == request.Name);

            if (customer == null || !VerifyPassword(customer.Password, request.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new { message = "Login successful", customerId = customer.CustomerId });
        }

        // API để thay đổi mật khẩu
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var customer = _context.Customers.Find(request.CustomerId);

            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            if (!VerifyPassword(customer.Password, request.OldPassword))
            {
                return Unauthorized("Old password is incorrect.");
            }

            // Băm mật khẩu mới
            string newHashedPassword = HashPassword(request.NewPassword);
            customer.Password = newHashedPassword;
            _context.SaveChanges();

            return Ok(new { message = "Password changed successfully." });
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

            // Gửi email thông báo giao dịch
            SendEmail(customer.Name, "Withdraw", request.Amount);

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

            var transaction = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            // Gửi email thông báo giao dịch
            SendEmail(customer.Name, "Deposit", request.Amount);

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

        // API để xem lịch sử giao dịch của người dùng
        [HttpGet("transactions/{customerId}")]
        public IActionResult TransactionsHistory(int customerId)
        {
            var transactionHistory = _context.Transactions
                .Where(t => t.CustomerId == customerId)
                .ToList();

            if (transactionHistory == null || transactionHistory.Count == 0)
            {
                return NotFound("No transactions found for this customer.");
            }

            return Ok(transactionHistory);
        }

        // API để chuyển tiền giữa các tài khoản
        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TransferRequest request)
        {
            var sender = _context.Customers.Find(request.SenderId);
            var receiver = _context.Customers.Find(request.ReceiverId);

            if (sender == null || receiver == null)
            {
                return NotFound("Sender or receiver not found.");
            }

            if (sender.Balance < request.Amount)
            {
                return BadRequest("Sender has insufficient balance.");
            }

            sender.Balance -= request.Amount;
            receiver.Balance += request.Amount;

            var senderTransaction = new Transaction
            {
                CustomerId = sender.CustomerId,
                Amount = -request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

            var receiverTransaction = new Transaction
            {
                CustomerId = receiver.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };

            _context.Transactions.Add(senderTransaction);
            _context.Transactions.Add(receiverTransaction);
            _context.SaveChanges();

            // Gửi email thông báo giao dịch
            SendEmail(sender.Name, "Transfer Out", request.Amount);
            SendEmail(receiver.Name, "Transfer In", request.Amount);

            return Ok(new
            {
                message = "Transfer successful",
                senderId = sender.CustomerId,
                receiverId = receiver.CustomerId,
                transferredAmount = request.Amount
            });
        }

        // Phương thức hỗ trợ băm mật khẩu
        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }

        // Phương thức hỗ trợ xác thực mật khẩu
        private bool VerifyPassword(string hashedPassword, string enteredPassword)
        {
            return hashedPassword == HashPassword(enteredPassword);
        }

        // Phương thức gửi email thông báo giao dịch
        private void SendEmail(string customerName, string transactionType, decimal amount)
{
    try
    {
        MailMessage mail = new MailMessage();
        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

        mail.From = new MailAddress("hkpojjj@gmail.com");
        mail.To.Add("dtth2304010@fpt.edu.vn"); // Địa chỉ email đúng
        mail.Subject = $"{transactionType} Notification";
        mail.Body = $"Dear {customerName},\n\nYour account has a {transactionType} of {amount:C}.\n\nThank you for using our service.";

        SmtpServer.Port = 587;
        SmtpServer.Credentials = new System.Net.NetworkCredential("hkpojjj@gmail.com", "thao2005");
        SmtpServer.EnableSsl = true;

        SmtpServer.Send(mail);
        Console.WriteLine("Email sent successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to send email: " + ex.Message);
    }
}


        // Lớp cho yêu cầu đăng ký
        public class RegisterRequest
        {
            public string Name { get; set; }
            public string Password { get; set; }
        }

        // Lớp cho yêu cầu đăng nhập
        public class LoginRequest
        {
            public string Name { get; set; }
            public string Password { get; set; }
        }

        // Lớp cho yêu cầu thay đổi mật khẩu
        public class ChangePasswordRequest
        {
            public int CustomerId { get; set; }
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }

        // Lớp cho yêu cầu rút tiền
        public class WithdrawRequest
        {
            public int CustomerId { get; set; }
            public decimal Amount { get; set; }
        }

        // Lớp cho yêu cầu nạp tiền
        public class DepositRequest
        {
            public int CustomerId { get; set; }
            public decimal Amount { get; set; }
        }

        // Lớp cho yêu cầu chuyển tiền
        public class TransferRequest
        {
            public int SenderId { get; set; }
            public int ReceiverId { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
