using Xunit;
using Moq;
using FluentAssertions;
using EcommerceApplication.Controllers;
using EcommerceApplication.Services.Interfaces;
using EcommerceApplication.DTO;
using EcommerceApplication.Models;
using Microsoft.AspNetCore.Mvc;//used as APIController, ControllerBase classes are used in main project
namespace EcommerceTest.ControllerTest
{
    public class CompanyControllerTests
    {
        private readonly Mock<ICompanyService> _mockCompanyService;//Mock is used for fake implementation of ICompanyService
        private readonly CompanyController _companyController;

        public CompanyControllerTests()
        {
            _mockCompanyService = new Mock<ICompanyService>();
            _companyController = new CompanyController(_mockCompanyService.Object);

        }
        [Fact]
        public void GetCompany_ReturnsOk_WhenCompanyExists()
        {
            // Arrange
            var companyDto = new CompanyDTO();
            var pagedcompanyDto = new PagedList<CompanyDTO>(
                 new List<CompanyDTO> { companyDto },
                 1,
                 1,
                 10
            );
            _mockCompanyService.Setup(x => x.GetAll(1, 10))
                        .Returns(pagedcompanyDto);
            // Act
            var result = _companyController.GetCompanies(1, 10);
            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public void GetCompany_ReturnsNotFound_WhenNoCompanyExists()
        {

            _mockCompanyService.Setup(x => x.GetAll(1, 10))
                        .Returns((PagedList<CompanyDTO>)null);
            // Act
            var result = _companyController.GetCompanies(1, 10);
            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
        [Fact]
        public void GetCompanyById_ReturnsOk_WhenCompanyExists()
        {
            // Arrange
            var companyDto = new CompanyDTO();

            _mockCompanyService.Setup(x => x.GetById(1))
                        .Returns(companyDto);

            // Act
            var result = _companyController.GetCompanyById(1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetCompanyById_ReturnsNotFound_WhenCompanyDoesNotExists()
        {

            _mockCompanyService.Setup(x => x.GetById(152))
                        .Returns((CompanyDTO)null);

            // Act
            var result = _companyController.GetCompanyById(152);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public void GetCompanyById_ReturnsBadRequest_WhenIdIsInvalid()
        {

            // Act
            var result = _companyController.GetCompanyById(0);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }


        [Fact]
        public void GetCompanyByName_ReturnsOk_WhenCompanyExists()
        {
            // Arrange
            var companyDto = new CompanyDTO();

            _mockCompanyService.Setup(x => x.GetByName("Johnsons"))
                        .Returns(companyDto);

            // Act
            var result = _companyController.GetCompanyByName("Johnsons");

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();//to check if the OK() object is returned 
        }
        [Fact]
        public void GetCompanyByName_ReturnsNotFound_WhenCompanyDoesNotExists()
        {

            _mockCompanyService.Setup(x => x.GetByName("Apple"))
                        .Returns((CompanyDTO)null);

            // Act
            var result = _companyController.GetCompanyByName("Johnsons");

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }
        [Fact]
        public void DeleteCompany_ReturnsOk_WhenCompanyExists()
        {
            var companyDto = new CompanyDTO();
            _mockCompanyService.Setup(x => x.GetById(1))
                        .Returns(companyDto);

            // Act
            var result = _companyController.DeleteCompany(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
        [Fact]//to indicate that this is a test method and should be executed by the test runner
        public void DeleteCompany_ReturnsNotFound_WhenCompanyDoesNotExists()
        {

            _mockCompanyService.Setup(x => x.GetById(1))
                        .Returns((CompanyDTO)null);

            // Act
            var result = _companyController.DeleteCompany(1);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public void DeleteCompany_ReturnBadRequest_CompanyIdIsNull()
        {
            var companyDto = new CompanyDTO();
            var result = _companyController.DeleteCompany(-1);
            result.Should().BeOfType<BadRequestObjectResult>();

        }

        [Fact]
        public void SearchCompanies_ReturnsOk_WhenCompanyExists()
        {
            var companies = new List<CompanyDTO>{
                 new CompanyDTO() };
            var pagedCompanies = new PagedList<CompanyDTO>(
                 companies,
                 companies.Count,
                 1,
                 10
            );
            _mockCompanyService.Setup(x => x.Search("Johnson", 1, 10))
                        .Returns(pagedCompanies);

            // Act
            var result = _companyController.SearchCompanies("Johnson");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public void SearchCompanies_ReturnsBadRequest_WhenCompanyNameNotProvided()
        {

            var result = _companyController.SearchCompanies("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void SearchCompaniesByLocation_ReturnsBadRequest_WhenCompanyNameNotProvided()
        {

            var result = _companyController.SearchByLocation("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public void SearchCompaniesByLocation_ReturnsOk_WhenCompanyExists()
        {
            var companies = new List<CompanyDTO>{
                 new CompanyDTO() };
            var pagedCompanies = new PagedList<CompanyDTO>(
                 companies,
                 companies.Count,
                 1,
                 10
            );
            _mockCompanyService.Setup(x => x.SearchByLocation("Delhi", 1, 10))
                        .Returns(pagedCompanies);

            // Act
            var result = _companyController.SearchByLocation("Delhi");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public void SearchCompaniesByLocation_ReturnsNotFound_WhenCompanyDoesNotExists()
        {
            var companies = new List<CompanyDTO>();

            var pagedCompanies = new PagedList<CompanyDTO>(
                companies,
                companies.Count,
                1,
                10
            );
            _mockCompanyService.Setup(x => x.SearchByLocation("Singapore", 1, 10))
                        .Returns(pagedCompanies);

            // Act
            var result = _companyController.SearchByLocation("Singapore");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
        [Fact]
        public void PutCompany_ReturnOk_CompanyExists()
        {
            var company = new Company();
            _mockCompanyService.Setup(x => x.Update(1, company))
                        .Returns(company);
            var result = _companyController.PutCompany(1, company);
            result.Result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public void PutCompany_ReturnNotFound_CompanyDoesNotExists()
        {
            var company = new Company();
            _mockCompanyService.Setup(x => x.Update(1, company))
                        .Returns((Company)null);
            var result = _companyController.PutCompany(1, company);
            result.Result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public void PutCompany_ReturnBadRequest_CompanyIdIsNull()
        {
            var company = new Company();
            var result = _companyController.PutCompany(-1, company);
            result.Result.Should().BeOfType<BadRequestResult>();

        }
        [Fact]
        public void PatchCompany_ReturnOk_CompanyExists()
        {
            var companypatchDto = new CompanyPatchDTO();
            _mockCompanyService.Setup(x => x.Patch(1, companypatchDto))
                        .Returns(new Company());
            var result = _companyController.PatchCompany(1, companypatchDto);
            result.Result.Should().BeOfType<OkObjectResult>();

        }
        [Fact]
        public void PatchCompany_ReturnBadRequest_CompanyIdInvalid()
        {
            var result = _companyController.PatchCompany(-1, new CompanyPatchDTO());
            result.Result.Should().BeOfType<BadRequestResult>();
        }
        [Fact]
        public void PostCompany_ReturnCreated_CompanyCreated()
        {
            var company = new Company();
            _mockCompanyService.Setup(x => x.Create(company))
                        .Returns(company);
            var result = _companyController.PostCompany(company);
            result.Result.Should().BeOfType<OkObjectResult>();
        }
        [Fact]
        public void PostCompany_ReturnsBadRequest_WhenCompanyIsNull()
        {
            // Act
            var result = _companyController.PostCompany(null);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
       
    }
}