using System;
using System.Collections.Generic;

namespace Memento_Gogol
{
    internal class Program
    {
        public class BasketSnapshot
        {
            private readonly List<BasketPosition> savedItems;
            private readonly DateTime saveTime;

            public BasketSnapshot(List<BasketPosition> items)
            {
                savedItems = new List<BasketPosition>();
                foreach (var pos in items)
                {
                    savedItems.Add(new BasketPosition(pos.ItemCode, pos.ItemTitle, pos.Amount, pos.CostPerOne));
                }
                saveTime = DateTime.Now;
            }

            public List<BasketPosition> GetStoredData()
            {
                var copy = new List<BasketPosition>();
                foreach (var pos in savedItems)
                {
                    copy.Add(new BasketPosition(pos.ItemCode, pos.ItemTitle, pos.Amount, pos.CostPerOne));
                }
                return copy;
            }

            public DateTime WhenSaved()
            {
                return saveTime;
            }
        }

        public class BasketPosition
        {
            public int ItemCode { get; }
            public string ItemTitle { get; }
            public int Amount { get; set; }
            public decimal CostPerOne { get; }

            public decimal TotalCost => Amount * CostPerOne;

            public BasketPosition(int itemCode, string itemTitle, int amount, decimal costPerOne)
            {
                ItemCode = itemCode;
                ItemTitle = itemTitle;
                Amount = amount;
                CostPerOne = costPerOne;
            }

            public override string ToString()
            {
                return $"{ItemTitle} (Код: {ItemCode}) - {Amount} шт. * {CostPerOne} = {TotalCost} руб.";
            }
        }

        public class Basket
        {
            private List<BasketPosition> positions;

            public Basket()
            {
                positions = new List<BasketPosition>();
            }

            public void PutItem(int itemCode, string itemTitle, int amount, decimal costPerOne)
            {
                var existing = FindByCode(itemCode);

                if (existing != null)
                {
                    existing.Amount += amount;
                    Console.WriteLine($"Изменили количество '{itemTitle}': теперь {existing.Amount} шт.");
                }
                else
                {
                    positions.Add(new BasketPosition(itemCode, itemTitle, amount, costPerOne));
                    Console.WriteLine($"Положили в корзину '{itemTitle}': {amount} шт.");
                }
            }

            public void TakeOutItem(int itemCode, int takeAmount = 0)
            {
                var position = FindByCode(itemCode);

                if (position == null)
                {
                    Console.WriteLine($"Товар с кодом {itemCode} не найден.");
                    return;
                }

                if (takeAmount <= 0 || takeAmount >= position.Amount)
                {
                    positions.Remove(position);
                    Console.WriteLine($"Убрали товар '{position.ItemTitle}' полностью.");
                }
                else
                {
                    position.Amount -= takeAmount;
                    Console.WriteLine($"Уменьшили количество '{position.ItemTitle}': осталось {position.Amount} шт.");
                }
            }

            public void EmptyBasket()
            {
                positions.Clear();
                Console.WriteLine("Корзина теперь пуста.");
            }

            public BasketSnapshot CreateSnapshot()
            {
                Console.WriteLine($"Сохранили состояние корзины ({positions.Count} позиций)");
                return new BasketSnapshot(positions);
            }

            public void LoadSnapshot(BasketSnapshot snapshot)
            {
                if (snapshot == null)
                {
                    Console.WriteLine("Ошибка: нечего загружать.");
                    return;
                }

                positions = snapshot.GetStoredData();
                Console.WriteLine($"Загрузили состояние от {snapshot.WhenSaved():HH:mm}");
            }

            public void ShowContents()
            {
                if (positions.Count == 0)
                {
                    Console.WriteLine("Корзина пустая.");
                    return;
                }

                Console.WriteLine("\n=== ВАША КОРЗИНА ===");
                int counter = 1;
                decimal overallSum = 0;
                int totalItems = 0;

                foreach (var pos in positions)
                {
                    Console.WriteLine($"{counter}. {pos}");
                    overallSum += pos.TotalCost;
                    totalItems += pos.Amount;
                    counter++;
                }

                Console.WriteLine($"ИТОГО: {overallSum} руб.");
                Console.WriteLine($"Всего товаров: {totalItems} шт.");
                Console.WriteLine("====================\n");
            }

            public int GetPositionsCount()
            {
                return positions.Count;
            }

            private BasketPosition FindByCode(int code)
            {
                foreach (var pos in positions)
                {
                    if (pos.ItemCode == code)
                        return pos;
                }
                return null;
            }
        }

        public class BasketHistory
        {
            private readonly Stack<BasketSnapshot> historyStack;
            private readonly Stack<BasketSnapshot> forwardStack;
            private readonly int maxRecords;

            public BasketHistory(int maxRecords = 8)
            {
                historyStack = new Stack<BasketSnapshot>();
                forwardStack = new Stack<BasketSnapshot>();
                this.maxRecords = maxRecords;
            }

            public void RecordState(Basket basket)
            {
                if (historyStack.Count >= maxRecords)
                {
                    ClearOldest();
                }

                historyStack.Push(basket.CreateSnapshot());
                forwardStack.Clear();
            }

            public void StepBack(Basket basket)
            {
                if (historyStack.Count <= 1)
                {
                    Console.WriteLine("Нельзя откатиться: история пуста.");
                    return;
                }

                forwardStack.Push(historyStack.Pop());

                if (historyStack.Count > 0)
                {
                    var previous = historyStack.Peek();
                    basket.LoadSnapshot(previous);
                }
            }

            public void StepForward(Basket basket)
            {
                if (forwardStack.Count == 0)
                {
                    Console.WriteLine("Нельзя повторить: нет отмененных действий.");
                    return;
                }

                var nextState = forwardStack.Pop();
                historyStack.Push(nextState);
                basket.LoadSnapshot(nextState);
            }

            public int HistoryCount()
            {
                return historyStack.Count;
            }

            public int ForwardCount()
            {
                return forwardStack.Count;
            }

            private void ClearOldest()
            {
                var temp = new Stack<BasketSnapshot>();
                while (historyStack.Count > 1)
                {
                    temp.Push(historyStack.Pop());
                }
                historyStack.Clear();
                while (temp.Count > 0)
                {
                    historyStack.Push(temp.Pop());
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== УПРАВЛЕНИЕ КОРЗИНОЙ ТОВАРОВ ===\n");

            var myBasket = new Basket();
            var history = new BasketHistory(maxRecords: 6);

            history.RecordState(myBasket);

            bool working = true;

            while (working)
            {
                Console.WriteLine("\nДоступные действия:");
                Console.WriteLine("1 - Посмотреть корзину");
                Console.WriteLine("2 - Добавить товар");
                Console.WriteLine("3 - Убрать товар");
                Console.WriteLine("4 - Откатить действие");
                Console.WriteLine("5 - Вернуть действие");
                Console.WriteLine("6 - Очистить корзину");
                Console.WriteLine("7 - Инфо о истории");
                Console.WriteLine("0 - Закрыть программу");
                Console.Write("\nЧто делаем? ");

                string input = Console.ReadLine();
                if (!int.TryParse(input, out int choice))
                {
                    Console.WriteLine("Не понял команду.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        myBasket.ShowContents();
                        break;

                    case 2:
                        try
                        {
                            Console.Write("Код товара: ");
                            int code = Convert.ToInt32(Console.ReadLine());
                            Console.Write("Название: ");
                            string name = Console.ReadLine();
                            Console.Write("Сколько: ");
                            int count = Convert.ToInt32(Console.ReadLine());
                            Console.Write("Цена за штуку: ");
                            decimal price = Convert.ToDecimal(Console.ReadLine());

                            myBasket.PutItem(code, name, count, price);
                            history.RecordState(myBasket);
                        }
                        catch
                        {
                            Console.WriteLine("Некорректный ввод.");
                        }
                        break;

                    case 3:
                        try
                        {
                            Console.Write("Код товара для удаления: ");
                            int remCode = Convert.ToInt32(Console.ReadLine());
                            Console.Write("Сколько убрать (0 - всё): ");
                            int remCount = Convert.ToInt32(Console.ReadLine());

                            myBasket.TakeOutItem(remCode, remCount);
                            history.RecordState(myBasket);
                        }
                        catch
                        {
                            Console.WriteLine("Некорректный ввод.");
                        }
                        break;

                    case 4:
                        Console.WriteLine("\n--- Откат ---");
                        history.StepBack(myBasket);
                        break;

                    case 5:
                        Console.WriteLine("\n--- Повтор ---");
                        history.StepForward(myBasket);
                        break;

                    case 6:
                        myBasket.EmptyBasket();
                        history.RecordState(myBasket);
                        break;

                    case 7:
                        Console.WriteLine($"Сохранено состояний: {history.HistoryCount()}");
                        Console.WriteLine($"Можно повторить: {history.ForwardCount()}");
                        break;

                    case 0:
                        working = false;
                        Console.WriteLine("Завершение работы.");
                        break;

                    default:
                        Console.WriteLine("Такой команды нет.");
                        break;
                }
            }
        }
    }
}
