using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace CarRentalManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext? _context;
    private readonly IWebHostEnvironment? _env;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext? context = null, IWebHostEnvironment? env = null)
    {
        _logger = logger;
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            if (_context != null)
            {
                var vehicles = await _context.Vehicles
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(6) // Show only 6 latest vehicles on home page
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
                    ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                    Description = v.Description,
                    Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                    CreatedAt = v.CreatedAt
                }).ToList();

                ViewBag.FeaturedVehicles = vehicleViewModels;
            }
            else
            {
                ViewBag.FeaturedVehicles = new List<VehicleViewModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured vehicles");
            ViewBag.FeaturedVehicles = new List<VehicleViewModel>();
        }
        
        return View();
    }

    [HttpDelete]
    [Route("Home/DeleteVehicle/{id}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        try
        {
            if (_context == null)
            {
                return StatusCode(500, new { success = false, message = "Database context not available" });
            }

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
            {
                return NotFound(new { success = false, message = "Vehicle not found" });
            }

            // Attempt to delete the image file only if it is a local upload under wwwroot/uploads/vehicles
            try
            {
                if (!string.IsNullOrWhiteSpace(vehicle.ImageUrl) && _env != null)
                {
                    // Accept both relative "/uploads/vehicles/..." and absolute paths that map under webroot
                    var imagePath = vehicle.ImageUrl.Replace("\\", "/");
                    if (imagePath.StartsWith("/uploads/vehicles/", StringComparison.OrdinalIgnoreCase))
                    {
                        var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
            }
            catch (Exception fileEx)
            {
                // Log and continue; failing to delete a file should not block DB deletion
                _logger.LogWarning(fileEx, "Failed to delete vehicle image for vehicle {VehicleId}", id);
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Vehicle deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
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

    public async Task<IActionResult> AvailableCars()
    {
        try
        {
            if (_context == null)
            {
                ViewBag.Vehicles = new List<VehicleViewModel>();
                return View();
            }

            var vehicles = await _context.Vehicles
                .Where(v => v.Status == "Available")
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
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            ViewBag.Vehicles = vehicleViewModels;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available vehicles");
            ViewBag.Vehicles = new List<VehicleViewModel>();
            return View();
        }
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
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
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

    public async Task<IActionResult> ViewVehiclesByCategory(string category)
    {
        try
        {
            if (_context == null)
            {
                ViewBag.Category = category;
                ViewBag.Vehicles = new List<VehicleViewModel>();
                return View();
            }

            var vehicles = await _context.Vehicles
                .Where(v => v.VehicleType.ToLower() == category.ToLower())
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
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            ViewBag.Category = category;
            ViewBag.Vehicles = vehicleViewModels;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles by category");
            ViewBag.Category = category;
            ViewBag.Vehicles = new List<VehicleViewModel>();
            return View();
        }
    }

    private string GetDefaultImageUrl(string vehicleType, string currentImageUrl)
    {
        // If there's already an image URL, use it
        if (!string.IsNullOrEmpty(currentImageUrl))
        {
            return currentImageUrl;
        }

        // Return default images based on vehicle type
        return vehicleType.ToLower() switch
        {
            "motor bike" => "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&h=300&fit=crop",
            "cng" => "https://images.unsplash.com/photo-1549924231-f129b911e442?w=400&h=300&fit=crop",
            "private car" => "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=400&h=300&fit=crop",
            "pickup" => "https://images.unsplash.com/photo-1582639510494-c80b5de9f148?w=400&h=300&fit=crop",
            "truck" => "https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=400&h=300&fit=crop",
            "covered van" => "https://images.unsplash.com/photo-1582639510494-c80b5de9f148?w=400&h=300&fit=crop",
            _ => "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=400&h=300&fit=crop"
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetFleet()
    {
        try
        {
            if (_context == null)
            {
                return Ok(new List<VehicleViewModel>());
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
                ImageUrl = GetDefaultImageUrl(v.VehicleType, v.ImageUrl),
                Description = v.Description,
                Features = string.IsNullOrEmpty(v.Features) ? new List<string>() : v.Features.Split(',').ToList(),
                CreatedAt = v.CreatedAt
            }).ToList();

            return Ok(vehicleViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fleet");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadVehicleImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            if (_env == null)
            {
                return StatusCode(500, new { success = false, message = "Hosting environment not available" });
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "vehicles");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var safeFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(uploadsRoot, safeFileName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/vehicles/{safeFileName}";
            return Ok(new { success = true, url = relativePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading vehicle image");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
