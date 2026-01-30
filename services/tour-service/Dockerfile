# Build stage
FROM golang:1.24-alpine AS build
WORKDIR /app

# Copy go mod files
COPY services/tour-service/go.mod services/tour-service/go.sum ./
RUN go mod download

# Copy proto files
COPY protos/ /app/protos/

# Copy source code
COPY services/tour-service/ .

# Build
RUN CGO_ENABLED=0 GOOS=linux go build -o tour-service ./main.go

# Final stage
FROM alpine:latest
WORKDIR /app
COPY --from=build /app/tour-service .
EXPOSE 5003
ENTRYPOINT ["./tour-service"]