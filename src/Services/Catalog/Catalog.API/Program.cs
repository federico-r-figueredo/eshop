using Catalog.DataAccess;
using Catalog.DataAccess.Managers.CatalogItems;
using Catalog.DataAccess.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Catalog.API {
	public class Program {
		public static async Task Main(string[] args) {
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
			AddServicesToContainer(builder);
			WebApplication app = builder.Build();
			ConfigureHttpRequestPipeline(app);
			AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
			await PrepareDatabase.MigrateAndSeedAsync(app);
			app.Run();
		}

		private static void AddServicesToContainer(WebApplicationBuilder builder) {
			builder.Services.AddOptions();
			builder.Services.AddLogging();
			builder.Services.AddHealthChecks();
			//webApplicationBuilder.Services.Configure<CatalogOptions>(webApplicationBuilder.Configuration);
			builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Services.AddAutoMapper(assemblies: AppDomain.CurrentDomain.GetAssemblies());
			builder.Services.AddRouting(opt => opt.LowercaseUrls = true);

			//builder.Services.AddDbContext<CatalogDbContext>(dbContextOptionsBuilder => {
			//	dbContextOptionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLCatalogConnectionstring"),
			//									  npgSqlDbContextOptionsBuilder => npgSqlDbContextOptionsBuilder.MigrationsAssembly("Catalog.DataAccess")
			//																								    .EnableRetryOnFailure(
			//																										maxRetryCount: 6,
			//																										maxRetryDelay: TimeSpan.FromSeconds(30),
			//																										errorCodesToAdd: null));
			//	dbContextOptionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning));
			//}); 

			builder.Services.AddDbContext<CatalogDbContext>(options => {
				options.UseSqlServer(connectionString: builder.Configuration.GetConnectionString("SQLServerCatalogConnectionstring"),
									 sqlServerOptionsAction: sqlServerOptionsAction => {
										 sqlServerOptionsAction.MigrationsAssembly("Catalog.DataAccess")
															   .EnableRetryOnFailure(maxRetryCount: 6, 
																					 maxRetryDelay: TimeSpan.FromSeconds(30), 
																					 errorNumbersToAdd: null);
										 });
				options.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning));
			});

			builder.Services.AddTransient<ICatalogBrandRepository, CatalogBrandRepository>();
			builder.Services.AddTransient<ICatalogItemRepository, CatalogItemRepository>();
			builder.Services.AddTransient<ICatalogTypeRepository, CatalogTypeRepository>();

			builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

			builder.Services.AddTransient<ICatalogItemManager, CatalogItemManager>();
		}

		private static void ConfigureHttpRequestPipeline(WebApplication app) {
			if (app.Environment.IsDevelopment()) {
				app.UseSwagger();
				app.UseSwaggerUI(options => {
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
					//options.RoutePrefix = string.Empty;
				});
			}

			//app.UseProbe();
			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();
		}
	}
}