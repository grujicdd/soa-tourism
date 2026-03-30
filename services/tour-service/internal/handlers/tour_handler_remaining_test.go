package handlers

import (
	"context"
	"errors"
	"testing"
	"time"
	"tour-service/internal/models"
	pb "tour-service/proto"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"go.mongodb.org/mongo-driver/bson/primitive"
)

// ── GetTourById ───────────────────────────────────────────────────────────────

func TestGetTourById_ValidId_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")
	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)

	req := &pb.GetTourByIdRequest{TourId: tour.ID.Hex()}
	result, err := handler.GetTourById(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Equal(t, "Test Tour", result.Tour.Name)
}

func TestGetTourById_NotFound_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetTourByID", mock.Anything, mock.Anything).
		Return(nil, errors.New("not found"))

	req := &pb.GetTourByIdRequest{TourId: primitive.NewObjectID().Hex()}
	result, err := handler.GetTourById(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "not found")
}

func TestGetTourById_InvalidId_ReturnsFailure(t *testing.T) {
	handler, _ := newTestHandler()

	req := &pb.GetTourByIdRequest{TourId: "not-a-valid-id"}
	result, err := handler.GetTourById(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "Invalid tour ID")
}

// ── GetMyTours ────────────────────────────────────────────────────────────────

func TestGetMyTours_ValidGuide_ReturnsTours(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tours := []*models.Tour{
		createTestTour("guide123"),
		createTestTour("guide123"),
	}

	mockRepo.On("GetToursByGuideID", mock.Anything, "guide123").
		Return(tours, nil)

	req := &pb.GetMyToursRequest{GuideId: "guide123"}
	result, err := handler.GetMyTours(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Len(t, result.Tours, 2)
}

func TestGetMyTours_RepositoryError_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetToursByGuideID", mock.Anything, "guide123").
		Return(nil, errors.New("database error"))

	req := &pb.GetMyToursRequest{GuideId: "guide123"}
	result, err := handler.GetMyTours(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

// ── UpdatePosition ────────────────────────────────────────────────────────────

func TestUpdatePosition_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("UpsertPosition", mock.Anything, mock.AnythingOfType("*models.Position")).
		Return(nil)

	req := &pb.UpdatePositionRequest{
		TouristId: "tourist123",
		Latitude:  44.8176,
		Longitude: 20.4569,
	}

	result, err := handler.UpdatePosition(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Equal(t, 44.8176, result.Position.Latitude)
	assert.Equal(t, 20.4569, result.Position.Longitude)
}

func TestUpdatePosition_RepositoryError_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("UpsertPosition", mock.Anything, mock.AnythingOfType("*models.Position")).
		Return(errors.New("database error"))

	req := &pb.UpdatePositionRequest{
		TouristId: "tourist123",
		Latitude:  44.8176,
		Longitude: 20.4569,
	}

	result, err := handler.UpdatePosition(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

// ── GetCurrentPosition ────────────────────────────────────────────────────────

func TestGetCurrentPosition_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	position := &models.Position{
		TouristID: "tourist123",
		Latitude:  44.8176,
		Longitude: 20.4569,
		UpdatedAt: time.Now(),
	}

	mockRepo.On("GetPosition", mock.Anything, "tourist123").
		Return(position, nil)

	req := &pb.GetPositionRequest{TouristId: "tourist123"}
	result, err := handler.GetCurrentPosition(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Equal(t, 44.8176, result.Position.Latitude)
}

func TestGetCurrentPosition_NotFound_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetPosition", mock.Anything, "tourist123").
		Return(nil, errors.New("not found"))

	req := &pb.GetPositionRequest{TouristId: "tourist123"}
	result, err := handler.GetCurrentPosition(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "not found")
}

// ── Checkout ──────────────────────────────────────────────────────────────────

func TestCheckout_ValidCart_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tourID := primitive.NewObjectID()
	cart := &models.ShoppingCart{
		TouristID: "tourist123",
		Items: []models.CartItem{
			{TourID: tourID, TourName: "Test Tour", Price: 25.0},
		},
		TotalPrice: 25.0,
	}

	mockRepo.On("GetOrCreateCart", mock.Anything, "tourist123").
		Return(cart, nil)
	mockRepo.On("CreatePurchaseToken", mock.Anything, mock.AnythingOfType("*models.PurchaseToken")).
		Return(nil)
	mockRepo.On("ClearCart", mock.Anything, "tourist123").
		Return(nil)

	req := &pb.CheckoutRequest{TouristId: "tourist123"}
	result, err := handler.Checkout(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Len(t, result.Tokens, 1)
}

func TestCheckout_EmptyCart_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	cart := &models.ShoppingCart{
		TouristID:  "tourist123",
		Items:      []models.CartItem{},
		TotalPrice: 0,
	}

	mockRepo.On("GetOrCreateCart", mock.Anything, "tourist123").
		Return(cart, nil)

	req := &pb.CheckoutRequest{TouristId: "tourist123"}
	result, err := handler.Checkout(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "empty")
}

func TestCheckout_RepositoryError_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetOrCreateCart", mock.Anything, "tourist123").
		Return(nil, errors.New("database error"))

	req := &pb.CheckoutRequest{TouristId: "tourist123"}
	result, err := handler.Checkout(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

// ── CompleteTour ──────────────────────────────────────────────────────────────

func TestCompleteTour_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	executionID := primitive.NewObjectID()
	execution := &models.TourExecution{
		ID:        executionID,
		TouristID: "tourist123",
		TourID:    primitive.NewObjectID(),
		Status:    "active",
	}

	mockRepo.On("GetExecution", mock.Anything, executionID).
		Return(execution, nil)
	mockRepo.On("UpdateExecution", mock.Anything, mock.AnythingOfType("*models.TourExecution")).
		Return(nil)

	req := &pb.CompleteExecutionRequest{
		ExecutionId: executionID.Hex(),
		TouristId:   "tourist123",
	}

	result, err := handler.CompleteTour(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestCompleteTour_ExecutionNotFound_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetExecution", mock.Anything, mock.Anything).
		Return(nil, errors.New("not found"))

	req := &pb.CompleteExecutionRequest{
		ExecutionId: primitive.NewObjectID().Hex(),
		TouristId:   "tourist123",
	}

	result, err := handler.CompleteTour(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

func TestCompleteTour_WrongTourist_ReturnsUnauthorized(t *testing.T) {
	handler, mockRepo := newTestHandler()

	executionID := primitive.NewObjectID()
	execution := &models.TourExecution{
		ID:        executionID,
		TouristID: "tourist123",
		Status:    "active",
	}

	mockRepo.On("GetExecution", mock.Anything, executionID).
		Return(execution, nil)

	req := &pb.CompleteExecutionRequest{
		ExecutionId: executionID.Hex(),
		TouristId:   "differenttourist",
	}

	result, err := handler.CompleteTour(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "Unauthorized")
}

// ── AbandonTour ───────────────────────────────────────────────────────────────

func TestAbandonTour_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	executionID := primitive.NewObjectID()
	execution := &models.TourExecution{
		ID:        executionID,
		TouristID: "tourist123",
		TourID:    primitive.NewObjectID(),
		Status:    "active",
	}

	mockRepo.On("GetExecution", mock.Anything, executionID).
		Return(execution, nil)
	mockRepo.On("UpdateExecution", mock.Anything, mock.AnythingOfType("*models.TourExecution")).
		Return(nil)

	req := &pb.AbandonExecutionRequest{
		ExecutionId: executionID.Hex(),
		TouristId:   "tourist123",
	}

	result, err := handler.AbandonTour(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestAbandonTour_WrongTourist_ReturnsUnauthorized(t *testing.T) {
	handler, mockRepo := newTestHandler()

	executionID := primitive.NewObjectID()
	execution := &models.TourExecution{
		ID:        executionID,
		TouristID: "tourist123",
		Status:    "active",
	}

	mockRepo.On("GetExecution", mock.Anything, executionID).
		Return(execution, nil)

	req := &pb.AbandonExecutionRequest{
		ExecutionId: executionID.Hex(),
		TouristId:   "differenttourist",
	}

	result, err := handler.AbandonTour(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "Unauthorized")
}

// ── CheckProximity ────────────────────────────────────────────────────────────

func TestCheckProximity_NearKeypoint_ReturnsNear(t *testing.T) {
	handler, mockRepo := newTestHandler()

	executionID := primitive.NewObjectID()
	tourID := primitive.NewObjectID()
	keypointID := primitive.NewObjectID()

	execution := &models.TourExecution{
		ID:                 executionID,
		TouristID:          "tourist123",
		TourID:             tourID,
		Status:             "active",
		CompletedKeypoints: []models.CompletedKeypoint{},
	}

	keypoints := []*models.KeyPoint{
		{
			ID:        keypointID,
			TourID:    tourID,
			Latitude:  44.8176, // same as tourist position
			Longitude: 20.4569,
			Order:     1,
		},
	}

	mockRepo.On("GetExecution", mock.Anything, executionID).
		Return(execution, nil)
	mockRepo.On("GetKeyPointsByTourID", mock.Anything, tourID).
		Return(keypoints, nil)
	mockRepo.On("UpdateExecution", mock.Anything, mock.AnythingOfType("*models.TourExecution")).
		Return(nil)

	req := &pb.CheckProximityRequest{
		ExecutionId:      executionID.Hex(),
		TouristId:        "tourist123",
		CurrentLatitude:  44.8176, // exactly at keypoint
		CurrentLongitude: 20.4569,
	}

	result, err := handler.CheckProximity(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.True(t, result.NearKeyPoint)
}

func TestCheckProximity_FarFromKeypoint_ReturnsNotNear(t *testing.T) {
	handler, mockRepo := newTestHandler()

	executionID := primitive.NewObjectID()
	tourID := primitive.NewObjectID()
	keypointID := primitive.NewObjectID()

	execution := &models.TourExecution{
		ID:                 executionID,
		TouristID:          "tourist123",
		TourID:             tourID,
		Status:             "active",
		CompletedKeypoints: []models.CompletedKeypoint{},
	}

	keypoints := []*models.KeyPoint{
		{
			ID:        keypointID,
			TourID:    tourID,
			Latitude:  44.8176,
			Longitude: 20.4569,
			Order:     1,
		},
	}

	mockRepo.On("GetExecution", mock.Anything, executionID).
		Return(execution, nil)
	mockRepo.On("GetKeyPointsByTourID", mock.Anything, tourID).
		Return(keypoints, nil)
	mockRepo.On("UpdateExecution", mock.Anything, mock.AnythingOfType("*models.TourExecution")).
		Return(nil)

	req := &pb.CheckProximityRequest{
		ExecutionId:      executionID.Hex(),
		TouristId:        "tourist123",
		CurrentLatitude:  45.2671,
		CurrentLongitude: 19.8335,
	}

	result, err := handler.CheckProximity(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.False(t, result.NearKeyPoint)
}

func TestCheckProximity_ExecutionNotFound_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetExecution", mock.Anything, mock.Anything).
		Return(nil, errors.New("not found"))

	req := &pb.CheckProximityRequest{
		ExecutionId:      primitive.NewObjectID().Hex(),
		TouristId:        "tourist123",
		CurrentLatitude:  44.8176,
		CurrentLongitude: 20.4569,
	}

	result, err := handler.CheckProximity(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

func TestCheckProximity_WrongTourist_ReturnsUnauthorized(t *testing.T) {
	handler, mockRepo := newTestHandler()

	executionID := primitive.NewObjectID()
	execution := &models.TourExecution{
		ID:        executionID,
		TouristID: "tourist123",
		Status:    "active",
	}

	mockRepo.On("GetExecution", mock.Anything, executionID).
		Return(execution, nil)

	req := &pb.CheckProximityRequest{
		ExecutionId:      executionID.Hex(),
		TouristId:        "differenttourist",
		CurrentLatitude:  44.8176,
		CurrentLongitude: 20.4569,
	}

	result, err := handler.CheckProximity(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "Unauthorized")
}

// ── RemoveFromCart ────────────────────────────────────────────────────────────

func TestRemoveFromCart_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tourID := primitive.NewObjectID()
	cart := &models.ShoppingCart{
		TouristID: "tourist123",
		Items: []models.CartItem{
			{TourID: tourID, TourName: "Test Tour", Price: 25.0},
		},
		TotalPrice: 25.0,
	}

	mockRepo.On("GetOrCreateCart", mock.Anything, "tourist123").
		Return(cart, nil)
	mockRepo.On("UpdateCart", mock.Anything, mock.AnythingOfType("*models.ShoppingCart")).
		Return(nil)

	req := &pb.RemoveFromCartRequest{
		TouristId: "tourist123",
		TourId:    tourID.Hex(),
	}

	result, err := handler.RemoveFromCart(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestRemoveFromCart_InvalidTourId_ReturnsFailure(t *testing.T) {
	handler, _ := newTestHandler()

	req := &pb.RemoveFromCartRequest{
		TouristId: "tourist123",
		TourId:    "invalid-id",
	}

	result, err := handler.RemoveFromCart(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

// ── GetCart ───────────────────────────────────────────────────────────────────

func TestGetCart_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	cart := &models.ShoppingCart{
		TouristID:  "tourist123",
		Items:      []models.CartItem{},
		TotalPrice: 0,
	}

	mockRepo.On("GetOrCreateCart", mock.Anything, "tourist123").
		Return(cart, nil)

	req := &pb.GetCartRequest{TouristId: "tourist123"}
	result, err := handler.GetCart(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestGetCart_RepositoryError_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetOrCreateCart", mock.Anything, "tourist123").
		Return(nil, errors.New("database error"))

	req := &pb.GetCartRequest{TouristId: "tourist123"}
	result, err := handler.GetCart(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

// ── DeleteKeyPoint ────────────────────────────────────────────────────────────

func TestDeleteKeyPoint_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tourID := primitive.NewObjectID()
	keypointID := primitive.NewObjectID()
	tour := createTestTour("guide123")
	tour.ID = tourID

	mockRepo.On("GetTourByID", mock.Anything, tourID).
		Return(tour, nil)
	mockRepo.On("DeleteKeyPoint", mock.Anything, keypointID).
		Return(nil)

	req := &pb.DeleteKeyPointRequest{
		TourId:     tourID.Hex(),
		KeyPointId: keypointID.Hex(),
		GuideId:    "guide123",
	}

	result, err := handler.DeleteKeyPoint(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestDeleteKeyPoint_WrongGuide_ReturnsUnauthorized(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tourID := primitive.NewObjectID()
	tour := createTestTour("guide123")
	tour.ID = tourID

	mockRepo.On("GetTourByID", mock.Anything, tourID).
		Return(tour, nil)

	req := &pb.DeleteKeyPointRequest{
		TourId:     tourID.Hex(),
		KeyPointId: primitive.NewObjectID().Hex(),
		GuideId:    "differentguide",
	}

	result, err := handler.DeleteKeyPoint(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "Unauthorized")
}
