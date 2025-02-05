﻿namespace SimpleEcommerce.Models
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
