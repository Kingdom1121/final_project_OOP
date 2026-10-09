using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineBookstore
{
    // 1. Lớp Product (Sản phẩm)
    public class Product
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // Ví dụ: Sách, Văn phòng phẩm...
        public decimal Price { get; set; }
        public int Stock { get; set; } // Số lượng tồn kho (Hỗ trợ Inventory Tracking)

        public Product(string productId, string name, string type, decimal price, int stock)//constructor cho sản phẩm
        {
            ProductId = productId;
            Name = name;
            Type = type;
            Price = price;
            Stock = stock;
        }
    }

    // Lớp bổ trợ để lưu trữ sản phẩm và số lượng trong giỏ hàng hoặc đơn hàng
    public class OrderItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public OrderItem(Product product, int quantity)//constructor lưu trữ sản phẩm 
        {
            Product = product;
            Quantity = quantity;
        }

        public decimal GetTotalPrice() => Product.Price * Quantity;
    }

    // 2. Lớp Cart (Giỏ hàng) - Thể hiện tính Encapsulation (Tự quản lý danh sách món hàng)
    public class Cart
    {
        private readonly List<OrderItem> _items = new List<OrderItem>();

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        // Thêm sản phẩm vào giỏ hàng
        public void AddItem(Product product, int quantity)
        {
            if (quantity <= 0) return;

            var existingItem = _items.FirstOrDefault(i => i.Product.ProductId == product.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _items.Add(new OrderItem(product, quantity));
            }
        }

        // Xóa hoặc giảm bớt sản phẩm khỏi giỏ hàng
        public void RemoveItem(string productId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.Product.ProductId == productId);
            if (item != null)
            {
                item.Quantity -= quantity;
                if (item.Quantity <= 0)
                {
                    _items.Remove(item);
                }
            }
        }

        // Xóa sạch giỏ hàng sau khi thanh toán
        public void Clear()
        {
            _items.Clear();
        }

        // Tính tổng tiền tạm tính của giỏ hàng
        public decimal GetSubTotal()
        {
            return _items.Sum(item => item.GetTotalPrice());
        }
    }

    // 3. Lớp Order (Đơn hàng đã thanh toán)
    public class Order
    {
        //properties cho từng thứ
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalTotal { get; set; }
        // constructor cho order
        public Order(string orderId, List<OrderItem> items, decimal subTotal, decimal discountAmount, decimal taxAmount, decimal finalTotal)
        {
            OrderId = orderId;
            OrderDate = DateTime.Now;
            Items = items.Select(i => new OrderItem(i.Product, i.Quantity)).ToList();
            SubTotal = subTotal;
            DiscountAmount = discountAmount;
            TaxAmount = taxAmount;
            FinalTotal = finalTotal;
        }

        public void PrintOrderDetails()
        {
            Console.WriteLine($"--- MÃ ĐƠN HÀNG: {OrderId} ({OrderDate:yyyy-MM-dd HH:mm}) ---");
            foreach (var item in Items)
            {
                Console.WriteLine($"- {item.Product.Name} x {item.Quantity} = {item.GetTotalPrice():N0} đ");
            }
            // in các thông tin thanh toán
            Console.WriteLine($"Tạm tính: {SubTotal:N0} đ");
            Console.WriteLine($"Giảm giá: -{DiscountAmount:N0} đ");
            Console.WriteLine($"Thuế (Tax): +{TaxAmount:N0} đ");
            Console.WriteLine($"=> TỔNG THANH TOÁN: {FinalTotal:N0} đ\n");
        }
    }

    // 4. Lớp Customer (Khách hàng)
    public class Customer
    {
        //properties cho khách hàng
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public Cart Cart { get; set; }
        public List<Order> OrderHistory { get; set; } // Lưu lịch sử mua hàng[cite: 1]
        //constructor cho khách hàng
        public Customer(string customerId, string name)
        {
            CustomerId = customerId;
            Name = name;
            Cart = new Cart();
            OrderHistory = new List<Order>();
        }
    }
}