package handlers

import (
	"math"
	"testing"

	"github.com/stretchr/testify/assert"
)

// ── calculateDistance (Haversine) ────────────────────────────────────────────

func TestCalculateDistance_SamePoint_ReturnsZero(t *testing.T) {
	distance := calculateDistance(44.8176, 20.4569, 44.8176, 20.4569)
	assert.Equal(t, 0.0, distance)
}

func TestCalculateDistance_KnownPoints_ReturnsCorrectDistance(t *testing.T) {
	// Belgrade to Novi Sad — known distance ~70km
	distance := calculateDistance(44.8176, 20.4569, 45.2671, 19.8335)
	assert.InDelta(t, 70000, distance, 5000) // within 5km tolerance
}

func TestCalculateDistance_ClosePoints_ReturnsSmallDistance(t *testing.T) {
	// Two points ~30 meters apart
	distance := calculateDistance(44.8176, 20.4569, 44.8178, 20.4569)
	assert.Less(t, distance, 50.0)
}

func TestCalculateDistance_FarPoints_ReturnsLargeDistance(t *testing.T) {
	// Belgrade to London — ~2000km
	distance := calculateDistance(44.8176, 20.4569, 51.5074, -0.1278)
	assert.Greater(t, distance, 1000000.0) // more than 1000km in meters
}

func TestCalculateDistance_IsSymmetric(t *testing.T) {
	// Distance A→B should equal distance B→A
	d1 := calculateDistance(44.8176, 20.4569, 45.2671, 19.8335)
	d2 := calculateDistance(45.2671, 19.8335, 44.8176, 20.4569)
	assert.InDelta(t, d1, d2, 0.001)
}

func TestCalculateDistance_NorthPoleToEquator(t *testing.T) {
	// ~10,000km — quarter of Earth's circumference
	distance := calculateDistance(90.0, 0.0, 0.0, 0.0)
	assert.InDelta(t, 10007543, distance, 100000)
}

func TestCalculateDistance_WithinProximityThreshold(t *testing.T) {
	// Points within 50m should be considered "near"
	distance := calculateDistance(44.8176, 20.4569, 44.8177, 20.4570)
	assert.Less(t, distance, 50.0)
}

func TestCalculateDistance_OutsideProximityThreshold(t *testing.T) {
	// Points more than 50m apart should NOT be considered "near"
	distance := calculateDistance(44.8176, 20.4569, 44.8200, 20.4600)
	assert.Greater(t, distance, 50.0)
}

func TestCalculateDistance_UsesEarthRadiusInMeters(t *testing.T) {
	// Full circle around Earth should be ~40,075km
	distance := calculateDistance(0, 0, 0, 180)
	halfCircumference := math.Pi * 6371000
	assert.InDelta(t, halfCircumference, distance, 100000)
}
