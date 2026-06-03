using EcommerceApplication.Data;
using EcommerceApplication.Models;
using EcommerceApplication.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using EcommerceApplication.Pagination;
using Xunit;
using FluentAssertions;

public class CompanyRepositoryTests
{
    private EcommerceContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<EcommerceContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;//to specify to use a fake db with unique new name each time the test runs

        return new EcommerceContext(options);
    }
    [Fact]
    public void Add_ShouldInsertCompany()
    {
        var context = GetDbContext();
        var repo = new CompanyRepository(context);

        var company = new Company
        {
            CompanyId = 1,
            CompanyName = "ABC",
            Description = "it is a sample ABC Company",
            Email = "abccargo@cargoservices.com",
            Location = "Delhi"
        };

        repo.Add(company);

        context.Companys.Count().Should().Be(1);
    }
    [Fact]
    public void Delete_ShouldDeleteCompany()
    {
        var context = GetDbContext();
        var repo = new CompanyRepository(context);

        var company = new Company
        {
            CompanyId = 1,
            CompanyName = "ABC",
            Description = "it is a sample ABC Company",
            Email = "abccargo@cargoservices.com",
            Location = "Delhi"
        };
        context.Companys.Add(company);
        context.SaveChanges();

        repo.Delete(company);

        context.Companys.Count().Should().Be(0);
    }
    [Fact]
    public void GetAll_ShouldReturnCompanies()
    {
        var context = GetDbContext();
        var repo = new CompanyRepository(context);

        context.Companys.AddRange(
            new Company { CompanyId = 1, CompanyName = "ABC",
                Description = "it is a sample ABC Company",
                Email = "abccargo@cargoservices.com",
                Location = "Delhi"
            },
            new Company { CompanyId = 2, CompanyName = "XYZ",
                Description = "it is a sample XYZ Company",
                Email = "xyzspices@healthyspices.com",
                Location = "Mumbai"
            }
        );
        context.SaveChanges();

        var result = repo.GetAll(new RequestParameters
        {
            PageNumber = 1,
            PageSize = 10
        });

        result.Count.Should().Be(2);
    }
    [Fact]
    public void Search_ShouldReturnMatchingCompanies()
    {
        var context = GetDbContext();
        var repo = new CompanyRepository(context);

        context.Companys.AddRange(
            new Company { CompanyId = 1, CompanyName = "ABC" , 
            Description="it is a sample ABC Company", 
            Email="abccargo@cargoservices.com",
            Location="Delhi"},
            new Company { CompanyId = 2, CompanyName = "XYZ",
                Description = "it is a sample XYZ Company",
                Email = "xyzspices@healthyspice.com",
                Location = "Mumbai"
            }
        );
        context.SaveChanges();

        var result = repo.Search("ABC", new RequestParameters
        {
            PageNumber = 1,
            PageSize = 10
        });

        result.Count.Should().Be(1);
        result[0].CompanyName.Should().Be("ABC");
    }
    [Fact]
    public void SearchByLocation_ShouldReturnMatchingCompanies()
    {
        var context = GetDbContext();
        var repo = new CompanyRepository(context);

        context.Companys.AddRange(
            new Company { CompanyId = 1, CompanyName="ABC",
                Description = "it is a sample ABC Company",
                Email = "abccargo@cargoservices.com",
                Location = "Delhi"
            },
            new Company { CompanyId = 2, CompanyName="XYZ",
                Description = "it is a sample XYZ Company",
                Email = "xyzspiceso@healthyspices.com",
                Location = "Mumbai" }
        );
        context.SaveChanges();

        var result = repo.SearchByLocation("Mumbai", new RequestParameters
        {
            PageNumber = 1,
            PageSize = 10
        });

        result.Count.Should().Be(1);
    }
    [Fact]
    public void GetById_ShouldReturnCompany()
    {
        var context = GetDbContext();
        var repo = new CompanyRepository(context);

        var company = new Company
        {
            CompanyId = 1,
            CompanyName = "ABC",
            Description = "it is a sample ABC Company",
            Email = "abccargo@cargoservices.com",
            Location = "Delhi"
        };

        context.Companys.Add(company);
        context.SaveChanges();

        var result = repo.GetById(1);

        result.Should().NotBeNull();
        result.CompanyName.Should().Be("ABC");
    }
    [Fact]
    public void GetByName_ShouldReturnCompany()
    {
        var context = GetDbContext();
        var repo = new CompanyRepository(context);

        var company = new Company
        {
            CompanyId = 1,
            CompanyName = "ABC",
            Description = "it is a sample ABC Company",
            Email = "abccargo@cargoservices.com",
            Location = "Delhi"
        };

        context.Companys.Add(company);
        context.SaveChanges();

        var result = repo.GetByName("ABC");

        result.Should().NotBeNull();
        result.CompanyName.Should().Be("ABC");
    }
}
