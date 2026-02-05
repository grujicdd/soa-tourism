import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import 'leaflet/dist/leaflet.css';
import './PositionSimulator.css';

// Fix for default marker icon
import L from 'leaflet';
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

let DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41]
});

L.Marker.prototype.options.icon = DefaultIcon;

// Component to handle map clicks
function LocationMarker({ position, setPosition }) {
  useMapEvents({
    click(e) {
      setPosition({
        lat: e.latlng.lat,
        lng: e.latlng.lng
      });
    },
  });

  return position === null ? null : (
    <Marker position={[position.lat, position.lng]} />
  );
}

function PositionSimulator() {
  const [position, setPosition] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const { user } = useAuth();

  // Default center (Belgrade, Serbia)
  const defaultCenter = { lat: 44.8176, lng: 20.4569 };

  const fetchCurrentPosition = useCallback(async () => {
    try {
      const response = await tourAPI.getPosition(user.userId);
      if (response.data.success && response.data.position) {
        setPosition({
          lat: response.data.position.latitude,
          lng: response.data.position.longitude
        });
      } else {
        // No saved position, use default
        setPosition(defaultCenter);
      }
    } catch (err) {
      console.error('Failed to load position:', err);
      setPosition(defaultCenter);
    } finally {
      setLoading(false);
    }
  }, [user.userId]);

  useEffect(() => {
    fetchCurrentPosition();
  }, [fetchCurrentPosition]);

  const handleSavePosition = async () => {
  if (!position) {
    alert('Please select a position on the map first');
    return;
  }

  setSaving(true);

  try {
    console.log('Sending position:', {
      touristId: user.userId,
      latitude: position.lat,
      longitude: position.lng
    });
    
    const response = await tourAPI.updatePosition({
      touristId: user.userId,
      latitude: position.lat,
      longitude: position.lng
    });

    console.log('Response:', response.data);

    if (response.data.success) {
      alert('Position saved successfully!');
    } else {
      alert(response.data.message);
    }
  } catch (err) {
    console.error('Error saving position:', err);
    alert('Failed to save position');
  } finally {
    setSaving(false);
  }
};

  const handleManualInput = (lat, lng) => {
    const latitude = parseFloat(lat);
    const longitude = parseFloat(lng);
    
    if (!isNaN(latitude) && !isNaN(longitude)) {
      setPosition({ lat: latitude, lng: longitude });
    }
  };

  if (loading) return <Layout><div className="loading">Loading...</div></Layout>;

  return (
    <Layout>
      <div className="position-simulator-page">
        <div className="page-header">
          <div>
            <h1>Position Simulator</h1>
            <p>Set your current GPS location for tour execution</p>
          </div>
        </div>

        <div className="simulator-container">
          <div className="map-section">
            <h3>Click on the map to set your position</h3>
            <div className="map-wrapper">
              <MapContainer
                center={position ? [position.lat, position.lng] : [defaultCenter.lat, defaultCenter.lng]}
                zoom={13}
                style={{ height: '500px', width: '100%' }}
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                <LocationMarker position={position} setPosition={setPosition} />
              </MapContainer>
            </div>
          </div>

          <div className="controls-section">
            <div className="current-position">
              <h3>Current Position</h3>
              {position ? (
                <div className="position-display">
                  <div className="coord-item">
                    <span className="label">Latitude:</span>
                    <span className="value">{position.lat.toFixed(6)}</span>
                  </div>
                  <div className="coord-item">
                    <span className="label">Longitude:</span>
                    <span className="value">{position.lng.toFixed(6)}</span>
                  </div>
                </div>
              ) : (
                <p>No position set</p>
              )}
            </div>

            <div className="manual-input">
              <h3>Or Enter Coordinates Manually</h3>
              <div className="input-group">
                <div className="form-group">
                  <label>Latitude</label>
                  <input
                    type="number"
                    step="0.000001"
                    placeholder="44.8176"
                    onChange={(e) => {
                      if (position) {
                        handleManualInput(e.target.value, position.lng);
                      }
                    }}
                  />
                </div>
                <div className="form-group">
                  <label>Longitude</label>
                  <input
                    type="number"
                    step="0.000001"
                    placeholder="20.4569"
                    onChange={(e) => {
                      if (position) {
                        handleManualInput(position.lat, e.target.value);
                      }
                    }}
                  />
                </div>
              </div>
            </div>

            <button
              onClick={handleSavePosition}
              className="btn-save"
              disabled={!position || saving}
            >
              {saving ? 'Saving...' : 'Save Position'}
            </button>

          </div>
        </div>
      </div>
    </Layout>
  );
}

export default PositionSimulator;