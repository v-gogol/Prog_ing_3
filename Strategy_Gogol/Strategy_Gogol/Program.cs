using System;

namespace Strategy_Gogol
{
    public abstract class PaymentMethod
    {
        public abstract void ExecutePayment(decimal sum);
    }

    public class CardPayment : PaymentMethod
    {
        public override void ExecutePayment(decimal sum)
        {
            Console.WriteLine($"Проводим оплату картой на {sum} рублей");
            Console.WriteLine("Средства списываются с карточки...");
            Console.WriteLine("Платеж прошел успешно!\n");
        }
    }

    public class CashPaymentMethod : PaymentMethod
    {
        public override void ExecutePayment(decimal sum)
        {
            Console.WriteLine($"Принимаем наличные: {sum} рублей");
            Console.WriteLine("Ожидаем передачи денежных средств...");
            Console.WriteLine("Наличные получены, спасибо!\n");
        }
    }

    public class CourierPayment : PaymentMethod
    {
        public override void ExecutePayment(decimal sum)
        {
            Console.WriteLine($"Оформляем доставку с оплатой курьеру на {sum} рублей");
            Console.WriteLine("Товар передается курьерской службе для оплаты при получении");
            Console.WriteLine("Клиент расплатится при вручении заказа\n");
        }
    }

    public class ShopOrder
    {
        private PaymentMethod chosenPayment;
        public string Number { get; }
        public decimal Price { get; }
        public string Buyer { get; }

        public ShopOrder(string number, decimal price, string buyer)
        {
            Number = number;
            Price = price;
            Buyer = buyer;
        }

        public void ChoosePayment(PaymentMethod payment)
        {
            chosenPayment = payment;
            Console.WriteLine($"Для заказа {Number} выбран вариант оплаты: {payment.GetType().Name}");
        }

        public void PayForOrder()
        {
            if (chosenPayment == null)
            {
                Console.WriteLine("Ошибка: способ оплаты не выбран!");
                return;
            }

            Console.WriteLine($"\nОбрабатываем заказ №{Number}");
            Console.WriteLine($"Покупатель: {Buyer}");
            Console.WriteLine($"Сумма к оплате: {Price}");

            chosenPayment.ExecutePayment(Price);

            Console.WriteLine($"Заказ №{Number} успешно оплачен!");
        }
    }

    /*
       
        {КАК ДОБАВИТЬ НОВЫЙ СПОСОБ ОПЛАТЫ}
        
        
        ШАГ 1: Создать новый класс для способа оплаты
           - Создайте новый класс в этом же файле
           - Унаследуйте его от PaymentMethod
           - Реализуйте метод ExecutePayment(decimal sum)
        
        Пример добавления оплаты через электронный кошелек:
        
        public class WalletPayment : PaymentMethod
        {
            private string _walletType;
            
            public WalletPayment(string walletType = "ЮMoney")
            {
                _walletType = walletType;
            }
            
            public override void ExecutePayment(decimal sum)
            {
                Console.WriteLine($"Оплата через {_walletType} на сумму {sum}");
                // Здесь логика работы с электронным кошельком...
            }
        }
        
        
        ШАГ 2: Использовать новый способ оплаты в программе
           - Создайте экземпляр нового класса оплаты
           - Передайте его в метод ChoosePayment() заказа
           - Вызовите PayForOrder() для выполнения оплаты
        
        Пример:

        var order = new ShopOrder("ТМ-2024-999", 7500.00m, "Мария Сидорова");
        order.ChoosePayment(new WalletPayment("Qiwi"));
        order.PayForOrder();

       */
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Магазин электроники 'ТехноМир' ===\n");

            var firstOrder = new ShopOrder("ТМ-2024-456", 15999.99m, "Сергей Иванов");

            Console.WriteLine("\n--- Вариант 1: Оплата картой ---");
            firstOrder.ChoosePayment(new CardPayment());
            firstOrder.PayForOrder();

            Console.WriteLine("\n--- Вариант 2: Наличными ---");
            firstOrder.ChoosePayment(new CashPaymentMethod());
            firstOrder.PayForOrder();

            Console.WriteLine("\n--- Вариант 3: Курьеру при получении ---");
            firstOrder.ChoosePayment(new CourierPayment());
            firstOrder.PayForOrder();

            Console.WriteLine("\n=== Показываем смену способа оплаты ===");
            var secondOrder = new ShopOrder("ТМ-2024-789", 23500.50m, "Анна Петрова");

            secondOrder.ChoosePayment(new CardPayment());

            secondOrder.ChoosePayment(new CourierPayment());
            secondOrder.PayForOrder();
        }
    }
}
