using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl, UriKind.Absolute) };

Console.WriteLine("Nấc 1 - Danh sách bài tập");
using var assignmentsResponse = await httpClient.GetAsync("/api/assignments");
if (!assignmentsResponse.IsSuccessStatusCode)
{
    await PrintApiErrorAsync(assignmentsResponse);
    return;
}

var assignments = await assignmentsResponse.Content.ReadFromJsonAsync<List<AssignmentDto>>();
if (assignments is not null)
{
    foreach (var assignment in assignments)
    {
        Console.WriteLine(
            $"#{assignment.Id}: {assignment.Title} | Mở: {assignment.IsOpen} | {assignment.Description}");
    }
}

Console.WriteLine();
Console.WriteLine("Nấc 2 - Tạo lượt nộp hợp lệ");
using var createResponse = await httpClient.PostAsJsonAsync(
    "/api/assignments/1/submissions",
    new CreateSubmissionRequest("SV001", "https://github.com/example/edusubmit"));

if (createResponse.StatusCode != HttpStatusCode.Created)
{
    await PrintApiErrorAsync(createResponse);
    return;
}

var createdSubmission = await createResponse.Content.ReadFromJsonAsync<SubmissionDto>();
if (createdSubmission is null)
{
    Console.WriteLine("Server trả về body rỗng hoặc không hợp lệ cho lượt nộp vừa tạo.");
    return;
}

var location = createResponse.Headers.Location;
Console.WriteLine($"ID: {createdSubmission.Id}");
Console.WriteLine($"Trạng thái: {createdSubmission.Status}");
Console.WriteLine($"Location: {location}");

if (location is null)
{
    Console.WriteLine("Response 201 không có header Location.");
    return;
}

Console.WriteLine();
Console.WriteLine("Nấc 3 - Đọc lại lượt nộp theo Location");
using var readResponse = await httpClient.GetAsync(location);
if (!readResponse.IsSuccessStatusCode)
{
    await PrintApiErrorAsync(readResponse);
    return;
}

var submission = await readResponse.Content.ReadFromJsonAsync<SubmissionDto>();
if (submission is not null)
{
    Console.WriteLine(
        $"ID: {submission.Id} | Bài tập: {submission.AssignmentId} | Sinh viên: {submission.StudentCode}");
    Console.WriteLine($"Repository: {submission.RepositoryUrl}");
    Console.WriteLine($"Trạng thái: {submission.Status} | Nhận lúc: {submission.ReceivedAt:O}");
}

Console.WriteLine();
Console.WriteLine("Nấc 4 - Gửi lượt nộp không hợp lệ (mong đợi 400)");
using var invalidResponse = await httpClient.PostAsJsonAsync(
    "/api/assignments/1/submissions",
    new CreateSubmissionRequest("", ""));

if (invalidResponse.StatusCode == HttpStatusCode.BadRequest)
{
    await PrintApiErrorAsync(invalidResponse);
}
else
{
    Console.WriteLine($"Mã HTTP không như mong đợi: {(int)invalidResponse.StatusCode} {invalidResponse.StatusCode}");
    if (!invalidResponse.IsSuccessStatusCode)
    {
        await PrintApiErrorAsync(invalidResponse);
    }
}

static async Task PrintApiErrorAsync(HttpResponseMessage response)
{
    Console.WriteLine($"HTTP: {(int)response.StatusCode} {response.StatusCode}");

    try
    {
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        if (error is not null)
        {
            Console.WriteLine($"Mã lỗi: {error.Code}");
            Console.WriteLine($"Thông điệp: {error.Message}");
            return;
        }
    }
    catch (JsonException)
    {
        // The server may return a non-ApiError body for framework-generated responses.
    }

    Console.WriteLine(await response.Content.ReadAsStringAsync());
}

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
