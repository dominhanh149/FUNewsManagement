
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.Services;
using Assignmen_PRN232_1.Services.IServices;

namespace UsersApp.Extensions
{
    public static class ServicesRegister
    {
        public static void RegisterCustomServices(this IServiceCollection services)
        {


            services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();


            services.AddTransient<ITagRepository, TagRepository>();
            services.AddScoped<ITagService, TagService>();


            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();


            services.AddTransient<INewsArticleRepository, NewsArticleRepository>();
            services.AddScoped<INewsArticleService, NewsArticleService>();


            services.AddTransient<ISystemAccountRepository, SystemAccountRepository>();
            services.AddScoped<ISystemAccountService, SystemAccountService>();


            services.AddTransient<IReportRepository, ReportRepository>();
            services.AddScoped<IReportService, ReportService>();

            services.AddScoped<JwtTokenService>();
        }
    }
}
