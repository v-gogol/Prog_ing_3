using System;
using System.Collections.Generic;

namespace Iterator_Gogol
{
    public interface ICollectionWalker
    {
        bool HasMore();
        Item GetNext();
        List<Item> TakeNext(int howMany);
        void Restart();
    }

    public class Item
    {
        public string Title { get; set; }
        public string Group { get; set; }
        public decimal Cost { get; set; }
        public int Rating { get; set; }

        public Item(string title, string group, decimal cost, int rating)
        {
            Title = title;
            Group = group;
            Cost = cost;
            Rating = rating;
        }

        public override string ToString()
        {
            return $"{Title} (Группа: {Group}, Стоимость: {Cost}, Оценка: {Rating}/10)";
        }
    }

    public class GroupWalker : ICollectionWalker
    {
        private List<Item> allItems;
        private List<string> uniqueGroups;
        private int currentGroupIdx;
        private int currentItemIdx;

        public GroupWalker(List<Item> items)
        {
            allItems = new List<Item>();
            foreach (var it in items)
            {
                allItems.Add(it);
            }
            allItems.Sort((a, b) =>
            {
                int groupCompare = a.Group.CompareTo(b.Group);
                return groupCompare != 0 ? groupCompare : a.Title.CompareTo(b.Title);
            });

            uniqueGroups = new List<string>();
            foreach (var it in allItems)
            {
                if (!uniqueGroups.Contains(it.Group))
                    uniqueGroups.Add(it.Group);
            }
            Restart();
        }

        public bool HasMore()
        {
            if (currentGroupIdx >= uniqueGroups.Count) return false;

            string currentGroup = uniqueGroups[currentGroupIdx];
            var groupItems = GetItemsForGroup(currentGroup);
            return currentItemIdx < groupItems.Count;
        }

        public Item GetNext()
        {
            if (!HasMore()) return null;

            string currentGroup = uniqueGroups[currentGroupIdx];
            var groupItems = GetItemsForGroup(currentGroup);
            Item result = groupItems[currentItemIdx];

            currentItemIdx++;
            if (currentItemIdx >= groupItems.Count)
            {
                currentGroupIdx++;
                currentItemIdx = 0;
            }

            return result;
        }

        public List<Item> TakeNext(int howMany)
        {
            List<Item> taken = new List<Item>();
            for (int i = 0; i < howMany && HasMore(); i++)
            {
                taken.Add(GetNext());
            }
            return taken;
        }

        public void Restart()
        {
            currentGroupIdx = 0;
            currentItemIdx = 0;
        }

        private List<Item> GetItemsForGroup(string group)
        {
            List<Item> groupItems = new List<Item>();
            foreach (var it in allItems)
            {
                if (it.Group == group)
                    groupItems.Add(it);
            }
            return groupItems;
        }
    }

    public class PriceWalker : ICollectionWalker
    {
        private List<Item> sortedItems;
        private int position;

        public PriceWalker(List<Item> items)
        {
            sortedItems = new List<Item>(items);
            sortedItems.Sort((a, b) =>
            {
                int priceCompare = a.Cost.CompareTo(b.Cost);
                return priceCompare != 0 ? priceCompare : a.Title.CompareTo(b.Title);
            });
            Restart();
        }

        public bool HasMore()
        {
            return position < sortedItems.Count;
        }

        public Item GetNext()
        {
            if (!HasMore()) return null;
            return sortedItems[position++];
        }

        public List<Item> TakeNext(int howMany)
        {
            List<Item> taken = new List<Item>();
            for (int i = 0; i < howMany && HasMore(); i++)
            {
                taken.Add(GetNext());
            }
            return taken;
        }

        public void Restart()
        {
            position = 0;
        }
    }

    public class RatingWalker : ICollectionWalker
    {
        private List<Item> sortedItems;
        private int position;

        public RatingWalker(List<Item> items)
        {
            sortedItems = new List<Item>(items);
            sortedItems.Sort((a, b) =>
            {
                int ratingCompare = b.Rating.CompareTo(a.Rating);
                return ratingCompare != 0 ? ratingCompare : a.Title.CompareTo(b.Title);
            });
            Restart();
        }

        public bool HasMore()
        {
            return position < sortedItems.Count;
        }

        public Item GetNext()
        {
            if (!HasMore()) return null;
            return sortedItems[position++];
        }

        public List<Item> TakeNext(int howMany)
        {
            List<Item> taken = new List<Item>();
            for (int i = 0; i < howMany && HasMore(); i++)
            {
                taken.Add(GetNext());
            }
            return taken;
        }

        public void Restart()
        {
            position = 0;
        }
    }

    public class ItemCollection
    {
        private List<Item> items;
        private ICollectionWalker currentWalker;

        public ItemCollection()
        {
            items = new List<Item>();
            FillWithDefault();
        }

        private void FillWithDefault()
        {
            items.Add(new Item("Ноутбук Acer", "Электроника", 55000, 7));
            items.Add(new Item("Смартфон Xiaomi", "Электроника", 28000, 8));
            items.Add(new Item("Джинсы", "Одежда", 2500, 6));
            items.Add(new Item("Свитшот", "Одежда", 1800, 5));
            items.Add(new Item("Книга '1984'", "Литература", 650, 9));
            items.Add(new Item("Наушники JBL", "Электроника", 8500, 8));
            items.Add(new Item("Чайник", "Техника для дома", 2200, 6));
            items.Add(new Item("Роман 'Преступление и наказание'", "Литература", 550, 8));
            items.Add(new Item("Кеды Adidas", "Одежда", 4500, 7));
            items.Add(new Item("Микроволновка", "Техника для дома", 12000, 6));
        }

        public void SetWalkStyle(string style)
        {
            switch (style.ToLower())
            {
                case "group":
                    currentWalker = new GroupWalker(items);
                    break;
                case "price":
                    currentWalker = new PriceWalker(items);
                    break;
                case "rating":
                    currentWalker = new RatingWalker(items);
                    break;
                default:
                    throw new ArgumentException($"Неизвестный стиль обхода: {style}");
            }
        }

        public ICollectionWalker GetWalker()
        {
            if (currentWalker == null)
            {
                currentWalker = new GroupWalker(items);
            }
            return currentWalker;
        }

        public void AddItem(Item newItem)
        {
            items.Add(newItem);
            if (currentWalker != null)
            {
                string currentStyle = GetCurrentStyleName();
                SetWalkStyle(currentStyle);
            }
        }

        private string GetCurrentStyleName()
        {
            if (currentWalker is GroupWalker) return "group";
            if (currentWalker is PriceWalker) return "price";
            if (currentWalker is RatingWalker) return "rating";
            return "group";
        }

        public void ShowAllItems()
        {
            ICollectionWalker walker = GetWalker();
            walker.Restart();

            Console.WriteLine("\n--- Просмотр всей коллекции ---");
            int counter = 1;
            while (walker.HasMore())
            {
                Item current = walker.GetNext();
                Console.WriteLine($"{counter++}. {current}");
            }
            Console.WriteLine("--- Конец ---\n");
        }

        public void ShowSomeItems(int number)
        {
            ICollectionWalker walker = GetWalker();
            walker.Restart();

            Console.WriteLine($"\n--- Первые {number} элементов ---");
            List<Item> someItems = walker.TakeNext(number);
            for (int i = 0; i < someItems.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {someItems[i]}");
            }
            Console.WriteLine("--- Конец ---\n");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            ItemCollection myCollection = new ItemCollection();

            Console.WriteLine("Тестирование разных способов обхода коллекции\n");

            myCollection.SetWalkStyle("group");
            Console.WriteLine("1. По группам:");
            myCollection.ShowAllItems();

            myCollection.SetWalkStyle("price");
            Console.WriteLine("2. По возрастанию цены:");
            myCollection.ShowAllItems();

            myCollection.SetWalkStyle("rating");
            Console.WriteLine("3. По убыванию рейтинга:");
            myCollection.ShowAllItems();

            Console.WriteLine("4. Показать первые 4 по рейтингу:");
            myCollection.ShowSomeItems(4);

            Console.WriteLine("5. Добавим новый товар и покажем по цене:");
            myCollection.AddItem(new Item("Планшет Huawei", "Электроника", 32000, 8));
            myCollection.SetWalkStyle("price");
            myCollection.ShowAllItems();

            Console.WriteLine("6. Ручная работа с обходчиком (по группам):");
            myCollection.SetWalkStyle("group");
            ICollectionWalker walker = myCollection.GetWalker();

            Console.WriteLine("Два первых элемента:");
            List<Item> firstTwo = walker.TakeNext(2);
            foreach (Item it in firstTwo)
            {
                Console.WriteLine($"  - {it}");
            }

            Console.WriteLine("\nЕще три элемента:");
            List<Item> nextThree = walker.TakeNext(3);
            foreach (Item it in nextThree)
            {
                Console.WriteLine($"  - {it}");
            }

            Console.WriteLine("\nСброс и повторный вывод двух первых:");
            walker.Restart();
            firstTwo = walker.TakeNext(2);
            foreach (Item it in firstTwo)
            {
                Console.WriteLine($"  - {it}");
            }
        }
    }
}
