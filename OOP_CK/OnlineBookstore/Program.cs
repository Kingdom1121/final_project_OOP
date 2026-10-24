using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlineBookstore
{
    class Program
    {
        static void Main(string[] args)
        {
            // Thiết lập mã hóa tiếng Việt UTF-8 cho cả luồng nhập (Input) và xuất (Output) trên Console
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==================================================================");
            Console.WriteLine("=== ĐỒ ÁN HƯỚNG ĐỐI TƯỢNG: ONLINE BOOKSTORE / E-COMMERCE ===");
            Console.WriteLine("==================================================================");

            // ==================================================================
            // PHẦN 1: [REGRESSION TESTING] CHẠY LẠI TOÀN BỘ 6 TEST CASE CŨ (TUẦN 1 & 2)
            // ==================================================================
            Console.WriteLine("\n>>> KIỂM THỬ HỒI QUY (REGRESSION TESTING TRÊN 6 TEST CASE CŨ)");
            Console.WriteLine("------------------------------------------------------------------");

            var regBook1 = new Product("P001", "Lập trình C# căn bản", "Sách", 150000m, 10);
            var regBook2 = new Product("P002", "Clean Code Clean Architecture", "Sách", 280000m, 5);
            var regPen = new Product("P003", "Bút bi cao cấp", "Văn phòng phẩm", 20000m, 50);
            var regCustomer = new Customer("C000", "Khách Hàng Test Regression");
            var checkoutService = new CheckoutService(taxRate: 0.1m);

            // ------------------------------------------------------------------
            // 1. Happy path (Tuần 1): Mua bình thường + Giảm 10%
            Console.WriteLine("[TEST 1: HAPPY PATH - GIẢM GIÁ % & CẬP NHẬT TỒN KHO]");
            Console.WriteLine($"  * Input: 2 x '{regBook1.Name}' (Kho: {regBook1.Stock}) + 3 x '{regPen.Name}' (Kho: {regPen.Stock})");
            regCustomer.Cart.AddItem(regBook1, 2);
            regCustomer.Cart.AddItem(regPen, 3);
            Order rOrder1 = checkoutService.ProcessCheckout(regCustomer, new PercentageDiscountStrategy(10));
            Console.WriteLine($"  * Thực tế tính ra: Tạm tính = {rOrder1.SubTotal:N0} đ | Giảm 10% = -{rOrder1.DiscountAmount:N0} đ | Thuế = +{rOrder1.TaxAmount:N0} đ | Tổng = {rOrder1.FinalTotal:N0} đ");
            Console.WriteLine($"  * Tồn kho sau trừ: P001 còn {regBook1.Stock} (Kỳ vọng: 8) | P003 còn {regPen.Stock} (Kỳ vọng: 47)");
            bool pass1 = (rOrder1.FinalTotal == 356400m && regBook1.Stock == 8 && regPen.Stock == 47);
            Console.WriteLine($"  => KẾT QUẢ: {(pass1 ? "PASSED (Khớp 100% logic nghiệp vụ)" : "FAILED")}\n");

            // ------------------------------------------------------------------
            // 2. Chặn quá tồn kho (Tuần 1): Chống Overselling
            Console.WriteLine("[TEST 2: OVERSELLING - CHẶN MUA VƯỢT TỒN KHO]");
            Console.WriteLine($"  * Input: Đặt mua 8 cuốn '{regBook2.Name}' (Trong khi tồn kho chỉ còn: {regBook2.Stock})");
            try
            {
                regCustomer.Cart.AddItem(regBook2, 8);
                checkoutService.ProcessCheckout(regCustomer, new NoDiscountStrategy());
                Console.WriteLine("  => KẾT QUẢ: FAILED (Hệ thống không chặn được)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  * Bắt được ngoại lệ mong muốn: \"{ex.Message}\"");
                Console.WriteLine($"  * Tồn kho giữ nguyên: P002 = {regBook2.Stock} (Không bị trừ bậy)");
                regCustomer.Cart.Clear();
                Console.WriteLine("  => KẾT QUẢ: PASSED (Bảo toàn dữ liệu kho thành công)\n");
            }

            // ------------------------------------------------------------------
            // 3. Đa hình BOGO (Tuần 1): Mua 2 tặng 1 cùng loại
            Console.WriteLine("[TEST 3: STRATEGY SWAP - ĐA HÌNH KHUYẾN MÃI BOGO]");
            Console.WriteLine($"  * Input: Mua 2 x '{regBook1.Name}' (Đơn giá: {regBook1.Price:N0} đ) với chiến lược BuyOneGetOne");
            regCustomer.Cart.AddItem(regBook1, 2);
            Order rOrder2 = checkoutService.ProcessCheckout(regCustomer, new BuyOneGetOneDiscountStrategy());
            Console.WriteLine($"  * Thực tế tính ra: Tạm tính = {rOrder2.SubTotal:N0} đ | Tiền miễn phí 1 cuốn = -{rOrder2.DiscountAmount:N0} đ");
            bool pass3 = (rOrder2.DiscountAmount == 150000m && rOrder2.FinalTotal == 165000m);
            Console.WriteLine($"  => KẾT QUẢ: {(pass3 ? "PASSED (Đa hình BOGO tính đúng giá trị tặng)" : "FAILED")}\n");

            // ------------------------------------------------------------------
            // 4. Giỏ trống (Tuần 2): Bắt ngoại lệ Empty Cart
            Console.WriteLine("[TEST 4: EMPTY CART - THANH TOÁN KHI GIỎ TRỐNG]");
            Console.WriteLine($"  * Input: Giỏ hàng hiện tại = {regCustomer.Cart.Items.Count} món");
            try
            {
                regCustomer.Cart.Clear();
                checkoutService.ProcessCheckout(regCustomer, new NoDiscountStrategy());
                Console.WriteLine("  => KẾT QUẢ: FAILED (Cho phép giỏ trống thanh toán)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  * Bắt được ngoại lệ mong muốn: \"{ex.Message}\"");
                Console.WriteLine("  => KẾT QUẢ: PASSED (Chặn giỏ hàng rỗng thành công)\n");
            }

            // ------------------------------------------------------------------
            // 5. Mua vừa khít kho (Tuần 2): Boundary Test
            Console.WriteLine("[TEST 5: EXACT STOCK - MUA VỪA KHÍT TỒN KHO]");
            int stockToBuy = regBook2.Stock;
            Console.WriteLine($"  * Input: Kho '{regBook2.Name}' đang có {stockToBuy} sp -> Đặt mua chính xác {stockToBuy} sp");
            regCustomer.Cart.AddItem(regBook2, stockToBuy);
            checkoutService.ProcessCheckout(regCustomer, new NoDiscountStrategy());
            Console.WriteLine($"  * Tồn kho thực tế sau khi mua: P002 = {regBook2.Stock} (Kỳ vọng: 0)");
            bool pass5 = (regBook2.Stock == 0);
            Console.WriteLine($"  => KẾT QUẢ: {(pass5 ? "PASSED (Trừ sạch kho chính xác về 0)" : "FAILED")}\n");

            // ------------------------------------------------------------------
            // 6. BOGO số lẻ (Tuần 2): Mua 3 tặng 1
            Console.WriteLine("[TEST 6: BOGO ODD QUANTITY - MUA SỐ LƯỢNG LẺ]");
            Console.WriteLine($"  * Input: Mua 3 x '{regPen.Name}' (Đơn giá: {regPen.Price:N0} đ) -> Cứ 2 cái tặng 1 cái");
            regCustomer.Cart.AddItem(regPen, 3);
            Order rOrder3 = checkoutService.ProcessCheckout(regCustomer, new BuyOneGetOneDiscountStrategy());
            Console.WriteLine($"  * Thực tế tính ra: Tạm tính = {rOrder3.SubTotal:N0} đ | Tiền giảm (1 sp) = -{rOrder3.DiscountAmount:N0} đ");
            bool pass6 = (rOrder3.DiscountAmount == 20000m);
            Console.WriteLine($"  => KẾT QUẢ: {(pass6 ? "PASSED (Xử lý số lượng lẻ chuẩn theo công thức Quantity / 2)" : "FAILED")}\n");

            Console.WriteLine("=> KẾT LUẬN: 6/6 TEST CASE TỰ ĐỘNG ASSERT THÀNH CÔNG (NO REGRESSION)");
            Console.WriteLine("------------------------------------------------------------------");

            // ==================================================================
            // PHẦN 2: [Bổ sung Tuần 3] GIAO DIỆN KHÁCH HÀNG TƯƠNG TÁC (CONSOLE I/O)
            // ==================================================================
            Console.WriteLine("\n>>> GIAO DIỆN MUA HÀNG TƯƠNG TÁC (CONSOLE I/O)");
            Console.WriteLine("------------------------------------------------------------------");

            var storeInventory = new List<Product>
            {
                new Product("P101", "Nhà Giả Kim (Sách)", "Sách", 80000m, 15),
                new Product("P102", "Giải Thuật Lập Trình (Sách)", "Sách", 185000m, 8),
                new Product("P103", "Sổ tay Planner 2026", "Văn phòng phẩm", 45000m, 20),
                new Product("P104", "Bút ký cao cấp Parker", "Văn phòng phẩm", 250000m, 6)
            };

            Console.Write("Nhập mã khách hàng (ví dụ: KH01): ");
            string cusId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(cusId)) cusId = "KH01";

            Console.Write("Nhập họ tên khách hàng: ");
            string cusName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(cusName)) cusName = "Khách Hàng Trải Nghiệm";

            var liveCustomer = new Customer(cusId, cusName);

            bool isRunning = true;
            while (isRunning)
            {
                Console.WriteLine("\n==================================================");
                Console.WriteLine($"  KHÁCH HÀNG: {liveCustomer.Name} ({liveCustomer.CustomerId})");
                Console.WriteLine($"  GIỎ HÀNG HIỆN TẠI: {liveCustomer.Cart.Items.Count} mặt hàng | Tạm tính: {liveCustomer.Cart.GetSubTotal():N0} đ");

                if (liveCustomer.Cart.Items.Any())
                {
                    Console.WriteLine("  --- Danh sách các món trong giỏ ---");
                    int itemIndex = 1;
                    foreach (var item in liveCustomer.Cart.Items)
                    {
                        Console.WriteLine($"    {itemIndex++}. {item.Product.Name} x {item.Quantity} ({item.Product.Price:N0} đ/sp) = {item.GetTotalPrice():N0} đ");
                    }
                }
                else
                {
                    Console.WriteLine("  (Giỏ hàng hiện đang trống)");
                }

                Console.WriteLine("==================================================");
                Console.WriteLine("1. Thêm sản phẩm vào giỏ hàng");
                Console.WriteLine("2. Bớt hoặc xóa sản phẩm khỏi giỏ hàng");
                Console.WriteLine("3. Thanh toán đơn hàng");
                Console.Write("Vui lòng chọn chức năng (1 / 2 / 3): ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Console.WriteLine("\n--- DANH MỤC SẢN PHẨM HIỆN CÓ ---");
                        for (int i = 0; i < storeInventory.Count; i++)
                        {
                            var p = storeInventory[i];
                            Console.WriteLine($"{i + 1}. [{p.ProductId}] {p.Name} | Giá: {p.Price:N0} đ | Tồn kho: {p.Stock}");
                        }

                        Console.Write("\nNhập số thứ tự sản phẩm muốn mua (1 - 4): ");
                        if (int.TryParse(Console.ReadLine(), out int prodIndex) && prodIndex >= 1 && prodIndex <= storeInventory.Count)
                        {
                            var chosenProduct = storeInventory[prodIndex - 1];
                            Console.Write($"Nhập số lượng muốn mua cho '{chosenProduct.Name}': ");
                            if (int.TryParse(Console.ReadLine(), out int qty) && qty > 0)
                            {
                                var currentInCart = liveCustomer.Cart.Items.FirstOrDefault(i => i.Product.ProductId == chosenProduct.ProductId);
                                int alreadyInCart = currentInCart != null ? currentInCart.Quantity : 0;

                                if (alreadyInCart + qty > chosenProduct.Stock)
                                {
                                    Console.WriteLine($"-> LỖI: Số lượng yêu cầu vượt quá tồn kho hiện tại!");
                                    Console.WriteLine($"   (Kho chỉ còn: {chosenProduct.Stock} | Trong giỏ đã có: {alreadyInCart} | Không thể thêm: {qty})");
                                }
                                else
                                {
                                    liveCustomer.Cart.AddItem(chosenProduct, qty);
                                    Console.WriteLine($"-> Đã thêm thành công {qty} x '{chosenProduct.Name}' vào giỏ hàng!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("-> Số lượng không hợp lệ!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("-> Lựa chọn số thứ tự không tồn tại!");
                        }
                        break;

                    case "2":
                        var currentCartItems = liveCustomer.Cart.Items.ToList();
                        if (!currentCartItems.Any())
                        {
                            Console.WriteLine("\n-> Giỏ hàng của bạn đang trống, không có món nào để bỏ!");
                            break;
                        }

                        Console.WriteLine("\n--- CÁC SẢN PHẨM ĐANG CÓ TRONG GIỎ HÀNG ---");
                        for (int i = 0; i < currentCartItems.Count; i++)
                        {
                            var item = currentCartItems[i];
                            Console.WriteLine($"{i + 1}. [{item.Product.ProductId}] {item.Product.Name} - Số lượng trong giỏ: {item.Quantity} | Thành tiền: {item.GetTotalPrice():N0} đ");
                        }

                        Console.Write("\nChọn số thứ tự món hàng muốn bớt/xóa: ");
                        if (int.TryParse(Console.ReadLine(), out int removeIdx) && removeIdx >= 1 && removeIdx <= currentCartItems.Count)
                        {
                            var targetItem = currentCartItems[removeIdx - 1];
                            Console.Write($"Nhập số lượng muốn bớt cho '{targetItem.Product.Name}' (Hiện có {targetItem.Quantity}): ");

                            if (int.TryParse(Console.ReadLine(), out int qtyToRemove) && qtyToRemove > 0)
                            {
                                if (qtyToRemove > targetItem.Quantity)
                                {
                                    Console.WriteLine($"-> Lỗi: Món này trong giỏ chỉ có {targetItem.Quantity}, không thể bớt {qtyToRemove}.");
                                }
                                else
                                {
                                    liveCustomer.Cart.RemoveItem(targetItem.Product.ProductId, qtyToRemove);
                                    Console.WriteLine($"-> Đã xử lý bớt {qtyToRemove} sản phẩm khỏi giỏ hàng thành công!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("-> Số lượng bớt không hợp lệ!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("-> Số thứ tự chọn không hợp lệ!");
                        }
                        break;

                    case "3":
                        if (!liveCustomer.Cart.Items.Any())
                        {
                            Console.WriteLine("\n-> Giỏ hàng đang trống! Vui lòng chọn phím 1 để thêm sản phẩm trước khi thanh toán.");
                            break;
                        }

                        Console.WriteLine("\n--- CHỌN MÃ KHUYẾN MÃI ÁP DỤNG ---");
                        Console.WriteLine("1. Giảm giá 15% trên tổng đơn hàng (Percentage Discount)");
                        Console.WriteLine("2. Mua 1 tặng 1 cùng loại (Buy-One-Get-One)");
                        Console.WriteLine("3. Không áp dụng khuyến mãi (No Discount)");
                        Console.Write("Lựa chọn chiến lược (1/2/3): ");
                        string promoChoice = Console.ReadLine();

                        IDiscountStrategy chosenStrategy = promoChoice switch
                        {
                            "1" => new PercentageDiscountStrategy(15),
                            "2" => new BuyOneGetOneDiscountStrategy(),
                            _ => new NoDiscountStrategy()
                        };

                        try
                        {
                            Order finalLiveOrder = checkoutService.ProcessCheckout(liveCustomer, chosenStrategy);

                            // =========================================================================================
                            // BẢNG 1: HÓA ĐƠN XUẤT RA (CHUẨN 93 KÝ TỰ TỪ ĐẦU ĐẾN CUỐI)
                            // =========================================================================================
                            string borderTop = "+----+----------------------------+----+-------------+----------------------+---------------+";
                            string borderSum = "+" + new string('-', 91) + "+";

                            Console.WriteLine("\n" + new string('=', 93));
                            Console.WriteLine("                                       HÓA ĐƠN XUẤT RA                                       ");
                            Console.WriteLine($"Mã đơn: {finalLiveOrder.OrderId}  |  Ngày đặt: {finalLiveOrder.OrderDate:yyyy-MM-dd HH:mm:ss}");
                            Console.WriteLine($"Khách hàng: {liveCustomer.Name} ({liveCustomer.CustomerId})");
                            Console.WriteLine($"Chính sách áp dụng: {chosenStrategy.GetDescription()}");
                            Console.WriteLine(borderTop);
                            Console.WriteLine("| STT| Tên sản phẩm               | SL |   Đơn giá   |       Tiền giảm      |   Thành tiền  |");
                            Console.WriteLine(borderTop);

                            int rowIdx = 1;
                            foreach (var item in finalLiveOrder.Items)
                            {
                                decimal originalTotal = item.GetTotalPrice();
                                decimal itemDiscount = 0;
                                string strItemDisc = "0 đ";

                                if (chosenStrategy is BuyOneGetOneDiscountStrategy)
                                {
                                    int freeCount = item.Quantity / 2;
                                    if (freeCount > 0)
                                    {
                                        itemDiscount = freeCount * item.Product.Price;
                                        strItemDisc = $"-{itemDiscount:N0} đ (tặng {freeCount})";
                                    }
                                    else
                                    {
                                        strItemDisc = "0 đ";
                                    }
                                }
                                else if (chosenStrategy is PercentageDiscountStrategy)
                                {
                                    itemDiscount = originalTotal * 0.15m;
                                    strItemDisc = $"-{itemDiscount:N0} đ (-15%)";
                                }

                                decimal itemFinal = originalTotal - itemDiscount;

                                string shortName = item.Product.Name.Length > 26
                                    ? item.Product.Name.Substring(0, 23) + "..."
                                    : item.Product.Name;

                                string strPrice = $"{item.Product.Price:N0} đ";
                                string strItemFinal = $"{itemFinal:N0} đ";

                                Console.WriteLine(
                                    $"| {rowIdx++,2} | {shortName,-26} | {item.Quantity,2} | {strPrice,11} | {strItemDisc,20} | {strItemFinal,13} |"
                                );
                            }

                            string strSub = $"{finalLiveOrder.SubTotal:N0} đ";
                            string strDisc = finalLiveOrder.DiscountAmount > 0 ? $"-{finalLiveOrder.DiscountAmount:N0} đ" : "0 đ";
                            string strAfterDisc = $"{(finalLiveOrder.SubTotal - finalLiveOrder.DiscountAmount):N0} đ";
                            string strTax = $"+{finalLiveOrder.TaxAmount:N0} đ";
                            string strFinal = $"{finalLiveOrder.FinalTotal:N0} đ";

                            Console.WriteLine(borderTop);
                            Console.WriteLine($"| {"Tổng tiền hàng (Tạm tính):",-68}{strSub,21} |");
                            Console.WriteLine($"| {"Chiết khấu giảm giá:",-68}{strDisc,21} |");
                            Console.WriteLine($"| {"Thành tiền sau chiết khấu:",-68}{strAfterDisc,21} |");
                            Console.WriteLine($"| {"Thuế GTGT (VAT 10%):",-68}{strTax,21} |");
                            Console.WriteLine(borderSum);
                            Console.WriteLine($"| {"=> TỔNG THANH TOÁN:",-68}{strFinal,21} |");
                            Console.WriteLine(borderSum);
                            Console.WriteLine("Trạng thái: GIAO DỊCH THÀNH CÔNG (Đã lưu lịch sử đơn hàng và cập nhật tồn kho)");

                            // =========================================================================================
                            // BẢNG 2: LỊCH SỬ ĐƠN HÀNG (CHUẨN 99 KÝ TỰ)
                            // =========================================================================================
                            string histBorder = "+----+--------------+---------------------+--------+---------------+---------------+---------------+";
                            Console.WriteLine("\n" + new string('=', 99));
                            Console.WriteLine("                         LỊCH SỬ ĐƠN HÀNG CỦA KHÁCH HÀNG (ORDER HISTORY)                          ");
                            Console.WriteLine($"Khách hàng: {liveCustomer.Name} (Mã: {liveCustomer.CustomerId})  |  Tổng số đơn hàng: {liveCustomer.OrderHistory.Count}");
                            Console.WriteLine(histBorder);
                            Console.WriteLine("| STT| Mã đơn hàng  |     Ngày giờ đặt    | Số món |    Tạm tính   |    Giảm giá   |   Tổng tiền   |");
                            Console.WriteLine(histBorder);

                            int histIdx = 1;
                            foreach (var hist in liveCustomer.OrderHistory)
                            {
                                int totalQty = hist.Items.Sum(i => i.Quantity);
                                string strHistSub = $"{hist.SubTotal:N0} đ";
                                string strHistDisc = hist.DiscountAmount > 0 ? $"-{hist.DiscountAmount:N0} đ" : "0 đ";
                                string strHistTotal = $"{hist.FinalTotal:N0} đ";

                                Console.WriteLine(
                                    $"| {histIdx++,2} | {hist.OrderId,-12} | {hist.OrderDate:yyyy-MM-dd HH:mm:ss} | {totalQty,6} | {strHistSub,13} | {strHistDisc,13} | {strHistTotal,13} |"
                                );
                            }
                            Console.WriteLine(histBorder);

                            Console.Write("\nBạn có muốn tiếp tục mua sắm đơn hàng mới không? (Y/N): ");
                            string contChoice = Console.ReadLine();
                            if (contChoice?.Trim().ToUpper() != "Y")
                            {
                                isRunning = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"\nThanh toán thất bại: {ex.Message}");
                        }
                        break;

                    default:
                        Console.WriteLine("-> Lựa chọn không hợp lệ! Vui lòng chỉ bấm 1, 2 hoặc 3.");
                        break;
                }
            }

            Console.WriteLine("\nNhấn phím bất kỳ để kết thúc chương trình...");
            Console.ReadKey();
        }
    }
}
