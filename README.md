# BÁO CÁO TIẾN ĐỘ TUẦN 1 — ĐỒ ÁN CUỐI KỲ

> **Học phần:** Lập trình Hướng đối tượng (Object-Oriented Programming)  
> **Mã lớp học phần:** `OOP230179_Dot1` — HK1 2026-2027  
> **Giảng viên hướng dẫn (GVHD):** ThS. Huỳnh Xuân Phụng  
> **Đề tài:** Online Bookstore / E-commerce Platform (Core Engine)  
> **Kho lưu trữ (Repository):** [Kingdom1121/OOP_OnlineBookstore_Final_Project](https://github.com/Kingdom1121/OOP_OnlineBookstore_Final_Project)  
> **Trạng thái:** Hoàn thành thiết kế hướng đối tượng cốt lõi, Strategy Pattern và kịch bản kiểm thử Tuần 1  

---

## 1. Giới thiệu đề tài

Dự án tập trung xây dựng phần lõi (Core Engine) của hệ thống thương mại điện tử bán sách trực tuyến (Online Bookstore), mô phỏng đầy đủ chu trình từ chọn hàng, quản lý giỏ hàng, xử lý tồn kho đến thanh toán và lưu lịch sử mua sắm.

Mục tiêu trọng tâm của dự án là hiện thực hóa chuẩn xác các nguyên lý lập trình hướng đối tượng (OOP) và Design Pattern:
1. **Domain Modeling:** Xây dựng mô hình thực thể rõ ràng gồm `Product`, `OrderItem`, `Cart`, `Order`, `Customer`.
2. **Strategy Pattern (Polymorphism & Open/Closed Principle):** Thiết kế cơ chế giảm giá linh hoạt, cho phép bổ sung thuật toán khuyến mãi mới mà không cần chỉnh sửa code xử lý thanh toán hiện có.
3. **Encapsulation & Inventory Tracking:** Giữ tính toàn vẹn dữ liệu giỏ hàng (`Cart`), kiểm soát số lượng tồn kho theo thời gian thực nhằm ngăn chặn tình trạng bán vượt kho (Overselling).

---

## 2. Các nội dung đã hiện thực & tối ưu trong mã nguồn

Dựa trên yêu cầu của đồ án và các nguyên lý OOP bắt buộc, các nội dung kỹ thuật cụ thể gồm:

- **Tính Đóng gói trong lớp `Cart` (Encapsulation):**
  * Khai báo danh sách món hàng `_items` dưới dạng `private readonly List<OrderItem>`.
  * Chỉ cung cấp quyền đọc dữ liệu ra bên ngoài thông qua thuộc tính `IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly()`.
  * Các tác vụ thêm sản phẩm (`AddItem`), bớt/xóa món (`RemoveItem`), dọn sạch (`Clear`) và tính tạm tính (`GetSubTotal`) được xử lý hoàn toàn bên trong lớp `Cart`, không cho phép các đối tượng bên ngoài tự ý can thiệp làm sai lệch dữ liệu giỏ hàng.

- **Cơ chế giảm giá với Strategy Pattern (Polymorphism & Open/Closed Principle):**
  * Thiết kế interface chuẩn `IDiscountStrategy` với hai phương thức: `CalculateDiscount(Cart cart)` và `GetDescription()`.
  * Cài đặt 3 chiến lược cụ thể độc lập:
    * `PercentageDiscountStrategy`: Giảm giá theo % trên tổng đơn hàng.
    * `BuyOneGetOneDiscountStrategy` (BOGO): Mua 2 tặng 1 (giảm 50% số lượng theo từng cặp sản phẩm).
    * `NoDiscountStrategy`: Không áp dụng khuyến mãi.
  * Lớp xử lý thanh toán `CheckoutService` nhận `IDiscountStrategy` qua tham số hàm (Dependency Injection ở cấp method). Khi cần thêm chính sách mới (ví dụ: Giảm giá cố định theo mã Voucher), chỉ cần tạo class mới kế thừa interface mà không cần sửa một dòng code nào trong `CheckoutService` hay `Order`.

- **Quản lý tồn kho và thanh toán (`CheckoutService`):**
  * **Chống bán quá tồn kho (Overselling rejection):** Kiểm tra điều kiện `item.Quantity > item.Product.Stock` trước khi thanh toán. Nếu số lượng mua vượt tồn kho, hệ thống ném ra `InvalidOperationException` và dừng giao dịch ngay lập tức.
  * **Trừ kho thực tế:** Tiến hành trừ số lượng tồn kho `Product.Stock` khi đơn hàng hợp lệ.
  * **Tính toán chi phí:** Tính tạm tính (SubTotal) $\to$ Trừ tiền giảm giá (ràng buộc không vượt quá SubTotal) $\to$ Cộng thuế VAT 10% $\to$ Tính tổng tiền thanh toán cuối cùng (FinalTotal).
  * **Lưu vết đơn hàng:** Tự động ghi nhận `Order` vào danh sách `OrderHistory` của khách hàng và làm rỗng giỏ hàng sau khi hoàn tất.

- **Kịch bản kiểm thử trong `Program.cs`:**
  * **Test Case 1 (Happy Path):** Mua sách và văn phòng phẩm, áp dụng giảm giá 10%, thanh toán thành công, kiểm tra tồn kho bị trừ đúng và in hóa đơn chi tiết.
  * **Test Case 2 (Validation/Error):** Đặt mua 8 cuốn sách khi kho chỉ còn 5 cuốn. Hệ thống từ chối thanh toán, bắt ngoại lệ và giữ an toàn dữ liệu giỏ hàng.
  * **Test Case 3 (Polymorphism Swap):** Hoán đổi linh hoạt sang chiến lược BOGO (Mua 1 Tặng 1) ở thời điểm chạy mà không cần sửa mã nguồn thanh toán.
  * **Kiểm tra Order History:** In danh sách các đơn hàng đã thanh toán thành công của khách hàng.

---
