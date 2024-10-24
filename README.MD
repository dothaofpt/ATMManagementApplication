1. Tạo project webapi:
dotnet new webapi -n ten_project 
(webapp, webapi, mvc)

2. Cài đặt thư viện cho Entity Framework (data model)
dotnet add package Pomelo.EntityFrameworkCore.MySql (Pomelo MySQL Provider)
dotnet add package Microsoft.EntityFrameworkCore.Tools
(nếu bị lỗi câu lệnh trên thì: dotnet tool intall --global dotnet-ef)

3. Đồng bộ hoá với Database (Tạo Migration)
dotnet ef migrations add InitialCreate

Cập nhận vào database 
dotnet ef database update 
