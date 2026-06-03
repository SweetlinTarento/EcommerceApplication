using EcommerceApplication.DTO;
using EcommerceApplication.Models;

namespace EcommerceApplication.Services.Interfaces
{
    public interface IProductService
    {

        PagedList<ProductDTO> GetAll(ProductFilterDTO filter);
        ProductDTO? GetById(int id);
        ProductDTO? GetByName(string name);
        Product? Create(Product product);
        Product? Update(int id, ProductDTO product);
        Product? Patch(int id, ProductPatchDTO product);
        List<ProductDTO> GetAllProductsByCompanyId(int id);
        Task<string> UploadProductImage(int id, IFormFile file);

        bool Delete(int id);
    }
}
