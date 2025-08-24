using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext? _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext? context = null)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        
        return View();
    }

    public IActionResult AvailableCars()
    {
        return View();
    }

    public IActionResult FleetManagement()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddVehicle([FromBody] VehicleViewModel vehicleData)
    {
        try
        {
            if (_context == null)
            {
                return Json(new { success = false, message = "Database context not available" });
            }

            // Check if registration number already exists
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.RegistrationNumber == vehicleData.RegistrationNumber);
            
            if (existingVehicle != null)
            {
                return Json(new { success = false, message = "Vehicle with this registration number already exists" });
            }

            // Check if chassis number already exists
            existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.ChassisNumber == vehicleData.ChassisNumber);
            
            if (existingVehicle != null)
            {
                return Json(new { success = false, message = "Vehicle with this chassis number already exists" });
            }

            // Create new vehicle
            var vehicle = new Vehicle
            {
                VehicleType = vehicleData.VehicleType,
                Model = vehicleData.Model,
                Year = vehicleData.Year,
                RegistrationNumber = vehicleData.RegistrationNumber,
                ChassisNumber = vehicleData.ChassisNumber,
                Color = vehicleData.Color,
                EngineCapacity = vehicleData.EngineCapacity,
                FuelType = vehicleData.FuelType,
                DailyRate = vehicleData.DailyRate,
                Seats = vehicleData.Seats,
                Status = "Available",
                ImageUrl = vehicleData.ImageUrl,
                Description = vehicleData.Description,
                Features = string.Join(",", vehicleData.Features),
                CreatedAt = DateTime.Now
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Vehicle added successfully!", vehicleId = vehicle.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle");
            return Json(new { success = false, message = "Error adding vehicle: " + ex.Message });
        }
    }

    public async Task<IActionResult> ViewAllVehicles()
    {
        try
        {
            if (_context == null)
            {
                ViewBag.Vehicles = new List<VehicleViewModel>();
                return View();
            }

            var vehicles = await _context.Vehicles
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var vehicleViewModels = vehicles.Select(v => new VehicleViewModel
            {
                Id = v.Id,
                VehicleType = v.VehicleType,
                Model = v.Model,
                Year = v.Year,
                RegistrationNumber = v.RegistrationNumber,
                ChassisNumber = v.ChassisNumber,
                Color = v.Color,
                EngineCapacity = v.EngineCapacity,
                FuelType = v.FuelType,
                DailyRate = v.DailyRate,
                Seats = v.Seats,
                Status = v.Status,
                ImageUrl = v.ImageUrl,
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            ViewBag.Vehicles = vehicleViewModels;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            ViewBag.Vehicles = new List<VehicleViewModel>();
            return View();
        }
    }

    public IActionResult FinesManagement()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var userName = HttpContext.Session.GetString("UserName");
        ViewBag.UserName = userName;
        
        return View();
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult VehicleTypes()
    {
        return View();
    }

    public IActionResult ViewVehiclesByCategory(string category)
    {
        // For now, we'll use mock data since we don't have a database connection
        // In a real application, this would query the database for vehicles by category
        var vehicles = GetMockVehiclesByCategory(category);
        ViewBag.Category = category;
        ViewBag.Vehicles = vehicles;
        return View();
    }

    private List<VehicleViewModel> GetMockVehiclesByCategory(string category)
    {
        var vehicles = new List<VehicleViewModel>();
        
        switch (category?.ToLower())
        {
            case "motor-bike":
                vehicles.AddRange(new List<VehicleViewModel>
                {
                    new VehicleViewModel { Id = 1, VehicleType = "Motor Bike", Model = "Honda CG125", Year = 2023, RegistrationNumber = "MB-001", Color = "Red", EngineCapacity = "125cc", FuelType = "Petrol", DailyRate = 15.00M, Seats = 2, Status = "Available", ImageUrl = "/images/motor-bike-1.jpg" },
                    new VehicleViewModel { Id = 2, VehicleType = "Motor Bike", Model = "Yamaha YBR125", Year = 2022, RegistrationNumber = "MB-002", Color = "Blue", EngineCapacity = "125cc", FuelType = "Petrol", DailyRate = 18.00M, Seats = 2, Status = "Available", ImageUrl = "/images/motor-bike-2.jpg" },
                    new VehicleViewModel { Id = 3, VehicleType = "Motor Bike", Model = "Suzuki GSX-R150", Year = 2024, RegistrationNumber = "MB-003", Color = "Black", EngineCapacity = "150cc", FuelType = "Petrol", DailyRate = 25.00M, Seats = 2, Status = "Rented", ImageUrl = "/images/motor-bike-3.jpg" }
                });
                break;
                
            case "cng":
                vehicles.AddRange(new List<VehicleViewModel>
                {
                    new VehicleViewModel { Id = 4, VehicleType = "CNG", Model = "Toyota Corolla CNG", Year = 2023, RegistrationNumber = "CNG-001", Color = "White", EngineCapacity = "1800cc", FuelType = "CNG", DailyRate = 25.00M, Seats = 5, Status = "Available", ImageUrl = "/images/cng-1.jpg" },
                    new VehicleViewModel { Id = 5, VehicleType = "CNG", Model = "Honda City CNG", Year = 2022, RegistrationNumber = "CNG-002", Color = "Silver", EngineCapacity = "1500cc", FuelType = "CNG", DailyRate = 28.00M, Seats = 5, Status = "Available", ImageUrl = "/images/cng-2.jpg" }
                });
                break;
                
            case "private-car":
                vehicles.AddRange(new List<VehicleViewModel>
                {
                    new VehicleViewModel { Id = 6, VehicleType = "Private Car", Model = "Toyota Camry", Year = 2024, RegistrationNumber = "PC-001", Color = "Black", EngineCapacity = "2500cc", FuelType = "Petrol", DailyRate = 75.00M, Seats = 5, Status = "Available", ImageUrl = "/images/car-1.jpg" },
                    new VehicleViewModel { Id = 7, VehicleType = "Private Car", Model = "Honda Civic", Year = 2023, RegistrationNumber = "PC-002", Color = "White", EngineCapacity = "1800cc", FuelType = "Petrol", DailyRate = 65.00M, Seats = 5, Status = "Rented", ImageUrl = "/images/car-2.jpg" },
                    new VehicleViewModel { Id = 8, VehicleType = "Private Car", Model = "BMW 3 Series", Year = 2024, RegistrationNumber = "PC-003", Color = "Blue", EngineCapacity = "2000cc", FuelType = "Petrol", DailyRate = 120.00M, Seats = 5, Status = "Available", ImageUrl = "/images/car-3.jpg" },
                    new VehicleViewModel { Id = 9, VehicleType = "Private Car", Model = "Mercedes C-Class", Year = 2023, RegistrationNumber = "PC-004", Color = "Silver", EngineCapacity = "2000cc", FuelType = "Diesel", DailyRate = 150.00M, Seats = 5, Status = "Available", ImageUrl = "/images/car-4.jpg" }
                });
                break;
                
            case "pickup":
                vehicles.AddRange(new List<VehicleViewModel>
                {
                    new VehicleViewModel { Id = 10, VehicleType = "Pickup", Model = "Ford F-150", Year = 2023, RegistrationNumber = "PK-001", Color = "Red", EngineCapacity = "3500cc", FuelType = "Petrol", DailyRate = 80.00M, Seats = 5, Status = "Available", ImageUrl = "/images/pickup-1.jpg" },
                    new VehicleViewModel { Id = 11, VehicleType = "Pickup", Model = "Toyota Hilux", Year = 2022, RegistrationNumber = "PK-002", Color = "White", EngineCapacity = "2400cc", FuelType = "Diesel", DailyRate = 70.00M, Seats = 5, Status = "Available", ImageUrl = "/images/pickup-2.jpg" }
                });
                break;
                
            case "truck":
                vehicles.AddRange(new List<VehicleViewModel>
                {
                    new VehicleViewModel { Id = 12, VehicleType = "Truck", Model = "Volvo FH16", Year = 2023, RegistrationNumber = "TR-001", Color = "Blue", EngineCapacity = "16000cc", FuelType = "Diesel", DailyRate = 200.00M, Seats = 3, Status = "Available", ImageUrl = "/images/truck-1.jpg" },
                    new VehicleViewModel { Id = 13, VehicleType = "Truck", Model = "Scania R500", Year = 2022, RegistrationNumber = "TR-002", Color = "Red", EngineCapacity = "13000cc", FuelType = "Diesel", DailyRate = 180.00M, Seats = 3, Status = "Rented", ImageUrl = "/images/truck-2.jpg" }
                });
                break;
                
            case "covered-van":
                vehicles.AddRange(new List<VehicleViewModel>
                {
                    new VehicleViewModel { Id = 14, VehicleType = "Covered Van", Model = "Mercedes Sprinter", Year = 2023, RegistrationNumber = "CV-001", Color = "White", EngineCapacity = "2200cc", FuelType = "Diesel", DailyRate = 90.00M, Seats = 3, Status = "Available", ImageUrl = "/images/van-1.jpg" },
                    new VehicleViewModel { Id = 15, VehicleType = "Covered Van", Model = "Ford Transit", Year = 2022, RegistrationNumber = "CV-002", Color = "Silver", EngineCapacity = "2000cc", FuelType = "Diesel", DailyRate = 85.00M, Seats = 3, Status = "Available", ImageUrl = "/images/van-2.jpg" }
                });
                break;
        }
        
        return vehicles;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
