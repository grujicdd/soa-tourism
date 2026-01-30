using Gateway.DTOs;
using Gateway.GrpcClients;
using Microsoft.AspNetCore.Mvc;
using TourService.Protos;

namespace Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TourController : ControllerBase
{
    private readonly TourGrpcClient _tourClient;
    private readonly ILogger<TourController> _logger;

    public TourController(TourGrpcClient tourClient, ILogger<TourController> logger)
    {
        _tourClient = tourClient;
        _logger = logger;
    }

    // ============ Tour Operations ============

    [HttpPost]
    public async Task<ActionResult<TourResponseDto>> CreateTour([FromBody] CreateTourRequestDto request)
    {
        try
        {
            var grpcRequest = new CreateTourRequest
            {
                GuideId = request.GuideId,
                Name = request.Name,
                Description = request.Description,
                Difficulty = request.Difficulty,
                Tags = { request.Tags }
            };

            var response = await _tourClient.Client.CreateTourAsync(grpcRequest);

            return Ok(new TourResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Tour = response.Tour != null ? MapTourToDto(response.Tour) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating tour: {ex.Message}");
            return StatusCode(500, new TourResponseDto
            {
                Success = false,
                Message = "An error occurred while creating tour."
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ToursResponseDto>> GetTours([FromQuery] bool publishedOnly = true)
    {
        try
        {
            var grpcRequest = new GetToursRequest
            {
                PublishedOnly = publishedOnly
            };

            var response = await _tourClient.Client.GetToursAsync(grpcRequest);

            return Ok(new ToursResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Tours = response.Tours.Select(MapTourToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting tours: {ex.Message}");
            return StatusCode(500, new ToursResponseDto
            {
                Success = false,
                Message = "An error occurred while getting tours."
            });
        }
    }

    [HttpGet("my/{guideId}")]
    public async Task<ActionResult<ToursResponseDto>> GetMyTours(string guideId)
    {
        try
        {
            var grpcRequest = new GetMyToursRequest
            {
                GuideId = guideId
            };

            var response = await _tourClient.Client.GetMyToursAsync(grpcRequest);

            return Ok(new ToursResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Tours = response.Tours.Select(MapTourToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting my tours: {ex.Message}");
            return StatusCode(500, new ToursResponseDto
            {
                Success = false,
                Message = "An error occurred while getting tours."
            });
        }
    }

    [HttpGet("{tourId}")]
    public async Task<ActionResult<TourResponseDto>> GetTourById(string tourId)
    {
        try
        {
            var grpcRequest = new GetTourByIdRequest
            {
                TourId = tourId
            };

            var response = await _tourClient.Client.GetTourByIdAsync(grpcRequest);

            if (!response.Success)
            {
                return NotFound(new TourResponseDto
                {
                    Success = false,
                    Message = response.Message
                });
            }

            return Ok(new TourResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Tour = response.Tour != null ? MapTourToDto(response.Tour) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting tour: {ex.Message}");
            return StatusCode(500, new TourResponseDto
            {
                Success = false,
                Message = "An error occurred while getting tour."
            });
        }
    }

    [HttpPut("{tourId}/publish")]
    public async Task<ActionResult<TourResponseDto>> PublishTour(string tourId, [FromBody] PublishTourRequestDto request)
    {
        try
        {
            var grpcRequest = new PublishTourRequest
            {
                TourId = tourId,
                GuideId = request.GuideId,
                Price = request.Price
            };

            var response = await _tourClient.Client.PublishTourAsync(grpcRequest);

            return Ok(new TourResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Tour = response.Tour != null ? MapTourToDto(response.Tour) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error publishing tour: {ex.Message}");
            return StatusCode(500, new TourResponseDto
            {
                Success = false,
                Message = "An error occurred while publishing tour."
            });
        }
    }

    // ============ KeyPoint Operations ============

    [HttpPost("{tourId}/keypoints")]
    public async Task<ActionResult<KeyPointResponseDto>> AddKeyPoint(string tourId, [FromBody] AddKeyPointRequestDto request)
    {
        try
        {
            var grpcRequest = new AddKeyPointRequest
            {
                TourId = tourId,
                GuideId = request.GuideId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Name = request.Name,
                Description = request.Description,
                Image = request.Image,
                Order = request.Order
            };

            var response = await _tourClient.Client.AddKeyPointAsync(grpcRequest);

            return Ok(new KeyPointResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                KeyPoint = response.KeyPoint != null ? MapKeyPointToDto(response.KeyPoint) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding keypoint: {ex.Message}");
            return StatusCode(500, new KeyPointResponseDto
            {
                Success = false,
                Message = "An error occurred while adding keypoint."
            });
        }
    }

    [HttpGet("{tourId}/keypoints")]
    public async Task<ActionResult<KeyPointsResponseDto>> GetKeyPoints(string tourId, [FromQuery] string? userId = null)
    {
        try
        {
            var grpcRequest = new GetKeyPointsRequest
            {
                TourId = tourId,
                UserId = userId ?? string.Empty
            };

            var response = await _tourClient.Client.GetKeyPointsAsync(grpcRequest);

            return Ok(new KeyPointsResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                KeyPoints = response.KeyPoints.Select(MapKeyPointToDto).ToList(),
                IsPurchased = response.IsPurchased
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting keypoints: {ex.Message}");
            return StatusCode(500, new KeyPointsResponseDto
            {
                Success = false,
                Message = "An error occurred while getting keypoints."
            });
        }
    }

    // ============ Position Simulator ============

    [HttpPost("position")]
    public async Task<ActionResult<PositionResponseDto>> UpdatePosition([FromBody] UpdatePositionRequestDto request)
    {
        try
        {
            var grpcRequest = new UpdatePositionRequest
            {
                TouristId = request.TouristId,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            var response = await _tourClient.Client.UpdatePositionAsync(grpcRequest);

            return Ok(new PositionResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Position = response.Position != null ? MapPositionToDto(response.Position) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating position: {ex.Message}");
            return StatusCode(500, new PositionResponseDto
            {
                Success = false,
                Message = "An error occurred while updating position."
            });
        }
    }

    [HttpGet("position/{touristId}")]
    public async Task<ActionResult<PositionResponseDto>> GetPosition(string touristId)
    {
        try
        {
            var grpcRequest = new GetPositionRequest
            {
                TouristId = touristId
            };

            var response = await _tourClient.Client.GetCurrentPositionAsync(grpcRequest);

            return Ok(new PositionResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Position = response.Position != null ? MapPositionToDto(response.Position) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting position: {ex.Message}");
            return StatusCode(500, new PositionResponseDto
            {
                Success = false,
                Message = "An error occurred while getting position."
            });
        }
    }

    // ============ Shopping Cart ============

    [HttpPost("cart/{touristId}/items")]
    public async Task<ActionResult<CartResponseDto>> AddToCart(string touristId, [FromBody] string tourId)
    {
        try
        {
            var grpcRequest = new AddToCartRequest
            {
                TouristId = touristId,
                TourId = tourId
            };

            var response = await _tourClient.Client.AddToCartAsync(grpcRequest);

            return Ok(new CartResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Cart = response.Cart != null ? MapCartToDto(response.Cart) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding to cart: {ex.Message}");
            return StatusCode(500, new CartResponseDto
            {
                Success = false,
                Message = "An error occurred while adding to cart."
            });
        }
    }

    [HttpDelete("cart/{touristId}/items/{tourId}")]
    public async Task<ActionResult<CartResponseDto>> RemoveFromCart(string touristId, string tourId)
    {
        try
        {
            var grpcRequest = new RemoveFromCartRequest
            {
                TouristId = touristId,
                TourId = tourId
            };

            var response = await _tourClient.Client.RemoveFromCartAsync(grpcRequest);

            return Ok(new CartResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Cart = response.Cart != null ? MapCartToDto(response.Cart) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error removing from cart: {ex.Message}");
            return StatusCode(500, new CartResponseDto
            {
                Success = false,
                Message = "An error occurred while removing from cart."
            });
        }
    }

    [HttpGet("cart/{touristId}")]
    public async Task<ActionResult<CartResponseDto>> GetCart(string touristId)
    {
        try
        {
            var grpcRequest = new GetCartRequest
            {
                TouristId = touristId
            };

            var response = await _tourClient.Client.GetCartAsync(grpcRequest);

            return Ok(new CartResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Cart = response.Cart != null ? MapCartToDto(response.Cart) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting cart: {ex.Message}");
            return StatusCode(500, new CartResponseDto
            {
                Success = false,
                Message = "An error occurred while getting cart."
            });
        }
    }

    [HttpPost("cart/{touristId}/checkout")]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout(string touristId)
    {
        try
        {
            var grpcRequest = new CheckoutRequest
            {
                TouristId = touristId
            };

            var response = await _tourClient.Client.CheckoutAsync(grpcRequest);

            return Ok(new CheckoutResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Tokens = response.Tokens.Select(t => new PurchaseTokenDto
                {
                    TourId = t.TourId,
                    Token = t.Token,
                    PurchasedAt = t.PurchasedAt
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during checkout: {ex.Message}");
            return StatusCode(500, new CheckoutResponseDto
            {
                Success = false,
                Message = "An error occurred during checkout."
            });
        }
    }

    // ============ Tour Execution ============

    [HttpPost("{tourId}/execute")]
    public async Task<ActionResult<ExecutionResponseDto>> StartExecution(string tourId, [FromBody] StartExecutionRequestDto request)
    {
        try
        {
            var grpcRequest = new StartExecutionRequest
            {
                TouristId = request.TouristId,
                TourId = tourId,
                StartLatitude = request.StartLatitude,
                StartLongitude = request.StartLongitude
            };

            var response = await _tourClient.Client.StartTourExecutionAsync(grpcRequest);

            return Ok(new ExecutionResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Execution = response.Execution != null ? MapExecutionToDto(response.Execution) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting execution: {ex.Message}");
            return StatusCode(500, new ExecutionResponseDto
            {
                Success = false,
                Message = "An error occurred while starting execution."
            });
        }
    }

    [HttpPost("executions/{executionId}/proximity")]
    public async Task<ActionResult<ProximityResponseDto>> CheckProximity(string executionId, [FromBody] UpdatePositionRequestDto request)
    {
        try
        {
            var grpcRequest = new CheckProximityRequest
            {
                ExecutionId = executionId,
                TouristId = request.TouristId,
                CurrentLatitude = request.Latitude,
                CurrentLongitude = request.Longitude
            };

            var response = await _tourClient.Client.CheckProximityAsync(grpcRequest);

            return Ok(new ProximityResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                NearKeyPoint = response.NearKeyPoint,
                NearbyKeyPoint = response.NearbyKeyPoint != null ? MapKeyPointToDto(response.NearbyKeyPoint) : null,
                Distance = response.Distance
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking proximity: {ex.Message}");
            return StatusCode(500, new ProximityResponseDto
            {
                Success = false,
                Message = "An error occurred while checking proximity."
            });
        }
    }

    [HttpPost("executions/{executionId}/complete")]
    public async Task<ActionResult<ExecutionResponseDto>> CompleteTour(string executionId, [FromBody] string touristId)
    {
        try
        {
            var grpcRequest = new CompleteExecutionRequest
            {
                ExecutionId = executionId,
                TouristId = touristId
            };

            var response = await _tourClient.Client.CompleteTourAsync(grpcRequest);

            return Ok(new ExecutionResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Execution = response.Execution != null ? MapExecutionToDto(response.Execution) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error completing tour: {ex.Message}");
            return StatusCode(500, new ExecutionResponseDto
            {
                Success = false,
                Message = "An error occurred while completing tour."
            });
        }
    }

    [HttpPost("executions/{executionId}/abandon")]
    public async Task<ActionResult<ExecutionResponseDto>> AbandonTour(string executionId, [FromBody] string touristId)
    {
        try
        {
            var grpcRequest = new AbandonExecutionRequest
            {
                ExecutionId = executionId,
                TouristId = touristId
            };

            var response = await _tourClient.Client.AbandonTourAsync(grpcRequest);

            return Ok(new ExecutionResponseDto
            {
                Success = response.Success,
                Message = response.Message,
                Execution = response.Execution != null ? MapExecutionToDto(response.Execution) : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error abandoning tour: {ex.Message}");
            return StatusCode(500, new ExecutionResponseDto
            {
                Success = false,
                Message = "An error occurred while abandoning tour."
            });
        }
    }

    // ============ Helper Methods ============

    private static TourDto MapTourToDto(Tour tour)
    {
        return new TourDto
        {
            Id = tour.Id,
            GuideId = tour.GuideId,
            Name = tour.Name,
            Description = tour.Description,
            Difficulty = tour.Difficulty,
            Tags = tour.Tags.ToList(),
            Status = tour.Status,
            Price = tour.Price,
            IsPublished = tour.IsPublished,
            PublishedAt = tour.PublishedAt,
            CreatedAt = tour.CreatedAt
        };
    }

    private static KeyPointDto MapKeyPointToDto(KeyPoint kp)
    {
        return new KeyPointDto
        {
            Id = kp.Id,
            TourId = kp.TourId,
            Latitude = kp.Latitude,
            Longitude = kp.Longitude,
            Name = kp.Name,
            Description = kp.Description,
            Image = kp.Image,
            Order = kp.Order
        };
    }

    private static PositionDto MapPositionToDto(Position pos)
    {
        return new PositionDto
        {
            TouristId = pos.TouristId,
            Latitude = pos.Latitude,
            Longitude = pos.Longitude,
            UpdatedAt = pos.UpdatedAt
        };
    }

    private static ShoppingCartDto MapCartToDto(ShoppingCart cart)
    {
        return new ShoppingCartDto
        {
            TouristId = cart.TouristId,
            Items = cart.Items.Select(i => new CartItemDto
            {
                TourId = i.TourId,
                TourName = i.TourName,
                Price = i.Price
            }).ToList(),
            TotalPrice = cart.TotalPrice
        };
    }

    private static TourExecutionDto MapExecutionToDto(TourExecution exec)
    {
        return new TourExecutionDto
        {
            Id = exec.Id,
            TouristId = exec.TouristId,
            TourId = exec.TourId,
            Status = exec.Status,
            StartedAt = exec.StartedAt,
            CompletedAt = exec.CompletedAt,
            LastActivity = exec.LastActivity,
            StartPosition = exec.StartPosition != null ? MapPositionToDto(exec.StartPosition) : null,
            CompletedKeypoints = exec.CompletedKeypoints.Select(kp => new CompletedKeyPointDto
            {
                KeypointId = kp.KeypointId,
                CompletedAt = kp.CompletedAt
            }).ToList()
        };
    }
}