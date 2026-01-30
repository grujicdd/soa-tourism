package main

import (
	"context"
	"fmt"
	"log"
	"net"
	"time"

	"tour-service/internal/config"
	"tour-service/internal/handlers"
	"tour-service/internal/repository"
	pb "tour-service/proto"

	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
	"google.golang.org/grpc"
)

func main() {
	// Load configuration
	cfg := config.LoadConfig()

	// Connect to MongoDB
	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	client, err := mongo.Connect(ctx, options.Client().ApplyURI(cfg.MongoURI))
	if err != nil {
		log.Fatalf("Failed to connect to MongoDB: %v", err)
	}
	defer client.Disconnect(context.Background())

	// Ping MongoDB to verify connection
	err = client.Ping(ctx, nil)
	if err != nil {
		log.Fatalf("Failed to ping MongoDB: %v", err)
	}
	log.Println("Successfully connected to MongoDB")

	// Get database and create repository
	db := client.Database(cfg.DatabaseName)
	repo := repository.NewTourRepository(db)

	// Create gRPC server
	grpcServer := grpc.NewServer()

	// Register Tour Service
	tourHandler := handlers.NewTourServiceHandler(repo)
	pb.RegisterTourServiceServer(grpcServer, tourHandler)

	// Start listening
	address := fmt.Sprintf("0.0.0.0:%s", cfg.ServerPort)
	listener, err := net.Listen("tcp", address)
	if err != nil {
		log.Fatalf("Failed to listen on %s: %v", address, err)
	}

	log.Printf("Tour Service is running on %s", address)
	if err := grpcServer.Serve(listener); err != nil {
		log.Fatalf("Failed to serve: %v", err)
	}
}
