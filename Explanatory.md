# Comprehensive System Architecture & OOP Design Report
**Project: E-Commerce Bookstore System**

This document provides an in-depth architectural breakdown of the E-Commerce system. It details every domain model, property, OOP principle, and design pattern used in the source code, specifically emphasizing the rationale (the "why") behind these technical decisions to prepare for system defense.

## 1. Domain Models & Property Architecture

The system is built upon strongly typed entities. Each property and class exists to fulfill a specific business requirement and maintain data integrity.

### 1.1. The `Product` Class (Entity)
*   **Properties:** `ProductId`, `Name`, `Type`, `Price`, `Stock`.
*   **Defense/Rationale:**
    *   **Why does `Stock` exist here?** Placing the `Stock` property directly inside the `Product` entity allows for real-time **Inventory Tracking**. By passing the `Product` object by reference throughout the system, any deduction made during checkout (e.g., `item.Product.Stock -= item.Quantity`) immediately reflects globally.
    *   **Constructor Initialization:** Forcing these properties to be set via the constructor ensures that no `Product` can exist in an invalid state (e.g., a product without a price or ID cannot be instantiated).

### 1.2. The `OrderItem` Class (Composition & Helper)
*   **Properties:** `Product` (Reference), `Quantity` (int).
*   **Methods:** `GetTotalPrice()`.
*   **Defense/Rationale:**
    *   **Why not just use a `List<Product>` in the Cart?** If we only used a list of products, buying 10 pens would mean adding 10 duplicate `Product` objects to a list. This is highly inefficient for memory and processing. By introducing `OrderItem`, we utilize **Composition**. It wraps the `Product` reference alongside a `Quantity` integer, optimizing memory and making logic (like increasing quantity) mathematically simpler.
    *   **Encapsulation of Logic:** The math for calculating a line item's cost is encapsulated inside `GetTotalPrice() => Product.Price * Quantity`. This prevents other classes from having to write this calculation manually, adhering to the DRY (Don't Repeat Yourself) principle.

### 1.3. The `Order` Class (Immutable Snapshot)
*   **Properties:** `OrderId`, `OrderDate`, `Items`, `SubTotal`, `DiscountAmount`, `TaxAmount`, `FinalTotal`.
*   **Defense/Rationale - The Deep Copy Concept:**
    *   In the constructor, you wrote: `Items = items.Select(i => new OrderItem(i.Product, i.Quantity)).ToList();`. 
    *   *Why did you use LINQ to create `new OrderItem` instead of just `Items = items`?* Because of object references. If you merely assigned the cart's list to the order, they would share the same memory reference. When the system later calls `customer.Cart.Clear()`, it would accidentally wipe out the items inside the completed `Order` in the history! By using `.Select(new OrderItem)`, you are creating a copy of the cart at the exact moment of purchase. An order is a historical record; it must be immutable and immune to future cart changes.

### 1.4. The `Customer` Class (Aggregate Root)
*   **Properties:** `CustomerId`, `Name`, `Cart`, `OrderHistory`.
*   **Defense/Rationale:**
    *   **Constructor Guarantees:** In the constructor, `Cart = new Cart();` and `OrderHistory = new List<Order>();` are initialized immediately. *Why?* To prevent `NullReferenceException`. If they were not initialized, trying to do `customer.Cart.AddItem()` on a new customer would crash the application.
    *   **Data Aggregation:** The `Customer` acts as the root entity that connects the user's identity to their active state (`Cart`) and their historical state (`OrderHistory`).

---

## 2. Core OOP Principles Applied

### 2.1. Encapsulation: Protecting State in `Cart`
*   **Implementation:** The cart uses `private readonly List<OrderItem> _items` and exposes `public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();`.
*   **Defense/Rationale:**
    *   By hiding the underlying list, the `Cart` maintains absolute control over its data. 
    *   When `AddItem` is called, the system uses LINQ (`FirstOrDefault`) to check if the product is already in the cart. If it is, it safely increments the existing `Quantity`. If developers could access `_items.Add()` directly from outside, they might accidentally add duplicate rows for the same product, breaking the checkout UI.
    *   In `RemoveItem`, encapsulation allows the class to safely handle business logic: if reducing the quantity drops it to 0 or below, the item is automatically removed from the list entirely (`_items.Remove(item)`).

### 2.2. Polymorphism: Dynamic Runtime Behavior
*   **Implementation:** The `CheckoutService.ProcessCheckout` method accepts an `IDiscountStrategy` interface parameter rather than a concrete class.
*   **Defense/Rationale:**
    *   This allows Late Binding or we can call dynamic dispatch. At runtime, the C# compiler determines which `CalculateDiscount` method to invoke based on whether a `PercentageDiscountStrategy` or `BuyOneGetOneDiscountStrategy` was passed in. 
    *   **Benefit:** As proven in the `Program.cs` Test Cases, we can hot-swap the promotion mechanism without altering the checkout flow or redeploying the core system.

---

## 3. Design Patterns

*   **Implementation:** `IDiscountStrategy` (Abstraction), `PercentageDiscountStrategy` / `BuyOneGetOneDiscountStrategy` (Concrete Strategies), `CheckoutService` (Context).
*   **Defense/Rationale:**
    *   **Single Responsibility Principle:** `CheckoutService` should only orchestrate the checkout. It should not contain the complex math for marketing campaigns.
    *   **Open/Closed Principle:** The system is *open for extension* (we can add a new `FixedVoucherStrategy` tomorrow by creating a new file) but *closed for modification* (we do not need to touch or risk breaking `CheckoutService.cs` to add this new voucher).

---

## 4. Advanced Business Logic & Flow Control (CheckoutService)

The `CheckoutService` is the orchestrator. Its `ProcessCheckout` method is carefully structured to handle edge cases and maintain system integrity:

1.  **Empty Cart Validation:** `!cartItems.Any()` throws an exception. This prevents the system from generating useless \$0 invoices in the database.
2.  **Anti-Overselling Mechanism:** 
    *   Before any math is done or any stock is deducted, the service loops through the cart to verify `item.Quantity > item.Product.Stock`.
    *   *Why do this first?* This is the "Fail-Fast" principle. If a customer tries to buy 8 books but only 5 are in stock, the transaction is rejected immediately via an `InvalidOperationException` (Test Case 2). This prevents partial transactions or negative inventory in the warehouse.
3.  **Financial Safety Check:** `if (discountAmount > subTotal) discountAmount = subTotal;`
    *   *Why?* Without this line, a poorly configured discount (e.g., \$100 off a \$50 order) would result in a negative total, meaning the system would owe the customer money. This boundary check ensures business safety.
4.  **Transaction Finalization:** The service deducts the stock, calculates VAT (`_taxRate`), links the `Order` to the `Customer`, and finally executes `customer.Cart.Clear()`. Clearing the cart is done *last* to ensure that if any exception occurred during the checkout steps, the user does not lose their shopping cart items.
