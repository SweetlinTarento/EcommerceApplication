using EcommerceApplication.Data;
using EcommerceApplication.DTO;
using EcommerceApplication.Models;
using EcommerceApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;

namespace EcommerceApplication.Controllers
{
    
    [ApiController]
    [Route("ecommerce/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _service;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ICompanyService service, ILogger<CompanyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: ecommerce/Company/allcompany?pageNumber=1&pageSize=10
        [Authorize]
        [HttpGet("allcompany")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult GetCompanies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = _service.GetAll(pageNumber, pageSize);
                if (result == null)
                {
                    _logger.LogWarning("No companies found for pageNumber: {PageNumber} and pageSize: {PageSize}", pageNumber, pageSize);
                    return NotFound("No Companies to show");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching companies for pageNumber: {PageNumber} and pageSize: {PageSize}", pageNumber, pageSize);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        // GET: ecommerce/company/search?searchTerm=apple&pageNumber=1&pageSize=10
        [Authorize]
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult SearchCompanies([FromQuery] string searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _logger.LogWarning("Search term cannot be empty.");
                    return BadRequest("Search term cannot be empty.");
                }

                var result = _service.Search(searchTerm, pageNumber, pageSize);
                if (result.Count == 0)
                {
                    _logger.LogInformation("No companies found matching search term: {SearchTerm}", searchTerm);
                    return NotFound($"No companies found matching '{searchTerm}'");
                }
               
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for companies with search term: {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

        }
        // GET: ecommerce/company/searchbylocation?location=NYC&pageNumber=1&pageSize=10
        [Authorize]
        [HttpGet("searchbylocation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult SearchByLocation([FromQuery] string location, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            { 
            if (string.IsNullOrEmpty(location))
            {
                _logger.LogWarning("Location cannot be empty.");
                return BadRequest("Location cannot be empty.");
            }

            var result = _service.SearchByLocation(location, pageNumber, pageSize);
            if (result.Count == 0)
            {
                return NotFound($"No companies found in '{location}'");
            }
            
            return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for companies in location: {Location}", location);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

        }

        // GET: ecommerce/company/bycompanyid/{id}
        [Authorize]
        [HttpGet("bycompanyid/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult<CompanyDTO> GetCompanyById(int id)
        {
            try
            { 
            if (id <= 0)
            {
                _logger.LogWarning("Invalid company id: {Id}. Id must be greater than 0.", id);
                return BadRequest("Id cannot be less than or equal to 0");
            }
            var result = _service.GetById(id);
            if (result == null)
                return NotFound($"No company with id {id} found");
            
            return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching company with id: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

        }
        // GET: ecommerce/company/bycompanyname/{name}
        [Authorize]
        [HttpGet("bycompanyname/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult<CompanyDTO> GetCompanyByName(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    _logger.LogWarning("Name not provided");
                    return BadRequest("Name cannot be null.");
                }
                var company = _service.GetByName(name);

                if (company == null)
                {
                    return NotFound($"Company {name} cannot found");
                }
               
                return Ok(company);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while fetching company with name: {name}", name);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // POST: ecommerce/company/createcompany
        [Authorize(Roles = "ADMIN")]
        [HttpPost("createcompany")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult<Company> PostCompany(Company company)
        {
            try
            {
                if (company == null)
                {
                    _logger.LogWarning("Company not provided");
                    return BadRequest("Company cannot be null.");
                }
                var c = _service.Create(company);
               
                return Ok(c);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while creating company");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // PATCH: ecommerce/company/updatecompany/{id}
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("updatecompany/{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult<CompanyPatchDTO> PatchCompany(int id, CompanyPatchDTO company)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Id for company not provided");
                    return BadRequest();
                }
                var existingCompany = _service.Patch(id, company);
               
                return Ok(existingCompany);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while creating company");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        //Put: ecommerce/company/companyupdate/{id}
        [Authorize(Roles = "ADMIN")]
        [HttpPut("companyupdate/{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult<Company> PutCompany(int id, Company company)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Id for company not provided");
                    return BadRequest();
                }
                var existingCompany = _service.Update(id, company);
                if (existingCompany == null)
                {
                    _logger.LogWarning("Company with company {id} not found", id);
                    return NotFound();
                }
                
                return Ok(existingCompany);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while creating company");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
        //DELETE: ecommerce/company/deletecompany/{id}
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("deletecompany/{id}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult DeleteCompany(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Id for company not provided or less than or equal to 0");
                    return BadRequest("Id cannot be 0.");
                }
                var company = _service.GetById(id);
                if (company == null)
                {
                    _logger.LogWarning("No such company exists with this id: {id}", id);
                    return NotFound();
                }

                _service.Delete(id);
                
                return NoContent();
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while creating company");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

        }
    }
}

