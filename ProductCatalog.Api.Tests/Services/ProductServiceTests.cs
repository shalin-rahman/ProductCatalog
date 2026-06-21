using AutoMapper;
using Moq;
using NUnit.Framework;
using ProductCatalog.Api.Domain;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalog.Api.Tests.Services
{
    [TestFixture]
    public class ProductServiceTests
    {
        private Mock<IProductRepository> _mockRepo;
        private Mock<IMapper> _mockMapper;
        private ProductService _productService;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _productService = new ProductService(_mockRepo.Object, _mockMapper.Object);
        }

        [Test]
        public async Task GetAllProductsAsync_ReturnsMappedProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10, Description = "Desc 1" },
                new Product { Id = 2, Name = "Product 2", Price = 20, Description = "Desc 2" }
            };
            var dtos = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product 1", Price = 10, Description = "Desc 1" },
                new ProductDto { Id = 2, Name = "Product 2", Price = 20, Description = "Desc 2" }
            };

            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDto>>(products)).Returns(dtos);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetProductByIdAsync_WhenExists_ReturnsMappedProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1", Price = 10, Description = "Desc 1" };
            var dto = new ProductDto { Id = 1, Name = "Product 1", Price = 10, Description = "Desc 1" };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);
            _mockMapper.Setup(m => m.Map<ProductDto>(product)).Returns(dto);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Product 1"));
        }

        [Test]
        public async Task GetProductByIdAsync_WhenNotExists_ReturnsNull()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(99);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateProductAsync_SavesAndReturnsProductDto()
        {
            // Arrange
            var createDto = new ProductCreateDto { Name = "New Product", Price = 50, Description = "New Desc" };
            var productToSave = new Product { Name = "New Product", Price = 50, Description = "New Desc" };
            var savedProduct = new Product { Id = 3, Name = "New Product", Price = 50, Description = "New Desc" };
            var responseDto = new ProductDto { Id = 3, Name = "New Product", Price = 50, Description = "New Desc" };

            _mockMapper.Setup(m => m.Map<Product>(createDto)).Returns(productToSave);
            _mockRepo.Setup(repo => repo.CreateAsync(productToSave)).ReturnsAsync(savedProduct);
            _mockMapper.Setup(m => m.Map<ProductDto>(savedProduct)).Returns(responseDto);

            // Act
            var result = await _productService.CreateProductAsync(createDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(3));
            Assert.That(result.Name, Is.EqualTo("New Product"));
            _mockRepo.Verify(repo => repo.CreateAsync(productToSave), Times.Once);
        }

        [Test]
        public async Task DeleteProductAsync_WhenExists_ReturnsTrue()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _productService.DeleteProductAsync(1);

            // Assert
            Assert.That(result, Is.True);
            _mockRepo.Verify(repo => repo.DeleteAsync(1), Times.Once);
        }
    }
}
