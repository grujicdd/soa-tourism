namespace Gateway.DTOs;

// Tour Operations
public class CreateTourRequestDto
{
    public string GuideId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class TourDto
{
    public string Id { get; set; } = string.Empty;
    public string GuideId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool IsPublished { get; set; }
    public string PublishedAt { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class TourResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TourDto? Tour { get; set; }
}

public class ToursResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<TourDto> Tours { get; set; } = new();
}

public class PublishTourRequestDto
{
    public string GuideId { get; set; } = string.Empty;
    public double Price { get; set; }
}

// KeyPoint Operations
public class AddKeyPointRequestDto
{
    public string GuideId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class KeyPointDto
{
    public string Id { get; set; } = string.Empty;
    public string TourId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class KeyPointResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public KeyPointDto? KeyPoint { get; set; }
}

public class KeyPointsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<KeyPointDto> KeyPoints { get; set; } = new();
    public bool IsPurchased { get; set; }
}

// Position
public class UpdatePositionRequestDto
{
    public string TouristId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class PositionDto
{
    public string TouristId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string UpdatedAt { get; set; } = string.Empty;
}

public class PositionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PositionDto? Position { get; set; }
}

// Shopping Cart
public class CartItemDto
{
    public string TourId { get; set; } = string.Empty;
    public string TourName { get; set; } = string.Empty;
    public double Price { get; set; }
}

public class ShoppingCartDto
{
    public string TouristId { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = new();
    public double TotalPrice { get; set; }
}

public class CartResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShoppingCartDto? Cart { get; set; }
}

public class CheckoutResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<PurchaseTokenDto> Tokens { get; set; } = new();
}

public class PurchaseTokenDto
{
    public string TourId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string PurchasedAt { get; set; } = string.Empty;
}

// Tour Execution
public class StartExecutionRequestDto
{
    public string TouristId { get; set; } = string.Empty;
    public string TourId { get; set; } = string.Empty;
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
}

public class TourExecutionDto
{
    public string Id { get; set; } = string.Empty;
    public string TouristId { get; set; } = string.Empty;
    public string TourId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StartedAt { get; set; } = string.Empty;
    public string CompletedAt { get; set; } = string.Empty;
    public string LastActivity { get; set; } = string.Empty;
    public PositionDto? StartPosition { get; set; }
    public List<CompletedKeyPointDto> CompletedKeypoints { get; set; } = new();
}

public class CompletedKeyPointDto
{
    public string KeypointId { get; set; } = string.Empty;
    public string CompletedAt { get; set; } = string.Empty;
}

public class ExecutionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TourExecutionDto? Execution { get; set; }
}

public class ProximityResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool NearKeyPoint { get; set; }
    public KeyPointDto? NearbyKeyPoint { get; set; }
    public double Distance { get; set; }
}
