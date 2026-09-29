namespace MechanicalDesigns.Domain.Enums;

public enum UserRole { Client, Admin }
public enum ReviewStatus { Pending, Approved, Rejected }
public enum PaymentMethod { CashOnDelivery }
public enum PaymentStatus { Unpaid, Paid, Failed, Refunded }
public enum OrderStatus { PendingConfirmation, Confirmed, Processing, ReadyForShipping, Shipped, Delivered, Cancelled }
public enum OrderItemStatus { Available, OutOfStock, Cancelled }
public enum CustomOrderStatus { New, Contacted, InDiscussion, Accepted, Rejected, Completed }
