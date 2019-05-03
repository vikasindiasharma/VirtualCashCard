using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualCashCard
{
  public  interface IPinValidatorService
    {
        Task<bool> ValidatePin(string cardNumber,int pin);
    }
}
