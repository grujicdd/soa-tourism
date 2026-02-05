import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import { MapContainer, TileLayer, Marker, useMapEvents } from 'react-leaflet';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import './KeyPoints.css';

// Fix default marker icon
const DefaultIcon = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41]
});
L.Marker.prototype.options.icon = DefaultIcon;

// Component to handle map clicks
function MapClickHandler({ onLocationSelect }) {
  useMapEvents({
    click(e) {
      onLocationSelect(e.latlng.lat, e.latlng.lng);
    },
  });
  return null;
}

function KeyPoints() {
  const [keypoints, setKeypoints] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  
  // Form state
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [latitude, setLatitude] = useState('');
  const [longitude, setLongitude] = useState('');
  const [image, setImage] = useState('');
  const [order, setOrder] = useState(1);
  
  // Map state
  const [selectedPosition, setSelectedPosition] = useState(null);
  const [mapCenter, setMapCenter] = useState([44.8176, 20.4569]); // Belgrade

  const { tourId } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();

  const fetchKeyPoints = useCallback(async () => {
    try {
      const response = await tourAPI.getKeyPoints(tourId, user.userId);
      if (response.data.success) {
        setKeypoints(response.data.keyPoints);
        
        // Set next order number
        if (response.data.keyPoints.length > 0) {
          const maxOrder = Math.max(...response.data.keyPoints.map(kp => kp.order));
          setOrder(maxOrder + 1);
          
          // Center map on last keypoint
          const lastKp = response.data.keyPoints[response.data.keyPoints.length - 1];
          setMapCenter([lastKp.latitude, lastKp.longitude]);
        }
      }
    } catch (err) {
      console.error('Failed to load keypoints:', err);
    } finally {
      setLoading(false);
    }
  }, [tourId, user.userId]);

  useEffect(() => {
    fetchKeyPoints();
  }, [fetchKeyPoints]);

  const handleMapClick = (lat, lng) => {
    setLatitude(lat.toFixed(6));
    setLongitude(lng.toFixed(6));
    setSelectedPosition({ lat, lng });
  };

  const handleAddKeyPoint = async (e) => {
    e.preventDefault();
    setSaving(true);

    try {
      const response = await tourAPI.addKeyPoint(tourId, {
        guideId: user.userId,
        latitude: parseFloat(latitude),
        longitude: parseFloat(longitude),
        name,
        description,
        image,
        order
      });

      if (response.data.success) {
        alert('Key point added successfully!');
        // Reset form
        setName('');
        setDescription('');
        setLatitude('');
        setLongitude('');
        setImage('');
        setSelectedPosition(null);
        // Refresh keypoints
        fetchKeyPoints();
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to add key point');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Layout><div className="loading">Loading...</div></Layout>;

  return (
    <Layout>
      <div className="keypoints-page">
        <div className="page-header">
          <h1>Manage Key Points</h1>
          <button onClick={() => navigate('/my-tours')} className="btn-secondary">
            Back to My Tours
          </button>
        </div>

        <div className="keypoints-container">
          {/* Map Section */}
          <div className="map-section">
            <h2>Select Location on Map</h2>
            <p className="map-instructions">Click anywhere on the map to set keypoint coordinates</p>
            <div className="map-wrapper">
              <MapContainer
                center={mapCenter}
                zoom={13}
                style={{ height: '500px', width: '100%' }}
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                <MapClickHandler onLocationSelect={handleMapClick} />
                
                {/* Show selected position */}
                {selectedPosition && (
                  <Marker position={[selectedPosition.lat, selectedPosition.lng]} />
                )}
                
                {/* Show existing keypoints */}
                {keypoints.map((kp) => (
                  <Marker
                    key={kp.id}
                    position={[kp.latitude, kp.longitude]}
                  />
                ))}
              </MapContainer>
            </div>
          </div>

          {/* Form Section */}
          <div className="form-section">
            <h2>Add Key Point</h2>
            <form onSubmit={handleAddKeyPoint}>
              <div className="form-group">
                <label>Name *</label>
                <input
                  type="text"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="e.g., Kalemegdan Fortress"
                  required
                  disabled={saving}
                />
              </div>

              <div className="form-group">
                <label>Description *</label>
                <textarea
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Describe this location..."
                  rows="3"
                  required
                  disabled={saving}
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Latitude *</label>
                  <input
                    type="number"
                    step="0.000001"
                    value={latitude}
                    onChange={(e) => setLatitude(e.target.value)}
                    placeholder="Click map or enter manually"
                    required
                    disabled={saving}
                  />
                </div>

                <div className="form-group">
                  <label>Longitude *</label>
                  <input
                    type="number"
                    step="0.000001"
                    value={longitude}
                    onChange={(e) => setLongitude(e.target.value)}
                    placeholder="Click map or enter manually"
                    required
                    disabled={saving}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Image URL</label>
                <input
                  type="text"
                  value={image}
                  onChange={(e) => setImage(e.target.value)}
                  placeholder="https://example.com/image.jpg"
                  disabled={saving}
                />
              </div>

              <div className="form-group">
                <label>Order</label>
                <input
                  type="number"
                  value={order}
                  onChange={(e) => setOrder(parseInt(e.target.value))}
                  min="1"
                  disabled={saving}
                />
              </div>

              <button type="submit" className="btn-submit" disabled={saving || !latitude || !longitude}>
                {saving ? 'Adding...' : 'Add Key Point'}
              </button>
            </form>

            {/* Existing Keypoints List */}
            <div className="keypoints-list">
              <h3>Existing Key Points ({keypoints.length})</h3>
              {keypoints.length === 0 ? (
                <p className="empty-message">No keypoints yet. Add your first one!</p>
              ) : (
                keypoints.map((kp, index) => (
                  <div key={kp.id} className="keypoint-item">
                    <span className="kp-order">#{kp.order}</span>
                    <div className="kp-details">
                      <strong>{kp.name}</strong>
                      <p>{kp.description}</p>
                      <small>{kp.latitude.toFixed(4)}, {kp.longitude.toFixed(4)}</small>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}

export default KeyPoints;