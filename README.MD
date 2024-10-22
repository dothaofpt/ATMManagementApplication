1. Tạo project webapi: dotnet new wepapi -n ten_project
2. Cài đặt thư viện cho Entity FrameWork (data model)
 dotnet add package Pomelo.EntityFrameworkCore.MySql (Pomelo MySQL Provider)
dotnet add package Microsoft.EntityFrameWorkCore.Tools

3. Đồng bộ hóa với database (Tạo migration)
dotnet ef migrations add InitialCreate
(nếu bị lỗi câu lệnh trên thì: dotnet tool install --global dotnet -ef )

Cập nhật vào database
dotnet ef database update