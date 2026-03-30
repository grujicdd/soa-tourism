package repository

import (
	"context"
	"tour-service/internal/models"

	"github.com/stretchr/testify/mock"
	"go.mongodb.org/mongo-driver/bson/primitive"
)

type MockTourRepository struct {
	mock.Mock
}

// ── Tour operations ──────────────────────────────────────────────────────────

func (m *MockTourRepository) CreateTour(ctx context.Context, tour *models.Tour) error {
	args := m.Called(ctx, tour)
	return args.Error(0)
}

func (m *MockTourRepository) GetTourByID(ctx context.Context, id primitive.ObjectID) (*models.Tour, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Tour), args.Error(1)
}

func (m *MockTourRepository) GetToursByGuideID(ctx context.Context, guideID string) ([]*models.Tour, error) {
	args := m.Called(ctx, guideID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*models.Tour), args.Error(1)
}

func (m *MockTourRepository) GetPublishedTours(ctx context.Context) ([]*models.Tour, error) {
	args := m.Called(ctx)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*models.Tour), args.Error(1)
}

func (m *MockTourRepository) UpdateTour(ctx context.Context, tour *models.Tour) error {
	args := m.Called(ctx, tour)
	return args.Error(0)
}

func (m *MockTourRepository) PublishTour(ctx context.Context, tourID primitive.ObjectID, price float64) error {
	args := m.Called(ctx, tourID, price)
	return args.Error(0)
}

// ── KeyPoint operations ──────────────────────────────────────────────────────

func (m *MockTourRepository) CreateKeyPoint(ctx context.Context, keypoint *models.KeyPoint) error {
	args := m.Called(ctx, keypoint)
	return args.Error(0)
}

func (m *MockTourRepository) GetKeyPointsByTourID(ctx context.Context, tourID primitive.ObjectID) ([]*models.KeyPoint, error) {
	args := m.Called(ctx, tourID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*models.KeyPoint), args.Error(1)
}

func (m *MockTourRepository) UpdateKeyPoint(ctx context.Context, keypoint *models.KeyPoint) error {
	args := m.Called(ctx, keypoint)
	return args.Error(0)
}

func (m *MockTourRepository) DeleteKeyPoint(ctx context.Context, keypointID primitive.ObjectID) error {
	args := m.Called(ctx, keypointID)
	return args.Error(0)
}

// ── Position operations ──────────────────────────────────────────────────────

func (m *MockTourRepository) UpsertPosition(ctx context.Context, position *models.Position) error {
	args := m.Called(ctx, position)
	return args.Error(0)
}

func (m *MockTourRepository) GetPosition(ctx context.Context, touristID string) (*models.Position, error) {
	args := m.Called(ctx, touristID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Position), args.Error(1)
}

// ── Cart operations ──────────────────────────────────────────────────────────

func (m *MockTourRepository) GetOrCreateCart(ctx context.Context, touristID string) (*models.ShoppingCart, error) {
	args := m.Called(ctx, touristID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.ShoppingCart), args.Error(1)
}

func (m *MockTourRepository) UpdateCart(ctx context.Context, cart *models.ShoppingCart) error {
	args := m.Called(ctx, cart)
	return args.Error(0)
}

func (m *MockTourRepository) ClearCart(ctx context.Context, touristID string) error {
	args := m.Called(ctx, touristID)
	return args.Error(0)
}

// ── Purchase token operations ────────────────────────────────────────────────

func (m *MockTourRepository) CreatePurchaseToken(ctx context.Context, token *models.PurchaseToken) error {
	args := m.Called(ctx, token)
	return args.Error(0)
}

func (m *MockTourRepository) HasPurchased(ctx context.Context, touristID string, tourID primitive.ObjectID) (bool, error) {
	args := m.Called(ctx, touristID, tourID)
	return args.Bool(0), args.Error(1)
}

// ── Execution operations ─────────────────────────────────────────────────────

func (m *MockTourRepository) CreateExecution(ctx context.Context, execution *models.TourExecution) error {
	args := m.Called(ctx, execution)
	return args.Error(0)
}

func (m *MockTourRepository) GetExecution(ctx context.Context, executionID primitive.ObjectID) (*models.TourExecution, error) {
	args := m.Called(ctx, executionID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.TourExecution), args.Error(1)
}

func (m *MockTourRepository) UpdateExecution(ctx context.Context, execution *models.TourExecution) error {
	args := m.Called(ctx, execution)
	return args.Error(0)
}

func (m *MockTourRepository) GetActiveExecution(ctx context.Context, touristID string, tourID primitive.ObjectID) (*models.TourExecution, error) {
	args := m.Called(ctx, touristID, tourID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.TourExecution), args.Error(1)
}

func (m *MockTourRepository) GetExecutionsByTouristID(ctx context.Context, touristID string) ([]*models.TourExecution, error) {
	args := m.Called(ctx, touristID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*models.TourExecution), args.Error(1)
}
