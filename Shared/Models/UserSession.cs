using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiriMovies.Shared.Models;

public class UserSession
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string DeviceType { get; set; } = string.Empty; // Mobile, Desktop, Tablet, TV, etc.
    
    [MaxLength(100)]
    public string? DeviceId { get; set; } // Unique device identifier/fingerprint
    
    [MaxLength(100)]
    public string? DeviceName { get; set; } // User-friendly device name (e.g., "John's Laptop", "Living Room PC")
    
    [MaxLength(100)]
    public string? ComputerName { get; set; } // System computer name
    
    [MaxLength(100)]
    public string? MacAddress { get; set; } // Network MAC address
    
    [MaxLength(100)]
    public string? ProcessorId { get; set; } // CPU identifier
    
    [MaxLength(50)]
    public string? LocalIP { get; set; } // Local network IP
    
    [MaxLength(500)]
    public string? UserAgent { get; set; } // Browser/app user agent
    
    [MaxLength(45)]
    public string? IpAddress { get; set; } // IPv4 or IPv6
    
    [MaxLength(100)]
    public string? Location { get; set; } // City, Country (optional)
    
    // Certificate-based authentication fields
    [MaxLength(100)]
    public string? CertificateThumbprint { get; set; } // Certificate unique identifier
    
    [MaxLength(500)]
    public string? CertificateSubject { get; set; } // Certificate subject DN
    
    public DateTime? CertificateValidFrom { get; set; } // Certificate start date
    
    public DateTime? CertificateValidTo { get; set; } // Certificate expiration date
    
    [Required]
    public DateTime LoginTime { get; set; }
    
    public DateTime? LastActivity { get; set; }
    
    public DateTime? LogoutTime { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(256)]
    public string? SessionToken { get; set; } // For session validation
}