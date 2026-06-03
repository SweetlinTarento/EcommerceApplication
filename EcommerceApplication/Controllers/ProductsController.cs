using EcommerceApplication.Data;
using EcommerceApplication.DTO;
using EcommerceApplication.Models;
using EcommerceApplication.Services;
using EcommerceApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EcommerceApplication.Controllers
{
    [Authorize]
    [ApiController]
    [Route("ecommerce/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService service, ILogger<ProductsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: ecommerce/products/allproduct?pageNumber=1&pageSize=10
        [Authorize]
        [HttpGet("allproduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProducts([FromQuery] ProductFilterDTO filter)
        {
            try
            {
                var result = _service.GetAll(filter);
                if (result == null || !result.Any())
                    return NotFound("No Products to show");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products with the given filter criteria.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }


        // GET: ecommerce/products/byproductid/5
        [Authorize]
        [HttpGet("byproductid/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProductDTO> GetProductById(int id)
        {
            try
            {

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID: {Id}. Product ID must be greater than 0.", id);
                    return BadRequest("Id cannot be 0.");
                }
                var product = _service.GetById(id);

                if (product == null)
                {
                    return NotFound($"Product with id {id} not found");
                }
                
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving product with id {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

        }
        // GET: ecommerce/products/byproductname/{name} 
        [Authorize]
        [HttpGet("byproductname/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProductDTO> GetProductByName(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name) )
                {
                    _logger.LogWarning("Product name cannot be empty.");
                    return BadRequest("Name cannot be null.");
                }
                var product = _service.GetByName(name);

                if (product == null)
                {
                    return NotFound($"Product {name} cannot found");
                }
                
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving product with name {Name}.", name);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // POST: ecommerce/products/createproduct
        [Authorize(Roles = "ADMIN")]
        [HttpPost("createproduct")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PostProduct(Product product)
        {
            try
            {
                if (product == null || product.CompanyId == 0)
                {
                    _logger.LogWarning("Product cannot be null and must have a valid CompanyId.");
                    return BadRequest("Product cannot be null.");
                }

                var p = _service.Create(product);
                if (p != null)
                {
                   
                    return Ok(p);
                }
                else
                    return BadRequest("Product not present");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }


        // PATCH: ecommerce/products/updatecompany/5
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("updatecompany/{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProductDTO> PatchProduct(int id, ProductPatchDTO product)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID: {Id}. Product ID must be greater than 0.", id);
                    return BadRequest();
                }
                var existingProduct = _service.Patch(id, product);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                
                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating product with id {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        //Put: ecommerce/products/productupdate/5
        [Authorize(Roles = "ADMIN")]
        [HttpPut("productupdate/{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProductDTO> PutProduct(int id, ProductDTO product)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID: {Id}. Product ID must be greater than 0.", id);
                    return BadRequest();
                }
                var existingProduct = _service.Update(id, product);
                if (existingProduct == null)
                {
                    return NotFound();
                }
               
                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating product with id {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        //Get: ecommerce/products/sortedbycompany/5
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("sortedbycompany/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProductDTO> GetProductsByCompany(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid company ID: {Id}. Company ID must be greater than 0.", id);
                    return BadRequest();
                }
                var existingProduct = _service.GetAllProductsByCompanyId(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                
                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products for company with id {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        //DELETE: ecommerce/products/5
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("deleteproduct/{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product ID: {Id}. Product ID must be greater than 0.", id);
                    return BadRequest("Id cannot be 0.");
                }
                var product = _service.GetById(id);
                if (product == null)
                {
                    return NotFound();
                }

                _service.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting product with id {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        //to upload picture of product
        [Authorize(Roles ="ADMIN")]
        [HttpPost("upload-image/{productId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadImage(int productId, IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    _logger.LogWarning("No file provided for product {ProductId}", productId);
                    return BadRequest("File is required");
                }

                var result = await _service.UploadProductImage(productId, file);

                if (result == null)
                {
                    return BadRequest("Upload failed");
                }

                return Ok(new
                {
                    message = "Image uploaded successfully",
                    imageUrl = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for product {ProductId}", productId);
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
