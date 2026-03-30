using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private static List<TaskItem> tasks = new List<TaskItem>();
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public IActionResult Get()
        {
            //return Ok(tasks);
            return Ok(_taskService.GetAll());

        }

        [Authorize(Roles ="Admin")]
        [HttpPost("create-task")]
        public IActionResult Post(TaskItem task)
        {
            _taskService.Add(task);
            return Ok(_taskService.GetAll());

            //tasks.Add(task);
            //return Ok(tasks);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            //var task = tasks.FirstOrDefault(t => t.Id == id);

            var task = _taskService.GetById(id);
            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, TaskItem updatedTask)
        {
            var task = _taskService.Update(id, updatedTask);

            //var task = tasks.FirstOrDefault(t => t.Id == id);

            //if (task == null)
            //{
            //    return NotFound();
            //}

            //task.Title = updatedTask.Title;
            //task.Description = updatedTask.Description;
            //task.IsCompleted = updatedTask.IsCompleted;

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var task = _taskService.Delete(id);

            //var task = tasks.FirstOrDefault(t => t.Id == id);

            if(task)
                return Ok("Deleted Successfully");
                
            return NotFound();

            //tasks.Remove(task);


        }
    }
}
