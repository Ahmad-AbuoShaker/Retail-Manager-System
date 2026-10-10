using Application.DTOs;
using Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface
{
    public interface IProductService
    {
       
        Task<BaseResponse<IEnumerable<ProductDto>>> GetProducts();
    }
}
