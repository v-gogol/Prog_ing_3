using System;

namespace Gogol_Chain_of_Responsibility
{
    public class RefundCase
    {
        public int CaseNumber { get; }
        public decimal ClaimSum { get; }
        public string Explanation { get; }

        public RefundCase(int caseNumber, decimal claimSum, string explanation)
        {
            CaseNumber = caseNumber;
            ClaimSum = claimSum;
            Explanation = explanation;
        }
    }

    public abstract class CaseHandler
    {
        private CaseHandler nextInLine;

        public void LinkNext(CaseHandler nextHandler)
        {
            nextInLine = nextHandler;
        }

        public bool TryResolveCase(RefundCase refundCase)
        {
            if (CanTakeCase(refundCase))
            {
                ExecuteCase(refundCase);
                return true;
            }

            if (nextInLine != null)
            {
                Console.WriteLine($"Дело №{refundCase.CaseNumber} передано дальше.");
                return nextInLine.TryResolveCase(refundCase);
            }

            Console.WriteLine($"Дело №{refundCase.CaseNumber} не удалось разрешить.");
            return false;
        }

        protected abstract bool CanTakeCase(RefundCase refundCase);
        protected abstract void ExecuteCase(RefundCase refundCase);
    }

    public class OperatorHandler : CaseHandler
    {
        protected override bool CanTakeCase(RefundCase refundCase)
        {
            return refundCase.ClaimSum <= 1000m;
        }

        protected override void ExecuteCase(RefundCase refundCase)
        {
            Console.WriteLine(
                $"Оператор подтвердил возврат по делу №{refundCase.CaseNumber} на сумму {refundCase.ClaimSum}. " +
                $"(Основание: {refundCase.Explanation})");
        }
    }

    public class DepartmentHeadHandler : CaseHandler
    {
        protected override bool CanTakeCase(RefundCase refundCase)
        {
            return refundCase.ClaimSum > 1000m && refundCase.ClaimSum <= 10000m;
        }

        protected override void ExecuteCase(RefundCase refundCase)
        {
            Console.WriteLine(
                $"Начальник отдела подтвердил возврат по делу №{refundCase.CaseNumber} на сумму {refundCase.ClaimSum}. " +
                $"(Основание: {refundCase.Explanation})");
        }
    }

    public class AdministrationHandler : CaseHandler
    {
        protected override bool CanTakeCase(RefundCase refundCase)
        {
            return refundCase.ClaimSum > 10000m && refundCase.ClaimSum <= 50000m;
        }

        protected override void ExecuteCase(RefundCase refundCase)
        {
            Console.WriteLine(
                $"Администрация рассмотрела и утвердила возврат по делу №{refundCase.CaseNumber} " +
                $"на сумму {refundCase.ClaimSum}. (Основание: {refundCase.Explanation})");
        }
    }
    internal class Program
    {
        static void ProcessCase(CaseHandler firstHandler, RefundCase refundCase)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 70));
            Console.WriteLine(
                $"Начинаем работу с делом №{refundCase.CaseNumber} на сумму {refundCase.ClaimSum}. " +
                $"(Основание: {refundCase.Explanation})");

            bool resolved = firstHandler.TryResolveCase(refundCase);

            if (!resolved)
            {
                Console.WriteLine(
                    $"Дело №{refundCase.CaseNumber} требует дополнительного согласования " +
                    "или изменения процедуры.");
            }
        }

        static void Main()
        {
            CaseHandler operatorHandler = new OperatorHandler();
            CaseHandler deptHeadHandler = new DepartmentHeadHandler();
            CaseHandler adminHandler = new AdministrationHandler();

            operatorHandler.LinkNext(deptHeadHandler);
            deptHeadHandler.LinkNext(adminHandler);

            RefundCase minorCase = new RefundCase(
                caseNumber: 101,
                claimSum: 750m,
                explanation: "Не устроил цвет"
            );

            RefundCase standardCase = new RefundCase(
                caseNumber: 102,
                claimSum: 4500m,
                explanation: "Обнаружен недостаток"
            );

            RefundCase majorCase = new RefundCase(
                caseNumber: 103,
                claimSum: 22000m,
                explanation: "Партия с браком"
            );

            RefundCase specialCase = new RefundCase(
                caseNumber: 104,
                claimSum: 125000m,
                explanation: "Крупная поставка, нужен особый порядок"
            );

            ProcessCase(operatorHandler, minorCase);
            ProcessCase(operatorHandler, standardCase);
            ProcessCase(operatorHandler, majorCase);
            ProcessCase(operatorHandler, specialCase);

            Console.WriteLine("\nНажмите Enter для завершения...");
            Console.ReadLine();
        }
    }
}
