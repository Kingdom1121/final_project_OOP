# BÁO CÁO TIẾN ĐỘ TUẦN 1 — ĐỒ ÁN CUỐI KỲ

> **Học phần:** Lập trình Hướng đối tượng (Object-Oriented Programming)  
> **Mã lớp học phần:** `OOP230179_Dot1` — HK1 2026-2027  
> **Giảng viên hướng dẫn (GVHD):** ThS. Huỳnh Xuân Phụng  
> **Đề tài:** Online Bookstore / E-commerce Platform  
> **Kho lưu trữ (Repository):** [Kingdom1121/OOP_OnlineBookstore_Final_Project](https://github.com/Kingdom1121/OOP_OnlineBookstore_Final_Project)  
> **Trạng thái:** Đã cập nhật và chỉnh sửa theo yêu cầu đề tài

## 1. Giới thiệu đề tài

Dự án này xây dựng phần xử lý cốt lõi (Core Engine) cho một hệ thống bán sách trực tuyến (Online Bookstore) đơn giản, mô phỏng các thao tác quen thuộc: chọn sản phẩm, quản lý giỏ hàng, áp dụng khuyến mãi và tính tiền thanh toán.

Hệ thống được tổ chức xoay quanh 5 class chính:
- **`Product`**: Quản lý thông tin sản phẩm (sách, văn phòng phẩm), gồm các thuộc tính `ProductId`, `Name`, `Type`, `Price`, và số lượng tồn kho `Stock`.
- **`OrderItem`**: Đại diện cho một món hàng được chọn, gồm `Product`, số lượng mua `Quantity` và hàm tính thành tiền `GetTotalPrice()`.
- **`Cart`**: Giỏ hàng của khách, lưu danh sách món `_items` và các hàm thao tác: thêm món (`AddItem`), bớt/xóa món (`RemoveItem`), làm rỗng giỏ (`Clear`), tính tiền tạm tính (`GetSubTotal`).
- **`Order`**: Đơn hàng sau khi thanh toán thành công, lưu `OrderId`, `OrderDate`, `Items`, tiền tạm tính `SubTotal`, tiền giảm giá `DiscountAmount`, tiền thuế VAT `TaxAmount`, tổng thanh toán `FinalTotal` và hàm in hóa đơn `PrintOrderDetails()`.
- **`Customer`**: Thông tin người mua, gồm `CustomerId`, `Name`, giỏ hàng cá nhân `Cart` và danh sách các đơn đã đặt `OrderHistory`.

Mục tiêu chính của đề tài là áp dụng trực tiếp các nguyên lý OOP và Design Pattern đã học vào bài toán thực tế:
1. **Tính Đóng gói (Encapsulation):** Lớp `Cart` tự quản lý dữ liệu nội bộ của mình; danh sách `_items` được để `private` và chỉ cho bên ngoài đọc qua `IReadOnlyCollection`, tránh việc các lớp khác can thiệp sửa trực tiếp danh sách món hàng.
2. **Strategy Pattern (Đa hình & Open/Closed Principle):** Tách riêng logic giảm giá ra interface `IDiscountStrategy` với các chiến lược cụ thể (`PercentageDiscountStrategy`, `BuyOneGetOneDiscountStrategy`, `NoDiscountStrategy`). Nhờ vậy, sau này có thêm loại mã giảm giá mới chỉ cần viết thêm class mới mà không cần sửa code của hàm thanh toán.
3. **Quản lý tồn kho & Kiểm tra nghiệp vụ:** Lớp `CheckoutService` chịu trách nhiệm kiểm tra số lượng tồn kho (`Stock`) trước khi trừ hàng để chống bán vượt số lượng hiện có (Overselling), đồng thời tính đúng thuế VAT 10% và lưu đơn hàng vào lịch sử của khách.
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
