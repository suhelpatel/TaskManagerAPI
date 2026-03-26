using TaskManagerAPI.Models;

namespace TaskManagerAPI.Interfaces
{
    public interface ITaskService
    {
        List<TaskItem> GetAll();
        TaskItem GetById(int id);
        TaskItem Add(TaskItem task);
        TaskItem Update(int id,TaskItem task);
        bool Delete(int id);
    }
}
