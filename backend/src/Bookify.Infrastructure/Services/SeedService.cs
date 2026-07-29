using Bookify.Domain.Entities;
using Bookify.Domain.Enums;
using Bookify.Infrastructure.Authentication;
using Bookify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Services;

public class SeedService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SeedService> _logger;

    public SeedService(AppDbContext context, IPasswordHasher passwordHasher, ILogger<SeedService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        _logger.LogInformation("Starting database seed...");

        // Admin User
        var admin = new User(
            "System",
            "Admin",
            "admin@bookify.com",
            _passwordHasher.Hash("Admin@123456"),
            UserRole.Admin);
        _context.Users.Add(admin);

        // Categories
        var doctors = new Category("Doctors", "doctors", "medical_services", 1);
        var salons = new Category("Salons", "salons", "content_cut", 2);
        var spas = new Category("Spas", "spas", "spa", 3);
        var gyms = new Category("Gyms", "gyms", "fitness_center", 4);
        var dining = new Category("Dining", "dining", "restaurant", 5);
        var hotels = new Category("Hotels", "hotels", "hotel", 6);

        _context.Categories.AddRange(doctors, salons, spas, gyms, dining, hotels);

        // SubCategories
        _context.SubCategories.AddRange(
            new SubCategory(doctors.Id, "Dentist", "dentist"),
            new SubCategory(doctors.Id, "Dermatologist", "dermatologist"),
            new SubCategory(doctors.Id, "Cardiologist", "cardiologist"),
            new SubCategory(doctors.Id, "Ophthalmologist", "ophthalmologist"),
            new SubCategory(doctors.Id, "General Practitioner", "general-practitioner"),
            new SubCategory(salons.Id, "Hair Styling", "hair-styling"),
            new SubCategory(salons.Id, "Nail Art", "nail-art"),
            new SubCategory(salons.Id, "Barber", "barber"),
            new SubCategory(spas.Id, "Massage", "massage"),
            new SubCategory(spas.Id, "Facial", "facial"),
            new SubCategory(spas.Id, "Body Treatment", "body-treatment"),
            new SubCategory(gyms.Id, "Personal Training", "personal-training"),
            new SubCategory(gyms.Id, "Yoga", "yoga"),
            new SubCategory(gyms.Id, "Pilates", "pilates"),
            new SubCategory(dining.Id, "Fine Dining", "fine-dining"),
            new SubCategory(dining.Id, "Casual Dining", "casual-dining"),
            new SubCategory(hotels.Id, "Luxury", "luxury"),
            new SubCategory(hotels.Id, "Boutique", "boutique"));

        await _context.SaveChangesAsync();
        _logger.LogInformation("Database seed completed successfully.");
    }
}
