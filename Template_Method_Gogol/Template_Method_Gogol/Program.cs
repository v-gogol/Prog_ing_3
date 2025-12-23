using System;

namespace Template_Method_Gogol
{
    public enum DeliveryType
    {
        Regular = 1,
        Fast = 2
    }

    public class Purchase
    {
        public string ItemName { get; }
        public int ItemCount { get; }
        public decimal PricePerItem { get; }
        public string ShipToAddress { get; }
        public DeliveryType ShipType { get; }

        public decimal TotalCost => PricePerItem * ItemCount;

        public Purchase(string itemName, int itemCount, decimal pricePerItem, string shipToAddress, DeliveryType shipType)
        {
            ItemName = itemName;
            ItemCount = itemCount;
            PricePerItem = pricePerItem;
            ShipToAddress = shipToAddress;
            ShipType = shipType;
        }

        public void Handle()
        {
            PurchaseHandler handler;
            if (ShipType == DeliveryType.Regular)
            {
                handler = new RegularPurchaseHandler();
            }
            else if (ShipType == DeliveryType.Fast)
            {
                handler = new FastPurchaseHandler();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(ShipType), "Неизвестный тип доставки.");
            }

            handler.Execute(this);
        }
    }

    public abstract class PurchaseHandler
    {
        public void Execute(Purchase purchase)
        {
            PickItem(purchase);
            PlaceOrder(purchase);
            ProcessPayment(purchase);
            ArrangeDelivery(purchase);
        }

        protected virtual void PickItem(Purchase purchase)
        {
            Console.WriteLine($"Выбрано: {purchase.ItemName}");
            Console.WriteLine($"Кол-во: {purchase.ItemCount} шт.");
            Console.WriteLine($"На сумму: {purchase.TotalCost} руб.");
        }

        protected virtual void PlaceOrder(Purchase purchase)
        {
            Console.WriteLine("Оформляем покупку...");
            Console.WriteLine($"Куда доставить: {purchase.ShipToAddress}");
        }

        protected virtual void ProcessPayment(Purchase purchase)
        {
            Console.WriteLine("Обрабатываем оплату...");
            Console.WriteLine("Средства успешно списаны.");
        }

        protected virtual void ArrangeDelivery(Purchase purchase)
        {
            string method = GetShippingDetails(purchase);
            Console.WriteLine($"Способ: {method}");
            Console.WriteLine("Заказ передан для отправки.");
        }

        protected abstract string GetShippingDetails(Purchase purchase);
    }

    public class RegularPurchaseHandler : PurchaseHandler
    {
        protected override string GetShippingDetails(Purchase purchase)
        {
            return "Обычная почта (3–5 рабочих дней)";
        }
    }

    public class FastPurchaseHandler : PurchaseHandler
    {
        protected override string GetShippingDetails(Purchase purchase)
        {
            return "Курьерская служба (1–2 дня)";
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Интернет-магазин 'Товары у дома'");
            Console.WriteLine("Выберите способ доставки:");
            Console.WriteLine("1 — Обычная (3-5 дней)");
            Console.WriteLine("2 — Срочная (1-2 дня)");
            Console.Write("Ваш вариант: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out int choice) || (choice != 1 && choice != 2))
            {
                Console.WriteLine("Неправильный выбор.");
                return;
            }

            var deliveryType = (DeliveryType)choice;

            Console.Write("Что покупаем? ");
            var product = Console.ReadLine() ?? "Без названия";

            Console.Write("Сколько штук? ");
            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Некорректное число.");
                return;
            }

            Console.Write("Цена за одну штуку (руб): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
            {
                Console.WriteLine("Некорректная цена.");
                return;
            }

            Console.Write("Куда доставить (адрес): ");
            var address = Console.ReadLine() ?? "Адрес не указан";

            var myPurchase = new Purchase(product, count, price, address, deliveryType);

            Console.WriteLine();
            Console.WriteLine("=== Начинаем обработку ===");
            myPurchase.Handle();
            Console.WriteLine("=== Завершено ===");

            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}
