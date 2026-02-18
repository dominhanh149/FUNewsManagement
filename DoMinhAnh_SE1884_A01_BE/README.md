# FUNews Management System

## Cách chạy project

### 1. Yêu cầu
- .NET 6.0 SDK trở lên
- SQL Server
- Visual Studio 2022 hoặc VS Code

### 2. Cấu hình Database
- Mở file `Assignmen_PRN232_1/appsettings.json`
- Cập nhật connection string phù hợp với SQL Server của bạn
- Database sẽ tự động được tạo khi chạy project lần đầu (Code First)

### 3. Chạy Backend API
```bash
cd Assignmen_PRN232_1
dotnet run
```
Backend sẽ chạy tại: `https://localhost:7053`

### 4. Chạy Frontend MVC
```bash
cd Frontend
dotnet run
```
Frontend sẽ chạy tại: `https://localhost:7024`

### 5. Đăng nhập
**Default Admin Account:**
- Email: admin@FUNewsManagementSystem.org
- Password: @@abc123@@
- Role: Admin

## Danh sách API

### SystemAccounts API
**Base URL:** `/api/systemaccounts`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/` | Admin | Lấy danh sách account (phân trang) |
| GET | `/all` | Admin | Lấy tất cả account |
| GET | `/{id}` | Admin | Lấy account theo ID |
| POST | `/create-or-edit` | Admin | Tạo hoặc cập nhật account |
| DELETE | `/{id}` | Admin | Xóa account |
| POST | `/login` | Public | Đăng nhập |
| POST | `/logout` | Public | Đăng xuất |

**Query Parameters (GET /):**
- `PageIndex`: Trang hiện tại
- `PageSize`: Số bản ghi mỗi trang
- `Items`: Tìm kiếm theo tên hoặc email
- `AccountRole`: Lọc theo role (0=Admin, 1=Staff, 2=Lecturer)

---

### NewsArticles API
**Base URL:** `/api/newsarticles`

| Method | Endpoint             | Auth   | Description                                                |
| ------ | -------------------- | ------ | ---------------------------------------------------------- |
| GET    | `/`                  | Staff  | Lấy danh sách bài viết (phân trang)                        |
| POST   | `/paging`            | Staff  | Lấy danh sách bài viết (phân trang) – gửi filter bằng body |
| POST   | `/public/paging`     | Public | Lấy bài viết công khai (phân trang)                        |
| GET    | `/{id}`              | Public | Lấy bài viết theo ID                                       |
| POST   | `/`                  | Staff  | Tạo bài viết (set `CreatedById` = current user)            |
| PUT    | `/{id}`              | Staff  | Cập nhật bài viết (set `UpdatedById`, `ModifiedDate`)      |
| DELETE | `/{id}`              | Staff  | Xóa bài viết                                               |
| POST   | `/{id}/duplicate`    | Staff  | Nhân bản bài viết                                          |
| POST   | `/{id}/tags/{tagId}` | Staff  | Thêm tag vào bài viết                                      |
| DELETE | `/{id}/tags/{tagId}` | Staff  | Xóa tag khỏi bài viết                                      |
| PUT    | `/{id}/tags`         | Staff  | Set nhiều tag cho bài viết (1 request)                     |
|

**Query Parameters (GET /):**
- `PageIndex`: Trang hiện tại
- `PageSize`: Số bản ghi mỗi trang
- `Title`: Tìm kiếm theo tiêu đề
- `Author`: Tìm kiếm theo tác giả
- `CategoryId`: Lọc theo category
- `Status`: Lọc theo trạng thái (true/false)
- `FromDate`: Lọc từ ngày
- `ToDate`: Lọc đến ngày
- `CreatedById`: Lọc theo người tạo

---

### Categories API
**Base URL:** `/api/categories`

| Method | Endpoint               | Auth         | Description                                                    |
| ------ | ---------------------- | ------------ | -------------------------------------------------------------- |
| GET    | `/`                    | Staff, Admin | Lấy tất cả categories (kèm article count)                      |
| GET    | `/{id}`                | Staff, Admin | Lấy category theo ID (kèm article count)                       |
| POST   | `/search`              | Staff, Admin | Tìm kiếm categories (phân trang + filter)                      |
| GET    | `/active`              | Public       | Lấy tất cả category đang active (dùng cho dropdown, public UI) |
| GET    | `/parents`             | Staff, Admin | Lấy danh sách parent categories (không có parent)              |
| GET    | `/{parentId}/children` | Staff, Admin | Lấy danh sách child categories của 1 parent                    |
| GET    | `/{id}/with-children`  | Staff, Admin | Lấy category kèm toàn bộ children                              |
| GET    | `/{id}/article-count`  | Staff, Admin | Lấy số lượng bài viết trong category                           |
| POST   | `/`                    | Staff, Admin | Tạo category mới                                               |
| PUT    | `/{id}`                | Staff, Admin | Cập nhật category theo ID                                      |
| DELETE | `/{id}`                | Staff, Admin | Xóa category (không xóa nếu có articles/children)              |
| PATCH  | `/toggle-status`       | Staff, Admin | Bật/tắt trạng thái active của category                         |


**Query Parameters (GET /):**
- `PageIndex`: Trang hiện tại
- `PageSize`: Số bản ghi mỗi trang
- `Keyword`: Tìm kiếm theo tên

---

### Tags API
**Base URL:** `/api/tags`

| Method | Endpoint                  | Auth         | Description                                             |
| ------ | ------------------------- | ------------ | ------------------------------------------------------- |
| GET    | `/`                       | Staff, Admin | Lấy tất cả tags                                         |
| GET    | `/{id}`                   | Staff, Admin | Lấy tag theo ID                                         |
| GET    | `/{id}/articles`          | Staff, Admin | Lấy tag kèm danh sách bài viết liên quan                |
| POST   | `/`                       | Staff, Admin | Tạo tag mới                                             |
| PUT    | `/{id}`                   | Staff, Admin | Cập nhật tag theo ID                                    |
| DELETE | `/{id}`                   | Staff, Admin | Xóa tag theo ID                                         |
| POST   | `/search`                 | Staff, Admin | Search tags (phân trang + filter)                       |
| GET    | `/{id}/articles-list`     | Staff, Admin | Lấy danh sách bài viết theo tag ID (dạng list đơn giản) |
| GET    | `/by-article/{articleId}` | Staff, Admin | Lấy danh sách tags theo article ID                      |
| GET    | `/{id}/exists`            | Staff, Admin | Kiểm tra tag có tồn tại không                           |
| GET    | `/name-available`         | Staff, Admin | Kiểm tra tên tag có khả dụng không (unique)             |

**Query Parameters (GET /):**
- `PageIndex`: Trang hiện tại
- `PageSize`: Số bản ghi mỗi trang
- `Keyword`: Tìm kiếm theo tên

---

### Report API
**Base URL:** `/api/report`

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/news-article-report` | Admin | Lấy báo cáo thống kê bài viết |

**Query Parameters:**
- `FromDate`: Từ ngày
- `ToDate`: Đến ngày

---

## Phân quyền

### Admin
- Quản lý System Account
- Quản lý News Article (tất cả)
- Quản lý Category
- Quản lý Tag
- Xem Report

### Staff
- Quản lý News Article (tất cả)
- Xem My Articles (chỉ bài viết của mình)
- Quản lý Category
- Quản lý Tag

### Public
- Xem Public News (bài viết có category active)

---

## Cấu trúc Project

```
PRN232_Ass1/
├── Assignmen_PRN232_1/                    # Backend API (.NET Web API)
│   ├── Controllers/                       # API Controllers
│   ├── Data/
│   │   └── AppDbContext.cs                # EF Core DbContext
│   ├── Dto/                               # Data Transfer Objects
│   ├── Enums/
│   │   ├── AccountRole.cs                 # Role enum (Admin, Staff, Lecturer)
│   │   └── ApiStatusCode.cs               # API status codes
│   ├── Extensions/
│   │   └── ServicesRegister.cs            # Dependency Injection registration
│   ├── Models/                            # Entity Models
│   ├── Repositories/                      # Repository + UnitOfWork
│   ├── Services/                          # Business Logic Layer
│   ├── appsettings.json                   # Backend configuration
│   ├── Program.cs                         # Application entry point
│   └── Assignmen_PRN232_1.http             # API testing file
└── DoMinhAnh_SE1884_A01_FE/               # Frontend MVC (.NET MVC)
    ├── Controllers/                       # MVC Controllers
    ├── Models/                            # ViewModels
    ├── Views/                             # Razor Views
    ├── wwwroot/                           # Static files
    │   ├── css/                           # Stylesheets
    │   ├── js/                            # Client-side scripts
    │   ├── lib/                           # Client libraries
    │   └── favicon.ico
    ├── appsettings.json                   # Frontend configuration (API URL)
    └── Program.cs                         # MVC startup & cookie auth


```

---

## Tính năng chính

1. **Authentication & Authorization**: Cookie-based authentication với role-based authorization
2. **News Article Management**: CRUD, search, filter, duplicate, tag management
3. **Category Management**: CRUD với validation (không xóa nếu có bài viết)
4. **Tag Management**: CRUD với validation (không xóa nếu đang được sử dụng)
5. **System Account Management**: CRUD với validation email unique
6. **Report**: Thống kê bài viết theo category và author
7. **My Articles**: Staff xem lịch sử bài viết của mình
8. **Public News**: Hiển thị bài viết công khai cho người dùng chưa đăng nhập
