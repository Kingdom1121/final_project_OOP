# BÁO CÁO TIẾN ĐỘ TUẦN 1 — ĐỒ ÁN CUỐI KỲ

> **Học phần:** Lập trình Hướng đối tượng (Object-Oriented Programming)  
> **Mã lớp học phần:** `OOP230179_Dot1` — HK1 2026-2027  
> **Giảng viên hướng dẫn (GVHD):** ThS. Huỳnh Xuân Phụng  
> **Đề tài:** Online Bookstore / E-commerce Platform  
> **Kho lưu trữ (Repository):** [Kingdom1121/OOP_OnlineBookstore_Final_Project](https://github.com/Kingdom1121/OOP_OnlineBookstore_Final_Project)  
> **Trạng thái:** Đã cập nhật và chỉnh sửa theo yêu cầu đề tài

## 1. Giới thiệu đề tài

Đề tài hướng tới việc thiết kế kiến trúc cốt lõi (Core Engine) cho hệ thống bán lẻ sách trực tuyến, tập trung mô phỏng luồng giỏ hàng, thanh toán và khuyến mãi theo chuẩn mô hình lập trình hướng đối tượng.

Mục tiêu trọng tâm của dự án là thể hiện rõ ràng các nguyên lý OOP và mẫu thiết kế:
1. **Mô hình hóa thực thể:** Xây dựng đầy đủ các lớp `Product`, `OrderItem`, `Cart`, `Order`, `Customer` với trách nhiệm rõ ràng, tránh dồn việc vào một lớp lớn (God class).
2. **Strategy Pattern cho giảm giá:** Tách rời thuật toán tính chiết khấu ra khỏi luồng xử lý đơn hàng chính thông qua interface `IDiscountStrategy`, đảm bảo tính **Đa hình (Polymorphism)** và nguyên lý **Đóng/Mở (Open/Closed Principle)**.
3. **Đóng gói & Quản lý tồn kho:** Lớp `Cart` tự đóng gói và bảo vệ dữ liệu bên trong; luồng `CheckoutService` chủ động kiểm tra lượng hàng tồn kho (`Stock`) để ngăn chặn việc đặt hàng vượt mức (Overselling).

---

## 2. Các nội dung đã chỉnh sửa & tối ưu trong mã nguồn

Dựa trên mã nguồn ban đầu và các yêu cầu thiết kế của môn học, em đã rà soát và thực hiện các chỉnh sửa, tối ưu cụ thể sau:

- **Sửa lỗi tính toán công thức trong `Service.cs`:**
  * Khắc phục lỗi thiếu toán tử nhân và trừ khi tính số tiền giảm giá, tiền chịu thuế và tiền thuế VAT:
    * `subTotal * (_percentage / 100m)` thay vì bị dính chuỗi toán tử.
    * `decimal taxableAmount = subTotal - discountAmount`.
    * `decimal taxAmount = taxableAmount * _taxRate`.
  * Thêm điều kiện chặn tiền giảm giá không được vượt quá tiền hàng: `if (discountAmount > subTotal) discountAmount = subTotal;` để tránh trường hợp tổng tiền bị âm.

- **Tối ưu tính Đóng gói (Encapsulation) cho lớp `Cart`:**
  * Chuyển danh sách `_items` thành `private readonly List<OrderItem>`.
  * Không trả về trực tiếp danh sách nội bộ mà công khai qua thuộc tính chỉ đọc: `public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly()`. Điều này ngăn các lớp bên ngoài tự ý thêm, sửa, xóa phần tử mà bắt buộc phải thông qua các phương thức của `Cart` như `AddItem`, `RemoveItem`, `Clear`.

- **Cải tiến kiểm tra chống bán vượt kho (Overselling) trong `CheckoutService`:**
  * Chia việc xử lý tồn kho làm hai bước rõ ràng: bước 1 duyệt kiểm tra điều kiện an toàn (`item.Quantity > item.Product.Stock`), nếu vi phạm thì lập tức ném ngoại lệ `InvalidOperationException` và dừng toàn bộ giao dịch.
  * Chỉ khi toàn bộ sản phẩm trong giỏ hàng đều đủ số lượng thì mới chuyển sang bước 2 để trừ tồn kho thực tế, đảm bảo tính nhất quán của dữ liệu.

- **Đồng bộ hóa luồng xóa giỏ và cập nhật lịch sử:**
  * Sau khi đơn hàng được tạo thành công, tiến hành lưu đối tượng `Order` vào danh sách `OrderHistory` của khách hàng và gọi `customer.Cart.Clear()` để làm sạch giỏ hàng.
  * Trong kịch bản test gặp lỗi tồn kho (`Test Case 2`), bổ sung lệnh dọn giỏ hàng trong khối `catch` để reset trạng thái cho các bài test tiếp theo.

- **Định dạng hiển thị Console trong `Program.cs`:**
  * Thêm cấu hình `Console.OutputEncoding = System.Text.Encoding.UTF8;` ở đầu hàm `Main` để in hóa đơn, tên sách và tiếng Việt có dấu rõ ràng, không bị lỗi font trên màn hình console.
