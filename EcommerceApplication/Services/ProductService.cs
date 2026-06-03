using AutoMapper;
using EcommerceApplication.DTO;
using EcommerceApplication.Models;
using EcommerceApplication.Repository.Interfaces;
using EcommerceApplication.Services.Interfaces;

namespace EcommerceApplication.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repository, IMapper mapper, ILogger<ProductService> logger, IWebHostEnvironment env    )
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _env = env;
        }

        public Product Create(Product product)
        {
            _repository.Add(product);
            _logger.LogInformation($"Product created with ID: {product.Id}");
            return product;
        }

        public bool Delete(int id)
        {
            var product = _repository.GetById(id);
            if (product == null)
                return false;
            _logger.LogInformation($"Deleting product with ID: {id}");
            _repository.Delete(product);
            return true;
        }

        public PagedList<ProductDTO> GetAll(ProductFilterDTO filter)
        {
            if (filter == null)
                filter = new ProductFilterDTO();

            if (filter.Page <= 0)
                filter.Page = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;
            else if (filter.PageSize > 100)
                filter.PageSize = 10;
            
            var query = _repository.GetAllProducts();

            // Apply search filter (by name or description)
            if (!string.IsNullOrEmpty(filter.Search))   
                query = query.Where(p => p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search));

            if (!string.IsNullOrEmpty(filter.CategoryName))
                query = query.Where(p => p.ProductCategory != null &&
                                         p.ProductCategory.CategoryName.Contains(filter.CategoryName));

            // Apply price filters
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            // Apply quantity filters
            if (filter.MinQuantity.HasValue)
                query = query.Where(p => p.Quantity >= filter.MinQuantity.Value);

            if (filter.MaxQuantity.HasValue)
                query = query.Where(p => p.Quantity <= filter.MaxQuantity.Value);

            // Apply sorting
            var sortedQuery = filter.SortBy?.ToLower() switch
            {
                "price" => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),

                "name" => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),

                "quantity" => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Quantity)
                    : query.OrderBy(p => p.Quantity),

                _ => query.OrderBy(p => p.Id)
            };

            // Get total count before pagination
            var totalCount = sortedQuery.Count();

            
            var products = sortedQuery
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            // Map to DTOs
            var productsDTO = _mapper.Map<List<ProductDTO>>(products);
            _logger.LogInformation($"Retrieved {productsDTO.Count} products for page {filter.Page} with page size {filter.PageSize}");
            return new PagedList<ProductDTO>(
                productsDTO,
                totalCount,
                filter.Page,
                filter.PageSize
            );
        }

        public List<ProductDTO> GetAllProductsByCompanyId(int id)
        {
            var products = _repository.GetProductByCompanyId(id);
            if (products == null || !products.Any())
                return new List<ProductDTO>();
            _logger.LogInformation($"Retrieved {products.Count} products for company ID: {id}");

            return _mapper.Map<List<ProductDTO>>(products);
        }

        public ProductDTO? GetById(int id)
        {
            var product = _repository.GetById(id);
            if (product == null)
                return null;

            _logger.LogInformation($"Retrieved product with ID: {id}");
            return _mapper.Map<ProductDTO>(product);
        }

        public ProductDTO? GetByName(string name)
        {
            var product = _repository.GetByName(name);
            if (product == null)
                return null;
            
            _logger.LogInformation($"Retrieved product with name: {name}");
            return _mapper.Map<ProductDTO>(product);
        }

        public Product? Patch(int id, ProductPatchDTO product)
        {
            var existing = _repository.GetById(id);
            if (existing == null) 
                return null;

            if (!string.IsNullOrEmpty(product.Name))
                existing.Name = product.Name;

            if (product.Price.HasValue && product.Price.Value > 0)
                existing.Price = product.Price.Value;

            if (!string.IsNullOrEmpty(product.Description))
                existing.Description = product.Description;

            
            _repository.Update(existing);
            _logger.LogInformation($"Patched product with ID: {id}");
            return existing;
        }

        public Product? Update(int id, ProductDTO product)
        {
            var existing = _repository.GetById(id);
            if (existing == null) 
                return null;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;

            
            _repository.Update(existing);
            _logger.LogInformation($"Updated product with ID: {id}");
            return existing;
        }
        public async Task<string> UploadProductImage(int productId, IFormFile file)
        {
            var product = _repository.GetById(productId);

            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return null;
            }

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Empty file upload for product: {ProductId}", productId);
                return null;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file type for product {ProductId}", productId);
                return null;
            }

            var folder = Path.Combine(_env.WebRootPath, "Uploads", "Products");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/products/{fileName}";

            product.ImageUrl = imageUrl;
            _repository.Update(product);

            _logger.LogInformation("Image uploaded for product {ProductId}: {ImageUrl}",
                productId, imageUrl);

            return imageUrl;
        }
    }
}
