package models

import (
	"time"

	"go.mongodb.org/mongo-driver/bson/primitive"
)

type Tour struct {
	ID          primitive.ObjectID `bson:"_id,omitempty"`
	GuideID     string             `bson:"guideId"`
	Name        string             `bson:"name"`
	Description string             `bson:"description"`
	Difficulty  string             `bson:"difficulty"`
	Tags        []string           `bson:"tags"`
	Status      string             `bson:"status"` // "draft", "published"
	Price       float64            `bson:"price"`
	IsPublished bool               `bson:"isPublished"`
	PublishedAt time.Time          `bson:"publishedAt,omitempty"`
	CreatedAt   time.Time          `bson:"createdAt"`
}

type KeyPoint struct {
	ID          primitive.ObjectID `bson:"_id,omitempty"`
	TourID      primitive.ObjectID `bson:"tourId"`
	Latitude    float64            `bson:"latitude"`
	Longitude   float64            `bson:"longitude"`
	Name        string             `bson:"name"`
	Description string             `bson:"description"`
	Image       string             `bson:"image"`
	Order       int32              `bson:"order"`
}

type Position struct {
	ID        primitive.ObjectID `bson:"_id,omitempty"`
	TouristID string             `bson:"touristId"`
	Latitude  float64            `bson:"latitude"`
	Longitude float64            `bson:"longitude"`
	UpdatedAt time.Time          `bson:"updatedAt"`
}

type ShoppingCart struct {
	ID         primitive.ObjectID `bson:"_id,omitempty"`
	TouristID  string             `bson:"touristId"`
	Items      []CartItem         `bson:"items"`
	TotalPrice float64            `bson:"totalPrice"`
}

type CartItem struct {
	TourID   primitive.ObjectID `bson:"tourId"`
	TourName string             `bson:"tourName"`
	Price    float64            `bson:"price"`
}

type PurchaseToken struct {
	ID          primitive.ObjectID `bson:"_id,omitempty"`
	TouristID   string             `bson:"touristId"`
	TourID      primitive.ObjectID `bson:"tourId"`
	Token       string             `bson:"token"`
	PurchasedAt time.Time          `bson:"purchasedAt"`
}

type TourExecution struct {
	ID                 primitive.ObjectID  `bson:"_id,omitempty"`
	TouristID          string              `bson:"touristId"`
	TourID             primitive.ObjectID  `bson:"tourId"`
	Status             string              `bson:"status"` // "active", "completed", "abandoned"
	StartedAt          time.Time           `bson:"startedAt"`
	CompletedAt        time.Time           `bson:"completedAt,omitempty"`
	LastActivity       time.Time           `bson:"lastActivity"`
	StartLatitude      float64             `bson:"startLatitude"`
	StartLongitude     float64             `bson:"startLongitude"`
	CompletedKeypoints []CompletedKeypoint `bson:"completedKeypoints"`
}

type CompletedKeypoint struct {
	KeypointID  primitive.ObjectID `bson:"keypointId"`
	CompletedAt time.Time          `bson:"completedAt"`
}
