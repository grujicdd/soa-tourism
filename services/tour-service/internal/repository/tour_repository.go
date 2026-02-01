package repository

import (
	"context"
	"time"
	"tour-service/internal/models"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

type TourRepository struct {
	toursCollection     *mongo.Collection
	keypointsCollection *mongo.Collection
	positionCollection  *mongo.Collection
	cartCollection      *mongo.Collection
	tokenCollection     *mongo.Collection
	executionCollection *mongo.Collection
}

func NewTourRepository(db *mongo.Database) *TourRepository {
	return &TourRepository{
		toursCollection:     db.Collection("tours"),
		keypointsCollection: db.Collection("keypoints"),
		positionCollection:  db.Collection("positions"),
		cartCollection:      db.Collection("carts"),
		tokenCollection:     db.Collection("purchase_tokens"),
		executionCollection: db.Collection("executions"),
	}
}

// ============ Tour Operations ============

func (r *TourRepository) CreateTour(ctx context.Context, tour *models.Tour) error {
	tour.CreatedAt = time.Now()
	tour.Status = "draft"
	tour.Price = 0
	tour.IsPublished = false

	result, err := r.toursCollection.InsertOne(ctx, tour)
	if err != nil {
		return err
	}
	tour.ID = result.InsertedID.(primitive.ObjectID)
	return nil
}

func (r *TourRepository) GetTourByID(ctx context.Context, id primitive.ObjectID) (*models.Tour, error) {
	var tour models.Tour
	err := r.toursCollection.FindOne(ctx, bson.M{"_id": id}).Decode(&tour)
	if err != nil {
		return nil, err
	}
	return &tour, nil
}

func (r *TourRepository) GetToursByGuideID(ctx context.Context, guideID string) ([]*models.Tour, error) {
	cursor, err := r.toursCollection.Find(ctx, bson.M{"guideId": guideID})
	if err != nil {
		return nil, err
	}
	defer cursor.Close(ctx)

	var tours []*models.Tour
	if err = cursor.All(ctx, &tours); err != nil {
		return nil, err
	}
	return tours, nil
}

func (r *TourRepository) GetPublishedTours(ctx context.Context) ([]*models.Tour, error) {
	cursor, err := r.toursCollection.Find(ctx, bson.M{"isPublished": true})
	if err != nil {
		return nil, err
	}
	defer cursor.Close(ctx)

	var tours []*models.Tour
	if err = cursor.All(ctx, &tours); err != nil {
		return nil, err
	}
	return tours, nil
}

func (r *TourRepository) UpdateTour(ctx context.Context, tour *models.Tour) error {
	_, err := r.toursCollection.UpdateOne(
		ctx,
		bson.M{"_id": tour.ID},
		bson.M{"$set": tour},
	)
	return err
}

func (r *TourRepository) PublishTour(ctx context.Context, tourID primitive.ObjectID, price float64) error {
	_, err := r.toursCollection.UpdateOne(
		ctx,
		bson.M{"_id": tourID},
		bson.M{"$set": bson.M{
			"isPublished": true,
			"status":      "published",
			"price":       price,
			"publishedAt": time.Now(),
		}},
	)
	return err
}

// ============ KeyPoint Operations ============

func (r *TourRepository) CreateKeyPoint(ctx context.Context, keypoint *models.KeyPoint) error {
	result, err := r.keypointsCollection.InsertOne(ctx, keypoint)
	if err != nil {
		return err
	}
	keypoint.ID = result.InsertedID.(primitive.ObjectID)
	return nil
}

func (r *TourRepository) GetKeyPointsByTourID(ctx context.Context, tourID primitive.ObjectID) ([]*models.KeyPoint, error) {
	cursor, err := r.keypointsCollection.Find(ctx, bson.M{"tourId": tourID})
	if err != nil {
		return nil, err
	}
	defer cursor.Close(ctx)

	var keypoints []*models.KeyPoint
	if err = cursor.All(ctx, &keypoints); err != nil {
		return nil, err
	}
	return keypoints, nil
}

func (r *TourRepository) UpdateKeyPoint(ctx context.Context, keypoint *models.KeyPoint) error {
	_, err := r.keypointsCollection.UpdateOne(
		ctx,
		bson.M{"_id": keypoint.ID},
		bson.M{"$set": keypoint},
	)
	return err
}

func (r *TourRepository) DeleteKeyPoint(ctx context.Context, keypointID primitive.ObjectID) error {
	_, err := r.keypointsCollection.DeleteOne(ctx, bson.M{"_id": keypointID})
	return err
}

// ============ Position Operations ============

func (r *TourRepository) UpsertPosition(ctx context.Context, position *models.Position) error {
	position.UpdatedAt = time.Now()
	
	// Use upsert option to insert if not found
	opts := options.Update().SetUpsert(true)
	_, err := r.positionCollection.UpdateOne(
		ctx,
		bson.M{"touristId": position.TouristID},
		bson.M{"$set": position},
		opts,
	)
	return err
}

func (r *TourRepository) GetPosition(ctx context.Context, touristID string) (*models.Position, error) {
	var position models.Position
	err := r.positionCollection.FindOne(ctx, bson.M{"touristId": touristID}).Decode(&position)
	if err != nil {
		return nil, err
	}
	return &position, nil
}

// ============ Shopping Cart Operations ============

func (r *TourRepository) GetOrCreateCart(ctx context.Context, touristID string) (*models.ShoppingCart, error) {
	var cart models.ShoppingCart
	err := r.cartCollection.FindOne(ctx, bson.M{"touristId": touristID}).Decode(&cart)

	if err == mongo.ErrNoDocuments {
		// Create new cart
		cart = models.ShoppingCart{
			TouristID:  touristID,
			Items:      []models.CartItem{},
			TotalPrice: 0,
		}
		result, err := r.cartCollection.InsertOne(ctx, cart)
		if err != nil {
			return nil, err
		}
		cart.ID = result.InsertedID.(primitive.ObjectID)
		return &cart, nil
	}

	if err != nil {
		return nil, err
	}
	return &cart, nil
}

func (r *TourRepository) UpdateCart(ctx context.Context, cart *models.ShoppingCart) error {
	_, err := r.cartCollection.UpdateOne(
		ctx,
		bson.M{"_id": cart.ID},
		bson.M{"$set": cart},
	)
	return err
}

func (r *TourRepository) ClearCart(ctx context.Context, touristID string) error {
	_, err := r.cartCollection.UpdateOne(
		ctx,
		bson.M{"touristId": touristID},
		bson.M{"$set": bson.M{
			"items":      []models.CartItem{},
			"totalPrice": 0,
		}},
	)
	return err
}

// ============ Purchase Token Operations ============

func (r *TourRepository) CreatePurchaseToken(ctx context.Context, token *models.PurchaseToken) error {
	token.PurchasedAt = time.Now()
	result, err := r.tokenCollection.InsertOne(ctx, token)
	if err != nil {
		return err
	}
	token.ID = result.InsertedID.(primitive.ObjectID)
	return nil
}

func (r *TourRepository) HasPurchased(ctx context.Context, touristID string, tourID primitive.ObjectID) (bool, error) {
	count, err := r.tokenCollection.CountDocuments(ctx, bson.M{
		"touristId": touristID,
		"tourId":    tourID,
	})
	return count > 0, err
}

// ============ Tour Execution Operations ============

func (r *TourRepository) CreateExecution(ctx context.Context, execution *models.TourExecution) error {
	execution.StartedAt = time.Now()
	execution.LastActivity = time.Now()
	execution.Status = "active"
	execution.CompletedKeypoints = []models.CompletedKeypoint{}

	result, err := r.executionCollection.InsertOne(ctx, execution)
	if err != nil {
		return err
	}
	execution.ID = result.InsertedID.(primitive.ObjectID)
	return nil
}

func (r *TourRepository) GetExecution(ctx context.Context, executionID primitive.ObjectID) (*models.TourExecution, error) {
	var execution models.TourExecution
	err := r.executionCollection.FindOne(ctx, bson.M{"_id": executionID}).Decode(&execution)
	if err != nil {
		return nil, err
	}
	return &execution, nil
}

func (r *TourRepository) UpdateExecution(ctx context.Context, execution *models.TourExecution) error {
	execution.LastActivity = time.Now()
	_, err := r.executionCollection.UpdateOne(
		ctx,
		bson.M{"_id": execution.ID},
		bson.M{"$set": execution},
	)
	return err
}

func (r *TourRepository) GetActiveExecution(ctx context.Context, touristID string, tourID primitive.ObjectID) (*models.TourExecution, error) {
	var execution models.TourExecution
	err := r.executionCollection.FindOne(ctx, bson.M{
		"touristId": touristID,
		"tourId":    tourID,
		"status":    "active",
	}).Decode(&execution)
	
	if err != nil {
		return nil, err
	}
	return &execution, nil
}

func (r *TourRepository) GetExecutionsByTouristID(ctx context.Context, touristID string) ([]*models.TourExecution, error) {
	cursor, err := r.executionCollection.Find(ctx, bson.M{
		"touristId": touristID,
		"status":    "active", // Only active executions
	})
	if err != nil {
		return nil, err
	}
	defer cursor.Close(ctx)

	var executions []*models.TourExecution
	if err = cursor.All(ctx, &executions); err != nil {
		return nil, err
	}
	return executions, nil
}