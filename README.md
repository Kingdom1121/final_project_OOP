# BÁO CÁO TIẾN ĐỘ TUẦN 1 — ĐỒ ÁN CUỐI KỲ

> **Học phần:** Lập trình Hướng đối tượng (Object-Oriented Programming)  
> **Mã lớp học phần:** `OOP230179_Dot1` — HK1 2026-2027  
> **Giảng viên hướng dẫn (GVHD):** ThS. Huỳnh Xuân Phụng  
> **Đề tài:** Online Bookstore / E-commerce Platform  
> **Kho lưu trữ (Repository):** [Kingdom1121/OOP_OnlineBookstore_Final_Project](https://github.com/Kingdom1121/OOP_OnlineBookstore_Final_Project)  
> **Trạng thái:** Đã cập nhật và chỉnh sửa theo yêu cầu đề tài

## 1. Giới thiệu đề tài

Đề tài hướng tới việc thiết kế kiến trúc cốt lõi (Core Engine) cho hệ thống bán lẻ sách trực tuyến (Online Bookstore / E-commerce Platform), tập trung mô phỏng toàn diện quy trình từ quản lý giỏ hàng, áp dụng các chính sách chiết khấu, kiểm soát tồn kho đến tính toán thanh toán đơn hàng.

Hệ thống được tổ chức xoay quanh 5 lớp thực thể chính:
- **`Product`**: Đại diện cho sản phẩm sách/văn phòng phẩm, gồm các thuộc tính `ProductId`, `Name`, `Type`, `Price`, và `Stock` (số lượng hàng tồn kho phục vụ kiểm soát bán hàng).
- **`OrderItem`**: Đại diện cho một dòng sản phẩm được chọn, gồm thuộc tính `Product`, `Quantity` (số lượng mua) và phương thức `GetTotalPrice()` để tính thành tiền của dòng đó.
- **`Cart`**: Giỏ hàng của khách hàng, chứa danh sách món hàng `_items`, hỗ trợ các thao tác nghiệp vụ: thêm món (`AddItem`), bớt/xóa món (`RemoveItem`), làm rỗng giỏ (`Clear`) và tính tạm tính (`GetSubTotal`).
- **`Order`**: Đơn hàng sau khi thanh toán thành công, lưu trữ `OrderId`, `OrderDate`, `Items`, `SubTotal` (tạm tính), `DiscountAmount` (tiền giảm giá), `TaxAmount` (thuế VAT 10%), `FinalTotal` (tổng tiền thanh toán cuối cùng) và phương thức `PrintOrderDetails()`.
- **`Customer`**: Khách hàng trong hệ thống, gồm `CustomerId`, `Name`, giỏ hàng cá nhân `Cart` và danh sách lịch sử các đơn đã mua `OrderHistory`.

Mục tiêu trọng tâm của dự án là thể hiện rõ nét các nguyên lý OOP và mẫu thiết kế:
1. **Tính Đóng gói (Encapsulation):** Lớp `Cart` tự đóng gói và bảo vệ danh sách hàng hóa bên trong (sử dụng `private` kết hợp `IReadOnlyCollection`), không để bên ngoài can thiệp trực tiếp vào danh sách.
2. **Strategy Pattern (Polymorphism & Open/Closed Principle):** Tách biệt thuật toán giảm giá khỏi luồng xử lý đơn hàng chính thông qua interface `IDiscountStrategy` (với các chiến lược `PercentageDiscountStrategy`, `BuyOneGetOneDiscountStrategy`, `NoDiscountStrategy`). Khi bổ sung chính sách khuyến mãi mới, chỉ cần thêm class mới mà không cần chỉnh sửa code của lớp thanh toán.
3. **Quản lý tồn kho & Kiểm soát nghiệp vụ (Inventory Tracking):** Lớp `CheckoutService` chủ động kiểm tra số lượng tồn kho trước khi thanh toán nhằm ngăn chặn triệt để tình trạng bán quá số lượng hiện có (Overselling), đồng thời tự động áp dụng công thức tính thuế VAT chuẩn và cập nhật lịch sử mua sắm cho khách hàng.
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
