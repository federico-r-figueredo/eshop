using Catalog.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.DataAccess.Repositories {
	public interface ICatalogItemRepository : IRepository<CatalogItem> {
		new Task<CatalogItem> CreateAsync(CatalogItem catalogItem);
		Task<IEnumerable<CatalogItem>> GetAllAsync(byte pageSize, byte pageIndex, bool includeNested);
		new Task<CatalogItem> GetByIDAsync(int id);
		Task<bool> NameExistsAsync(string name);
		Task<CatalogItem> UpdateAsync(CatalogItem catalogItemUpdateDTO);
	}
}