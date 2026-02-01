package handlers

import (
	"context"
	"fmt"
	"log"
	"math"
	"time"
	"tour-service/internal/models"
	"tour-service/internal/repository"
	pb "tour-service/proto"

	"github.com/google/uuid"
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type TourServiceHandler struct {
	pb.UnimplementedTourServiceServer
	repo *repository.TourRepository
}

func NewTourServiceHandler(repo *repository.TourRepository) *TourServiceHandler {
	return &TourServiceHandler{
		repo: repo,
	}
}

// ============ Tour CRUD Operations ============

func (h *TourServiceHandler) CreateTour(ctx context.Context, req *pb.CreateTourRequest) (*pb.TourResponse, error) {
	tour := &models.Tour{
		GuideID:     req.GuideId,
		Name:        req.Name,
		Description: req.Description,
		Difficulty:  req.Difficulty,
		Tags:        req.Tags,
	}

	err := h.repo.CreateTour(ctx, tour)
	if err != nil {
		log.Printf("Error creating tour: %v", err)
		return &pb.TourResponse{
			Success: false,
			Message: "Failed to create tour",
		}, nil
	}

	return &pb.TourResponse{
		Success: true,
		Message: "Tour created successfully",
		Tour:    mapTourToProto(tour),
	}, nil
}

func (h *TourServiceHandler) GetTours(ctx context.Context, req *pb.GetToursRequest) (*pb.ToursResponse, error) {
	var tours []*models.Tour
	var err error

	if req.PublishedOnly {
		tours, err = h.repo.GetPublishedTours(ctx)
	} else if req.UserId != "" {
		tours, err = h.repo.GetToursByGuideID(ctx, req.UserId)
	} else {
		tours, err = h.repo.GetPublishedTours(ctx)
	}

	if err != nil {
		log.Printf("Error getting tours: %v", err)
		return &pb.ToursResponse{
			Success: false,
			Message: "Failed to get tours",
		}, nil
	}

	protoTours := make([]*pb.Tour, len(tours))
	for i, tour := range tours {
		protoTours[i] = mapTourToProto(tour)
	}

	return &pb.ToursResponse{
		Success: true,
		Message: "Tours retrieved successfully",
		Tours:   protoTours,
	}, nil
}

func (h *TourServiceHandler) GetMyTours(ctx context.Context, req *pb.GetMyToursRequest) (*pb.ToursResponse, error) {
	tours, err := h.repo.GetToursByGuideID(ctx, req.GuideId)
	if err != nil {
		log.Printf("Error getting my tours: %v", err)
		return &pb.ToursResponse{
			Success: false,
			Message: "Failed to get tours",
		}, nil
	}

	protoTours := make([]*pb.Tour, len(tours))
	for i, tour := range tours {
		protoTours[i] = mapTourToProto(tour)
	}

	return &pb.ToursResponse{
		Success: true,
		Message: "Tours retrieved successfully",
		Tours:   protoTours,
	}, nil
}

func (h *TourServiceHandler) GetTourById(ctx context.Context, req *pb.GetTourByIdRequest) (*pb.TourResponse, error) {
	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.TourResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	tour, err := h.repo.GetTourByID(ctx, tourID)
	if err != nil {
		log.Printf("Error getting tour: %v", err)
		return &pb.TourResponse{
			Success: false,
			Message: "Tour not found",
		}, nil
	}

	return &pb.TourResponse{
		Success: true,
		Message: "Tour retrieved successfully",
		Tour:    mapTourToProto(tour),
	}, nil
}

func (h *TourServiceHandler) PublishTour(ctx context.Context, req *pb.PublishTourRequest) (*pb.TourResponse, error) {
	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.TourResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	tour, err := h.repo.GetTourByID(ctx, tourID)
	if err != nil {
		return &pb.TourResponse{
			Success: false,
			Message: "Tour not found",
		}, nil
	}

	// Verify ownership
	if tour.GuideID != req.GuideId {
		return &pb.TourResponse{
			Success: false,
			Message: "Unauthorized: You don't own this tour",
		}, nil
	}

	err = h.repo.PublishTour(ctx, tourID, req.Price)
	if err != nil {
		log.Printf("Error publishing tour: %v", err)
		return &pb.TourResponse{
			Success: false,
			Message: "Failed to publish tour",
		}, nil
	}

	// Get updated tour
	tour, _ = h.repo.GetTourByID(ctx, tourID)

	return &pb.TourResponse{
		Success: true,
		Message: "Tour published successfully",
		Tour:    mapTourToProto(tour),
	}, nil
}

// ============ KeyPoint Operations ============

func (h *TourServiceHandler) AddKeyPoint(ctx context.Context, req *pb.AddKeyPointRequest) (*pb.KeyPointResponse, error) {
	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	// Verify tour exists and user owns it
	tour, err := h.repo.GetTourByID(ctx, tourID)
	if err != nil {
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Tour not found",
		}, nil
	}

	if tour.GuideID != req.GuideId {
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Unauthorized: You don't own this tour",
		}, nil
	}

	keypoint := &models.KeyPoint{
		TourID:      tourID,
		Latitude:    req.Latitude,
		Longitude:   req.Longitude,
		Name:        req.Name,
		Description: req.Description,
		Image:       req.Image,
		Order:       req.Order,
	}

	err = h.repo.CreateKeyPoint(ctx, keypoint)
	if err != nil {
		log.Printf("Error creating keypoint: %v", err)
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Failed to create keypoint",
		}, nil
	}

	return &pb.KeyPointResponse{
		Success:  true,
		Message:  "Keypoint added successfully",
		KeyPoint: mapKeyPointToProto(keypoint),
	}, nil
}

func (h *TourServiceHandler) GetKeyPoints(ctx context.Context, req *pb.GetKeyPointsRequest) (*pb.KeyPointsResponse, error) {
	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.KeyPointsResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	keypoints, err := h.repo.GetKeyPointsByTourID(ctx, tourID)
	if err != nil {
		log.Printf("Error getting keypoints: %v", err)
		return &pb.KeyPointsResponse{
			Success: false,
			Message: "Failed to get keypoints",
		}, nil
	}

	// Get the tour to check if user is the guide
tour, _ := h.repo.GetTourByID(ctx, tourID)

	// Check if user has purchased the tour OR is the guide
	isPurchased := false
	isOwner := false
	if req.UserId != "" {
	    isPurchased, _ = h.repo.HasPurchased(ctx, req.UserId, tourID)
	    if tour != nil {
	        isOwner = tour.GuideID == req.UserId
	    }
	}
	
	// If not purchased AND not the owner, only return first keypoint
	protoKeypoints := make([]*pb.KeyPoint, 0)
	if !isPurchased && !isOwner && len(keypoints) > 0 {
	    protoKeypoints = append(protoKeypoints, mapKeyPointToProto(keypoints[0]))
	} else {
	    for _, kp := range keypoints {
	        protoKeypoints = append(protoKeypoints, mapKeyPointToProto(kp))
	    }
	}

	return &pb.KeyPointsResponse{
		Success:     true,
		Message:     "Keypoints retrieved successfully",
		KeyPoints:   protoKeypoints,
		IsPurchased: isPurchased,
	}, nil
}

func (h *TourServiceHandler) UpdateKeyPoint(ctx context.Context, req *pb.UpdateKeyPointRequest) (*pb.KeyPointResponse, error) {
	keypointID, err := primitive.ObjectIDFromHex(req.KeyPointId)
	if err != nil {
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Invalid keypoint ID",
		}, nil
	}

	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	// Verify ownership
	tour, err := h.repo.GetTourByID(ctx, tourID)
	if err != nil || tour.GuideID != req.GuideId {
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Unauthorized",
		}, nil
	}

	keypoint := &models.KeyPoint{
		ID:          keypointID,
		TourID:      tourID,
		Latitude:    req.Latitude,
		Longitude:   req.Longitude,
		Name:        req.Name,
		Description: req.Description,
		Image:       req.Image,
	}

	err = h.repo.UpdateKeyPoint(ctx, keypoint)
	if err != nil {
		log.Printf("Error updating keypoint: %v", err)
		return &pb.KeyPointResponse{
			Success: false,
			Message: "Failed to update keypoint",
		}, nil
	}

	return &pb.KeyPointResponse{
		Success:  true,
		Message:  "Keypoint updated successfully",
		KeyPoint: mapKeyPointToProto(keypoint),
	}, nil
}

func (h *TourServiceHandler) DeleteKeyPoint(ctx context.Context, req *pb.DeleteKeyPointRequest) (*pb.DeleteKeyPointResponse, error) {
	keypointID, err := primitive.ObjectIDFromHex(req.KeyPointId)
	if err != nil {
		return &pb.DeleteKeyPointResponse{
			Success: false,
			Message: "Invalid keypoint ID",
		}, nil
	}

	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.DeleteKeyPointResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	// Verify ownership
	tour, err := h.repo.GetTourByID(ctx, tourID)
	if err != nil || tour.GuideID != req.GuideId {
		return &pb.DeleteKeyPointResponse{
			Success: false,
			Message: "Unauthorized",
		}, nil
	}

	err = h.repo.DeleteKeyPoint(ctx, keypointID)
	if err != nil {
		log.Printf("Error deleting keypoint: %v", err)
		return &pb.DeleteKeyPointResponse{
			Success: false,
			Message: "Failed to delete keypoint",
		}, nil
	}

	return &pb.DeleteKeyPointResponse{
		Success: true,
		Message: "Keypoint deleted successfully",
	}, nil
}

// ============ Position Simulator ============

func (h *TourServiceHandler) UpdatePosition(ctx context.Context, req *pb.UpdatePositionRequest) (*pb.PositionResponse, error) {
	position := &models.Position{
		TouristID: req.TouristId,
		Latitude:  req.Latitude,
		Longitude: req.Longitude,
	}

	err := h.repo.UpsertPosition(ctx, position)
	if err != nil {
		log.Printf("Error updating position: %v", err)
		return &pb.PositionResponse{
			Success: false,
			Message: "Failed to update position",
		}, nil
	}

	return &pb.PositionResponse{
		Success: true,
		Message: "Position updated successfully",
		Position: &pb.Position{
			TouristId: position.TouristID,
			Latitude:  position.Latitude,
			Longitude: position.Longitude,
			UpdatedAt: position.UpdatedAt.Format(time.RFC3339),
		},
	}, nil
}

func (h *TourServiceHandler) GetCurrentPosition(ctx context.Context, req *pb.GetPositionRequest) (*pb.PositionResponse, error) {
	position, err := h.repo.GetPosition(ctx, req.TouristId)
	if err != nil {
		log.Printf("Error getting position: %v", err)
		return &pb.PositionResponse{
			Success: false,
			Message: "Position not found",
		}, nil
	}

	return &pb.PositionResponse{
		Success: true,
		Message: "Position retrieved successfully",
		Position: &pb.Position{
			TouristId: position.TouristID,
			Latitude:  position.Latitude,
			Longitude: position.Longitude,
			UpdatedAt: position.UpdatedAt.Format(time.RFC3339),
		},
	}, nil
}

// ============ Shopping Cart ============

func (h *TourServiceHandler) AddToCart(ctx context.Context, req *pb.AddToCartRequest) (*pb.CartResponse, error) {
	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.CartResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	// Get tour details
	tour, err := h.repo.GetTourByID(ctx, tourID)
	if err != nil {
		return &pb.CartResponse{
			Success: false,
			Message: "Tour not found",
		}, nil
	}

	if !tour.IsPublished {
		return &pb.CartResponse{
			Success: false,
			Message: "Cannot add unpublished tour to cart",
		}, nil
	}

	// Get or create cart
	cart, err := h.repo.GetOrCreateCart(ctx, req.TouristId)
	if err != nil {
		log.Printf("Error getting cart: %v", err)
		return &pb.CartResponse{
			Success: false,
			Message: "Failed to access cart",
		}, nil
	}

	// Check if tour already in cart
	for _, item := range cart.Items {
		if item.TourID == tourID {
			return &pb.CartResponse{
				Success: false,
				Message: "Tour already in cart",
			}, nil
		}
	}

	// Add to cart
	cart.Items = append(cart.Items, models.CartItem{
		TourID:   tourID,
		TourName: tour.Name,
		Price:    tour.Price,
	})
	cart.TotalPrice += tour.Price

	err = h.repo.UpdateCart(ctx, cart)
	if err != nil {
		log.Printf("Error updating cart: %v", err)
		return &pb.CartResponse{
			Success: false,
			Message: "Failed to update cart",
		}, nil
	}

	return &pb.CartResponse{
		Success: true,
		Message: "Tour added to cart",
		Cart:    mapCartToProto(cart),
	}, nil
}

func (h *TourServiceHandler) RemoveFromCart(ctx context.Context, req *pb.RemoveFromCartRequest) (*pb.CartResponse, error) {
	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.CartResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	cart, err := h.repo.GetOrCreateCart(ctx, req.TouristId)
	if err != nil {
		return &pb.CartResponse{
			Success: false,
			Message: "Failed to access cart",
		}, nil
	}

	// Remove item and recalculate total
	newItems := []models.CartItem{}
	newTotal := 0.0
	for _, item := range cart.Items {
		if item.TourID != tourID {
			newItems = append(newItems, item)
			newTotal += item.Price
		}
	}

	cart.Items = newItems
	cart.TotalPrice = newTotal

	err = h.repo.UpdateCart(ctx, cart)
	if err != nil {
		return &pb.CartResponse{
			Success: false,
			Message: "Failed to update cart",
		}, nil
	}

	return &pb.CartResponse{
		Success: true,
		Message: "Tour removed from cart",
		Cart:    mapCartToProto(cart),
	}, nil
}

func (h *TourServiceHandler) GetCart(ctx context.Context, req *pb.GetCartRequest) (*pb.CartResponse, error) {
	cart, err := h.repo.GetOrCreateCart(ctx, req.TouristId)
	if err != nil {
		return &pb.CartResponse{
			Success: false,
			Message: "Failed to get cart",
		}, nil
	}

	return &pb.CartResponse{
		Success: true,
		Message: "Cart retrieved successfully",
		Cart:    mapCartToProto(cart),
	}, nil
}

func (h *TourServiceHandler) Checkout(ctx context.Context, req *pb.CheckoutRequest) (*pb.CheckoutResponse, error) {
	cart, err := h.repo.GetOrCreateCart(ctx, req.TouristId)
	if err != nil {
		return &pb.CheckoutResponse{
			Success: false,
			Message: "Failed to get cart",
		}, nil
	}

	if len(cart.Items) == 0 {
		return &pb.CheckoutResponse{
			Success: false,
			Message: "Cart is empty",
		}, nil
	}

	// Create purchase tokens for each item
	tokens := []*pb.PurchaseToken{}
	for _, item := range cart.Items {
		token := &models.PurchaseToken{
			TouristID: req.TouristId,
			TourID:    item.TourID,
			Token:     uuid.New().String(),
		}

		err := h.repo.CreatePurchaseToken(ctx, token)
		if err != nil {
			log.Printf("Error creating purchase token: %v", err)
			continue
		}

		tokens = append(tokens, &pb.PurchaseToken{
			TourId:      item.TourID.Hex(),
			Token:       token.Token,
			PurchasedAt: token.PurchasedAt.Format(time.RFC3339),
		})
	}

	// Clear cart
	err = h.repo.ClearCart(ctx, req.TouristId)
	if err != nil {
		log.Printf("Error clearing cart: %v", err)
	}

	return &pb.CheckoutResponse{
		Success: true,
		Message: fmt.Sprintf("Successfully purchased %d tours", len(tokens)),
		Tokens:  tokens,
	}, nil
}

// ============ Tour Execution ============

func (h *TourServiceHandler) StartTourExecution(ctx context.Context, req *pb.StartExecutionRequest) (*pb.ExecutionResponse, error) {
	tourID, err := primitive.ObjectIDFromHex(req.TourId)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Invalid tour ID",
		}, nil
	}

	// Check if user has purchased the tour
	hasPurchased, err := h.repo.HasPurchased(ctx, req.TouristId, tourID)
	if err != nil || !hasPurchased {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Tour not purchased. Please buy the tour first.",
		}, nil
	}

	// Check if there's already an active execution for this tour
	existingExecution, err := h.repo.GetActiveExecution(ctx, req.TouristId, tourID)
	if err == nil && existingExecution != nil {
		// Active execution already exists, return it instead of creating new one
		log.Printf("Active execution already exists for tourist %s and tour %s", req.TouristId, tourID.Hex())
		return &pb.ExecutionResponse{
			Success:   true,
			Message:   "Continuing existing tour execution",
			Execution: mapExecutionToProto(existingExecution),
		}, nil
	}

	// Create new execution
	execution := &models.TourExecution{
		TouristID:      req.TouristId,
		TourID:         tourID,
		StartLatitude:  req.StartLatitude,
		StartLongitude: req.StartLongitude,
	}

	err = h.repo.CreateExecution(ctx, execution)
	if err != nil {
		log.Printf("Error creating execution: %v", err)
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Failed to start tour execution",
		}, nil
	}

	return &pb.ExecutionResponse{
		Success:   true,
		Message:   "Tour execution started",
		Execution: mapExecutionToProto(execution),
	}, nil
}

func (h *TourServiceHandler) CheckProximity(ctx context.Context, req *pb.CheckProximityRequest) (*pb.ProximityResponse, error) {
	executionID, err := primitive.ObjectIDFromHex(req.ExecutionId)
	if err != nil {
		return &pb.ProximityResponse{
			Success: false,
			Message: "Invalid execution ID",
		}, nil
	}

	execution, err := h.repo.GetExecution(ctx, executionID)
	if err != nil {
		return &pb.ProximityResponse{
			Success: false,
			Message: "Execution not found",
		}, nil
	}

	if execution.TouristID != req.TouristId {
		return &pb.ProximityResponse{
			Success: false,
			Message: "Unauthorized",
		}, nil
	}

	// Get all keypoints for this tour
	keypoints, err := h.repo.GetKeyPointsByTourID(ctx, execution.TourID)
	if err != nil {
		return &pb.ProximityResponse{
			Success: false,
			Message: "Failed to get keypoints",
		}, nil
	}

	// Check proximity to each uncompleted keypoint
	for _, kp := range keypoints {
		// Check if already completed
		alreadyCompleted := false
		for _, completed := range execution.CompletedKeypoints {
			if completed.KeypointID == kp.ID {
				alreadyCompleted = true
				break
			}
		}

		if alreadyCompleted {
			continue
		}

		// Calculate distance
		distance := calculateDistance(req.CurrentLatitude, req.CurrentLongitude, kp.Latitude, kp.Longitude)
		log.Printf("Distance check - Tourist: (%.6f, %.6f), Keypoint '%s': (%.6f, %.6f), Distance: %.2f meters", 
    			req.CurrentLatitude, req.CurrentLongitude, kp.Name, kp.Latitude, kp.Longitude, distance)


		// If within 50 meters
		if distance <= 50 {
			// Mark as completed
			execution.CompletedKeypoints = append(execution.CompletedKeypoints, models.CompletedKeypoint{
				KeypointID:  kp.ID,
				CompletedAt: time.Now(),
			})
			err = h.repo.UpdateExecution(ctx, execution)
			if err != nil {
				log.Printf("Error updating execution: %v", err)
			}

			return &pb.ProximityResponse{
				Success:        true,
				Message:        "Near keypoint",
				NearKeyPoint:   true,
				NearbyKeyPoint: mapKeyPointToProto(kp),
				Distance:       distance,
			}, nil
		}
	}

	// Update last activity
	err = h.repo.UpdateExecution(ctx, execution)
	if err != nil {
		log.Printf("Error updating execution: %v", err)
	}

	return &pb.ProximityResponse{
		Success:      true,
		Message:      "No nearby keypoints",
		NearKeyPoint: false,
	}, nil
}

func (h *TourServiceHandler) CompleteTour(ctx context.Context, req *pb.CompleteExecutionRequest) (*pb.ExecutionResponse, error) {
	executionID, err := primitive.ObjectIDFromHex(req.ExecutionId)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Invalid execution ID",
		}, nil
	}

	execution, err := h.repo.GetExecution(ctx, executionID)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Execution not found",
		}, nil
	}

	if execution.TouristID != req.TouristId {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Unauthorized",
		}, nil
	}

	execution.Status = "completed"
	execution.CompletedAt = time.Now()

	err = h.repo.UpdateExecution(ctx, execution)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Failed to complete tour",
		}, nil
	}

	return &pb.ExecutionResponse{
		Success:   true,
		Message:   "Tour completed successfully",
		Execution: mapExecutionToProto(execution),
	}, nil
}

func (h *TourServiceHandler) AbandonTour(ctx context.Context, req *pb.AbandonExecutionRequest) (*pb.ExecutionResponse, error) {
	executionID, err := primitive.ObjectIDFromHex(req.ExecutionId)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Invalid execution ID",
		}, nil
	}

	execution, err := h.repo.GetExecution(ctx, executionID)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Execution not found",
		}, nil
	}

	if execution.TouristID != req.TouristId {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Unauthorized",
		}, nil
	}

	execution.Status = "abandoned"
	execution.CompletedAt = time.Now()

	err = h.repo.UpdateExecution(ctx, execution)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Failed to abandon tour",
		}, nil
	}

	return &pb.ExecutionResponse{
		Success:   true,
		Message:   "Tour abandoned",
		Execution: mapExecutionToProto(execution),
	}, nil
}

func (h *TourServiceHandler) GetExecution(ctx context.Context, req *pb.GetExecutionRequest) (*pb.ExecutionResponse, error) {
	executionID, err := primitive.ObjectIDFromHex(req.ExecutionId)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Invalid execution ID",
		}, nil
	}

	execution, err := h.repo.GetExecution(ctx, executionID)
	if err != nil {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Execution not found",
		}, nil
	}

	if execution.TouristID != req.TouristId {
		return &pb.ExecutionResponse{
			Success: false,
			Message: "Unauthorized",
		}, nil
	}

	return &pb.ExecutionResponse{
		Success:   true,
		Message:   "Execution retrieved successfully",
		Execution: mapExecutionToProto(execution),
	}, nil
}

// ============ Helper Functions ============

func mapTourToProto(tour *models.Tour) *pb.Tour {
	return &pb.Tour{
		Id:          tour.ID.Hex(),
		GuideId:     tour.GuideID,
		Name:        tour.Name,
		Description: tour.Description,
		Difficulty:  tour.Difficulty,
		Tags:        tour.Tags,
		Status:      tour.Status,
		Price:       tour.Price,
		IsPublished: tour.IsPublished,
		PublishedAt: tour.PublishedAt.Format(time.RFC3339),
		CreatedAt:   tour.CreatedAt.Format(time.RFC3339),
	}
}

func mapKeyPointToProto(kp *models.KeyPoint) *pb.KeyPoint {
	return &pb.KeyPoint{
		Id:          kp.ID.Hex(),
		TourId:      kp.TourID.Hex(),
		Latitude:    kp.Latitude,
		Longitude:   kp.Longitude,
		Name:        kp.Name,
		Description: kp.Description,
		Image:       kp.Image,
		Order:       kp.Order,
	}
}

func mapCartToProto(cart *models.ShoppingCart) *pb.ShoppingCart {
	items := make([]*pb.CartItem, len(cart.Items))
	for i, item := range cart.Items {
		items[i] = &pb.CartItem{
			TourId:   item.TourID.Hex(),
			TourName: item.TourName,
			Price:    item.Price,
		}
	}

	return &pb.ShoppingCart{
		TouristId:  cart.TouristID,
		Items:      items,
		TotalPrice: cart.TotalPrice,
	}
}

func mapExecutionToProto(exec *models.TourExecution) *pb.TourExecution {
	completedKps := make([]*pb.CompletedKeyPoint, len(exec.CompletedKeypoints))
	for i, kp := range exec.CompletedKeypoints {
		completedKps[i] = &pb.CompletedKeyPoint{
			KeypointId:  kp.KeypointID.Hex(),
			CompletedAt: kp.CompletedAt.Format(time.RFC3339),
		}
	}

	return &pb.TourExecution{
		Id:           exec.ID.Hex(),
		TouristId:    exec.TouristID,
		TourId:       exec.TourID.Hex(),
		Status:       exec.Status,
		StartedAt:    exec.StartedAt.Format(time.RFC3339),
		CompletedAt:  exec.CompletedAt.Format(time.RFC3339),
		LastActivity: exec.LastActivity.Format(time.RFC3339),
		StartPosition: &pb.Position{
			TouristId: exec.TouristID,
			Latitude:  exec.StartLatitude,
			Longitude: exec.StartLongitude,
		},
		CompletedKeypoints: completedKps,
	}
}

// Haversine formula to calculate distance between two coordinates in meters
func calculateDistance(lat1, lon1, lat2, lon2 float64) float64 {
	const earthRadius = 6371000 // meters

	lat1Rad := lat1 * math.Pi / 180
	lat2Rad := lat2 * math.Pi / 180
	deltaLat := (lat2 - lat1) * math.Pi / 180
	deltaLon := (lon2 - lon1) * math.Pi / 180

	a := math.Sin(deltaLat/2)*math.Sin(deltaLat/2) +
		math.Cos(lat1Rad)*math.Cos(lat2Rad)*
			math.Sin(deltaLon/2)*math.Sin(deltaLon/2)

	c := 2 * math.Atan2(math.Sqrt(a), math.Sqrt(1-a))

	return earthRadius * c
}
