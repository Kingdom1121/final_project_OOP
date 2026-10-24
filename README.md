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

    ---

# BÁO CÁO TIẾN ĐỘ TUẦN 2 — CẬP NHẬT & TỐI ƯU

> **Nội dung thực hiện:** Tiếp thu nhận xét GVHD, bổ sung các test case kiểm thử trường hợp biên và tích hợp sơ đồ lớp UML.
### Bảng phân công nhiệm vụ

| STT | Họ và tên | MSSV | Vai trò | Nhiệm vụ đảm nhiệm |
| :---: | :--- | :---: | :---: | :--- |
| 1 | **Nguyễn Trọng Đại** | 25110015 | Nhóm trưởng | - Thiết kế sơ đồ lớp UML (Class Diagram), chuẩn hóa quan hệ giữa các entity và Strategy Pattern.<br>- Review, kiểm tra logic các test case trên file `Program.cs` và quản lý cấu trúc repository. |
| 2 | **Phan Hữu Tình** | 25110065 | Thành viên | - Cài đặt bổ sung 3 test case mới vào mã nguồn (Giỏ hàng rỗng, Mua vét kho, BOGO số lẻ).<br>- Thực hiện kiểm thử thực nghiệm trên console và commit trực tiếp các thay đổi lên GitHub. |

## 1. Các nội dung đã chỉnh sửa & bổ sung trong mã nguồn (Tuần 2)

Dựa trên yêu cầu tăng cường kiểm thử và thể hiện tiến độ qua các commit, mã nguồn được mở rộng thêm 3 test case nghiệp vụ chuyên sâu trong `Program.cs` (có đánh dấu chú thích `// [BỔ SUNG TUẦN 2]`):

- **Kiểm thử trường hợp biên — Giỏ hàng rỗng (`Test Case 4`):**
  * Kịch bản: Khách hàng chưa chọn sản phẩm hoặc đã làm rỗng giỏ (`customer.Cart.Clear()`) nhưng gọi hàm thanh toán `checkoutService.ProcessCheckout(customer, noDiscount)`.
  * Xử lý: Hệ thống bắt đúng điều kiện `!cartItems.Any()` trong `CheckoutService`, ném ra ngoại lệ `InvalidOperationException` kèm thông báo *"Giỏ hàng đang trống, không thể thanh toán!"* và dừng giao dịch, ngăn việc tạo đơn hàng ảo.

- **Kiểm thử giá trị biên tồn kho — Mua vét kho (`Test Case 5`):**
  * Kịch bản: Đặt mua chính xác bằng số lượng tồn kho còn lại của sản phẩm (`Quantity = Product.Stock = 5`).
  * Xử lý: Đơn hàng thanh toán thành công, kiểm tra việc trừ kho tự động đưa số tồn kho còn lại về đúng bằng 0, không bị âm và không bị chặn nhầm.

- **Kiểm thử nghiệp vụ khuyến mãi — BOGO số lượng lẻ (`Test Case 6`):**
  * Kịch bản: Áp dụng chiến lược `BuyOneGetOneDiscountStrategy` cho khách mua 3 sản phẩm cùng loại (ví dụ mua 3 cây bút giá 20,000 đ/cây).
  * Xử lý: Kiểm chứng phép chia nguyên `item.Quantity / 2` (3 / 2 = 1 phần tử được miễn phí), số tiền giảm trừ chính xác là 20,000 đ (tổng tiền thanh toán là 2 cây bút + thuế VAT 10%), chứng minh công thức tính chiết khấu hoạt động đúng với cả số chẵn lẫn số lẻ.

---

## 2. Sơ đồ lớp (Class Diagram)

Hệ thống đã hoàn thiện sơ đồ quan hệ đối tượng và mô hình Strategy Pattern:

<img width="1920" height="1080" alt="UML_BOX" src="https://github.com/user-attachments/assets/d9733726-5625-41a6-a148-ad3faed43f4d" />


* **Chi tiết kiến trúc trên sơ đồ:**
  * **Quan hệ cấu thành (Composition/Aggregation):** `Customer` sở hữu `Cart` và danh sách `OrderHistory`; `Cart` và `Order` chứa danh sách các `OrderItem`; mỗi `OrderItem` tham chiếu đến một đối tượng `Product`.
  * **Strategy Pattern (Tính Đa hình & OCP):** Interface `IDiscountStrategy` được hiện thực bởi 3 lớp cụ thể (`PercentageDiscountStrategy`, `BuyOneGetOneDiscountStrategy`, `NoDiscountStrategy`). Lớp `CheckoutService` chỉ giao tiếp thông qua interface này mà không phụ thuộc vào lớp cụ thể nào.

---

## 3. Tổng kết kết quả chạy kiểm thử (6 Test Cases)

Toàn bộ 6 kịch bản kiểm thử trong `Program.cs` đã được chạy thực nghiệm và đạt kết quả mong đợi:
1. `Test Case 1 (Happy Path)`: Mua hàng, giảm giá 10%, trừ tồn kho và tính thuế VAT chính xác.
2. `Test Case 2 (Overselling Validation)`: Chặn thành công đơn hàng mua vượt tồn kho.
3. `Test Case 3 (Polymorphism Swap)`: Đổi chiến lược sang BOGO số lượng chẵn hoạt động trơn tru.
4. `Test Case 4 (Empty Cart)`: Chặn thanh toán giỏ hàng trống an toàn.
5. `Test Case 5 (Boundary Exact Stock)`: Mua vét kho đưa tồn kho về 0 chính xác.
6. `Test Case 6 (BOGO Odd Quantity)`: Tính đúng tiền giảm giá khi mua số lượng lẻ.

## BÁO CÁO TIẾN ĐỘ TUẦN 3 — HOÀN THIỆN TÍNH NĂNG & KIỂM THỬ HỒI QUY

> **Nội dung thực hiện:** Thực hiện kiểm thử hồi quy 6 test case cũ theo chỉ đạo của GVHD, xây dựng giao diện tương tác Console I/O (chặn vượt tồn kho tức thì, xuất bảng hóa đơn & lịch sử đơn hàng), cập nhật sơ đồ lớp UML và biên soạn tài liệu Tự giải trình kiến trúc (`Explanatory.md`).

### Bảng phân công nhiệm vụ

| STT | Họ và tên | MSSV | Vai trò | Nhiệm vụ đảm nhiệm |
| :---: | :--- | :---: | :---: | :--- |
| 1 | **Nguyễn Trọng Đại** | 25110015 | Nhóm trưởng | - Cập nhật và chuẩn hóa sơ đồ lớp UML (`UML_BOX.pdf`), hoàn thiện bảng ký hiệu quan hệ thực thể.<br>- Soạn thảo tài liệu Tự giải trình kiến trúc (`Explanatory.md`) phân tích chi tiết Encapsulation, Polymorphism, Strategy Pattern và OCP. |
| 2 | **Phan Hữu Tình** | 25110065 | Thành viên | - Mở rộng mã nguồn `Program.cs`: Xây dựng giao diện Console I/O tương tác (Thêm hàng chặn vượt kho tức thì, Xóa/Bớt món, Thanh toán hóa đơn).<br>- Tối ưu và căn chỉnh bố cục hiển thị bảng Hóa đơn & Lịch sử đơn hàng (ASCII Table) chuẩn độ rộng từng ký tự, không bị lệch lề.<br>- Thực hiện kiểm thử hồi quy (Regression Testing) chạy lại 6 test case cũ, đảm bảo không phát sinh lỗi. |

---

### 1. Các nội dung đã chỉnh sửa & bổ sung (Tuần 3)

#### A. Kiểm thử hồi quy tự động (Regression Testing)
- **Tiếp thu ý kiến GVHD:** Trước khi đưa người dùng vào giao diện mua hàng mới, chương trình tự động chạy lại toàn bộ **6 ca kiểm thử cũ** (Tuần 1 & Tuần 2) để kiểm chứng không phát sinh lỗi hồi quy (No Regression):
  1. *Test 1 (Happy Path):* Mua sách + bút với chiến lược giảm giá 10%, assert đúng tiền trừ và tồn kho.
  2. *Test 2 (Overselling):* Bắt ngoại lệ khi cố tình mua vượt quá lượng tồn kho.
  3. *Test 3 (BOGO):* Kiểm tra tính đa hình của khuyến mãi Mua 1 Tặng 1 (mua 2 tính tiền 1).
  4. *Test 4 (Empty Cart):* Bắt ngoại lệ `InvalidOperationException` khi thanh toán giỏ hàng rỗng.
  5. *Test 5 (Boundary Test):* Mua vét sạch kho (tồn kho giảm chính xác về 0).
  6. *Test 6 (BOGO Odd Quantity):* Mua số lượng lẻ (mua 3 tặng 1 theo công thức `Quantity / 2`).

#### B. Mở rộng giao diện mua hàng tương tác Console I/O (`Program.cs`)
- **Chức năng 1 — Thêm sản phẩm:** Tích hợp cơ chế kiểm tra tồn kho tức thì (Input Validation). Nếu người dùng nhập số lượng cộng dồn vượt quá lượng hàng có trong kho, hệ thống lập tức từ chối và thông báo lỗi ngay tại màn hình chọn món, không đợi đến lúc checkout.
- **Chức năng 2 — Bớt / Xóa sản phẩm:** Cho phép bớt số lượng hoặc bỏ hẳn sản phẩm ra khỏi giỏ thông qua phương thức domain `RemoveItem()`.
- **Chức năng 3 — Thanh toán & Xuất hóa đơn dạng bảng:**
  - Lựa chọn linh hoạt giữa 3 chiến lược: Giảm giá 15%, Mua 1 Tặng 1 (BOGO) hoặc Không giảm giá.
  - Thiết kế bảng **HÓA ĐƠN XUẤT RA (ASCII Table)** căn chỉnh cố định đúng 93 ký tự: Hiển thị minh bạch cột Tiền giảm (ghi rõ số tiền kèm số lượng tặng của BOGO, món không đủ điều kiện ghi `0 đ`), đường viền dọc `|` thẳng hàng 100%.
  - Tự động ghi nhận và in bảng **LỊCH SỬ ĐƠN HÀNG (Order History)** chuẩn 99 ký tự hiển thị đầy đủ: Mã đơn, ngày giờ đặt, số món, tạm tính, giảm giá và tổng thanh toán.

#### C. Bổ sung tài liệu thiết kế & Tự giải trình OOP
- **Cập nhật sơ đồ UML (`UML_BOX.pdf`):** Chuẩn hóa toàn bộ các mũi tên quan hệ: *Composition* (Order chứa OrderItem), *Aggregation* (Cart chứa OrderItem), *Realization* (các chiến lược hiện thực hóa interface `IDiscountStrategy`) và *Dependency* (CheckoutService phụ thuộc chiến lược lúc gọi hàm).
  <img width="1396" height="787" alt="image" src="https://github.com/user-attachments/assets/f6e09a76-f0e5-4f27-a0b8-6f22c016efe3" />

- **Soạn thảo tài liệu `Explanatory.md`:** Giải trình chi tiết 4 nguyên lý trọng tâm gắn liền với các class và method cụ thể:
  1. *Encapsulation:* Bảo vệ toàn vẹn dữ liệu trong `Cart` qua biến `private readonly List<OrderItem> _items` và view chỉ đọc `IReadOnlyCollection`.
  2. *Polymorphism:* Đa hình động thông qua phương thức `CalculateDiscount(Cart)` của `IDiscountStrategy`.
  3. *Strategy Pattern:* Tách biệt thuật toán khuyến mãi thành các lớp riêng biệt, hoán đổi linh hoạt tại runtime.
  4. *Open/Closed Principle (OCP):* Mở rộng thêm loại khuyến mãi mới mà không cần chỉnh sửa mã nguồn cốt lõi trong `CheckoutService`.

---
