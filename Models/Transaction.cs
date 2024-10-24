
using System;
using System.ComponentModel.DataAnnotations;

namespace ATMManagementApplication.Models {
    public class Transaction {
        [Key] // Nếu TransactionId là primary key
        public int TransactionId { get; set; }

        public int CustomerId { get; set; } // Thêm CustomerId để liên kết với Customer

        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsSuccessful { get; set; }

        public int? TransferTo { get; set; } // Đúng, nếu có thể không có giá trị
    }
}
