1\. Implementation of Encapsulation

In modern software engineering, encapsulation serves as a foundational principle to safeguard state integrity and conceal internal representation details from external consumers. Within the system architecture, encapsulation is rigorously enforced in the domain layer, notably within the Cart class. The internal storage structure, \_items, is strictly declared as ***private*** readonly List<OrderItem>, ensuring that external components cannot manipulate the collection directly. To expose item data safely, the class offers a ***read-only view*** via the public property *IReadOnlyCollection<OrderItem> Items => \_items.AsReadOnly()*. All state modifications—such as adding, removing, or purging products—are strictly gated through dedicated domain methods like *AddItem(), RemoveItem(), and Clear()*. Similarly, operational attributes such as the \_percentage field in *PercentageDiscountStrategy* are declared ***private*** readonly, ensuring that crucial business variables remain immutable once instantiated through the constructor.



2\. Demonstration of Polymorphism

Polymorphism enables the system to treat differentiated object types through a uniform interface, deferring specific behavioral execution to runtime. This behavior is primarily demonstrated in the discount calculation subsystem centered around the *IDiscountStrategy* interface. The system defines multiple ***concrete*** implementations: *PercentageDiscountStrategy, BuyOneGetOneDiscountStrategy, and NoDiscountStrategy*. Each supplying its own specialized implementation of *CalculateDiscount(Cart cart)* and *GetDescription(*). During payment processing, the *CheckoutService* class receives an instance of *IDiscountStrategy* as an ***abstract*** parameter in its *ProcessCheckout(Customer, IDiscountStrategy)* method. Consequently, the execution of discount algorithms occurs polymorphically at runtime without the payment orchestrator needing explicit knowledge of the concrete strategy type being evaluated.



3\. Application of the Strategy Design Pattern

To maintain flexibility when evaluating promotional models, the system incorporates the Strategy Design Pattern. The design decouples the core checkout workflow from varying promotional algorithms by encapsulating each pricing rule into separate, interchangeable classes. The *IDiscountStrategy* ***interface*** serves as the formal Strategy Abstraction, defining the common operational contract. The concrete strategies: *PercentageDiscountStrategy* for percentage deductions, *BuyOneGetOneDiscountStrategy* for BOGO promotion logic, and *NoDiscountStrategy* for standard transactions—act as the ***Concrete*** Strategies implementing distinct logic. The *CheckoutService* operates as the Context, consuming the selected strategy dynamically during order finalization. This pattern isolates algorithm implementations and allows pricing behaviors to be altered at execution time without impacting peripheral domain services.



4\. Adherence to the Open/Closed Principle (OCP)

The system architecture directly aligns with the Open/Closed Principle—a core SOLID guideline stating that software entities should be open for extension but closed for modification. By abstracting discount calculations behind *IDiscountStrategy*, the core execution framework in *CheckoutService* remains invariant when new business rules are introduced. If additional promotional schemes (such as fixed value vouchers or tiered membership discounts) are required in future iterations, developers can seamlessly extend the application by implementing new classes under *IDiscountStrategy*. Existing execution pipelines, domain entities, and core logic components require zero code modifications, thereby mitigating regression risks and maintaining high software stability across updates.

