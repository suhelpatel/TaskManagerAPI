using System;

namespace ConsoleApp2_Learning_delete_later
{
    public delegate void Notify();
    class Program
    {
        //public delegate void Notify(string message);        

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // Day 1
            #region Day 1
            Employee emp = new Employee
            {
                Name = "John",
                Salary = 5000,
                Department = "IT"
            };

            Employee mgr = new Manager
            {
                Name = "Sarah",
                Salary = 8000,
                Department = "HR"
            };

            Console.WriteLine($"{emp.Name} ({emp.Department}) Bonus: {emp.CalculateBonus()}");
            Console.WriteLine($"{mgr.Name} ({mgr.Department}) Bonus: {mgr.CalculateBonus()}");

            emp.Work();
            mgr.Work();
            #endregion

            // Day 2
            #region Day 2
            //Notify notify = ShowMessage;
            //notify("Hello from delegate!!");

            //notify = SendEmail;
            //notify("Hello from Email!!");

            Process process = new Process();
            process.ProcessCompleted += ProcessCompletedHandler;

            process.StartProcess();

            static void ProcessCompletedHandler()
            {
                Console.WriteLine("Process Completed!");
            }
            #endregion
        }

        static void ShowMessage(string message)
        {
            Console.WriteLine($"{message}");
        }

        static void SendEmail(string message)
        {
            Console.WriteLine($"{message}", Console.ForegroundColor);
        }
    }
    

    class Employee
    {
        public string Name { get; set; }
        public int Salary { get; set; }
        public string Department { get; set; }

        public virtual void Work()
        {
            Console.WriteLine("Employee is Working");
        }

        public virtual double CalculateBonus()
        {
            return Salary * 0.10;
        }
    }

    class Manager : Employee
    {
        public override void Work()
        {
            Console.WriteLine("Manager is managing team");
        }

        public override double CalculateBonus()
        {
            return Salary * 0.20;
        }

        public void Work2()
        {
            Console.WriteLine("Manager is managing and working in team");
        }
    }

    class Process
    {
        public event Notify ProcessCompleted;

        public void StartProcess()
        {
            Console.WriteLine("Process Started...");

            // Simulate work
            System.Threading.Thread.Sleep(2000);

            OnProcessCompleted();
        }

        protected virtual void OnProcessCompleted()
        {
            ProcessCompleted?.Invoke();
        }
    }
}
