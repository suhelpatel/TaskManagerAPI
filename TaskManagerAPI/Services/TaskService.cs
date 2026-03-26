using TaskManagerAPI.Data;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Services
{
    public class TaskService : ITaskService
    {
        private static List<TaskItem> tasks = new List<TaskItem>();
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public List<TaskItem> GetAll()
        {
            //return tasks;
            return _context.Tasks.ToList();
        }

        public TaskItem GetById(int id)
        {
            //return tasks.FirstOrDefault(t => t.Id == id);
            return _context.Tasks.FirstOrDefault(t => t.Id == id);
        }

        public TaskItem Add(TaskItem task)
        {
            _context.Tasks.Add(task);
            _context.SaveChanges();
            return task;

            //tasks.Add(task);
            //return task;
        }

        public TaskItem Update(int id, TaskItem updatedTask)
        {
            var task = _context.Tasks.Find(id);
            if (task == null) return null;

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.IsCompleted = updatedTask.IsCompleted;

            _context.SaveChanges();
            return task;

            //var task = tasks.FirstOrDefault(t => t.Id == id);
            //if (task == null) return null;

            //task.Title = updatedTask.Title;
            //task.Description = updatedTask.Description;
            //task.IsCompleted = updatedTask.IsCompleted;

            //return task;
        }

        public bool Delete(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return true;

            //var task = tasks.FirstOrDefault(t => t.Id == id);
            //if (task == null) return false;

            //tasks.Remove(task);
            //return true;
        }
    }
}
