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

        // ════════════════════════════════════════════════
        // USERS — Admin, Providers, Customers
        // ════════════════════════════════════════════════
        var admin = new User("System", "Admin", "admin@bookify.com",
            _passwordHasher.Hash("Admin@123456"), UserRole.Admin);

        // ── Provider Users (one per business owner + extra staff) ──
        var providerUsers = new List<User>();
        var providerData = new[]
        {
            ("Sophia", "Chen", "sophia.chen@example.com", "sophia"),
            ("Marcus", "Johnson", "marcus.j@example.com", "marcus"),
            ("Aisha", "Patel", "aisha.p@example.com", "aisha"),
            ("James", "Wilson", "james.w@example.com", "james"),
            ("Elena", "Rodriguez", "elena.r@example.com", "elena"),
            ("David", "Kim", "david.k@example.com", "david"),
            ("Priya", "Sharma", "priya.s@example.com", "priya"),
            ("Omar", "Hassan", "omar.h@example.com", "omar"),
            ("Rachel", "Green", "rachel.g@example.com", "rachel"),
            ("Carlos", "Mendez", "carlos.m@example.com", "carlos"),
            ("Nadia", "Ibrahim", "nadia.i@example.com", "nadia"),
            ("Thomas", "Anderson", "thomas.a@example.com", "thomas"),
            ("Leila", "Ahmed", "leila.a@example.com", "leila"),
            ("Brian", "Thompson", "brian.t@example.com", "brian"),
            ("Grace", "Park", "grace.p@example.com", "grace"),
            ("Daniel", "Foster", "daniel.f@example.com", "daniel"),
            ("Yuki", "Tanaka", "yuki.t@example.com", "yuki"),
            ("Amara", "Okafor", "amara.o@example.com", "amara"),
            ("Victor", "Hughes", "victor.h@example.com", "victor"),
            ("Sara", "Mitchell", "sara.m@example.com", "sara"),
            ("Hassan", "Ali", "hassan.ali@example.com", "hassan"),
            ("Iris", "Wong", "iris.w@example.com", "iris"),
            ("Nathan", "Reed", "nathan.r@example.com", "nathan"),
            ("Maya", "Lopez", "maya.l@example.com", "maya"),
            ("Felix", "Bauer", "felix.b@example.com", "felix"),
            ("Zara", "Khan", "zara.k@example.com", "zara"),
            ("Olivia", "Bennett", "olivia.b@example.com", "olivia"),
            ("Raj", "Mehta", "raj.m@example.com", "raj"),
        };

        foreach (var (first, last, email, avatarKey) in providerData)
        {
            var user = new User(first, last, email,
                _passwordHasher.Hash("Provider@123"), UserRole.Provider);
            user.SetAvatar($"https://i.pravatar.cc/150?u={avatarKey}");
            providerUsers.Add(user);
        }

        // ── Customer Users (for reviews & bookings) ──
        var customerUsers = new List<User>();
        var customerData = new[]
        {
            ("Demo", "User", "demo@bookify.com", "demo"),
            ("Emma", "T.", "emma.t@example.com", "emma"),
            ("Liam", "S.", "liam.s@example.com", "liam"),
            ("Olivia", "M.", "olivia.m@example.com", "olivia2"),
            ("Noah", "K.", "noah.k@example.com", "noah"),
            ("Ava", "L.", "ava.l@example.com", "ava"),
            ("Ethan", "R.", "ethan.r@example.com", "ethan"),
            ("Isabella", "C.", "isabella.c@example.com", "isabella"),
            ("Mason", "D.", "mason.d@example.com", "mason"),
            ("Sophia", "W.", "sophia.w@example.com", "sophia2"),
            ("Lucas", "P.", "lucas.p@example.com", "lucas"),
            ("Mia", "J.", "mia.j@example.com", "mia"),
            ("Henry", "B.", "henry.b@example.com", "henry"),
            ("Charlotte", "G.", "charlotte.g@example.com", "charlotte"),
            ("Alexander", "N.", "alex.n@example.com", "alex"),
            ("Amelia", "F.", "amelia.f@example.com", "amelia"),
            ("Sebastian", "H.", "sebastian.h@example.com", "sebastian"),
            ("Layla", "V.", "layla.v@example.com", "layla"),
        };

        foreach (var (first, last, email, avatarKey) in customerData)
        {
            var user = new User(first, last, email,
                _passwordHasher.Hash("Demo@123456"), UserRole.Customer);
            user.SetAvatar($"https://i.pravatar.cc/150?u={avatarKey}");
            customerUsers.Add(user);
        }

        _context.Users.Add(admin);
        _context.Users.AddRange(providerUsers);
        _context.Users.AddRange(customerUsers);

        // ════════════════════════════════════════════════
        // CATEGORIES (10)
        // ════════════════════════════════════════════════
        var haircutBarber = new Category("Haircut & Barbershop", "haircut-barbershop", "content_cut", 1);
        var spaMassage = new Category("Spa & Massage", "spa-massage", "spa", 2);
        var dentalCare = new Category("Dental Care", "dental-care", "medical_services", 3);
        var fitnessYoga = new Category("Fitness & Yoga", "fitness-yoga", "fitness_center", 4);
        var nailSalon = new Category("Nail Salon", "nail-salon", "brush", 5);
        var skincare = new Category("Skincare & Aesthetics", "skincare-aesthetics", "face", 6);
        var homeCleaning = new Category("Home Cleaning", "home-cleaning", "cleaning_services", 7);
        var personalTraining = new Category("Personal Training", "personal-training", "sports_gymnastics", 8);
        var dining = new Category("Dining", "dining", "restaurant", 9);
        var hotels = new Category("Hotels & Stays", "hotels-stays", "hotel", 10);

        var categories = new[] { haircutBarber, spaMassage, dentalCare, fitnessYoga, nailSalon, skincare, homeCleaning, personalTraining, dining, hotels };
        _context.Categories.AddRange(categories);

        // ── Sub-Categories ──
        _context.SubCategories.AddRange(
            new SubCategory(haircutBarber.Id, "Hair Styling", "hair-styling"),
            new SubCategory(haircutBarber.Id, "Barber", "barber"),
            new SubCategory(haircutBarber.Id, "Coloring", "coloring"),
            new SubCategory(spaMassage.Id, "Massage", "massage"),
            new SubCategory(spaMassage.Id, "Facial", "facial"),
            new SubCategory(spaMassage.Id, "Body Treatment", "body-treatment"),
            new SubCategory(dentalCare.Id, "General Dentistry", "general-dentistry"),
            new SubCategory(dentalCare.Id, "Cosmetic Dentistry", "cosmetic-dentistry"),
            new SubCategory(dentalCare.Id, "Orthodontics", "orthodontics"),
            new SubCategory(fitnessYoga.Id, "Yoga", "yoga"),
            new SubCategory(fitnessYoga.Id, "Pilates", "pilates"),
            new SubCategory(fitnessYoga.Id, "Group Classes", "group-classes"),
            new SubCategory(nailSalon.Id, "Manicure", "manicure"),
            new SubCategory(nailSalon.Id, "Pedicure", "pedicure"),
            new SubCategory(nailSalon.Id, "Nail Art", "nail-art"),
            new SubCategory(skincare.Id, "Dermatology", "dermatology"),
            new SubCategory(skincare.Id, "Aesthetics", "aesthetics"),
            new SubCategory(homeCleaning.Id, "Deep Clean", "deep-clean"),
            new SubCategory(homeCleaning.Id, "Regular Cleaning", "regular-cleaning"),
            new SubCategory(personalTraining.Id, "Strength Training", "strength-training"),
            new SubCategory(personalTraining.Id, "Weight Loss", "weight-loss"),
            new SubCategory(dining.Id, "Fine Dining", "fine-dining"),
            new SubCategory(dining.Id, "Casual Dining", "casual-dining"),
            new SubCategory(hotels.Id, "Luxury", "luxury"),
            new SubCategory(hotels.Id, "Boutique", "boutique"));

        // ════════════════════════════════════════════════
        // BUSINESSES (12)
        // ════════════════════════════════════════════════
        Business CreateBusiness(
            int ownerIdx, string name, string slug, string addr, string city, string zip,
            string country, string tz, string currency, string desc, string? email, string? phone,
            string? website, string cancelPolicy, double lat, double lon, double rating, int reviews,
            bool verified, string? coverUrl = null)
        {
            var biz = new Business(providerUsers[ownerIdx].Id, name, slug, addr, city, zip, country, tz, currency);
            biz.UpdateDetails(desc, email, phone, website, cancelPolicy);
            biz.SetGeoLocation(lat, lon);
            if (coverUrl != null) biz.SetImages(coverUrl, null);
            if (verified) biz.Verify();
            biz.UpdateRating(rating, reviews);
            return biz;
        }

        var biz1 = CreateBusiness(0, "Luxe Hair Studio", "luxe-hair-studio",
            "123 Main Street", "New York", "10001", "US", "America/New_York", "USD",
            "Premium hair styling and coloring services in the heart of Manhattan. Our master stylists specialize in balayage, precision cuts, and bridal styling.",
            "hello@luxehairstudio.com", "+12125551234", "https://luxehairstudio.com",
            "Free cancellation up to 24 hours before appointment.", 40.7128, -74.0060, 4.8, 127, true,
            "https://images.unsplash.com/photo-1560066984-138dadb4c035?w=800");
        var biz2 = CreateBusiness(1, "Elite Barber Shop", "elite-barber-shop",
            "456 Oak Avenue", "Los Angeles", "90001", "US", "America/Los_Angeles", "USD",
            "Classic and modern barber services for men. Hot towel shaves, skin fades, beard grooming, and more in a relaxed atmosphere.",
            "info@elitebarber.com", "+13105551234", null,
            "Cancel up to 12 hours before booking.", 34.0522, -118.2437, 4.6, 89, true,
            "https://images.unsplash.com/photo-1503951918675-f72ffbfa538a?w=800");
        var biz3 = CreateBusiness(2, "Serenity Spa & Wellness", "serenity-spa",
            "789 Pine Road", "San Francisco", "94102", "US", "America/Los_Angeles", "USD",
            "Award-winning spa offering massages, facials, and holistic body treatments. Escape the city hustle and rejuvenate your body and mind.",
            "book@serenityspa.com", "+14155551234", "https://serenityspa.com",
            "24-hour cancellation policy. Late arrivals may result in shortened service.", 37.7749, -122.4194, 4.9, 203, true,
            "https://images.unsplash.com/photo-1544161515-4ab6ce6db834?w=800");
        var biz4 = CreateBusiness(3, "Peak Fitness Center", "peak-fitness",
            "321 Elm Street", "Chicago", "60601", "US", "America/Chicago", "USD",
            "State-of-the-art gym with personal training, yoga, and group classes. Achieve your fitness goals with our certified trainers.",
            "hello@peakfitness.com", "+13125551234", "https://peakfitness.com",
            "Classes cancelled less than 6 hours before may be charged.", 41.8781, -87.6298, 4.7, 156, true,
            "https://images.unsplash.com/photo-1571902943202-507ec2618e8f?w=800");
        var biz5 = CreateBusiness(4, "Bright Smile Dental", "bright-smile-dental",
            "55 Park Lane", "Boston", "02101", "US", "America/New_York", "USD",
            "Comprehensive dental care from routine cleanings to cosmetic dentistry. Modern facility with gentle, patient-first approach.",
            "care@brightsmiledental.com", "+16175551234", "https://brightsmiledental.com",
            "Appointments can be rescheduled up to 48 hours in advance.", 42.3601, -71.0589, 4.5, 92, true,
            "https://images.unsplash.com/photo-1606811841689-23dfddce3e95?w=800");
        var biz6 = CreateBusiness(5, "Glow Nail Bar", "glow-nail-bar",
            "88 Sunset Blvd", "Miami", "33101", "US", "America/New_York", "USD",
            "Luxury nail salon offering manicures, pedicures, gel art, and nail extensions. Relax in our chic, modern space.",
            "glow@nailbar.com", "+13055551234", null,
            "Cancel up to 6 hours before your appointment.", 25.7617, -80.1918, 4.4, 67, false,
            "https://images.unsplash.com/photo-1604654894610-df63bc536371?w=800");
        var biz7 = CreateBusiness(6, "Radiance Skin Clinic", "radiance-skin-clinic",
            "12 Harley Street", "London", "W1G 9PF", "GB", "Europe/London", "GBP",
            "Advanced skincare and aesthetic treatments. Laser therapy, chemical peels, anti-aging treatments, and dermatology consultations.",
            "info@radianceclinic.co.uk", "+442075551234", "https://radianceclinic.co.uk",
            "48-hour cancellation notice required for all aesthetic procedures.", 51.5074, -0.1278, 4.7, 134, true,
            "https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=800");
        var biz8 = CreateBusiness(7, "Sparkle Home Services", "sparkle-home-services",
            "200 Industrial Ave", "Toronto", "M4B 1B3", "CA", "America/Toronto", "CAD",
            "Professional home cleaning services. Deep cleaning, regular maintenance, move-in/move-out cleaning, and eco-friendly options.",
            "book@sparklehome.ca", "+14165551234", "https://sparklehome.ca",
            "Free rescheduling up to 24 hours before the service.", 43.6532, -79.3832, 4.3, 45, false,
            "https://images.unsplash.com/photo-1581578731548-cda46f23f14b?w=800");
        var biz9 = CreateBusiness(8, "Transform Personal Training", "transform-pt",
            "15 Fitness Way", "Houston", "77001", "US", "America/Chicago", "USD",
            "One-on-one personal training and small group sessions. Specialized in strength training, weight loss, and functional fitness.",
            "train@transformpt.com", "+17135551234", "https://transformpt.com",
            "Sessions can be rescheduled with 12 hours notice.", 29.7604, -95.3698, 4.8, 78, true,
            "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=800");
        var biz10 = CreateBusiness(9, "Zen Yoga Studio", "zen-yoga-studio",
            "77 Peaceful Lane", "Seattle", "98101", "US", "America/Los_Angeles", "USD",
            "Serene yoga studio offering Vinyasa, Hatha, Yin, and hot yoga classes. All levels welcome from beginner to advanced.",
            "hello@zenyoga.com", "+12065551234", "https://zenyoga.com",
            "Class bookings can be cancelled up to 4 hours before start time.", 47.6062, -122.3321, 4.9, 178, true,
            "https://images.unsplash.com/photo-1544367551-2e0f4b6f0e2f?w=800");
        var biz11 = CreateBusiness(10, "The Velvet Dining", "velvet-dining",
            "5 Gourmet Street", "Dubai", "00000", "AE", "Asia/Dubai", "AED",
            "Michelin-starred fine dining experience. Reserve your table for an unforgettable culinary journey with chef tasting menus.",
            "reserve@velvetdining.ae", "+97145551234", "https://velvetdining.ae",
            "Table reservations can be cancelled up to 4 hours before.", 25.2048, 55.2708, 4.6, 112, true,
            "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800");
        var biz12 = CreateBusiness(11, "Aurora Boutique Hotel", "aurora-boutique-hotel",
            "100 Coastal Drive", "Lisbon", "1100-100", "PT", "Europe/Lisbon", "EUR",
            "Boutique hotel with spa, rooftop pool, and concierge services. Book your stay or reserve spa treatments during your visit.",
            "stay@aurorahotel.pt", "+351215551234", "https://aurorahotel.pt",
            "Room bookings follow standard hotel cancellation policies.", 38.7223, -9.1393, 4.7, 89, false,
            "https://images.unsplash.com/photo-1566073771259-6a8506099948?w=800");

        var businesses = new[] { biz1, biz2, biz3, biz4, biz5, biz6, biz7, biz8, biz9, biz10, biz11, biz12 };
        _context.Businesses.AddRange(businesses);

        // ════════════════════════════════════════════════
        // BUSINESS-CATEGORY LINKING
        // ════════════════════════════════════════════════
        _context.BusinessCategories.AddRange(
            new BusinessCategory(biz1.Id, haircutBarber.Id),
            new BusinessCategory(biz2.Id, haircutBarber.Id),
            new BusinessCategory(biz3.Id, spaMassage.Id),
            new BusinessCategory(biz4.Id, fitnessYoga.Id),
            new BusinessCategory(biz5.Id, dentalCare.Id),
            new BusinessCategory(biz6.Id, nailSalon.Id),
            new BusinessCategory(biz7.Id, skincare.Id),
            new BusinessCategory(biz8.Id, homeCleaning.Id),
            new BusinessCategory(biz9.Id, personalTraining.Id),
            new BusinessCategory(biz10.Id, fitnessYoga.Id),
            new BusinessCategory(biz11.Id, dining.Id),
            new BusinessCategory(biz12.Id, hotels.Id),
            // Cross-link some businesses to multiple categories
            new BusinessCategory(biz3.Id, skincare.Id),
            new BusinessCategory(biz4.Id, personalTraining.Id),
            new BusinessCategory(biz9.Id, fitnessYoga.Id));

        // ════════════════════════════════════════════════
        // BUSINESS IMAGES (gallery)
        // ════════════════════════════════════════════════
        var businessImages = new List<BusinessImage>();
        var imageSeeds = new[] { "salon", "barber", "spa", "gym", "dental", "nail", "skincare", "cleaning", "training", "yoga", "dining", "hotel" };
        for (int i = 0; i < businesses.Length; i++)
        {
            var biz = businesses[i];
            var seed = imageSeeds[i];
            businessImages.Add(new BusinessImage(biz.Id,
                $"https://images.unsplash.com/photo-sources/{seed}-1?w=800", $"{biz.Name} interior", 1, true));
            businessImages.Add(new BusinessImage(biz.Id,
                $"https://images.unsplash.com/photo-sources/{seed}-2?w=800", $"{biz.Name} detail", 2, false));
            businessImages.Add(new BusinessImage(biz.Id,
                $"https://images.unsplash.com/photo-sources/{seed}-3?w=800", $"{biz.Name} team", 3, false));
        }
        _context.BusinessImages.AddRange(businessImages);

        // ════════════════════════════════════════════════
        // PROVIDERS (2-3 per business)
        // ════════════════════════════════════════════════
        Provider CreateProvider(int userIdx, Guid businessId, string title, string bio, int order = 1)
        {
            var prov = new Provider(providerUsers[userIdx].Id, businessId, title);
            prov.UpdateDetails(title, bio, order);
            return prov;
        }

        // biz1: Luxe Hair Studio — 3 providers
        var prov1_1 = CreateProvider(0, biz1.Id, "Senior Stylist", "Specializing in balayage, precision cuts, and bridal styling. 10+ years experience.", 1);
        var prov1_2 = CreateProvider(12, biz1.Id, "Color Specialist", "Master colorist with expertise in vivid colors, corrections, and gloss treatments.", 2);
        var prov1_3 = CreateProvider(13, biz1.Id, "Junior Stylist", "Fresh talent trained in modern cuts, blowouts, and styling techniques.", 3);

        // biz2: Elite Barber Shop — 2 providers
        var prov2_1 = CreateProvider(1, biz2.Id, "Master Barber", "Expert in classic cuts, hot towel shaves, and beard grooming. 8 years experience.", 1);
        var prov2_2 = CreateProvider(14, biz2.Id, "Barber", "Skilled in skin fades, modern cuts, and kids' haircuts.", 2);

        // biz3: Serenity Spa — 3 providers
        var prov3_1 = CreateProvider(2, biz3.Id, "Lead Massage Therapist", "Certified in deep tissue, Swedish, hot stone, and aromatherapy massage.", 1);
        var prov3_2 = CreateProvider(15, biz3.Id, "Esthetician", "Specializing in facials, chemical peels, and skincare consultations.", 2);
        var prov3_3 = CreateProvider(16, biz3.Id, "Spa Therapist", "Body treatments, scrubs, wraps, and holistic wellness therapies.", 3);

        // biz4: Peak Fitness — 2 providers
        var prov4_1 = CreateProvider(3, biz4.Id, "Head Trainer", "NASM-certified personal trainer specializing in strength training and functional fitness.", 1);
        var prov4_2 = CreateProvider(17, biz4.Id, "Yoga Instructor", "200-RYT certified in Vinyasa, Hatha, and restorative yoga.", 2);

        // biz5: Bright Smile Dental — 2 providers
        var prov5_1 = CreateProvider(4, biz5.Id, "Lead Dentist", "DDS with 15 years experience in general and cosmetic dentistry.", 1);
        var prov5_2 = CreateProvider(18, biz5.Id, "Dental Hygienist", "Specializing in cleanings, whitening, and preventive care.", 2);

        // biz6: Glow Nail Bar — 2 providers
        var prov6_1 = CreateProvider(5, biz6.Id, "Lead Nail Technician", "Expert in gel, acrylic, and nail art with 7 years experience.", 1);
        var prov6_2 = CreateProvider(19, biz6.Id, "Nail Technician", "Manicures, pedicures, and dip powder specialist.", 2);

        // biz7: Radiance Skin Clinic — 2 providers
        var prov7_1 = CreateProvider(6, biz7.Id, "Consultant Dermatologist", "Board-certified dermatologist specializing in acne, anti-aging, and laser treatments.", 1);
        var prov7_2 = CreateProvider(20, biz7.Id, "Aesthetic Nurse", "Licensed nurse practitioner for injectables, fillers, and skin rejuvenation.", 2);

        // biz8: Sparkle Home Services — 2 providers
        var prov8_1 = CreateProvider(7, biz8.Id, "Lead Cleaning Tech", "Experienced in deep cleaning, move-in/move-out, and eco-friendly cleaning.", 1);
        var prov8_2 = CreateProvider(21, biz8.Id, "Cleaning Technician", "Regular maintenance cleaning and organization specialist.", 2);

        // biz9: Transform PT — 2 providers
        var prov9_1 = CreateProvider(8, biz9.Id, "Head Personal Trainer", "Certified strength and conditioning specialist (CSCS). Weight loss and muscle gain expert.", 1);
        var prov9_2 = CreateProvider(22, biz9.Id, "Personal Trainer", "ACE-certified trainer focusing on functional fitness and mobility.", 2);

        // biz10: Zen Yoga — 2 providers
        var prov10_1 = CreateProvider(9, biz10.Id, "Senior Yoga Instructor", "E-RYT 500 with 12 years experience. Vinyasa, Yin, and meditation specialist.", 1);
        var prov10_2 = CreateProvider(23, biz10.Id, "Yoga Instructor", "200-RYT certified in Hatha and hot yoga.", 2);

        // biz11: The Velvet Dining — 2 providers
        var prov11_1 = CreateProvider(10, biz11.Id, "Head Sommelier", "Certified sommelier curating wine pairings for chef tasting menus.", 1);
        var prov11_2 = CreateProvider(24, biz11.Id, "Maitre D'", "Managing reservations and ensuring exceptional dining experiences.", 2);

        // biz12: Aurora Boutique Hotel — 2 providers
        var prov12_1 = CreateProvider(11, biz12.Id, "Spa Manager", "Overseeing hotel spa treatments, massages, and wellness programs.", 1);
        var prov12_2 = CreateProvider(25, biz12.Id, "Concierge", "Assisting guests with bookings, local experiences, and special requests.", 2);

        var allProviders = new[]
        {
            prov1_1, prov1_2, prov1_3, prov2_1, prov2_2, prov3_1, prov3_2, prov3_3,
            prov4_1, prov4_2, prov5_1, prov5_2, prov6_1, prov6_2, prov7_1, prov7_2,
            prov8_1, prov8_2, prov9_1, prov9_2, prov10_1, prov10_2, prov11_1, prov11_2,
            prov12_1, prov12_2
        };
        _context.Providers.AddRange(allProviders);

        // ════════════════════════════════════════════════
        // SERVICES
        // ════════════════════════════════════════════════
        var services = new List<Service>
        {
            // Luxe Hair Studio
            new(biz1.Id, "Women's Haircut & Style", 45, 65),
            new(biz1.Id, "Men's Haircut", 30, 35),
            new(biz1.Id, "Balayage", 120, 180),
            new(biz1.Id, "Blowout & Style", 45, 55),
            new(biz1.Id, "Hair Coloring (Full)", 90, 150),
            // Elite Barber Shop
            new(biz2.Id, "Classic Cut", 30, 35),
            new(biz2.Id, "Hot Towel Shave", 45, 45),
            new(biz2.Id, "Beard Trim & Shape", 20, 20),
            new(biz2.Id, "Haircut + Beard Combo", 45, 50),
            // Serenity Spa
            new(biz3.Id, "Swedish Massage (60 min)", 60, 95),
            new(biz3.Id, "Deep Tissue Massage (60 min)", 60, 110),
            new(biz3.Id, "Hot Stone Massage (75 min)", 75, 130),
            new(biz3.Id, "Classic Facial", 50, 85),
            new(biz3.Id, "Body Scrub & Wrap", 60, 100),
            // Peak Fitness
            new(biz4.Id, "Personal Training Session", 60, 75),
            new(biz4.Id, "Yoga Class", 60, 20),
            new(biz4.Id, "Pilates Class", 45, 20),
            new(biz4.Id, "Fitness Assessment", 30, 0),
            // Bright Smile Dental
            new(biz5.Id, "Routine Cleaning", 30, 80),
            new(biz5.Id, "Teeth Whitening", 60, 250),
            new(biz5.Id, "Dental Checkup", 30, 60),
            // Glow Nail Bar
            new(biz6.Id, "Classic Manicure", 30, 25),
            new(biz6.Id, "Gel Manicure", 45, 40),
            new(biz6.Id, "Spa Pedicure", 45, 45),
            new(biz6.Id, "Nail Art Design", 60, 55),
            // Radiance Skin Clinic
            new(biz7.Id, "Dermatology Consultation", 30, 120),
            new(biz7.Id, "Chemical Peel", 45, 150),
            new(biz7.Id, "Laser Skin Treatment", 60, 200),
            // Sparkle Home Services
            new(biz8.Id, "Deep Home Cleaning", 180, 150),
            new(biz8.Id, "Regular Cleaning (Weekly)", 120, 80),
            new(biz8.Id, "Move-In/Out Cleaning", 240, 200),
            // Transform PT
            new(biz9.Id, "1-on-1 Personal Training", 60, 75),
            new(biz9.Id, "Small Group Training", 45, 40),
            new(biz9.Id, "Nutrition Consultation", 30, 50),
            // Zen Yoga Studio
            new(biz10.Id, "Vinyasa Yoga Class", 60, 20),
            new(biz10.Id, "Hatha Yoga Class", 60, 18),
            new(biz10.Id, "Yin Yoga Class", 75, 25),
            // The Velvet Dining
            new(biz11.Id, "Chef Tasting Menu", 90, 120),
            new(biz11.Id, "Wine Pairing Experience", 60, 65),
            // Aurora Boutique Hotel
            new(biz12.Id, "Spa Massage (60 min)", 60, 90),
            new(biz12.Id, "Rooftop Pool Access", 120, 30),
        };
        _context.Services.AddRange(services);

        // ════════════════════════════════════════════════
        // PROVIDER-SERVICE LINKING
        // ════════════════════════════════════════════════
        _context.ProviderServices.AddRange(
            // Luxe Hair Studio — prov1_1 (Sophia) provides services 0-4
            new ProviderService(prov1_1.Id, services[0].Id),
            new ProviderService(prov1_1.Id, services[1].Id),
            new ProviderService(prov1_1.Id, services[2].Id),
            new ProviderService(prov1_1.Id, services[3].Id),
            new ProviderService(prov1_1.Id, services[4].Id),
            // prov1_2 (Leila) provides services 2,3,4 (color)
            new ProviderService(prov1_2.Id, services[2].Id),
            new ProviderService(prov1_2.Id, services[3].Id),
            new ProviderService(prov1_2.Id, services[4].Id),
            // prov1_3 (Thomas) provides services 0,1,3
            new ProviderService(prov1_3.Id, services[0].Id),
            new ProviderService(prov1_3.Id, services[1].Id),
            new ProviderService(prov1_3.Id, services[3].Id),
            // Elite Barber Shop — prov2_1 (Marcus) provides 5-8
            new ProviderService(prov2_1.Id, services[5].Id),
            new ProviderService(prov2_1.Id, services[6].Id),
            new ProviderService(prov2_1.Id, services[7].Id),
            new ProviderService(prov2_1.Id, services[8].Id),
            // prov2_2 (Brian) provides 5,7,8
            new ProviderService(prov2_2.Id, services[5].Id),
            new ProviderService(prov2_2.Id, services[7].Id),
            new ProviderService(prov2_2.Id, services[8].Id),
            // Serenity Spa — prov3_1 (Aisha) provides 9-13
            new ProviderService(prov3_1.Id, services[9].Id),
            new ProviderService(prov3_1.Id, services[10].Id),
            new ProviderService(prov3_1.Id, services[11].Id),
            new ProviderService(prov3_1.Id, services[12].Id),
            new ProviderService(prov3_1.Id, services[13].Id),
            // prov3_2 (Grace) provides 12,13
            new ProviderService(prov3_2.Id, services[12].Id),
            new ProviderService(prov3_2.Id, services[13].Id),
            // prov3_3 (Daniel) provides 9,10,13
            new ProviderService(prov3_3.Id, services[9].Id),
            new ProviderService(prov3_3.Id, services[10].Id),
            new ProviderService(prov3_3.Id, services[13].Id),
            // Peak Fitness — prov4_1 (James) provides 14,17
            new ProviderService(prov4_1.Id, services[14].Id),
            new ProviderService(prov4_1.Id, services[17].Id),
            // prov4_2 (Yuki) provides 15,16
            new ProviderService(prov4_2.Id, services[15].Id),
            new ProviderService(prov4_2.Id, services[16].Id),
            // Bright Smile Dental — prov5_1 (Elena) provides 18,19,20
            new ProviderService(prov5_1.Id, services[18].Id),
            new ProviderService(prov5_1.Id, services[19].Id),
            new ProviderService(prov5_1.Id, services[20].Id),
            // prov5_2 (Amara) provides 18,20
            new ProviderService(prov5_2.Id, services[18].Id),
            new ProviderService(prov5_2.Id, services[20].Id),
            // Glow Nail Bar — prov6_1 (David) provides 21-24
            new ProviderService(prov6_1.Id, services[21].Id),
            new ProviderService(prov6_1.Id, services[22].Id),
            new ProviderService(prov6_1.Id, services[23].Id),
            new ProviderService(prov6_1.Id, services[24].Id),
            // prov6_2 (Victor) provides 21,22,23
            new ProviderService(prov6_2.Id, services[21].Id),
            new ProviderService(prov6_2.Id, services[22].Id),
            new ProviderService(prov6_2.Id, services[23].Id),
            // Radiance Skin Clinic — prov7_1 (Priya) provides 25-27
            new ProviderService(prov7_1.Id, services[25].Id),
            new ProviderService(prov7_1.Id, services[26].Id),
            new ProviderService(prov7_1.Id, services[27].Id),
            // prov7_2 (Sara) provides 25,26,27
            new ProviderService(prov7_2.Id, services[25].Id),
            new ProviderService(prov7_2.Id, services[26].Id),
            new ProviderService(prov7_2.Id, services[27].Id),
            // Sparkle Home Services — prov8_1 (Omar) provides 28-30
            new ProviderService(prov8_1.Id, services[28].Id),
            new ProviderService(prov8_1.Id, services[29].Id),
            new ProviderService(prov8_1.Id, services[30].Id),
            // prov8_2 (Hassan) provides 28,29
            new ProviderService(prov8_2.Id, services[28].Id),
            new ProviderService(prov8_2.Id, services[29].Id),
            // Transform PT — prov9_1 (Rachel) provides 31-33
            new ProviderService(prov9_1.Id, services[31].Id),
            new ProviderService(prov9_1.Id, services[32].Id),
            new ProviderService(prov9_1.Id, services[33].Id),
            // prov9_2 (Iris) provides 31,33
            new ProviderService(prov9_2.Id, services[31].Id),
            new ProviderService(prov9_2.Id, services[33].Id),
            // Zen Yoga — prov10_1 (Carlos) provides 34-36
            new ProviderService(prov10_1.Id, services[34].Id),
            new ProviderService(prov10_1.Id, services[35].Id),
            new ProviderService(prov10_1.Id, services[36].Id),
            // prov10_2 (Nathan) provides 34,35
            new ProviderService(prov10_2.Id, services[34].Id),
            new ProviderService(prov10_2.Id, services[35].Id),
            // The Velvet Dining — prov11_1 (Nadia) provides 37,38
            new ProviderService(prov11_1.Id, services[37].Id),
            new ProviderService(prov11_1.Id, services[38].Id),
            // prov11_2 (Maya) provides 37
            new ProviderService(prov11_2.Id, services[37].Id),
            // Aurora Boutique Hotel — prov12_1 (James W) provides 39,40
            new ProviderService(prov12_1.Id, services[39].Id),
            new ProviderService(prov12_1.Id, services[40].Id),
            // prov12_2 (Felix) provides 39
            new ProviderService(prov12_2.Id, services[39].Id));

        // ════════════════════════════════════════════════
        // PROVIDER AVAILABILITY (Mon-Fri 9AM-6PM, Sat 10AM-3PM)
        // ════════════════════════════════════════════════
        var availabilities = new List<ProviderAvailability>();
        foreach (var prov in allProviders)
        {
            foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday })
            {
                availabilities.Add(new ProviderAvailability(prov.Id, day,
                    new TimeOnly(9, 0), new TimeOnly(18, 0), 60));
            }
            // Saturday 10AM-3PM
            availabilities.Add(new ProviderAvailability(prov.Id, DayOfWeek.Saturday,
                new TimeOnly(10, 0), new TimeOnly(15, 0), 60));
        }
        _context.ProviderAvailabilities.AddRange(availabilities);

        // ════════════════════════════════════════════════
        // PAST COMPLETED APPOINTMENTS (for reviews)
        // ════════════════════════════════════════════════
        var appointments = new List<Appointment>();
        var reviews = new List<Review>();
        var now = DateTime.UtcNow;
        var rand = new Random(42);

        // Helper: create a completed appointment + review for a business
        void CreateCompletedAppointmentWithReview(
            Business business, Provider provider, Service service,
            int customerIdx, int rating, string comment, int daysAgo)
        {
            var startTime = now.AddDays(-daysAgo).Date.AddHours(10 + (daysAgo % 6));
            var endTime = startTime.AddMinutes(service.DurationMinutes);
            var bookingRef = $"BK{startTime:yyMMddHHmm}";
            var appt = new Appointment(bookingRef, customerUsers[customerIdx % customerUsers.Count].Id, provider.Id,
                service.Id, business.Id, startTime, endTime, service.PriceAmount, business.Currency);
            appt.Confirm();
            appt.Start();
            appt.Complete();
            _context.Appointments.Add(appt);
            _context.SaveChanges(); // flush to get Id

            var review = new Review(appt.Id, business.Id, customerUsers[customerIdx % customerUsers.Count].Id, rating, comment);
            _context.Reviews.Add(review);
        }

        // Reviews for biz1 (Luxe Hair Studio) — 5 reviews
        CreateCompletedAppointmentWithReview(biz1, prov1_1, services[0], 1, 5, "Sophia did an amazing balayage and cut. My hair has never looked better!", 12);
        CreateCompletedAppointmentWithReview(biz1, prov1_2, services[4], 2, 4, "Great coloring service, though it took a bit longer than expected. Still very happy with the result.", 28);
        CreateCompletedAppointmentWithReview(biz1, prov1_1, services[3], 3, 5, "Loved my blowout! Very professional and the salon is beautiful.", 45);
        CreateCompletedAppointmentWithReview(biz1, prov1_3, services[1], 4, 5, "Quick and easy men's cut. Friendly service!", 60);
        CreateCompletedAppointmentWithReview(biz1, prov1_1, services[2], 5, 4, "The balayage was perfect. Will definitely book again.", 75);

        // Reviews for biz2 (Elite Barber Shop) — 4 reviews
        CreateCompletedAppointmentWithReview(biz2, prov2_1, services[5], 6, 5, "Best barber in LA. Classic cut and hot towel shave — felt amazing.", 15);
        CreateCompletedAppointmentWithReview(biz2, prov2_2, services[7], 7, 4, "Great beard trim and shape. Clean shop, good vibes.", 30);
        CreateCompletedAppointmentWithReview(biz2, prov2_1, services[8], 8, 5, "Haircut + beard combo was perfect. Highly recommend Marcus!", 50);
        CreateCompletedAppointmentWithReview(biz2, prov2_1, services[6], 9, 5, "Hot towel shave was a treat. Will be back soon.", 90);

        // Reviews for biz3 (Serenity Spa) — 5 reviews
        CreateCompletedAppointmentWithReview(biz3, prov3_1, services[9], 10, 5, "Best Swedish massage I've ever had. Aisha is incredible.", 10);
        CreateCompletedAppointmentWithReview(biz3, prov3_2, services[12], 11, 5, "The facial was amazing. My skin has never felt smoother.", 20);
        CreateCompletedAppointmentWithReview(biz3, prov3_1, services[10], 12, 4, "Deep tissue was intense but in a good way. Great value.", 35);
        CreateCompletedAppointmentWithReview(biz3, prov3_3, services[13], 13, 5, "Body scrub and wrap was heavenly. So relaxing.", 55);
        CreateCompletedAppointmentWithReview(biz3, prov3_1, services[11], 14, 5, "Hot stone massage was exactly what I needed. 5 stars!", 80);

        // Reviews for biz4 (Peak Fitness) — 4 reviews
        CreateCompletedAppointmentWithReview(biz4, prov4_1, services[14], 15, 5, "James helped me transform my fitness. Great personal training!", 18);
        CreateCompletedAppointmentWithReview(biz4, prov4_2, services[15], 16, 4, "Good yoga class. Will try the advanced one next time.", 40);
        CreateCompletedAppointmentWithReview(biz4, prov4_1, services[17], 17, 5, "Fitness assessment was thorough and motivating.", 65);
        CreateCompletedAppointmentWithReview(biz4, prov4_2, services[16], 18, 4, "Pilates class was challenging but fun.", 100);

        // Reviews for biz5 (Bright Smile Dental) — 3 reviews
        CreateCompletedAppointmentWithReview(biz5, prov5_1, services[18], 19, 5, "Dr. Elena was so gentle and professional. No pain at all!", 22);
        CreateCompletedAppointmentWithReview(biz5, prov5_2, services[20], 20, 4, "Routine checkup was quick and efficient. Great staff.", 48);
        CreateCompletedAppointmentWithReview(biz5, prov5_1, services[19], 21, 5, "Teeth whitening results are amazing! Highly recommend.", 70);

        // Reviews for biz6 (Glow Nail Bar) — 3 reviews
        CreateCompletedAppointmentWithReview(biz6, prov6_1, services[21], 22, 4, "Nice manicure. Clean salon, friendly staff.", 14);
        CreateCompletedAppointmentWithReview(biz6, prov6_2, services[23], 23, 5, "Gel manicure lasted 3 weeks! Great quality.", 33);
        CreateCompletedAppointmentWithReview(biz6, prov6_1, services[24], 24, 4, "Nail art design was exactly what I wanted. Love it!", 58);

        // Reviews for biz7 (Radiance Skin Clinic) — 4 reviews
        CreateCompletedAppointmentWithReview(biz7, prov7_1, services[25], 25, 5, "Dr. Priya was thorough and explained everything. Great consultation.", 16);
        CreateCompletedAppointmentWithReview(biz7, prov7_2, services[26], 26, 4, "Chemical peel was effective. Will do another session.", 38);
        CreateCompletedAppointmentWithReview(biz7, prov7_1, services[27], 27, 5, "Laser treatment was painless and effective. Highly recommend!", 62);
        CreateCompletedAppointmentWithReview(biz7, prov7_2, services[25], 28, 5, "Aesthetic nurse was very knowledgeable and kind.", 85);

        // Reviews for biz8 (Sparkle Home Services) — 3 reviews
        CreateCompletedAppointmentWithReview(biz8, prov8_1, services[28], 29, 5, "My house has never been this clean! Omar is a perfectionist.", 11);
        CreateCompletedAppointmentWithReview(biz8, prov8_2, services[29], 30, 4, "Regular cleaning service is reliable and thorough.", 42);
        CreateCompletedAppointmentWithReview(biz8, prov8_1, services[30], 31, 5, "Move-out cleaning was spotless. Saved my security deposit!", 68);

        // Reviews for biz9 (Transform PT) — 3 reviews
        CreateCompletedAppointmentWithReview(biz9, prov9_1, services[31], 32, 5, "Rachel helped me lose 20 pounds. Amazing trainer!", 19);
        CreateCompletedAppointmentWithReview(biz9, prov9_2, services[33], 33, 4, "Nutrition consultation was very helpful. Will book more.", 44);
        CreateCompletedAppointmentWithReview(biz9, prov9_1, services[32], 34, 5, "Small group training is fun and effective.", 72);

        // Reviews for biz10 (Zen Yoga) — 4 reviews
        CreateCompletedAppointmentWithReview(biz10, prov10_1, services[34], 35, 5, "Best yoga class in Seattle. Carlos is an amazing teacher.", 13);
        CreateCompletedAppointmentWithReview(biz10, prov10_2, services[35], 36, 4, "Hatha class was relaxing and well-paced.", 36);
        CreateCompletedAppointmentWithReview(biz10, prov10_1, services[36], 37, 5, "Yin yoga was exactly what I needed. So peaceful.", 52);
        CreateCompletedAppointmentWithReview(biz10, prov10_1, services[34], 38, 5, "Vinyasa class was challenging but rewarding. Love it!", 78);

        // Reviews for biz11 (The Velvet Dining) — 3 reviews
        CreateCompletedAppointmentWithReview(biz11, prov11_1, services[37], 39, 5, "Unforgettable dining experience. Wine pairing was perfect!", 21);
        CreateCompletedAppointmentWithReview(biz11, prov11_2, services[38], 40, 4, "Wine pairing experience was delightful. Great service.", 47);
        CreateCompletedAppointmentWithReview(biz11, prov11_1, services[37], 41, 5, "Michelin-starred quality. Will definitely return!", 74);

        // Reviews for biz12 (Aurora Boutique Hotel) — 3 reviews
        CreateCompletedAppointmentWithReview(biz12, prov12_1, services[39], 42, 5, "Amazing spa massage. The hotel is beautiful too.", 23);
        CreateCompletedAppointmentWithReview(biz12, prov12_2, services[40], 43, 4, "Pool access was great. Friendly concierge service.", 49);
        CreateCompletedAppointmentWithReview(biz12, prov12_1, services[39], 44, 5, "Best spa treatment I've had. Will book again!", 76);

        // ════════════════════════════════════════════════
        // FUTURE BOOKED APPOINTMENTS (to show booked slots)
        // ════════════════════════════════════════════════
        var futureAppointments = new List<Appointment>();
        // Create a few confirmed future appointments to make the calendar look realistic
        var futureSlots = new[]
        {
            (biz1, prov1_1, services[0], 1, 3),   // 3 days from now
            (biz1, prov1_2, services[4], 2, 5),   // 5 days from now
            (biz3, prov3_1, services[9], 3, 2),   // 2 days from now
            (biz3, prov3_2, services[12], 4, 7),   // 7 days from now
            (biz4, prov4_1, services[14], 5, 4),   // 4 days from now
            (biz5, prov5_1, services[18], 6, 6),   // 6 days from now
            (biz7, prov7_1, services[25], 7, 1),   // 1 day from now
            (biz9, prov9_1, services[31], 8, 8),   // 8 days from now
            (biz10, prov10_1, services[34], 9, 3),  // 3 days from now
            (biz11, prov11_1, services[37], 10, 9), // 9 days from now
        };

        foreach (var (business, provider, service, custIdx, daysAhead) in futureSlots)
        {
            var startTime = now.AddDays(daysAhead).Date.AddHours(14);
            var endTime = startTime.AddMinutes(service.DurationMinutes);
            var bookingRef = $"FUT{startTime:yyMMddHHmm}";
            var appt = new Appointment(bookingRef, customerUsers[custIdx].Id, provider.Id,
                service.Id, business.Id, startTime, endTime, service.PriceAmount, business.Currency);
            appt.Confirm();
            futureAppointments.Add(appt);
        }
        _context.Appointments.AddRange(futureAppointments);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Database seed completed successfully. Seeded {Categories} categories, {Businesses} businesses, {Providers} providers, {Services} services, {Reviews} reviews, {Appointments} appointments.",
            categories.Length, businesses.Length, allProviders.Length, services.Count, reviews.Count, appointments.Count + futureAppointments.Count);
    }
}
