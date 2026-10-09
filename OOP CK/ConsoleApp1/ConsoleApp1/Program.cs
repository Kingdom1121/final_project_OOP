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
            var book1 = new Product("P001", "Lập trình C# căn bản", "Sách", 150000m, 10);// sách lập trình
            var book2 = new Product("P002", "Clean Code Clean Architecture", "Sách", 280000m, 5);// sách kiến trúc code
            var pen = new Product("P003", "Bút bi cao cấp", "Văn phòng phẩm", 20000m, 50);// bút bi

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
                                                  // Tạm tính ban đầu = 360,000 đ

                // Sử dụng chiến lược giảm giá 10% (Polymorphism)[cite: 1]
                IDiscountStrategy discount1 = new PercentageDiscountStrategy(10);
                Console.WriteLine($"Áp dụng: {discount1.GetDescription()}");

                var checkoutService = new CheckoutService(taxRate: 0.1m); // Thuế VAT 10%
                Order order1 = checkoutService.ProcessCheckout(customer, discount1);

                order1.PrintOrderDetails();
                Console.WriteLine($"Tồn kho còn lại của '{book1.Name}': {book1.Stock} (Đã trừ thành công)");// trừ hàng tồn kho
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }

            // ==========================================
            // TEST CASE 2: ERROR / VALIDATION SCENARIO (Chặn mua vượt quá tồn kho - Chống Overselling)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("[TEST CASE 2] Error Path: Mua vượt quá tồn kho hiện tại -> Hệ thống từ chối (Reject)");
            Console.WriteLine("----------------------------------------");
            try
            {
                // Sách Clean Code chỉ còn tồn kho 5 cuốn, nhưng khách cố tình đặt 8 cuốn
                Console.WriteLine($"Yêu cầu đặt mua: 8 cuốn '{book2.Name}' (Tồn kho hiện tại chỉ có: {book2.Stock})");
                customer.Cart.AddItem(book2, 8);

                var checkoutService = new CheckoutService();
                IDiscountStrategy noDiscount = new NoDiscountStrategy();

                // Dòng lệnh này sẽ kích hoạt ngoại lệ do vượt quá tồn kho
                checkoutService.ProcessCheckout(customer, noDiscount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> KẾT QUẢ MONG ĐỢI (Đã bắt lỗi thành công): {ex.Message}\n");
                customer.Cart.Clear(); // Xóa giỏ hàng sau khi lỗi để reset trạng thái
            }

            // ==========================================
            // TEST CASE 3: POLYMORPHISM / STRATEGY SWAP SCENARIO (Đổi chiến lược giảm giá linh hoạt)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("[TEST CASE 3] Polymorphism Scenario: Thay đổi chiến lược giảm giá sang Mua 1 Tặng 1 (BOGO)");
            Console.WriteLine("----------------------------------------");
            try
            {
                customer.Cart.AddItem(book1, 2); // Mua 2 cuốn sách C# (Giá: 150,000 * 2 = 300,000 đ)

                // Hoán đổi sang chiến lược giảm giá Mua 1 Tặng 1 mà không cần sửa code cũ của CheckoutService (Open/Closed Principle)[cite: 1]
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
            // KIỂM TRA TÍNH NĂNG LỊCH SỬ ĐƠN HÀNG (Order History per Customer)
            // ==========================================
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"LỊCH SỬ MUA HÀNG CỦA KHÁCH HÀNG: {customer.Name} (Tổng số đơn: {customer.OrderHistory.Count})");
            Console.WriteLine("----------------------------------------");
            foreach (var histOrder in customer.OrderHistory)
            {
                Console.WriteLine($"- Mã đơn: {histOrder.OrderId} | Ngày: {histOrder.OrderDate:HH:mm:ss} | Tổng tiền thanh toán: {histOrder.FinalTotal:N0} đ");
            }

            Console.WriteLine("\nNhấn phím bất kỳ để thoát...");
            Console.ReadKey();
        }
    }
}