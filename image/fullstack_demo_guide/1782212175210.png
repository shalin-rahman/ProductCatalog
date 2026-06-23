# Implement Unit Testing & Update Documentation

This plan outlines the steps to introduce automated unit testing to the Product Catalog application and expand the documentation to explain testing strategies and concepts.

## User Review Required

> [!IMPORTANT]
> The Angular frontend was originally scaffolded using the `--skip-tests` flag, meaning it does not currently have a testing framework (Karma/Jasmine) configured out-of-the-box. 
> 
> **For this plan, I propose focusing the unit testing effort on the `.NET 8 Backend` using `xUnit` and `Moq`.** Backend business logic (Services) and API contracts (Controllers) are typically the most critical to unit test.
> 
> If you also strictly require Angular UI unit tests, please let me know and I will add the necessary steps to install Karma/Jasmine and write component tests. Otherwise, I will proceed with the backend testing strategy below.

## Proposed Changes

### 1. Create Backend Test Project
We will create a new xUnit test project and link it to the main API solution.

#### [NEW] `ProductCatalog.Api.Tests/ProductCatalog.Api.Tests.csproj`
- Add an xUnit test project.
- Add Nuget packages: `Moq` (for mocking dependencies) and `xunit.runner.visualstudio`.
- Add a project reference to `ProductCatalog.Api`.

### 2. Implement Service Tests
We will write tests targeting the core business logic. The `ProductService` is the ideal candidate because it relies on the repository, allowing us to demonstrate dependency mocking.

#### [NEW] `ProductCatalog.Api.Tests/Services/ProductServiceTests.cs`
- Create a test suite for `ProductService`.
- Use `Moq` to mock `IProductRepository`.
- Write tests for:
  - `GetAllAsync_ReturnsMappedProducts()`
  - `GetByIdAsync_WhenExists_ReturnsProduct()`
  - `GetByIdAsync_WhenNotExists_ReturnsNull()`
  - `CreateAsync_SavesAndReturnsProduct()`

### 3. Update Git History
- We will group the testing additions into logical atomic commits.

### 4. Update Documentation
We will update the `fullstack_demo_guide.md` to include a brand new testing section.

#### [MODIFY] `fullstack_demo_guide.md`
- Add a section on **Test-Driven Development (TDD) & Unit Testing**.
- Explain the "Arrange, Act, Assert" (AAA) pattern.
- Explain the concept of Mocking dependencies (why we mock the database instead of using a real one).
- Add terminal commands to execute the tests locally (`dotnet test`).

## Verification Plan

### Automated Tests
- Run `dotnet test ProductCatalog.Api.Tests` and ensure all written tests pass successfully.

### Manual Verification
- Review the generated documentation to ensure the testing strategy is properly explained and easy for a beginner to follow.
