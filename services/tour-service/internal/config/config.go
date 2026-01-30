package config

import (
	"os"
)

type Config struct {
	MongoURI       string
	DatabaseName   string
	ServerPort     string
	AuthServiceURL string
}

func LoadConfig() *Config {
	return &Config{
		MongoURI:       getEnv("MONGO_URI", "mongodb://localhost:27017"),
		DatabaseName:   getEnv("MONGO_DATABASE", "tour-db"),
		ServerPort:     getEnv("SERVER_PORT", "5003"),
		AuthServiceURL: getEnv("AUTH_SERVICE_URL", "localhost:5001"),
	}
}

func getEnv(key, defaultValue string) string {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}
	return value
}
