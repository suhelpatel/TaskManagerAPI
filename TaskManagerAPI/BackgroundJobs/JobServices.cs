namespace TaskManagerAPI.BackgroundJobs
{
    public class JobServices
    {
        public void WriteLog()
        {
            Console.WriteLine("🔥 Background job executed at: " + DateTime.Now);
        }
    }
}
