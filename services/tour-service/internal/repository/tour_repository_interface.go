package repository

import (
	"context"
	"tour-service/internal/models"

	"go.mongodb.org/mongo-driver/bson/primitive"
)

type TourRepositoryInterface interface {
	// Tour operations
	CreateTour(ctx context.Context, tour *models.Tour) error
	GetTourByID(ctx context.Context, id primitive.ObjectID) (*models.Tour, error)
	GetToursByGuideID(ctx context.Context, guideID string) ([]*models.Tour, error)
	GetPublishedTours(ctx context.Context) ([]*models.Tour, error)
	UpdateTour(ctx context.Context, tour *models.Tour) error
	PublishTour(ctx context.Context, tourID primitive.ObjectID, price float64) error

	// KeyPoint operations
	CreateKeyPoint(ctx context.Context, keypoint *models.KeyPoint) error
	GetKeyPointsByTourID(ctx context.Context, tourID primitive.ObjectID) ([]*models.KeyPoint, error)
	UpdateKeyPoint(ctx context.Context, keypoint *models.KeyPoint) error
	DeleteKeyPoint(ctx context.Context, keypointID primitive.ObjectID) error

	// Position operations
	UpsertPosition(ctx context.Context, position *models.Position) error
	GetPosition(ctx context.Context, touristID string) (*models.Position, error)

	// Cart operations
	GetOrCreateCart(ctx context.Context, touristID string) (*models.ShoppingCart, error)
	UpdateCart(ctx context.Context, cart *models.ShoppingCart) error
	ClearCart(ctx context.Context, touristID string) error

	// Purchase token operations
	CreatePurchaseToken(ctx context.Context, token *models.PurchaseToken) error
	HasPurchased(ctx context.Context, touristID string, tourID primitive.ObjectID) (bool, error)

	// Execution operations
	CreateExecution(ctx context.Context, execution *models.TourExecution) error
	GetExecution(ctx context.Context, executionID primitive.ObjectID) (*models.TourExecution, error)
	UpdateExecution(ctx context.Context, execution *models.TourExecution) error
	GetActiveExecution(ctx context.Context, touristID string, tourID primitive.ObjectID) (*models.TourExecution, error)
	GetExecutionsByTouristID(ctx context.Context, touristID string) ([]*models.TourExecution, error)
}
