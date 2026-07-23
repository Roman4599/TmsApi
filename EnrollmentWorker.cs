

/*public class EnrollmentWorker
{
    //EnrollmentWorker keeps a reference to IEnrollmentService
        private readonly IEnrollmentService _enrollmentService;

    public EnrollmentWorker(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    public void ProcessBatch()
    {
        _enrollmentService.GetAllAsync();
    }
}
*/
// lab 4Exercise 2 Step B: Fix captive dependency with IServiceScopeFactory

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ProcessBatch()
    {
   
    using var scope = _scopeFactory.CreateScope();

    var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
    await svc.GetAllAsync();
    }
}

 
