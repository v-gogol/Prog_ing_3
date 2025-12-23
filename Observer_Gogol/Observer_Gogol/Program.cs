using System;
using System.Collections.Generic;

namespace Observer_Gogol
{
    public enum DeliveryStage
    {
        Created,
        InWork, 
        SentOut, 
        Completed
    }

    public static class StageText
    {
        public static string GetText(this DeliveryStage stage)
        {
            switch (stage)
            {
                case DeliveryStage.Created:
                    return "'Создан'";
                case DeliveryStage.InWork:
                    return "'В работе'";
                case DeliveryStage.SentOut:
                    return "'Отправлен'";
                case DeliveryStage.Completed:
                    return "'Завершен'";
                default:
                    return stage.ToString();
            }
        }
    }

    public interface IStatusWatcher
    {
        void StatusUpdate(DeliveryItem item, DeliveryStage oldStage);
    }

    public class DeliveryItem
    {
        private readonly List<IStatusWatcher> watchers = new List<IStatusWatcher>();

        public int Code { get; }
        public DeliveryStage CurrentStage { get; private set; }

        public DeliveryItem(int code, DeliveryStage startStage)
        {
            Code = code;
            CurrentStage = startStage;
        }

        public void RegisterWatcher(IStatusWatcher watcher)
        {
            if (watcher == null)
                return;

            if (!watchers.Contains(watcher))
            {
                watchers.Add(watcher);
            }
        }

        public void UnregisterWatcher(IStatusWatcher watcher)
        {
            if (watcher == null)
                return;

            watchers.Remove(watcher);
        }

        public void UpdateStage(DeliveryStage newStage)
        {
            if (CurrentStage == newStage)
            {
                return;
            }

            var oldStage = CurrentStage;
            CurrentStage = newStage;

            InformWatchers(oldStage);
        }

        private void InformWatchers(DeliveryStage oldStage)
        {
            foreach (var watcher in watchers)
            {
                watcher.StatusUpdate(this, oldStage);
            }
        }
    }

    public class CustomerAlert : IStatusWatcher
    {
        private readonly string customer;
        private readonly string contact;

        public CustomerAlert(string customer, string contact)
        {
            this.customer = customer;
            this.contact = contact;
        }

        public void StatusUpdate(DeliveryItem item, DeliveryStage oldStage)
        {
            Console.WriteLine(
                $"[Клиент {customer}] Заказ №{item.Code}: изменился статус с {oldStage.GetText()} на {item.CurrentStage.GetText()}. " +
                $"Сообщение отправлено на {contact}.");
        }
    }

    public class SupervisorAlert : IStatusWatcher
    {
        private readonly string supervisor;

        public SupervisorAlert(string supervisor)
        {
            this.supervisor = supervisor;
        }

        public void StatusUpdate(DeliveryItem item, DeliveryStage oldStage)
        {
            Console.WriteLine(
                $"[Руководитель {supervisor}] Заказ №{item.Code}: статус изменился с {oldStage.GetText()} на {item.CurrentStage.GetText()}. " +
                "Проверьте выполнение заказа.\n");
        }
    }

    public class StatsTracker : IStatusWatcher
    {
        public void StatusUpdate(DeliveryItem item, DeliveryStage oldStage)
        {
            Console.WriteLine(
                $"[Статистика] Заказ №{item.Code}: {oldStage.GetText()} -> {item.CurrentStage.GetText()}.");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var item = new DeliveryItem(code: 2025, startStage: DeliveryStage.Created);

            var customer = new CustomerAlert("Алексей Ковалев", "alex@mail.ru");
            var supervisor = new SupervisorAlert("Ирина Васильева");
            var stats = new StatsTracker();

            item.RegisterWatcher(customer);
            item.RegisterWatcher(supervisor);
            item.RegisterWatcher(stats);

            Console.WriteLine("Начинаем менять статус заказа...\n");

            item.UpdateStage(DeliveryStage.InWork);
            item.UpdateStage(DeliveryStage.SentOut);

            Console.WriteLine("\nРуководитель больше не получает уведомления.\n");
            item.UnregisterWatcher(supervisor);

            item.UpdateStage(DeliveryStage.Completed);

            Console.WriteLine("\n--- Подключаем нового наблюдателя ---");
            var testWatcher = new CustomerAlert("Тестовый Клиент", "test@test.com");
            item.RegisterWatcher(testWatcher);
            item.UpdateStage(DeliveryStage.Completed); 
        }
    }
}