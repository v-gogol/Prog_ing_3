using System;
using System.Collections.Generic;

namespace Mediator_Gogol
{
    public abstract class Mediator
    {
        public abstract void Notify(object sender, string message, params object[] args);
    }

    public abstract class OrderParticipant
    {
        protected Mediator mediator;
        protected string name;

        public OrderParticipant(Mediator mediator, string name)
        {
            this.mediator = mediator;
            this.name = name;
        }

        public void Send(string message, params object[] args)
        {
            Console.WriteLine($"[{name} -> Все]: {message}");
            mediator.Notify(this, message, args);
        }

        public virtual void Receive(string from, string message, params object[] args)
        {
            Console.WriteLine($"[{name}] от {from}: {string.Format(message, args)}");
        }

        public string Name => name;
    }

    public class Client : OrderParticipant
    {
        private List<string> myOrders = new List<string>();
        private bool hasActiveOrder = false;

        public Client(Mediator mediator, string name) : base(mediator, $"Клиент {name}") { }

        public void PlaceOrder(string orderDetails)
        {
            if (hasActiveOrder)
            {
                Console.WriteLine($"{name}: У меня уже есть активный заказ!");
                return;
            }

            myOrders.Add(orderDetails);
            hasActiveOrder = true;
            Send($"Новый заказ: {orderDetails}", orderDetails);
        }

        public void CancelOrder(string orderId)
        {
            Send($"Отмена заказа #{orderId}", orderId);
        }

        public override void Receive(string from, string message, params object[] args)
        {
            base.Receive(from, message, args);

            if (message.Contains("заказ готов"))
            {
                Console.WriteLine($"{name}: Отлично! Заберу в ближайшее время.");
                hasActiveOrder = false;
            }
            else if (message.Contains("проблема"))
            {
                Console.WriteLine($"{name}: Ой, это плохо. Что случилось?");
            }
        }

        public void RequestStatus(string orderId)
        {
            Send($"Запрос статуса заказа #{orderId}", orderId);
        }
    }

    public class Manager : OrderParticipant
    {
        private int processedOrders = 0;
        private Random random = new Random();

        public Manager(Mediator mediator, string name) : base(mediator, $"Менеджер {name}") { }

        public override void Receive(string from, string message, params object[] args)
        {
            base.Receive(from, message, args);

            if (message.Contains("Новый заказ"))
            {
                string orderDetails = args.Length > 0 ? args[0].ToString() : "";

                if (random.Next(0, 10) > 7)
                {
                    Console.WriteLine($"{name}: Проверяю заказ вручную...");
                    System.Threading.Thread.Sleep(500); 
                }

                processedOrders++;
                Console.WriteLine($"{name}: Заказ принят в работу (всего обработано: {processedOrders})");

                bool isValid = ValidateOrder(orderDetails);
                if (isValid)
                {
                    Send($"Заказ проверен и одобрен", orderDetails);
                }
                else
                {
                    Send($"Проблема с заказом: неверные данные", orderDetails);
                }
            }
            else if (message.Contains("нет на складе"))
            {
                Console.WriteLine($"{name}: Ищу альтернативный товар...");
                if (random.Next(0, 10) > 3)
                {
                    Send($"Нашел замену товара", args);
                }
                else
                {
                    Send($"Не могу найти замену, нужно уведомить клиента", args);
                }
            }
        }

        private bool ValidateOrder(string orderDetails)
        {
            return !string.IsNullOrEmpty(orderDetails) &&
                   orderDetails.Length > 5;
        }

        public void ManualCheckOrder(string orderId)
        {
            Send($"Ручная проверка заказа #{orderId}", orderId);
        }
    }

    public class Warehouse : OrderParticipant
    {
        private Dictionary<string, int> inventory = new Dictionary<string, int>();
        private List<string> reservedItems = new List<string>();
        private Random random = new Random();

        public Warehouse(Mediator mediator, string name) : base(mediator, $"Склад {name}")
        {
            inventory["Ноутбук"] = 5;
            inventory["Телефон"] = 10;
            inventory["Наушники"] = 20;
            inventory["Монитор"] = 3;
        }

        public void CheckInventory()
        {
            Console.WriteLine("\n=== Текущие остатки на складе ===");
            foreach (var item in inventory)
            {
                Console.WriteLine($"  {item.Key}: {item.Value} шт.");
            }
        }

        public override void Receive(string from, string message, params object[] args)
        {
            base.Receive(from, message, args);

            if (message.Contains("заказ проверен"))
            {
                string orderDetails = args.Length > 0 ? args[0].ToString() : "";

                bool canFulfill = ProcessOrder(orderDetails);

                if (canFulfill)
                {
                    Send($"Заказ собран и готов к выдаче", orderDetails);
                }
                else
                {
                    Send($"Товара нет на складе", orderDetails);
                }
            }
            else if (message.Contains("запрос статуса"))
            {
                string orderId = args.Length > 0 ? args[0].ToString() : "";
                Send($"Статус заказа #{orderId}: на сборке", orderId);
            }
            else if (message.Contains("нашел замену"))
            {
                Console.WriteLine($"{name}: Собираю заказ с заменой товара...");
                Send($"Заказ собран с заменой", args);
            }
        }

        private bool ProcessOrder(string orderDetails)
        {
            if (random.Next(0, 10) > 8)
            {
                Console.WriteLine($"{name}: Внезапная проверка склада! Задержка...");
                return false;
            }

            foreach (var item in inventory)
            {
                if (orderDetails.Contains(item.Key))
                {
                    if (item.Value > 0)
                    {
                        inventory[item.Key] = item.Value - 1;
                        reservedItems.Add(item.Key);
                        return true;
                    }
                }
            }

            return false;
        }

        public void Restock(string item, int quantity)
        {
            if (inventory.ContainsKey(item))
            {
                inventory[item] += quantity;
            }
            else
            {
                inventory[item] = quantity;
            }
            Send($"Пополнение склада: {item} +{quantity}шт.", item, quantity);
        }
    }

    public class OrderMediator : Mediator
    {
        private List<OrderParticipant> participants = new List<OrderParticipant>();

        private Client client;
        private Manager manager;
        private Warehouse warehouse;

        public void RegisterClient(Client client)
        {
            this.client = client;
            participants.Add(client);
        }

        public void RegisterManager(Manager manager)
        {
            this.manager = manager;
            participants.Add(manager);
        }

        public void RegisterWarehouse(Warehouse warehouse)
        {
            this.warehouse = warehouse;
            participants.Add(warehouse);
        }

        public override void Notify(object sender, string message, params object[] args)
        {
            var senderName = (sender as OrderParticipant)?.Name ?? "Неизвестный";

            Console.WriteLine($"\n[Медиатор] {senderName}: {message}");

            if (sender is Client)
            {
                if (message.Contains("Новый заказ") || message.Contains("Отмена"))
                {
                    manager?.Receive(senderName, message, args);
                }
                else if (message.Contains("Запрос статуса"))
                {
                    warehouse?.Receive(senderName, message, args);
                }
            }
            else if (sender is Manager)
            {
                if (message.Contains("проверен и одобрен"))
                {
                    warehouse?.Receive(senderName, message, args);
                }
                else if (message.Contains("проблема с заказом") || message.Contains("нашел замену"))
                {
                    client?.Receive(senderName, message, args);
                }
            }
            else if (sender is Warehouse)
            {
                if (message.Contains("собран и готов"))
                {
                    client?.Receive(senderName, "Ваш заказ готов к выдаче!", args);
                    manager?.Receive(senderName, "Заказ собран", args);
                }
                else if (message.Contains("нет на складе"))
                {
                    manager?.Receive(senderName, message, args);
                }
                else if (message.Contains("Пополнение склада"))
                {
                    foreach (var p in participants)
                    {
                        if (p != sender)
                            p.Receive(senderName, message, args);
                    }
                }
            }

            if (message.Contains("ошибка") || message.Contains("проблема"))
            {
                Console.WriteLine($"[Медиатор] Зафиксирована проблема в системе!");
            }
        }

        public void SetupSystem(Client c, Manager m, Warehouse w)
        {
            RegisterClient(c);
            RegisterManager(m);
            RegisterWarehouse(w);
            Console.WriteLine("Система инициализирована с тремя участниками");
        }
    }

    public class Logger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[Логгер] {message}");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== Система управления заказами через Mediator ===\n");

            OrderMediator mediator = new OrderMediator();

            Client client = new Client(mediator, "Иван Иванов");
            Manager manager = new Manager(mediator, "Петр Петров");
            Warehouse warehouse = new Warehouse(mediator, "Основной");

            mediator.SetupSystem(client, manager, warehouse);

            warehouse.CheckInventory();

            Console.WriteLine("\n--- Симуляция работы системы ---\n");

            client.PlaceOrder("Ноутбук Lenovo, 2 шт.");

            System.Threading.Thread.Sleep(1000);
            Console.WriteLine();

            client.RequestStatus("ORD-001");

            System.Threading.Thread.Sleep(800);
            Console.WriteLine();

            manager.ManualCheckOrder("ORD-001");

            System.Threading.Thread.Sleep(1200);
            Console.WriteLine();

            warehouse.Restock("Ноутбук", 3);

            System.Threading.Thread.Sleep(900);
            Console.WriteLine();

            client.PlaceOrder("Монитор Samsung");

            System.Threading.Thread.Sleep(1500);
            Console.WriteLine();

            client.CancelOrder("ORD-001");

            Console.WriteLine("\n--- Демонстрация завершена ---");

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
