package handlers

import (
	"context"
	"errors"
	"testing"
	"time"
	"tour-service/internal/models"
	"tour-service/internal/repository"
	pb "tour-service/proto"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
	"go.mongodb.org/mongo-driver/bson/primitive"
)

// ── Helpers ───────────────────────────────────────────────────────────────────

func newTestHandler() (*TourServiceHandler, *repository.MockTourRepository) {
	mockRepo := new(repository.MockTourRepository)
	handler := NewTourServiceHandler(mockRepo)
	return handler, mockRepo
}

func createTestTour(guideID string) *models.Tour {
	return &models.Tour{
		ID:          primitive.NewObjectID(),
		GuideID:     guideID,
		Name:        "Test Tour",
		Description: "A test tour description",
		Difficulty:  "easy",
		Tags:        []string{"test", "nature"},
		Status:      "draft",
		Price:       0,
		IsPublished: false,
		CreatedAt:   time.Now(),
	}
}

func createTestKeyPoint(tourID primitive.ObjectID) *models.KeyPoint {
	return &models.KeyPoint{
		ID:          primitive.NewObjectID(),
		TourID:      tourID,
		Name:        "Test KeyPoint",
		Description: "A test keypoint",
		Latitude:    44.8176,
		Longitude:   20.4569,
		Order:       1,
	}
}

// ── CreateTour ────────────────────────────────────────────────────────────────

func TestCreateTour_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("CreateTour", mock.Anything, mock.AnythingOfType("*models.Tour")).
		Return(nil)

	req := &pb.CreateTourRequest{
		GuideId:     "guide123",
		Name:        "Test Tour",
		Description: "A great tour",
		Difficulty:  "easy",
		Tags:        []string{"nature"},
	}

	result, err := handler.CreateTour(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Equal(t, "Test Tour", result.Tour.Name)
}

func TestCreateTour_RepositoryError_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("CreateTour", mock.Anything, mock.AnythingOfType("*models.Tour")).
		Return(errors.New("database error"))

	req := &pb.CreateTourRequest{
		GuideId:     "guide123",
		Name:        "Test Tour",
		Description: "A great tour",
		Difficulty:  "easy",
	}

	result, err := handler.CreateTour(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Equal(t, "Failed to create tour", result.Message)
}

// ── GetTours ──────────────────────────────────────────────────────────────────

func TestGetTours_PublishedOnly_ReturnsPublishedTours(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tours := []*models.Tour{
		createTestTour("guide1"),
		createTestTour("guide2"),
	}
	tours[0].IsPublished = true
	tours[1].IsPublished = true

	mockRepo.On("GetPublishedTours", mock.Anything).
		Return(tours, nil)

	req := &pb.GetToursRequest{PublishedOnly: true}
	result, err := handler.GetTours(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Len(t, result.Tours, 2)
}

func TestGetTours_ByGuideID_ReturnsToursByGuide(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tours := []*models.Tour{createTestTour("guide123")}

	mockRepo.On("GetToursByGuideID", mock.Anything, "guide123").
		Return(tours, nil)

	req := &pb.GetToursRequest{UserId: "guide123"}
	result, err := handler.GetTours(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Len(t, result.Tours, 1)
}

func TestGetTours_RepositoryError_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetPublishedTours", mock.Anything).
		Return(nil, errors.New("database error"))

	req := &pb.GetToursRequest{PublishedOnly: true}
	result, err := handler.GetTours(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

// ── PublishTour ───────────────────────────────────────────────────────────────

func TestPublishTour_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")

	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)
	mockRepo.On("PublishTour", mock.Anything, tour.ID, 25.0).
		Return(nil)

	req := &pb.PublishTourRequest{
		TourId:  tour.ID.Hex(),
		GuideId: "guide123",
		Price:   25.0,
	}

	result, err := handler.PublishTour(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestPublishTour_TourNotFound_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetTourByID", mock.Anything, mock.Anything).
		Return(nil, errors.New("not found"))

	req := &pb.PublishTourRequest{
		TourId:  primitive.NewObjectID().Hex(),
		GuideId: "guide123",
		Price:   25.0,
	}

	result, err := handler.PublishTour(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

func TestPublishTour_WrongGuide_ReturnsUnauthorized(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")

	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)

	req := &pb.PublishTourRequest{
		TourId:  tour.ID.Hex(),
		GuideId: "differentguide",
		Price:   25.0,
	}

	result, err := handler.PublishTour(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "Unauthorized")
}

// ── AddKeyPoint ───────────────────────────────────────────────────────────────

func TestAddKeyPoint_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")

	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)
	mockRepo.On("CreateKeyPoint", mock.Anything, mock.AnythingOfType("*models.KeyPoint")).
		Return(nil)

	req := &pb.AddKeyPointRequest{
		TourId:      tour.ID.Hex(),
		GuideId:     "guide123",
		Name:        "Test Point",
		Description: "A keypoint",
		Latitude:    44.8176,
		Longitude:   20.4569,
		Order:       1,
	}

	result, err := handler.AddKeyPoint(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestAddKeyPoint_TourNotFound_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetTourByID", mock.Anything, mock.Anything).
		Return(nil, errors.New("not found"))

	req := &pb.AddKeyPointRequest{
		TourId:  primitive.NewObjectID().Hex(),
		GuideId: "guide123",
		Name:    "Test Point",
	}

	result, err := handler.AddKeyPoint(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}

// ── AddToCart ─────────────────────────────────────────────────────────────────

func TestAddToCart_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")
	tour.IsPublished = true
	tour.Price = 25.0

	cart := &models.ShoppingCart{
		TouristID:  "tourist123",
		Items:      []models.CartItem{},
		TotalPrice: 0,
	}

	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)
	mockRepo.On("HasPurchased", mock.Anything, "tourist123", tour.ID).
		Return(false, nil)
	mockRepo.On("GetOrCreateCart", mock.Anything, "tourist123").
		Return(cart, nil)
	mockRepo.On("UpdateCart", mock.Anything, mock.AnythingOfType("*models.ShoppingCart")).
		Return(nil)

	req := &pb.AddToCartRequest{
		TouristId: "tourist123",
		TourId:    tour.ID.Hex(),
	}

	result, err := handler.AddToCart(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestAddToCart_UnpublishedTour_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")
	tour.IsPublished = false // unpublished

	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)

	req := &pb.AddToCartRequest{
		TouristId: "tourist123",
		TourId:    tour.ID.Hex(),
	}

	result, err := handler.AddToCart(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "unpublished")
}

func TestAddToCart_TourNotFound_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetTourByID", mock.Anything, mock.Anything).
		Return(nil, errors.New("not found"))

	req := &pb.AddToCartRequest{
		TouristId: "tourist123",
		TourId:    primitive.NewObjectID().Hex(),
	}

	result, err := handler.AddToCart(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "Tour not found")
}

// ── StartTourExecution ────────────────────────────────────────────────────────

func TestStartTourExecution_ValidRequest_ReturnsSuccess(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")
	tour.IsPublished = true

	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)
	mockRepo.On("HasPurchased", mock.Anything, "tourist123", tour.ID).
		Return(true, nil)
	mockRepo.On("GetActiveExecution", mock.Anything, "tourist123", tour.ID).
		Return(nil, errors.New("not found"))
	mockRepo.On("CreateExecution", mock.Anything, mock.AnythingOfType("*models.TourExecution")).
		Return(nil)

	req := &pb.StartExecutionRequest{
		TouristId:      "tourist123",
		TourId:         tour.ID.Hex(),
		StartLatitude:  44.8176,
		StartLongitude: 20.4569,
	}

	result, err := handler.StartTourExecution(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
}

func TestStartTourExecution_NotPurchased_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tour := createTestTour("guide123")
	tour.IsPublished = true

	mockRepo.On("GetTourByID", mock.Anything, tour.ID).
		Return(tour, nil)
	mockRepo.On("HasPurchased", mock.Anything, "tourist123", tour.ID).
		Return(false, nil)

	req := &pb.StartExecutionRequest{
		TouristId: "tourist123",
		TourId:    tour.ID.Hex(),
	}

	result, err := handler.StartTourExecution(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
	assert.Contains(t, result.Message, "buy")
}

// ── GetKeyPoints ──────────────────────────────────────────────────────────────

func TestGetKeyPoints_AsOwner_ReturnsAllKeyPoints(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tourID := primitive.NewObjectID()
	tour := createTestTour("guide123")
	tour.ID = tourID

	kp1 := &models.KeyPoint{ID: primitive.NewObjectID(), TourID: tourID, Name: "KP1", Latitude: 44.8176, Longitude: 20.4569, Order: 1}
	kp2 := &models.KeyPoint{ID: primitive.NewObjectID(), TourID: tourID, Name: "KP2", Latitude: 44.8200, Longitude: 20.4600, Order: 2}
	keypoints := []*models.KeyPoint{kp1, kp2}

	mockRepo.On("GetKeyPointsByTourID", mock.Anything, tourID).
		Return(keypoints, nil)
	mockRepo.On("GetTourByID", mock.Anything, tourID).
		Return(tour, nil)
	mockRepo.On("HasPurchased", mock.Anything, "guide123", tourID).
		Return(false, nil)

	req := &pb.GetKeyPointsRequest{TourId: tourID.Hex(), UserId: "guide123"}
	result, err := handler.GetKeyPoints(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Len(t, result.KeyPoints, 2)
}

func TestGetKeyPoints_AsPurchasedTourist_ReturnsAllKeyPoints(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tourID := primitive.NewObjectID()
	tour := createTestTour("guide123")
	tour.ID = tourID

	kp1 := &models.KeyPoint{ID: primitive.NewObjectID(), TourID: tourID, Name: "KP1", Latitude: 44.8176, Longitude: 20.4569, Order: 1}
	kp2 := &models.KeyPoint{ID: primitive.NewObjectID(), TourID: tourID, Name: "KP2", Latitude: 44.8200, Longitude: 20.4600, Order: 2}
	keypoints := []*models.KeyPoint{kp1, kp2}

	mockRepo.On("GetKeyPointsByTourID", mock.Anything, tourID).
		Return(keypoints, nil)
	mockRepo.On("GetTourByID", mock.Anything, tourID).
		Return(tour, nil)
	mockRepo.On("HasPurchased", mock.Anything, "tourist123", tourID).
		Return(true, nil)

	req := &pb.GetKeyPointsRequest{TourId: tourID.Hex(), UserId: "tourist123"}
	result, err := handler.GetKeyPoints(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Len(t, result.KeyPoints, 2)
}

func TestGetKeyPoints_UnpurchasedTourist_ReturnsOnlyFirst(t *testing.T) {
	handler, mockRepo := newTestHandler()

	tourID := primitive.NewObjectID()
	tour := createTestTour("guide123")
	tour.ID = tourID

	kp1 := &models.KeyPoint{ID: primitive.NewObjectID(), TourID: tourID, Name: "KP1", Latitude: 44.8176, Longitude: 20.4569, Order: 1}
	kp2 := &models.KeyPoint{ID: primitive.NewObjectID(), TourID: tourID, Name: "KP2", Latitude: 44.8200, Longitude: 20.4600, Order: 2}
	keypoints := []*models.KeyPoint{kp1, kp2}

	mockRepo.On("GetKeyPointsByTourID", mock.Anything, tourID).
		Return(keypoints, nil)
	mockRepo.On("GetTourByID", mock.Anything, tourID).
		Return(tour, nil)
	mockRepo.On("HasPurchased", mock.Anything, "tourist123", tourID).
		Return(false, nil)

	req := &pb.GetKeyPointsRequest{TourId: tourID.Hex(), UserId: "tourist123"}
	result, err := handler.GetKeyPoints(context.Background(), req)

	assert.Nil(t, err)
	assert.True(t, result.Success)
	assert.Len(t, result.KeyPoints, 1)
}

func TestGetKeyPoints_RepositoryError_ReturnsFailure(t *testing.T) {
	handler, mockRepo := newTestHandler()

	mockRepo.On("GetKeyPointsByTourID", mock.Anything, mock.Anything).
		Return(nil, errors.New("database error"))

	req := &pb.GetKeyPointsRequest{TourId: primitive.NewObjectID().Hex()}
	result, err := handler.GetKeyPoints(context.Background(), req)

	assert.Nil(t, err)
	assert.False(t, result.Success)
}
