

public class EnrollmentWorker
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