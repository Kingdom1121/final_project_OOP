using System;

namespace OnlineBookstore
{
    class Program
    {
        static void Main(string[] args)
        {
            // Thiết lập mã hóa tiếng Việt cho Console 
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== HỆ THỐNG QUẢN LÝ E-COMMERCE / BOOKSTORE ===");

            // Khởi tạo dữ liệu sản phẩm (Có thiết lập sẵn Stock - Tồn kho)
            var book1 = new Product("P001", "Lập trình C# căn bản", "Sách", 150000m, 10);
            var book2 = new Product("P002", "Clean Code Clean Architecture", "Sách", 280000m, 5);
            var pen = new Product("P003", "Bút bi cao cấp", "Văn phòng phẩm", 20000m, 50);

            // Khởi tạo khách hàng
            var customer = new Customer("C001", "Nguyễn Văn An");

            // ==========================================
            // TEST CASE 1: HAPPY PATH (Luồng thanh toán thành công thông thường)
            // ==========================================
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("[TEST CASE 1] Happy Path: Thêm sản phẩm, áp giảm giá %, thanh toán thành công và trừ kho");
            Console.WriteLine("----------------------------------------");
            try
            {
                customer.Cart.AddItem(book1, 2); // Mua 2 cuốn sách C# (Tổng: 300,000 đ)
                customer.Cart.AddItem(pen, 3);    // Mua 3 cái bút (Tổng: 60,000 đ)

                IDiscountStrategy discount1 = new PercentageDiscountStrategy(10);
                Console.WriteLine($"Áp dụng: {discount1.GetDescription()}");

                var checkoutService = new CheckoutService(taxRate: 0.1m); // Thuế VAT 10%
                Order order1 = checkoutService.ProcessCheckout(customer, discount1);

                order1.PrintOrderDetails();
                Console.WriteLine($"Tồn kho còn lại của '{book1.Name}': {book1.Stock} (Đã trừ thành công)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }

            // ==========================================
            // TEST CASE 2: ERROR SCENARIO (Chặn mua vượt quá tồn kho - Chống Overselling)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("[TEST CASE 2] Error Path: Mua vượt quá tồn kho hiện tại -> Hệ thống từ chối (Reject)");
            Console.WriteLine("----------------------------------------");
            try
            {
                Console.WriteLine($"Yêu cầu đặt mua: 8 cuốn '{book2.Name}' (Tồn kho hiện tại chỉ có: {book2.Stock})");
                customer.Cart.AddItem(book2, 8);

                var checkoutService = new CheckoutService();
                IDiscountStrategy noDiscount = new NoDiscountStrategy();

                checkoutService.ProcessCheckout(customer, noDiscount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> KẾT QUẢ MONG ĐỢI (Đã bắt lỗi thành công): {ex.Message}\n");
                customer.Cart.Clear(); // Xóa giỏ hàng sau khi lỗi để reset trạng thái
            }

            // ==========================================
            // TEST CASE 3: POLYMORPHISM SCENARIO (Đổi chiến lược giảm giá linh hoạt)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("[TEST CASE 3] Polymorphism Scenario: Thay đổi chiến lược giảm giá sang Mua 1 Tặng 1 (BOGO)");
            Console.WriteLine("----------------------------------------");
            try
            {
                customer.Cart.AddItem(book1, 2); // Mua 2 cuốn sách C# (Giá: 150,000 * 2 = 300,000 đ)

                IDiscountStrategy bogoDiscount = new BuyOneGetOneDiscountStrategy();
                Console.WriteLine($"Áp dụng chiến lược mới: {bogoDiscount.GetDescription()}");

                var checkoutService = new CheckoutService(taxRate: 0.1m);
                Order order2 = checkoutService.ProcessCheckout(customer, bogoDiscount);

                order2.PrintOrderDetails();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }

            // ==========================================
            // // [BỔ SUNG TUẦN 2] TEST CASE 4: EDGE CASE (Thanh toán khi giỏ hàng rỗng)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("[TEST CASE 4] Validation: Thanh toán khi giỏ hàng trống -> Hệ thống từ chối");
            Console.WriteLine("----------------------------------------");
            try
            {
                // [BỔ SUNG TUẦN 2]: Kiểm tra bắt lỗi ném ngoại lệ khi giỏ hàng không có sản phẩm nào
                customer.Cart.Clear();
                var checkoutService = new CheckoutService();
                IDiscountStrategy noDiscount = new NoDiscountStrategy();

                checkoutService.ProcessCheckout(customer, noDiscount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> KẾT QUẢ MONG ĐỢI: {ex.Message}\n");
            }

            // ==========================================
            // // [BỔ SUNG TUẦN 2] TEST CASE 5: BOUNDARY TEST (Mua vừa đúng bằng số lượng tồn kho)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"[TEST CASE 5] Boundary: Mua vừa khít toàn bộ số lượng tồn kho hiện tại (Stock = {book2.Stock})");
            Console.WriteLine("----------------------------------------");
            try
            {
                // [BỔ SUNG TUẦN 2]: Kiểm tra mua vét sạch toàn bộ kho (tồn kho sau thanh toán phải giảm về đúng 0)
                int currentStock = book2.Stock;
                customer.Cart.AddItem(book2, currentStock); // Đặt mua đúng số lượng còn lại

                var checkoutService = new CheckoutService();
                IDiscountStrategy noDiscount = new NoDiscountStrategy();
                Order orderExactStock = checkoutService.ProcessCheckout(customer, noDiscount);

                Console.WriteLine($"Mua thành công {currentStock} cuốn '{book2.Name}'.");
                Console.WriteLine($"-> Tồn kho còn lại sau khi mua vét kho: {book2.Stock} (Khớp mong đợi)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }

            // ==========================================
            // // [BỔ SUNG TUẦN 2] TEST CASE 6: BOGO ODD QUANTITY (Mua 1 tặng 1 với số lượng lẻ)
            // ==========================================
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("[TEST CASE 6] BOGO Odd: Mua 3 tặng 1 (mua 3 cây bút chỉ tính tiền 2)");
            Console.WriteLine("----------------------------------------");
            try
            {
                // [BỔ SUNG TUẦN 2]: Mua 3 cây bút (giá 20,000 đ/cây). Công thức 3 / 2 = 1 cây được tặng -> Số tiền giảm đúng 20,000 đ
                customer.Cart.AddItem(pen, 3);

                IDiscountStrategy bogoDiscount = new BuyOneGetOneDiscountStrategy();
                var checkoutService = new CheckoutService(taxRate: 0.1m);
                Order orderBogoOdd = checkoutService.ProcessCheckout(customer, bogoDiscount);

                orderBogoOdd.PrintOrderDetails();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }

            // ==========================================
            // KIỂM TRA LỊCH SỬ ĐƠN HÀNG (Order History per Customer)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"LỊCH SỬ MUA HÀNG CỦA KHÁCH HÀNG: {customer.Name} (Tổng số đơn thành công: {customer.OrderHistory.Count})");
            Console.WriteLine("----------------------------------------");
            foreach (var histOrder in customer.OrderHistory)
            {
                Console.WriteLine($"- Mã đơn: {histOrder.OrderId} | Ngày: {histOrder.OrderDate:HH:mm:ss} | Tổng tiền: {histOrder.FinalTotal:N0} đ");
            }

            Console.WriteLine("\nNhấn phím bất kỳ để thoát...");
            Console.ReadKey();
        }
    }
}
