using cakeshop_api.Hubs;
using cakeshop_api.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace cakeshop_api.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderService(IMongoDatabase database, IHubContext<OrderHub> hubContext)
        {
            _orderCollection = database.GetCollection<Order>("Orders");
            _hubContext = hubContext;
        }

        // Create a pending order in the database and returns the order ID
        public async Task<string> CreatePendingOrder(Order order)
        {
            order.OrderStatus = Status.Pending;
            order.PaymentStatus = false;

            await _orderCollection.InsertOneAsync(order);
            return order.Id ?? throw new InvalidOperationException("Order ID cannot be null.");
        }

        // Get order by ID
        public async Task<Order> GetOrderById(string id)
        {
            var order = await _orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
            {
                throw new Exception($"Order with ID {id} not found.");
            }
            return order;
        }

        /// <summary>
        /// Fulfills an order after payment is completed
        /// </summary>
        public async Task FulfillOrder(string pendingOrderId, string paymentIntentId, string customerId)
        {
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.Id, pendingOrderId),
                Builders<Order>.Filter.Eq(o => o.PaymentStatus, false)
            );

            var update = Builders<Order>.Update
                .Set(o => o.PaymentStatus, true)
                .Set(o => o.OrderStatus, Status.Pending)
                .Set(o => o.PaymentIntentId, paymentIntentId)
                .Set(o => o.StripeCustomerId, customerId)
                .Set(o => o.CreatedDate, DateTime.UtcNow);

            var result = await _orderCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new Exception($"Pending order {pendingOrderId} not found or already paid.");
            }

            var fulfilledOrder = await GetOrderById(pendingOrderId);
            if (fulfilledOrder != null)
            {
                await _hubContext.Clients.All.SendAsync("NewOrder", fulfilledOrder);
            }
        }


        public async Task<List<Order>> GetOrdersByUserAsync(string userId)
        {
            return await _orderCollection.Find(o => o.UserId == userId).ToListAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderCollection.Find(o => true).ToListAsync();
        }

        public async Task<bool> DeleteOrderAsync(string id, string userRole)
        {
            var order = await _orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
            // Only admin can delete confirmed or completed orders
            if (order.OrderStatus == "Confirmed" || order.OrderStatus == "Completed")
            {
                if (userRole != "admin")
                {
                    return false;
                }
            }

            var result = await _orderCollection.DeleteOneAsync(o => o.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var result = await _orderCollection.ReplaceOneAsync(o => o.Id == order.Id, order);
            return result.ModifiedCount > 0;
        }

        public async Task<Order> GetPaymentIntent(string paymentIntentId)
        {
            var order = await _orderCollection.Find(o => o.PaymentIntentId == paymentIntentId).FirstOrDefaultAsync();
            if (order == null)
            {
                throw new Exception($"Order with PaymentIntent ID {paymentIntentId} not found.");
            }
            return order;
        }
    }
}