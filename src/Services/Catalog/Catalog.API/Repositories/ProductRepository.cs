using Catalog.API.Data;
using Catalog.API.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Repositories
{
	public class ProductRepository : IProductRepository
	{
		private readonly ICatalogContext _context;

		public ProductRepository(ICatalogContext context)
		{
			_context = context;
		}

		public async Task CreateProduct(Product product)
		{
			await _context.Products.InsertOneAsync(product);
		}

		public async Task<bool> DeleteProduct(string id)
		{
			FilterDefinition<Product> filter = Builders<Product>.Filter.ElemMatch(prop => prop.Id, id);
			DeleteResult deletedResult=await _context
							.Products
							.DeleteOneAsync(filter);
			return deletedResult.IsAcknowledged && deletedResult.DeletedCount > 0;

		}

		public async Task<Product> GetProductbyid(string id)
		{
			return await _context
							.Products.Find(p=>p.Id==id)
							.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<Product>> GetProductByCategory(string categoryname)
		{
			FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(prop => prop.Category, categoryname);
			return await _context
							.Products
							.Find(filter)
							.ToListAsync();
		}

		public async  Task<IEnumerable<Product>> GetProductByName(string name)
		{
			FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(prop => prop.Name, name);
			return await _context
							.Products
							.Find(filter)
							.ToListAsync();
		}

		public async Task<IEnumerable<Product>> GetProducts()
		{
			return await _context
							.Products.Find(prop => true)
							.ToListAsync();
		}

		public async Task<bool> UpdateProduct(Product product)
		{
			var updateresult = await _context
											.Products
											.ReplaceOneAsync(filter: g => g.Id == product.Id, replacement: product);

			return updateresult.IsAcknowledged && updateresult.ModifiedCount > 0;
		}
	}
}
