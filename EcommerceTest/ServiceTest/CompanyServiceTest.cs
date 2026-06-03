using AutoMapper;
using EcommerceApplication.DTO;
using EcommerceApplication.Models;
using EcommerceApplication.Repository.Interfaces;
using EcommerceApplication.Services;
using FluentAssertions;
using EcommerceApplication.Pagination;
using Moq;
using Xunit;

namespace EcommerceApplication.Tests.Services
{
    public class CompanyServiceTests
    {
        private readonly Mock<ICompanyRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CompanyService _service;

        public CompanyServiceTests()
        {
            _mockRepository = new Mock<ICompanyRepository>();
            _mockMapper = new Mock<IMapper>();

            _service = new CompanyService(
                _mockRepository.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public void Create_ReturnsTrue_WhenCompanyAdded()
        {
            // Arrange
            var company = new Company();

            _mockRepository
                .Setup(x => x.Add(company));

            // Act
            var result = _service.Create(company);

            // Assert
            result.Should().Be(company);
        }

        [Fact]
        public void Delete_ReturnsFalse_WhenCompanyNotFound()
        {
            // Arrange
            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns((Company)null);

            // Act
            var result = _service.Delete(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Delete_ReturnsTrue_WhenCompanyExists()
        {
            // Arrange
            var Company = new Company();

            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns(Company);

            // Act
            var result = _service.Delete(1);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(x => x.Delete(Company), Times.Once);//to verify that the delete method  was called exactly once
        }

        [Fact]
        public void GetById_ReturnsNull_WhenCompanyNotFound()
        {
            // Arrange
            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns((Company)null);

            // Act
            var result = _service.GetById(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetById_ReturnsCompanyDTO_WhenCompanyExists()
        {
            // Arrange
            var Company = new Company
            {
                CompanyId = 1,
                CompanyName = "Johnsons"
            };

            var CompanyDto = new CompanyDTO
            {
                CompanyName = "Johnsons"
            };

            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns(Company);

            _mockMapper
                .Setup(x => x.Map<CompanyDTO>(Company))
                .Returns(CompanyDto);

            // Act
            var result = _service.GetById(1);

            // Assert
            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("Johnsons");//! is to tell compiler that the reult will not be null, its like giving a guaratee 
        }
        [Fact]
        public void GetByName_ReturnsNull_WhenCompanyNotFound()
        {
            // Arrange
            _mockRepository
                .Setup(x => x.GetByName("Johnsons"))
                .Returns((Company)null);

            // Act
            var result = _service.GetByName("Johnsons");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetByName_ReturnsCompanyDTO_WhenCompanyExists()
        {
            // Arrange
            var Company = new Company
            {
                CompanyId = 1,
                CompanyName = "Prestige"
            };

            var CompanyDto = new CompanyDTO
            {
                CompanyName = "Prestige"
            };

            _mockRepository
                .Setup(x => x.GetByName("Prestige"))
                .Returns(Company);

            _mockMapper
                .Setup(x => x.Map<CompanyDTO>(Company))
                .Returns(CompanyDto);

            // Act
            var result = _service.GetByName("Prestige");

            // Assert
            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("Prestige");
        }

        [Fact]
        public void Patch_ReturnsNull_WhenCompanyNotFound()
        {
            // Arrange
            var patchDto = new CompanyPatchDTO();

            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns((Company)null);

            // Act
            var result = _service.Patch(1, patchDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Patch_UpdatesCompany_WhenCompanyExists()
        {
            // Arrange
            var existing = new Company
            {
                CompanyId = 1,
                CompanyName = "Johnsons",
                Email = "johnsonsbaby@gmail.com"
            };

            var patchDto = new CompanyPatchDTO
            {
                CompanyName = "Johnsons",
                Email = "johnsonsbaby@gmail.com"
            };

            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns(existing);

            // Act
            var result = _service.Patch(1, patchDto);

            // Assert
            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("Johnsons");
            result.Email.Should().Be("johnsonsbaby@gmail.com");

            _mockRepository.Verify(x => x.Update(existing), Times.Once);
        }

        [Fact]
        public void Update_ReturnsNull_WhenCompanyNotFound()
        {
            // Arrange
            var Company = new Company();

            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns((Company)null);

            // Act
            var result = _service.Update(1, Company);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Update_UpdatesCompany_WhenCompanyExists()
        {
            // Arrange
            var existing = new Company
            {
                CompanyId = 1,
                CompanyName = "Johnsons Baby"
            };

            _mockRepository
                .Setup(x => x.GetById(1))
                .Returns(existing);

            // Act
            var result = _service.Update(1, existing);

            // Assert
            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("Johnsons Baby");

            _mockRepository.Verify(x => x.Update(existing), Times.Once);
        }

        [Fact]
        public void GetAll_ReturnsPagedCompanys()
        {
            // Arrange
            var companies = new PagedList<Company>
            (
                new List<Company>{
                new Company { CompanyId = 1, CompanyName = "ABC" },
                new Company{ CompanyId=2,CompanyName="DEF"} },
                2,
                1, 10);

            var CompanyDtos = new List<CompanyDTO>
            {
                new CompanyDTO
                {
                    CompanyName = "Lego"
                }
            };

            var parameters = new RequestParameters
            {
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository
                .Setup(x => x.GetAll(It.IsAny<RequestParameters>()))
                .Returns(companies);

            _mockMapper
                .Setup(x => x.Map<List<CompanyDTO>>(It.IsAny<List<Company>>()))
                .Returns(CompanyDtos);


            // Act
            var result = _service.GetAll(1, 10);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
        }
        [Fact]
            public void Search_ReturnsPagedCompanys()
            {
                // Arrange
                var companies = new PagedList<Company>
                (
                    new List<Company>{
                    new Company {  CompanyId=1,CompanyName = "ABC" },
                    new Company {  CompanyId=1,CompanyName="DEF"} },
                    2,
                    1, 10);
                var CompanyDtos = new List<CompanyDTO>
                {
                    new CompanyDTO
                    {
                        CompanyName = "Lego"
                    }
                };
                var parameters = new RequestParameters
                {
                    PageNumber = 1,
                    PageSize = 10
                };
                _mockRepository
                    .Setup(x => x.Search(It.IsAny<string>(), It.IsAny<RequestParameters>()))
                    .Returns(companies);
                _mockMapper
                    .Setup(x => x.Map<List<CompanyDTO>>(It.IsAny<PagedList<Company>>()))
                    .Returns(CompanyDtos);
                var result = _service.Search("Legi", 1,10);

                // Assert
                result.Should().NotBeNull();
                result.TotalCount.Should().Be(2);
            }
        [Fact]
        public void Search_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var emptycompanies = new PagedList<Company>
            (
                new List<Company> { },
                0,
                1, 10);
            
            var parameters = new RequestParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
            _mockRepository
                .Setup(x => x.Search("Prestige", It.IsAny<RequestParameters>()))
                .Returns(emptycompanies);
            _mockMapper
                .Setup(x => x.Map<List<CompanyDTO>>(It.IsAny<PagedList<Company>>()))
                .Returns(new List<CompanyDTO>());
            var result = _service.Search("Prestige", 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
        }
        [Fact]
        public void SearchByLocation_ReturnsPagedCompanys()
        {
            // Arrange
            var companies = new PagedList<Company>
            (
                new List<Company>{
                new Company {  CompanyId=1,CompanyName = "ABC" },
                new Company {  CompanyId=1,CompanyName="DEF"} },
                2,
                1, 10);
            var CompanyDtos = new List<CompanyDTO>
            {
                new CompanyDTO
                {
                    CompanyName = "Lego"
                }
            };
            var parameters = new RequestParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
            _mockRepository
                .Setup(x => x.SearchByLocation("Delhi", It.IsAny<RequestParameters>()))
                .Returns(companies);
            _mockMapper
                .Setup(x => x.Map<List<CompanyDTO>>(It.IsAny<List<Company>>()))
                .Returns(CompanyDtos);
            var result = _service.SearchByLocation("Delhi", 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
        }
        [Fact]
        public void SearchByLocation_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var emptycompanies = new PagedList<Company>
            (
                new List<Company> { },
                0,
                1, 10);

            var parameters = new RequestParameters
            {
                PageNumber = 1,
                PageSize = 10
            };
            _mockRepository
                .Setup(x => x.SearchByLocation(It.IsAny<string>(), It.IsAny<RequestParameters>()))
                .Returns(emptycompanies);
            _mockMapper
                .Setup(x => x.Map<List<CompanyDTO>>(It.IsAny<PagedList<Company>>()))
                .Returns(new List<CompanyDTO>());
            var result = _service.SearchByLocation("",1,10);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
        }
    }
}