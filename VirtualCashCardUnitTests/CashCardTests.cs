using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualCashCard;

namespace VirtualCashCardUnitTests
{
    [TestFixture]
    public class CashCardTests
    {

        private static readonly int MOCK_PIN = 5698;
        private static readonly string MOCK_CARDNUMBER = "1111";

        [Test]
        public void NewCashCardBalanceStartFromZero()
        {
            int invalidPin = MOCK_PIN + 1;
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);

            var prebalance = cashCard.Balance;

            Assert.AreEqual(prebalance, 0.00M);
        }

        [Test]
        public async Task TopUpFailOnZeroDecimalAmount()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(MOCK_PIN, 0M);
            Assert.That(result, Is.False);
            Assert.AreEqual(0M, cashCard.Balance);
        }

        [Test]
        public async Task TopUpFailOnNegativeDecimalAmount()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(MOCK_PIN, -1.0M);
            Assert.That(result, Is.False);
            Assert.AreEqual(0M, cashCard.Balance);
        }

        [Test]
        public async Task TopUpFailOnMaxDecimalAmount()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(MOCK_PIN, decimal.MaxValue);
            Assert.That(result, Is.False);
            Assert.AreEqual(prebalance, cashCard.Balance);
        }

        [Test]
        public async Task TopupFailOnFailedPinVerification()
        {
            int invalidPin = MOCK_PIN + 900;
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(invalidPin, 200M);
            Assert.That(result, Is.False);
            Assert.AreEqual(prebalance, cashCard.Balance);
        }

        [TestCase(200),
         TestCase(230),
         TestCase(250),
         TestCase(1000)]
        public async Task CanTopupArbitraryAmount(decimal amount)
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(MOCK_PIN, amount);
            Assert.That(result, Is.True);
            Assert.AreEqual(decimal.Add(prebalance, amount), cashCard.Balance);
        }

        [Test]
        public void CanTopupFromMultiplePlacesSameTime()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;

            Task task1 = Task.Factory.StartNew(async () => { await cashCard.TopupBalance(MOCK_PIN, 500M); });
            Task task2 = Task.Factory.StartNew(async () => { await cashCard.TopupBalance(MOCK_PIN, 800M); });
            Task task3 = Task.Factory.StartNew(async () => { await cashCard.TopupBalance(MOCK_PIN, 900M); });

            var task = Task.Factory.ContinueWhenAll(
                new[] { task1, task2, task3 },
                innerTasks =>
                {
                    foreach (var innerTask in innerTasks)
                    {
                        Assert.That(innerTask.IsFaulted, Is.False);
                    }
                    Assert.AreEqual(cashCard.Balance, decimal.Add(prebalance, 2300M));
                });

        }

        [Test]
        public async Task WithdrawFailOnZeroDecimalAmount()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.Withdraw(MOCK_PIN, 0M);
            Assert.That(result, Is.False);
            Assert.AreEqual(0M, cashCard.Balance);
        }

        [Test]
        public async Task WithdrawFailOnNegativeDecimalAmount()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.Withdraw(MOCK_PIN, -1M);
            Assert.That(result, Is.False);
            Assert.AreEqual(0M, cashCard.Balance);
        }

        [Test]
        public async Task WithdrawFailOnPinVerificationFailure()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(MOCK_PIN, 200M);
            Assert.That(result, Is.True);
            Assert.AreEqual(decimal.Add(prebalance, 200M), cashCard.Balance);
            prebalance = cashCard.Balance;

            int invalidPin = MOCK_PIN + 900;
            result = await cashCard.Withdraw(invalidPin, 100M);
            Assert.That(result, Is.False);
            Assert.AreEqual(prebalance, cashCard.Balance);
        }

        [Test]
        public async Task WithdrawFailWhenInSufficientBalance()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(MOCK_PIN, 200M);
            Assert.That(result, Is.True);
            Assert.AreEqual(decimal.Add(prebalance, 200M), cashCard.Balance);
            prebalance = cashCard.Balance;
            result = await cashCard.Withdraw(MOCK_PIN, 400M);
            Assert.That(result, Is.False);
            Assert.AreEqual(prebalance, cashCard.Balance);
        }

        [Test]
        public async Task CanWithdrawAmountWhenSufficientBalance()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            var result = await cashCard.TopupBalance(MOCK_PIN, 200M);
            Assert.That(result, Is.True);
            Assert.AreEqual(decimal.Add(prebalance, 200M), cashCard.Balance);
            prebalance = cashCard.Balance;
            result = await cashCard.Withdraw(MOCK_PIN, 100M);
            Assert.That(result, Is.True);
            Assert.AreEqual(decimal.Subtract(prebalance, 100M), cashCard.Balance);
        }


        [Test]
        public async Task CanWithDrawFromMultiplePlacesSameTime()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            await cashCard.TopupBalance(MOCK_PIN, 2300M);
            Assert.AreEqual(cashCard.Balance, decimal.Add(prebalance, 2300M));


            Task task1 = Task.Factory.StartNew(async () => { await cashCard.Withdraw(MOCK_PIN, 500M); });
            Task task2 = Task.Factory.StartNew(async () => { await cashCard.Withdraw(MOCK_PIN, 200M); });
            Task task3 = Task.Factory.StartNew(async () => { await cashCard.Withdraw(MOCK_PIN, 400M); });

            var task = Task.Factory.ContinueWhenAll(
                new[] { task1, task2, task3 },
                innerTasks =>
                {
                    foreach (var innerTask in innerTasks)
                        Assert.That(innerTask.IsFaulted, Is.False);
                    Assert.AreEqual(cashCard.Balance, decimal.Subtract(prebalance, 1100M));
                });
        }


        [Test]
        public async Task CanWithDrawAndTopupFromMultiplePlacesSameTime()
        {
            var cashCard = new CashCard(GetMockValidator(), MOCK_CARDNUMBER);
            var prebalance = cashCard.Balance;
            await cashCard.TopupBalance(MOCK_PIN, 2300M);
            Assert.AreEqual(cashCard.Balance, decimal.Add(prebalance, 2300M));

            List<Task> tasksArray = new List<Task>();

            int totalCounter = 100;
            for (int index = 1; index <= totalCounter; index++)
            {
                if (index % 5 == 0)
                {
                    tasksArray.Add(Task.Factory.StartNew(async () => { await cashCard.Withdraw(MOCK_PIN, 500M); }));
                }
                else
                {
                    tasksArray.Add(Task.Factory.StartNew(async () => { await cashCard.TopupBalance(MOCK_PIN, 200M); }));
                }
            }            

            var task = Task.Factory.ContinueWhenAll(tasksArray.ToArray(),
                innerTasks =>
                {
                    foreach (var innerTask in innerTasks)
                        Assert.That(innerTask.IsFaulted, Is.False);
                    Assert.AreEqual(cashCard.Balance, 6000M);
                });

        }

        private IPinValidatorService GetMockValidator()
        {
            Mock<IPinValidatorService> mockPinValidator = new Mock<IPinValidatorService>();
            mockPinValidator.Setup(f => f.ValidatePin(It.IsAny<String>(), It.IsAny<int>()))
               .Returns<string, int>(async (card, pin) => { return pin == MOCK_PIN ? true : false; });

            return mockPinValidator.Object;
        }
    }
}