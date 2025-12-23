using System;
using System.Collections.Generic;

namespace Command_Gogol
{

    public abstract class Command
    {
        public abstract void Execute();
        public abstract void Undo();
        protected bool isReversible = true;
    }

    public class Elevator
    {
        private int currentFloor;
        private bool isDoorOpen;
        private readonly int minFloor;
        private readonly int maxFloor;

        public int CurrentFloor
        {
            get => currentFloor;
            private set => currentFloor = value;
        }

        public bool IsDoorOpen => isDoorOpen;

        public Elevator(int minFloor = 1, int maxFloor = 10)
        {
            this.minFloor = minFloor;
            this.maxFloor = maxFloor;
            currentFloor = minFloor;
            isDoorOpen = false;
        }

        public void MoveUp()
        {
            if (currentFloor < maxFloor)
            {
                currentFloor++;
                Console.WriteLine($"Лифт поднялся на этаж {currentFloor}");
            }
            else
            {
                Console.WriteLine("Достигнут максимальный этаж");
            }
        }

        public void MoveDown()
        {
            if (currentFloor > minFloor)
            {
                currentFloor--;
                Console.WriteLine($"Лифт опустился на этаж {currentFloor}");
            }
            else
            {
                Console.WriteLine("Достигнут минимальный этаж");
            }
        }

        public void OpenDoor()
        {
            if (!isDoorOpen)
            {
                isDoorOpen = true;
                Console.WriteLine("Дверь лифта открыта");
            }
            else
            {
                Console.WriteLine("Дверь уже открыта");
            }
        }

        public void CloseDoor()
        {
            if (isDoorOpen)
            {
                isDoorOpen = false;
                Console.WriteLine("Дверь лифта закрыта");
            }
            else
            {
                Console.WriteLine("Дверь уже закрыта");
            }
        }

        public void PrintStatus()
        {
            Console.WriteLine($"\nТекущий статус:");
            Console.WriteLine($"Этаж: {currentFloor}");
            Console.WriteLine($"Дверь: {(isDoorOpen ? "Открыта" : "Закрыта")}");
        }
    }

    public class MoveUpCommand : Command
    {
        private readonly Elevator elevator;
        private int previousFloor;

        public MoveUpCommand(Elevator elevator)
        {
            this.elevator = elevator;
        }

        public override void Execute()
        {
            previousFloor = elevator.CurrentFloor;
            elevator.MoveUp();
        }

        public override void Undo()
        {
            if (previousFloor < elevator.CurrentFloor)
            {
                elevator.MoveDown();
            }
        }
    }

    public class MoveDownCommand : Command
    {
        private readonly Elevator elevator;
        private int previousFloor;

        public MoveDownCommand(Elevator elevator)
        {
            this.elevator = elevator;
        }

        public override void Execute()
        {
            previousFloor = elevator.CurrentFloor;
            elevator.MoveDown();
        }

        public override void Undo()
        {
            if (previousFloor > elevator.CurrentFloor)
            {
                elevator.MoveUp();
            }
        }
    }

    public class OpenDoorCommand : Command
    {
        private readonly Elevator elevator;
        private bool previousState;

        public OpenDoorCommand(Elevator elevator)
        {
            this.elevator = elevator;
        }

        public override void Execute()
        {
            previousState = elevator.IsDoorOpen;
            elevator.OpenDoor();
        }

        public override void Undo()
        {
            if (previousState != elevator.IsDoorOpen)
            {
                elevator.CloseDoor();
            }
        }
    }

    public class CloseDoorCommand : Command
    {
        private readonly Elevator elevator;
        private bool previousState;

        public CloseDoorCommand(Elevator elevator)
        {
            this.elevator = elevator;
        }

        public override void Execute()
        {
            previousState = elevator.IsDoorOpen;
            elevator.CloseDoor();
        }

        public override void Undo()
        {
            if (previousState != elevator.IsDoorOpen)
            {
                elevator.OpenDoor();
            }
        }
    }

    public class CommandHistory
    {
        private readonly Stack<Command> history = new Stack<Command>();
        private readonly Stack<Command> redoStack = new Stack<Command>();

        public void Push(Command command)
        {
            history.Push(command);
            redoStack.Clear();
        }

        public bool CanUndo()
        {
            return history.Count > 0;
        }

        public bool CanRedo()
        {
            return redoStack.Count > 0;
        }

        public void Undo()
        {
            if (history.Count > 0)
            {
                Command command = history.Pop();
                command.Undo();
                redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                Command command = redoStack.Pop();
                command.Execute();
                history.Push(command);
            }
        }

        public void Clear()
        {
            history.Clear();
            redoStack.Clear();
        }
    }

    public class LiftControl
    {
        private readonly Elevator elevator;
        private readonly CommandHistory history;

        public LiftControl(Elevator elevator)
        {
            this.elevator = elevator;
            history = new CommandHistory();
        }

        public void ExecuteCommand(Command command)
        {
            command.Execute();
            history.Push(command);
        }

        public void Undo()
        {
            if (history.CanUndo())
            {
                Console.WriteLine("\n--- Отмена последней команды ---");
                history.Undo();
            }
            else
            {
                Console.WriteLine("Нет команд для отмены");
            }
        }

        public void Redo()
        {
            if (history.CanRedo())
            {
                Console.WriteLine("\n--- Повтор последней отмененной команды ---");
                history.Redo();
            }
            else
            {
                Console.WriteLine("Нет команд для повтора");
            }
        }

        public void ShowStatus()
        {
            elevator.PrintStatus();
        }

        public void ShowHistoryInfo()
        {
            Console.WriteLine($"\nИстория: доступно отмен - {history.CanUndo()}, доступно повторов - {history.CanRedo()}");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Elevator elevator = new Elevator(1, 10);
            LiftControl controller = new LiftControl(elevator);

            Console.WriteLine("=== Система управления лифтом ===\n");

            controller.ShowStatus();

            Command openDoor = new OpenDoorCommand(elevator);
            Command closeDoor = new CloseDoorCommand(elevator);
            Command moveUp = new MoveUpCommand(elevator);
            Command moveDown = new MoveDownCommand(elevator);

            Console.WriteLine("\n--- Выполнение команд ---");
            controller.ExecuteCommand(openDoor);
            controller.ExecuteCommand(closeDoor);
            controller.ExecuteCommand(moveUp);
            controller.ExecuteCommand(moveUp);
            controller.ExecuteCommand(openDoor);

            controller.ShowStatus();
            controller.ShowHistoryInfo();

            Console.WriteLine("\n--- Отмена нескольких команд ---");
            controller.Undo(); 
            controller.Undo(); 
            controller.ShowStatus();

            controller.Redo();
            controller.ShowStatus();

            controller.Undo();
            controller.Undo();
            controller.Undo();
            controller.ShowStatus();

            controller.Undo();

            Console.WriteLine("\n--- Новая последовательность команд ---");
            controller.ExecuteCommand(moveUp);
            controller.ExecuteCommand(moveUp);
            controller.ExecuteCommand(openDoor);
            controller.ExecuteCommand(moveDown);
            controller.ShowStatus();

            Console.WriteLine("\n=== Программа завершена ===");
        }
    }
}
