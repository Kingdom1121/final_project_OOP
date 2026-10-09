using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineBookstore
{
    // --- STRATEGY PATTERN CHO DISCOUNT (Polymorphism & Open/Closed Principle) ---[cite: 1]

    public interface IDiscountStrategy
    {
        decimal CalculateDiscount(Cart cart);
        string GetDescription();
    }

    // Chiến lược 1: Giảm giá theo phần trăm tổng đơn hàng
    public class PercentageDiscountStrategy : IDiscountStrategy
    {
        private readonly decimal _percentage; // Ví dụ: 10 nghĩa là giảm 10%

        public PercentageDiscountStrategy(decimal percentage)
        {
            _percentage = percentage;
        }

        //tính toán giảm giá dựa trên tổng giá trị đơn hàng
        public decimal CalculateDiscount(Cart cart)
        {
            decimal subTotal = cart.GetSubTotal();
            return subTotal * (_percentage / 100m);
        }

        public string GetDescription() => $"Giảm giá {_percentage}% trên tổng đơn hàng";
    }

    // Chiến lược 2: Mua 1 tặng 1 (Buy-One-Get-One) cho sản phẩm có giá trị thấp nhất hoặc áp dụng đơn giản cho dòng sản phẩm sách
    public class BuyOneGetOneDiscountStrategy : IDiscountStrategy
    {
        public decimal CalculateDiscount(Cart cart)
        {
            decimal discount = 0;
            foreach (var item in cart.Items)
            {
                // Cứ mua 2 sản phẩm bất kỳ cùng loại thì được miễn phí 1 sản phẩm có giá rẻ nhất trong cặp đó (tính đơn giản: mua 2 giảm giá 50% số lượng đó)
                int freeItems = item.Quantity / 2;
                discount += freeItems * item.Product.Price;
            }
            return discount;
        }

        public string GetDescription() => "Chương trình Mua 1 Tặng 1 (BOGO)";
    }

    // Trường hợp không áp dụng mã giảm giá nào
    public class NoDiscountStrategy : IDiscountStrategy
    {
        public decimal CalculateDiscount(Cart cart) => 0m;
        public string GetDescription() => "Không áp dụng mã giảm giá";
    }

    // --- DỊCH VỤ THANH TOÁN VÀ QUẢN LÝ TỒN KHO ---
    public class CheckoutService
    {
        private readonly decimal _taxRate; // Tỷ lệ thuế (Ví dụ: 0.1 tương ứng 10% VAT)

        public CheckoutService(decimal taxRate = 0.1m)
        {
            _taxRate = taxRate;
        }

        public Order ProcessCheckout(Customer customer, IDiscountStrategy discountStrategy)
        {
            var cartItems = customer.Cart.Items.ToList();
            if (!cartItems.Any())
            {
                throw new InvalidOperationException("Giỏ hàng đang trống, không thể thanh toán!");
            }

            // 1. Kiểm tra tồn kho trước khi thanh toán (Chống Overselling / Trừ kho)[cite: 1]
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Stock)
                {
                    throw new InvalidOperationException($"Sản phẩm '{item.Product.Name}' vượt quá số lượng tồn kho hiện tại (Kho còn: {item.Product.Stock})!");
                }
            }

            // 2. Tiến hành trừ tồn kho thực tế
            foreach (var item in cartItems)
            {
                item.Product.Stock -= item.Quantity;
            }

            // 3. Tính toán tiền tệ (Subtotal, Discount, Tax, Final Total)[cite: 1]
            decimal subTotal = customer.Cart.GetSubTotal();
            decimal discountAmount = discountStrategy.CalculateDiscount(customer.Cart);

            // Đảm bảo giảm giá không vượt quá tạm tính
            if (discountAmount > subTotal) discountAmount = subTotal;

            decimal taxableAmount = subTotal - discountAmount;
            decimal taxAmount = taxableAmount * _taxRate;
            decimal finalTotal = taxableAmount + taxAmount;

            // 4. Tạo đối tượng Order mới
            string orderId = "ORD-" + DateTime.Now.Ticks.ToString().Substring(10);
            var newOrder = new Order(orderId, cartItems, subTotal, discountAmount, taxAmount, finalTotal);

            // 5. Lưu vào lịch sử mua hàng của khách & Xóa sạch giỏ hàng hiện tại[cite: 1]
            customer.OrderHistory.Add(newOrder);
            customer.Cart.Clear();

            return newOrder;
        }
    }
}