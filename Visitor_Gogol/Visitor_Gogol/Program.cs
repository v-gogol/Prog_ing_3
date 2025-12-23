using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visitor_Gogol
{
    public interface IStoreItem
    {
        decimal CalculatePrice();
        void Apply(ICalculator calculator);
    }

    public abstract class ICalculator
    {
        public abstract void ProcessGoods(Goods goods);
        public abstract void ProcessContainer(Container container);
    }

    public class Goods : IStoreItem
    {
        public string Title { get; }
        public decimal Cost { get; }
        public double Mass { get; }

        public Goods(string title, decimal cost, double mass = 1.0)
        {
            Title = title;
            Cost = cost;
            Mass = mass;
        }

        public decimal CalculatePrice() => Cost;

        public void Apply(ICalculator calculator)
        {
            calculator.ProcessGoods(this);
        }
    }

    public class Container : IStoreItem
    {
        private readonly List<IStoreItem> contents = new List<IStoreItem>();

        public string Label { get; }
        public double Mass { get; }

        public Container(string label, double mass = 0.5)
        {
            Label = label;
            Mass = mass;
        }

        public void PutInside(IStoreItem element) => contents.Add(element);
        public void TakeOut(IStoreItem element) => contents.Remove(element);

        public decimal CalculatePrice()
        {
            decimal sum = 0;
            foreach (var element in contents)
            {
                sum += element.CalculatePrice();
            }
            return sum;
        }

        public void Apply(ICalculator calculator)
        {
            foreach (var element in contents)
            {
                element.Apply(calculator);
            }
            calculator.ProcessContainer(this);
        }

        public double TotalMass()
        {
            double total = Mass;
            foreach (var element in contents)
            {
                if (element is Goods product)
                {
                    total += product.Mass;
                }
                else if (element is Container box)
                {
                    total += box.TotalMass();
                }
            }
            return total;
        }
    }

    public class ShippingCostEstimator : ICalculator
    {
        private decimal shippingTotal = 0;

        private const decimal BaseShippingRatePerKg = 2.0m;
        private const decimal ContainerFee = 1.5m;
        private const decimal GoodsFee = 0.5m;

        public decimal FinalShippingCost => shippingTotal;

        public override void ProcessGoods(Goods goods)
        {
            decimal shipping = (decimal)goods.Mass * BaseShippingRatePerKg + GoodsFee;
            shippingTotal += shipping;

            Console.WriteLine($"Перевозка '{goods.Title}': {shipping:F2} у.е.");
        }

        public override void ProcessContainer(Container container)
        {
            decimal shipping = (decimal)container.Mass * BaseShippingRatePerKg + ContainerFee;
            shippingTotal += shipping;

            Console.WriteLine($"Перевозка тары '{container.Label}': {shipping:F2} у.е.");
        }

        public void Clear()
        {
            shippingTotal = 0;
        }
    }

    public class DutyCalculator : ICalculator
    {
        private decimal dutyTotal = 0;

        private const decimal NormalRate = 0.2m;
        private const decimal LowerRate = 0.1m;

        public decimal FinalDuty => dutyTotal;

        public override void ProcessGoods(Goods goods)
        {
            decimal rate = CheckIfElectronics(goods) ? LowerRate : NormalRate;
            decimal duty = goods.Cost * rate;
            dutyTotal += duty;

            Console.WriteLine($"Сбор на '{goods.Title}' ({rate * 100}%): {duty:F2} у.е.");
        }

        public override void ProcessContainer(Container container)
        {
            decimal packageDuty = (decimal)container.Mass * 0.1m;
            dutyTotal += packageDuty;

            Console.WriteLine($"Сбор на упаковку '{container.Label}': {packageDuty:F2} у.е.");
        }

        private bool CheckIfElectronics(Goods goods)
        {
            string[] techWords = { "ноутбук", "телефон", "планшет", "мышь", "клавиатура", "наушники", "кабель" };
            string nameLower = goods.Title.ToLower();

            foreach (string word in techWords)
            {
                if (nameLower.Contains(word))
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            dutyTotal = 0;
        }
    }

    public class Purchase
    {
        private readonly List<IStoreItem> elements = new List<IStoreItem>();

        public void Include(IStoreItem element) => elements.Add(element);

        public decimal SumWithoutCharges()
        {
            decimal sum = 0;
            foreach (var element in elements)
            {
                sum += element.CalculatePrice();
            }
            return sum;
        }

        public void ApplyCalculator(ICalculator calculator)
        {
            foreach (var element in elements)
            {
                element.Apply(calculator);
            }
        }

        public void ShowDetails()
        {
            Console.WriteLine("\n=== ИТОГИ ПОКУПКИ ===");

            decimal itemsSum = SumWithoutCharges();
            Console.WriteLine($"Товары: {itemsSum:F2} у.е.");

            var dutyCalc = new DutyCalculator();
            ApplyCalculator(dutyCalc);
            Console.WriteLine($"Сборы: {dutyCalc.FinalDuty:F2} у.е.");

            var shippingCalc = new ShippingCostEstimator();
            ApplyCalculator(shippingCalc);
            Console.WriteLine($"Доставка: {shippingCalc.FinalShippingCost:F2} у.е.");

            decimal final = itemsSum + dutyCalc.FinalDuty + shippingCalc.FinalShippingCost;
            Console.WriteLine($"К ОПЛАТЕ: {final:F2} у.е.");
        }
    }

    internal class Program
    {
        static void RunProgram()
        {
            Console.WriteLine("=== Подсчёт расходов для покупки ===\n");

            var notebook = new Goods("Ноутбук", 1200m, 2.5);
            var mouse = new Goods("Мышь беспроводная", 20m, 0.2);
            var keyboard = new Goods("Клавиатура", 60m, 1.2);
            var headphones = new Goods("Наушники", 80m, 0.3);
            var cable = new Goods("Кабель USB", 8m, 0.1);
            var book = new Goods("Книга по программированию", 35m, 0.8);

            var tinyBox = new Container("Маленькая коробка", 0.2);
            var mediumBox = new Container("Средняя коробка", 0.5);
            var bigBox = new Container("Большая коробка", 1.0);

            tinyBox.PutInside(mouse);
            tinyBox.PutInside(keyboard);

            mediumBox.PutInside(tinyBox);
            mediumBox.PutInside(headphones);

            bigBox.PutInside(mediumBox);
            bigBox.PutInside(notebook);

            var myPurchase = new Purchase();
            myPurchase.Include(bigBox);
            myPurchase.Include(cable);
            myPurchase.Include(book);

            decimal itemsOnly = myPurchase.SumWithoutCharges();
            Console.WriteLine($"Сумма за товары: {itemsOnly:F2} у.е.");

            Console.WriteLine("\n--- Сборы ---");
            var dutyCalc = new DutyCalculator();
            myPurchase.ApplyCalculator(dutyCalc);
            Console.WriteLine($"Всего сборов: {dutyCalc.FinalDuty:F2} у.е.");

            Console.WriteLine("\n--- Доставка ---");
            var shipCalc = new ShippingCostEstimator();
            myPurchase.ApplyCalculator(shipCalc);
            Console.WriteLine($"Доставка всего: {shipCalc.FinalShippingCost:F2} у.е.");

            myPurchase.ShowDetails();

            Console.WriteLine("\n=== Ещё один пример ===");

            dutyCalc.Clear();
            shipCalc.Clear();

            var simplePurchase = new Purchase();
            simplePurchase.Include(new Goods("Смартфон", 600m, 0.3));
            simplePurchase.Include(new Container("Коробка для телефона", 0.3));

            simplePurchase.ApplyCalculator(dutyCalc);
            simplePurchase.ApplyCalculator(shipCalc);

            Console.WriteLine($"Сборы в простом заказе: {dutyCalc.FinalDuty:F2} у.е.");
            Console.WriteLine($"Доставка простого заказа: {shipCalc.FinalShippingCost:F2} у.е.");
        }

        static void Main()
        {
            RunProgram();
        }
    }
}
