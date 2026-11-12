var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

var assignments = new List<AssignmentDto>
{
    new(1, "RESTful API", "Xây dựng API quản lý bài tập và lượt nộp.", true),
    new(2, "Console Client", "Gọi API bằng HttpClient từ ứng dụng console.", true),
    new(3, "Bài tập đã đóng", "Bài tập này không còn nhận lượt nộp.", false)
};
var submissions = new List<SubmissionDto>();

app.MapGet("/", () => Results.Ok(new { service = "Submission.Api", status = "running" }));
app.MapGet("/api/assignments", () => Results.Ok(assignments));

app.MapGet("/api/assignments/{assignmentId:int}", (int assignmentId) =>
{
    var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);

    return assignment is null
        ? Results.NotFound(new ApiError("ASSIGNMENT_NOT_FOUND", $"Không tìm thấy bài tập có ID {assignmentId}."))
        : Results.Ok(assignment);
});

app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
    {
        var assignment = assignments.FirstOrDefault(item => item.Id == assignmentId);
        if (assignment is null)
        {
            return Results.NotFound(
                new ApiError("ASSIGNMENT_NOT_FOUND", $"Không tìm thấy bài tập có ID {assignmentId}."));
        }

        if (!assignment.IsOpen)
        {
            return Results.BadRequest(
                new ApiError("ASSIGNMENT_CLOSED", $"Bài tập có ID {assignmentId} đã đóng."));
        }

        if (string.IsNullOrWhiteSpace(request.StudentCode) ||
            string.IsNullOrWhiteSpace(request.RepositoryUrl))
        {
            return Results.BadRequest(
                new ApiError("INVALID_SUBMISSION", "StudentCode và RepositoryUrl không được để trống."));
        }

        var submission = new SubmissionDto(
            Guid.NewGuid(),
            assignmentId,
            request.StudentCode.Trim(),
            request.RepositoryUrl.Trim(),
            "received",
            DateTimeOffset.UtcNow);

        submissions.Add(submission);

        return Results.Created($"/api/submissions/{submission.Id}", submission);
    });

app.MapGet("/api/submissions/{submissionId:guid}", (Guid submissionId) =>
{
    var submission = submissions.FirstOrDefault(item => item.Id == submissionId);

    return submission is null
        ? Results.NotFound(new ApiError("SUBMISSION_NOT_FOUND", $"Không tìm thấy lượt nộp có ID {submissionId}."))
        : Results.Ok(submission);
});

app.Run();

record AssignmentDto(int Id, string Title, string Description, bool IsOpen);
record CreateSubmissionRequest(string StudentCode, string RepositoryUrl);
record SubmissionDto(
    Guid Id,
    int AssignmentId,
    string StudentCode,
    string RepositoryUrl,
    string Status,
    DateTimeOffset ReceivedAt);
record ApiError(string Code, string Message);
