using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualCashCard
{
   public class CashCard
    {
        private decimal  balance ;
        private  readonly IPinValidatorService pinValidator;
        private readonly object locker = new object();
        private readonly string cardNumber;

        public CashCard (IPinValidatorService pinValidator, string cardNumber )
        {
            this.pinValidator = pinValidator;
            this.cardNumber = cardNumber;
        }

        public decimal Balance
        {
            get
            {
                lock (locker)
                {
                    return balance;
                }
            }
        }

        public async Task<bool> TopupBalance(int pin, decimal amount)
        {
          
            var isPinValid = await pinValidator.ValidatePin(this.cardNumber,pin);
            

            if (isPinValid != true) return false;

            if (amount <= 0.00M) return false;

            if (amount.Equals(decimal.MaxValue)) return false;
            
            lock (locker)
            {
                this.balance= decimal.Add ( this.balance, amount);
                return true;
            }
        }


        public async  Task<bool> Withdraw(int pin, decimal amount)
        {

            var isPinValid = await pinValidator.ValidatePin(this.cardNumber, pin);
            
            if (isPinValid != true) return false;            

            if (amount <= 0.00M) return false;

            lock (locker)
            {
                if (decimal.Subtract(balance, amount) <= 0.001M) return false;
                balance = decimal.Subtract(balance, amount);
                return true;
            }
        }       


    }
}
