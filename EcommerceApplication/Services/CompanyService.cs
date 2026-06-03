using AutoMapper;
using EcommerceApplication.DTO;
using EcommerceApplication.Models;
using EcommerceApplication.Repository.Interfaces;
using EcommerceApplication.Services.Interfaces;
using EcommerceApplication.Pagination;

namespace EcommerceApplication.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(ICompanyRepository repository, IMapper mapper, ILogger<CompanyService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public Company Create(Company company)
        {
            _repository.Add(company);
            _logger.LogInformation("Created new company with ID: {CompanyId}", company.CompanyId);
            return company;
        }

        public bool Delete(int id)
        {
            var company = _repository.GetById(id);
            if (company == null) return false;
            _logger.LogInformation("Deleting company with ID: {CompanyId}", id);
            _repository.Delete(company);
            return true;
        }

        public PagedList<CompanyDTO> GetAll(int pageNumber, int pageSize)
        {
            var parameters = new RequestParameters { PageNumber = pageNumber, PageSize = pageSize };
            var companies = _repository.GetAll(parameters);

            var companiesDTO = _mapper.Map<List<CompanyDTO>>(companies);
            _logger.LogInformation("Retrieved {Count} companies for page {PageNumber} with page size {PageSize}", companiesDTO.Count, pageNumber, pageSize);
            return new PagedList<CompanyDTO>(
                companiesDTO,
                companies.TotalCount,
                companies.CurrentPage,
                companies.PageSize
            );
        }

        public CompanyDTO? GetById(int id)
        {
            var company = _repository.GetById(id);
            if (company == null) return null;
            _logger.LogInformation("Retrieved company with ID: {CompanyId}", id);
            return _mapper.Map<CompanyDTO>(company);
        }

        public CompanyDTO? GetByName(string name)
        {
            var company = _repository.GetByName(name);
            if (company == null) return null;
            _logger.LogInformation("Retrieved company with name: {CompanyName}", name);
            return _mapper.Map<CompanyDTO>(company);
        }

        public Company? Patch(int id, CompanyPatchDTO company)
        {
            var existing = _repository.GetById(id);
            if (existing == null) return null;

            if (!string.IsNullOrEmpty(company.CompanyName))
                existing.CompanyName = company.CompanyName;

            if (!string.IsNullOrEmpty(company.Location))
                existing.Location = company.Location;

            if (!string.IsNullOrEmpty(company.Description))
                existing.Description = company.Description;

            if (company.PhoneNumber.HasValue)
                existing.PhoneNumber = company.PhoneNumber.Value;

            if (!string.IsNullOrEmpty(company.Email))
                existing.Email = company.Email;
            _logger.LogInformation("Patching company with ID: {CompanyId}", id);
            _repository.Update(existing);
            return existing;
        }

        public PagedList<CompanyDTO> Search(string searchTerm, int pageNumber, int pageSize)
        {
            var parameters = new RequestParameters { PageNumber = pageNumber, PageSize = pageSize };
            var companies = _repository.Search(searchTerm, parameters);
            var companiesDTO = _mapper.Map<List<CompanyDTO>>(companies);

            _logger.LogInformation("Searched companies with term: {SearchTerm}. Found {Count} results for page {PageNumber} with page size {PageSize}", searchTerm, companiesDTO.Count, pageNumber, pageSize);
            return new PagedList<CompanyDTO>(
                companiesDTO,
                companies.TotalCount,
                companies.CurrentPage,
                companies.PageSize
            );
        }

        public PagedList<CompanyDTO> SearchByLocation(string location, int pageNumber, int pageSize)
        {
            var parameters = new RequestParameters { PageNumber = pageNumber, PageSize = pageSize };
            var companies = _repository.SearchByLocation(location, parameters);
            var companiesDTO = _mapper.Map<List<CompanyDTO>>(companies);
            _logger.LogInformation("Searched companies by location: {Location}. Found {Count} results for page {PageNumber} with page size {PageSize}", location, companiesDTO.Count, pageNumber, pageSize);
            return new PagedList<CompanyDTO>(
                companiesDTO,
                companies.TotalCount,
                companies.CurrentPage,
                companies.PageSize
            );
        }
        public Company? Update(int id, Company company)
        {
            var existing = _repository.GetById(id);
            if (existing == null) return null;

            existing.CompanyName = company.CompanyName;
            existing.Description = company.Description;
            existing.Email = company.Email;
            existing.Location = company.Location;
            existing.PhoneNumber = company.PhoneNumber;
            existing.EstablishedYear = company.EstablishedYear;
            existing.ProductList = company.ProductList;

            _logger.LogInformation("Updating company with ID: {CompanyId}", id);
            _repository.Update(existing);
            return existing;
        }
    }
}
