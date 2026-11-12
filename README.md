# EduSubmit - Bài 2: RESTful API cho tương tác client-server

Ứng dụng mẫu gồm API ASP.NET Core Minimal API và client Console App trên .NET 10. Dữ liệu được lưu trong bộ nhớ của tiến trình API; khi khởi động lại server, các lượt nộp sẽ được xóa.

## Cấu trúc solution

```text
RestfulClientServer/
├── RestfulClientServer.slnx
├── .gitignore
├── requests.http
├── README.md
└── src/
    ├── Submission.Api/
    │   ├── Program.cs
    │   └── Submission.Api.csproj
    └── Submission.Client/
        ├── Program.cs
        └── Submission.Client.csproj
```

## Khởi tạo từ đầu bằng CLI

Yêu cầu .NET SDK 10. Chạy các lệnh sau trong thư mục muốn chứa solution:

```powershell
New-Item -ItemType Directory -Force RestfulClientServer
Set-Location RestfulClientServer
dotnet new sln --name RestfulClientServer --format slnx
New-Item -ItemType Directory -Force src
dotnet new web --name Submission.Api --output src\Submission.Api --framework net10.0
dotnet new console --name Submission.Client --output src\Submission.Client --framework net10.0
dotnet sln RestfulClientServer.slnx add src\Submission.Api\Submission.Api.csproj src\Submission.Client\Submission.Client.csproj
```

Sau khi khởi tạo, thay nội dung các tệp `Program.cs` bằng phần triển khai trong solution này và tạo `requests.http`, `README.md`, `.gitignore` ở thư mục gốc.

## Chạy ứng dụng

Mở hai terminal tại thư mục gốc `RestfulClientServer`.

**Terminal 1 - chạy API** (cố định tại `http://localhost:5000`):

```powershell
dotnet run --project src\Submission.Api
```

**Terminal 2 - chạy client**:

```powershell
dotnet run --project src\Submission.Client
```

Có thể truyền URL gốc khác làm tham số đầu tiên cho client:

```powershell
dotnet run --project src\Submission.Client -- http://localhost:5000
```

Client liệt kê bài tập, tạo một lượt nộp hợp lệ, đọc lại lượt nộp qua `Location`, rồi gửi lượt nộp rỗng. Ở nấc cuối, client xử lý mã 400 và parse `ApiError` thay vì để exception làm chương trình dừng bất thường.

## API contract

| Phương thức | URI | Kết quả |
|---|---|---|
| `GET` | `/` | `200`, trạng thái dịch vụ |
| `GET` | `/api/assignments` | `200`, danh sách bài tập |
| `GET` | `/api/assignments/{assignmentId}` | `200` hoặc `404 ASSIGNMENT_NOT_FOUND` |
| `POST` | `/api/assignments/{assignmentId}/submissions` | `201` hoặc lỗi nghiệp vụ `400`/`404` |
| `GET` | `/api/submissions/{submissionId}` | `200` hoặc `404 SUBMISSION_NOT_FOUND` |

Body tạo lượt nộp chỉ nhận `studentCode` và `repositoryUrl`. Server tự tạo `id`, `status` (`received`) và `receivedAt` (UTC). Mọi lỗi nghiệp vụ có cấu trúc `{ "code": "...", "message": "..." }`.

## Kiểm thử bằng REST Client

Cài extension **REST Client** trong VS Code, mở `requests.http`, chạy API rồi lần lượt chọn **Send Request**. Tệp có đúng 7 request; request tạo lượt nộp tên `createSubmission` truyền ID trả về trực tiếp sang request GET tiếp theo bằng `{{createSubmission.response.body.$.id}}`.

### Tổng hợp ca kiểm thử

| Mã HTTP | Ca kiểm thử | Kết quả mong đợi |
|---|---|---|
| `200 OK` | Health check, danh sách/chi tiết bài tập, đọc lượt nộp vừa tạo | Trả về JSON hợp lệ |
| `201 Created` | Nộp vào bài tập đang mở với dữ liệu hợp lệ | Body có ID do server sinh và header `Location` |
| `400 Bad Request` | Nộp cho bài đã đóng hoặc thiếu `StudentCode`/`RepositoryUrl` | `ApiError` lần lượt `ASSIGNMENT_CLOSED` hoặc `INVALID_SUBMISSION` |
| `404 Not Found` | Bài tập hoặc lượt nộp không tồn tại | `ApiError` `ASSIGNMENT_NOT_FOUND` hoặc `SUBMISSION_NOT_FOUND` |
| `405 Method Not Allowed` | Dùng phương thức không được khai báo cho URI, ví dụ `PUT /api/assignments` | Framework từ chối phương thức; không phải lỗi nghiệp vụ |
| `415 Unsupported Media Type` | Gửi POST body JSON nhưng không đặt `Content-Type: application/json` (hoặc dùng content type không hỗ trợ) | Framework từ chối định dạng body trước khi chạy logic endpoint |

Các mã `405` và `415` là ca phá thử của HTTP framework, không nằm trong 7 request nghiệp vụ cơ bản bên trên. Với `415`, cần gửi một body không rỗng và cố ý bỏ/sai `Content-Type`; body rỗng có thể được xử lý khác tùy cách client tạo request.

## Câu hỏi lý thuyết

### 1. Vì sao URI dùng danh từ, còn hành động do phương thức HTTP đảm nhận?

URI xác định **tài nguyên**; HTTP method xác định **thao tác** trên tài nguyên đó. Cách phân tách này giúp API có URI nhất quán, dễ khám phá, và tận dụng đúng ngữ nghĩa chuẩn của HTTP như an toàn của `GET` hay tạo mới bằng `POST`.

Ví dụ cùng tập trung vào tài nguyên lượt nộp:

```http
GET  /api/submissions/{id}                    # đọc một lượt nộp
POST /api/assignments/{assignmentId}/submissions # tạo lượt nộp thuộc bài tập
```

Không cần tạo các URI động từ như `/getSubmission/{id}` hay `/createSubmission`; method đã biểu đạt hành động.

### 2. "Stateless" thể hiện ở đâu trong code đã viết?

Mỗi request chứa đủ thông tin cần để server xử lý request đó: route chứa ID bài tập, còn body POST chứa mã sinh viên và URL repository. Server không dựa vào phiên làm việc hay trạng thái hội thoại được giữ riêng cho từng client; client cũng không cần gửi các giá trị do server quản lý như `Id`, `Status`, `ReceivedAt`.

Ví dụ API xử lý một request độc lập bằng route và body:

```csharp
app.MapPost("/api/assignments/{assignmentId:int}/submissions",
    (int assignmentId, CreateSubmissionRequest request) =>
{
    // Tìm bài tập từ route và kiểm tra dữ liệu trong request.
});
```

Danh sách `submissions` trong ví dụ chỉ là kho dữ liệu RAM dùng chung của dịch vụ, không phải session state. Tính stateless không đồng nghĩa với việc server không lưu tài nguyên; nó có nghĩa là server không lưu ngữ cảnh client để hiểu request kế tiếp. Khởi động lại API sẽ làm mất dữ liệu vì bài thực hành không dùng cơ sở dữ liệu.

### 3. Vì sao client và server mỗi bên khai báo DTO riêng mà vẫn khớp nhau?

DTO là hợp đồng trao đổi qua JSON, không phải kiểu .NET phải được chia sẻ chung giữa hai project. Client và server có thể khai báo type riêng, miễn tên thuộc tính, cấu trúc JSON và kiểu dữ liệu tương thích. Cấu hình JSON web mặc định của ASP.NET Core serialize tên thuộc tính theo camelCase, vì vậy `StudentCode` được gửi thành `studentCode`; client deserialize lại không phân biệt hoa thường theo mặc định.

Ví dụ request JSON:

```json
{
  "studentCode": "SV001",
  "repositoryUrl": "https://github.com/example/edusubmit"
}
```

Server nhận bằng `CreateSubmissionRequest`, client gửi bằng bản sao `CreateSubmissionRequest` của chính client. Việc tách DTO còn giúp client chỉ gửi hai trường đầu vào, không thể quyết định các trường do server sở hữu như ID, trạng thái hoặc thời điểm nhận. Với hệ thống lớn hơn, hợp đồng có thể được mô tả bằng OpenAPI và sinh client, nhưng hai phía vẫn độc lập ở thời điểm chạy.
