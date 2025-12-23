using System;

namespace State_Gogol
{
    public abstract class OrderState
    {
        public abstract void ProcessOrder(Order order);
        public abstract string GetStatus();

        protected void LogTransition(string from, string to)
        {
            Console.WriteLine($"[Лог] {DateTime.Now:HH:mm:ss}: Переход {from} -> {to}");
        }
    }

    public class NewState : OrderState
    {
        public override void ProcessOrder(Order order)
        {
            if (order.Validate())
            {
                LogTransition("Новый", "В обработке");
                order.SetState(new ProcessingState());
            }
            else
            {
                Console.WriteLine("Ошибка: невалидный заказ");
            }
        }

        public override string GetStatus()
        {
            return "Новый заказ";
        }
    }

    public class ProcessingState : OrderState
    {
        private readonly Random random = new Random();

        public override void ProcessOrder(Order order)
        {
            bool inStock = CheckStock(order);

            if (inStock)
            {
                LogTransition("В обработке", "Отправлен");
                order.SetState(new ShippedState());
            }
            else
            {
                Console.WriteLine("Товара нет в наличии, заказ остается в обработке");
            }
        }

        private bool CheckStock(Order order)
        {
            return random.Next(0, 10) > 2;
        }

        public override string GetStatus()
        {
            return "В обработке";
        }
    }

    public class ShippedState : OrderState
    {
        private DateTime shippedDate;

        public override void ProcessOrder(Order order)
        {
            shippedDate = DateTime.Now;

            Console.WriteLine($"Заказ отправлен {shippedDate:dd.MM.yyyy}");
            Console.WriteLine("Ожидайте доставки...");

            LogTransition("Отправлен", "Доставлен");
            order.SetState(new DeliveredState());
        }

        public override string GetStatus()
        {
            return shippedDate == default
                ? "Готов к отправке"
                : $"Отправлен {shippedDate:dd.MM.yyyy}";
        }
    }

    public class DeliveredState : OrderState
    {
        private readonly DateTime deliveredDate = DateTime.Now;

        public override void ProcessOrder(Order order)
        {
            Console.WriteLine("Заказ уже доставлен. Дальнейшая обработка невозможна.");
        }

        public override string GetStatus()
        {
            return $"Доставлен {deliveredDate:dd.MM.yyyy HH:mm}";
        }
    }

    public class CancelledState : OrderState
    {
        private readonly string cancellationReason;

        public CancelledState(string reason = "не указана")
        {
            cancellationReason = reason;
        }

        public override void ProcessOrder(Order order)
        {
            Console.WriteLine($"Заказ отменен. Причина: {cancellationReason}");
        }

        public override string GetStatus()
        {
            return $"Отменен (причина: {cancellationReason})";
        }
    }

    public class Order
    {
        private OrderState currentState;
        private readonly string orderNumber;
        private readonly string customerName;

        public string OrderNumber => orderNumber;
        public string CustomerName => customerName;

        public Order(string number, string customer)
        {
            orderNumber = number;
            customerName = customer;
            currentState = new NewState();

            InitializeOrder();
        }

        private void InitializeOrder()
        {
            Console.WriteLine($"Создан заказ #{orderNumber} для {customerName}");
        }

        public void SetState(OrderState state)
        {
            if (state == null)
            {
                Console.WriteLine("Ошибка: состояние не может быть null");
                return;
            }

            currentState = state;
        }

        public void Process()
        {
            Console.WriteLine($"\nОбработка заказа #{orderNumber}:");
            Console.WriteLine($"Текущий статус: {GetStatus()}");

            try
            {
                currentState.ProcessOrder(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
                Console.WriteLine("Перевожу заказ в состояние 'Отменен'");
                SetState(new CancelledState("системная ошибка"));
            }
        }

        public void Cancel(string reason)
        {
            Console.WriteLine($"\nОтмена заказа #{orderNumber}");
            Console.WriteLine($"Причина: {reason}");

            if (currentState is DeliveredState)
            {
                Console.WriteLine("Невозможно отменить доставленный заказ!");
                return;
            }

            currentState = new CancelledState(reason);
        }

        public string GetStatus()
        {
            return currentState.GetStatus();
        }

        public bool Validate()
        {
            return !string.IsNullOrEmpty(customerName) &&
                   !string.IsNullOrEmpty(orderNumber);
        }
    }

    public class OrderManager
    {
        public void DemoWorkflow()
        {
            Console.WriteLine("=== Демонстрация работы системы заказов ===\n");

            Order order = new Order("ORD-2026-001", "Иван Петров");

            order.Process(); 
            order.Process(); 

            order.Cancel("передумал");

            Console.WriteLine($"\nИтоговый статус: {order.GetStatus()}");

            Console.WriteLine("\n--- Второй заказ ---\n");

            Order order2 = new Order("ORD-2026-002", "Мария Сидорова");

            order2.Process();
            order2.Process(); 
            order2.Process(); 

            order2.Process();

            order2.Cancel("ошибка");
        }

        public void InteractiveDemo()
        {
            Console.WriteLine("\n=== Интерактивный режим ===");

            Order order = new Order("ORD-2026-003", "Тестовый Пользователь");

            while (true)
            {
                Console.WriteLine($"\nЗаказ #{order.OrderNumber}");
                Console.WriteLine($"Клиент: {order.CustomerName}");
                Console.WriteLine($"Статус: {order.GetStatus()}");
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1 - Обработать заказ");
                Console.WriteLine("2 - Отменить заказ");
                Console.WriteLine("3 - Выйти");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        order.Process();
                        break;
                    case "2":
                        Console.Write("Введите причину отмены: ");
                        string reason = Console.ReadLine();
                        order.Cancel(reason);
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Неверный ввод");
                        break;
                }
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            OrderManager manager = new OrderManager();

            manager.DemoWorkflow();

            Console.WriteLine("\nХотите попробовать интерактивный режим? (y/n)");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                manager.InteractiveDemo();
            }

            Console.WriteLine("\n=== Работа программы завершена ===");
            Console.ReadKey();
        }
    }
}