# BÁO CÁO BÀI THỰC HÀNH 1: XÂY DỰNG WEB API CƠ BẢN

* **Họ và tên:** Lê Hoàng Quân
* **Mã sinh viên:** DTC245200572
* **Lớp:** Công nghệ Thông tin

---

## 1. Môi trường phát triển
![Environment Check](./Screenshot%202026-11-05%20165847.png)

---

## 2. Kết quả MepDemo (Bước 7)
![MepDemo Result](./Screenshot%202026-11-05%20164402.png)

### 2.1 Trả lời câu hỏi MepDemo
1. **Số worker xử lý ở MEP 3:** Mỗi bài nộp `sub-x` chỉ do **1 worker** duy nhất xử lý (các worker chia nhau lấy công việc từ hàng đợi).
2. **Số dịch vụ nhận ở MEP 4:** Một sự kiện gửi tới **tất cả** các dịch vụ đang đăng ký nhận tin (Pub/Sub).
3. **Mô tả 5 MEP:**
   * **Request-Response:** Client gửi yêu cầu và chờ Server trả về một phản hồi duy nhất.
   * **One-Way:** Client phát đi thông điệp mà không cần chờ phản hồi hay xác nhận từ phía nhận.
   * **Queue (Hàng đợi):** Thông điệp đưa vào hàng đợi trung gian và phân phối lần lượt cho các worker để cân bằng tải.
   * **Publish-Subscribe:** Sự kiện phát hành đến kênh chung và tự động phân phối đến toàn bộ dịch vụ đăng ký.
   * **Streaming:** Dữ liệu được truyền liên tục thành từng luồng thời gian thực giữa hai bên.

---

## 3. Kết quả BasicWebApi (Bước 8)
![BasicWebApi Result](./Screenshot%202026-11-05%20164251.png)

### 3.1 Trả lời câu hỏi đọc code (Mục 8.4)
1. **Thứ tự dòng lệnh `builder.Services.AddSingleton...` và `var app = builder.Build();`:**
   * Dòng `builder.Services.AddSingleton...` **phải đứng trước** `var app = builder.Build();`.
   * **Giải thích:** `builder.Build()` đóng gói ứng dụng dựa trên các dịch vụ đã đăng ký trong DI Container trước đó. Nếu đăng ký sau khi `Build()`, ứng dụng sẽ không nhận diện được dịch vụ.
2. **Nguồn gốc tham số trong `/hello/{name}?language=en`:**
   * `name` lấy từ **Route (đường dẫn URL)**.
   * `language` lấy từ **Query String (tham số sau dấu `?`)**.
3. **Vì sao POST trả về `201 Created` thay vì `200 OK`:**
   * Theo chuẩn RESTful, `201 Created` biểu thị yêu cầu đã tạo mới thành công một tài nguyên trên hệ thống (kèm theo header `Location`).

---

## 4. Kết quả MiniDistributedSystem (Bước 9)
![MiniDistributedSystem Result](./Screenshot%202026-11-05%20165007.png)

### 4.1 Trả lời câu hỏi đọc code (Mục 9)
1. **Các endpoint Client đã gọi:**
   * `GET /api/assignments` -> Mã trạng thái `200 OK`.
   * `POST /api/submissions` -> Mã trạng thái `201 Created` (hoặc `400 Bad Request` nếu thiếu dữ liệu).
2. **Sử dụng chung code & Thống nhất:**
   * Client **không** dùng chung code hay tham chiếu trực tiếp đến API.
   * Hai bên thống nhất qua **Hợp đồng API (API Contract)** gồm: giao thức HTTP, định dạng URL và cấu trúc JSON.
3. **Địa chỉ truyền qua `-- http://localhost:5000`:**
   * Giúp client linh hoạt thay đổi môi trường triển khai hoặc cổng port mà không cần biên dịch lại mã nguồn.

---

## 5. Thí nghiệm lỗi (Mục 10)

| # | Dự đoán | Kết quả thực tế | Đúng/Sai | Giải thích |
| :---: | :--- | :--- | :---: | :--- |
| **a** | Client không cần sửa code. | Client vẫn hiển thị đủ 3 bài tập. | Đúng | Client nhận dữ liệu động qua JSON từ API. |
| **b** | Client báo lỗi kết nối mạng. | `HttpRequestException` / `actively refused it`. | Đúng | Tiến trình Server bị tắt nên không thiết lập được kết nối HTTP. |
| **c** | API từ chối và trả mã lỗi. | API trả mã `400 Bad Request`. | Đúng | Máy chủ kiểm tra dữ liệu đầu vào và chặn khi phát hiện rỗng. |
| **d** | Truyền đúng tham số port mới khi chạy client. | Chạy thành công với `-- http://localhost:5050`. | Đúng | Tham số cấu hình giúp client kết nối linh hoạt đến đúng cổng API. |

---

## 6. Kết quả dự án cá nhân EduSubmit-DTC245200572 (Bước 11 & 12)
![EduSubmit Result](./Screenshot%202026-11-05%20165706.png)

* **Luồng thông điệp:**
  * `Client` --(HTTP GET /api/assignments)--> `Api` : Request-Response (200 OK)
  * `Client` --(HTTP POST /api/submissions)--> `Api` : Request-Response (201 Created / 400 Bad Request)

---

## 7. Bài học rút ra
1. Cần chú ý chuyển đúng thư mục làm việc (`cd`) trước khi chạy các lệnh `dotnet run`.
2. Kiểm tra kỹ cổng (port) đang lắng nghe để tránh lỗi xung đột kết nối khi gọi API.
3. Khởi tạo Git ở thư mục gốc giúp quản lý toàn bộ mã nguồn và báo cáo bài tập thống nhất.