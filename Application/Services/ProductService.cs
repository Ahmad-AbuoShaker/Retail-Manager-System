using Application.DTOs;
using Application.DTOs.Product;
using Application.Interfaces;
using Application.Interfaces.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IAppDbContext _appDbContext;

        public ProductService(IAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<BaseResponse<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                var Products = await _appDbContext.Products
                    .AsNoTracking()
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        RetailPrice = p.RetailPrice,
                        QuantityInStock = p.QuantityInStock,
                        IsTaxable=p.IsTaxable
                    })
                    .ToListAsync();

                return new BaseResponse<IEnumerable<ProductDto>>
                {
                    Succeeded = true,
                    Message = "Products retrieved successfully",
                    Data = Products
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<ProductDto>>
                {
                    Succeeded = false,
                    Message = "An error occurred while retrieving products.",
                    Errors = new List<string> { ex.Message },
                    Data = []
                };
            }

        }

   
    }
}
