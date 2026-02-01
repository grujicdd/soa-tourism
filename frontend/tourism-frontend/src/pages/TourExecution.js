import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import Layout from '../components/Layout';
import { MapContainer, TileLayer, Marker, Polyline } from 'react-leaflet';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import './TourExecution.css';

// Custom icons for different marker types
const currentPositionIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
});

const completedIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
});

const pendingIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
});

function TourExecution() {
  const [execution, setExecution] = useState(null);
  const [tour, setTour] = useState(null);
  const [keypoints, setKeypoints] = useState([]);
  const [currentPosition, setCurrentPosition] = useState(null);
  const [nearbyKeypoint, setNearbyKeypoint] = useState(null);
  const [loading, setLoading] = useState(true);
  const [checking, setChecking] = useState(false);
  
  const { executionId } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const intervalRef = useRef(null);

  const fetchExecutionData = useCallback(async () => {
    try {
      // Use passed state if available
      if (location.state?.execution && location.state?.tour) {
        setExecution(location.state.execution);
        setTour(location.state.tour);
        
        // Get keypoints
        const kpResponse = await tourAPI.getKeyPoints(location.state.tour.id, user.userId);
        if (kpResponse.data.success) {
          setKeypoints(kpResponse.data.keyPoints);
        }
        
        // Get current position
        const posResponse = await tourAPI.getPosition(user.userId);
        if (posResponse.data.success && posResponse.data.position) {
          setCurrentPosition({
            lat: posResponse.data.position.latitude,
            lng: posResponse.data.position.longitude
          });
        }
        
        setLoading(false);
        return;
      }

      // Fallback: try to fetch from API (won't work yet)
      alert('Execution data not found. Please start the tour again.');
      navigate('/my-executions');
      
    } catch (err) {
      console.error('Failed to load execution:', err);
      alert('Failed to load tour execution');
      navigate('/my-executions');
    }
  }, [executionId, user.userId, navigate, location.state]);

  const checkProximity = useCallback(async () => {
  if (!currentPosition || !execution || execution.status !== 'active') {
    console.log('Skipping proximity check:', {
      hasPosition: !!currentPosition,
      hasExecution: !!execution,
      status: execution?.status
    });
    return;
  }

  console.log('Checking proximity...', {
    currentPosition,
    executionId,
    touristId: user.userId
  });

  setChecking(true);
    try {
      const response = await tourAPI.checkProximity(executionId, {
        touristId: user.userId,
        currentLatitude: currentPosition.lat,
        currentLongitude: currentPosition.lng
      });

      console.log('Proximity check response:', response.data);

      
      if (response.data.success && response.data.nearKeypoint) {
        console.log('Near keypoint!', response.data.nearbyKeypoint);
        setNearbyKeypoint(response.data.nearbyKeypoint);
        
        // Update execution with new completed keypoint
        if (response.data.nearbyKeypoint) {
          setExecution(prev => ({
            ...prev,
            completedKeypoints: [
              ...prev.completedKeypoints,
              {
                keypointId: response.data.nearbyKeypoint.id,
                completedAt: new Date().toISOString()
              }
            ]
          }));
        }
      } else {
        console.log('Not near any keypoint');
        setNearbyKeypoint(null);
      }
    } catch (err) {
      console.error('Error checking proximity:', err);
      console.error('Error response:', err.response?.data);
    } finally {
      setChecking(false);
    }
  }, [currentPosition, execution, executionId, user.userId, fetchExecutionData]);

  useEffect(() => {
    fetchExecutionData();
  }, [fetchExecutionData]);

  // Set up proximity checking interval
  useEffect(() => {
    if (execution && execution.status === 'active') {
      // Check immediately
      checkProximity();

      // Then check every 10 seconds
      intervalRef.current = setInterval(() => {
        checkProximity();
      }, 10000);

      return () => {
        if (intervalRef.current) {
          clearInterval(intervalRef.current);
        }
      };
    }
  }, [execution, checkProximity]);

  const handleCompleteTour = async () => {
    try {
      const response = await tourAPI.completeTour(executionId, user.userId);
      if (response.data.success) {
        alert('🎉 Tour completed successfully!');
        navigate('/my-executions');
      }
    } catch (err) {
      alert('Failed to complete tour');
      console.error(err);
    }
  };

  const handleAbandonTour = async () => {
    if (!window.confirm('Are you sure you want to abandon this tour?')) {
      return;
    }

    try {
      const response = await tourAPI.abandonTour(executionId, user.userId);
      if (response.data.success) {
        alert('Tour abandoned');
        navigate('/my-executions');
      }
    } catch (err) {
      alert('Failed to abandon tour');
      console.error(err);
    }
  };

  const handleUpdatePosition = () => {
    navigate('/position-simulator');
  };

  if (loading) return <Layout><div className="loading">Loading tour...</div></Layout>;
  if (!execution || !tour) return <Layout><div className="error">Tour not found</div></Layout>;

  const completedKeypointIds = execution.completedKeypoints.map(kp => kp.keypointId);
  const completedCount = completedKeypointIds.length;
  const totalCount = keypoints.length;
  const progress = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;

  return (
    <Layout>
      <div className="tour-execution-page">
        <div className="execution-header">
          <div>
            <h1>{tour.name}</h1>
            <p className="tour-description">{tour.description}</p>
          </div>
          <div className="status-badge-large">
            {execution.status}
          </div>
        </div>

        <div className="execution-container">
          {/* Map Section */}
          <div className="map-section">
            <div className="map-header">
              <h3>Tour Map</h3>
              {checking && <span className="checking-indicator">🔄 Checking proximity...</span>}
            </div>
            
            <div className="map-wrapper">
              <MapContainer
                center={currentPosition ? [currentPosition.lat, currentPosition.lng] : [44.8176, 20.4569]}
                zoom={14}
                style={{ height: '500px', width: '100%' }}
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                
                {/* Current position marker */}
                {currentPosition && (
                  <Marker 
                    position={[currentPosition.lat, currentPosition.lng]}
                    icon={currentPositionIcon}
                  />
                )}
                
                {/* Keypoint markers */}
                {keypoints.map((kp) => {
                  const isCompleted = completedKeypointIds.includes(kp.id);
                  return (
                    <Marker
                      key={kp.id}
                      position={[kp.latitude, kp.longitude]}
                      icon={isCompleted ? completedIcon : pendingIcon}
                    />
                  );
                })}
                
                {/* Route line */}
                {keypoints.length > 0 && (
                  <Polyline
                    positions={keypoints.map(kp => [kp.latitude, kp.longitude])}
                    color="#667eea"
                    weight={3}
                    opacity={0.7}
                  />
                )}
              </MapContainer>
            </div>
          </div>

          {/* Progress Section */}
          <div className="progress-section">
            <div className="progress-card">
              <h3>Tour Progress</h3>
              <div className="progress-bar">
                <div 
                  className="progress-fill" 
                  style={{ width: `${progress}%` }}
                />
              </div>
              <p className="progress-text">
                {completedCount} of {totalCount} key points completed ({progress.toFixed(0)}%)
              </p>
            </div>

            {nearbyKeypoint && (
              <div className="nearby-alert">
                <h4>🎯 You're near a key point!</h4>
                <p className="nearby-name">{nearbyKeypoint.name}</p>
                <p className="nearby-desc">{nearbyKeypoint.description}</p>
              </div>
            )}

            <div className="keypoints-list-card">
              <h3>Key Points</h3>
              <div className="keypoints-list">
                {keypoints.map((kp, index) => {
                  const isCompleted = completedKeypointIds.includes(kp.id);
                  return (
                    <div key={kp.id} className={`keypoint-item ${isCompleted ? 'completed' : ''}`}>
                      <div className="kp-number">#{index + 1}</div>
                      <div className="kp-info">
                        <h4>{kp.name}</h4>
                        <p>{kp.description}</p>
                      </div>
                      {isCompleted && <div className="kp-check">✓</div>}
                    </div>
                  );
                })}
              </div>
            </div>

            <div className="actions-card">
              <button 
                onClick={() => {
                  console.log('=== DEBUG INFO ===');
                  console.log('Keypoints:', keypoints);
                  console.log('Current Position:', currentPosition);
                  console.log('Execution:', execution);
                  console.log('Completed Keypoints:', execution?.completedKeypoints);
                }}
                className="btn-secondary"
              >
                Debug Info
              </button>

              <button 
                onClick={handleUpdatePosition}
                className="btn-secondary"
              >
                Update Position
              </button>
              
              {completedCount === totalCount ? (
                <button 
                  onClick={handleCompleteTour}
                  className="btn-complete"
                >
                  Complete Tour ✓
                </button>
              ) : (
                <button 
                  onClick={handleAbandonTour}
                  className="btn-abandon"
                >
                  Abandon Tour
                </button>
              )}
            </div>

            <div className="info-box">
              <h4>ℹ️ How it works</h4>
              <ul>
                <li>Your position is checked every 10 seconds</li>
                <li>Get within 50 meters of a key point to mark it as completed</li>
                <li>Update your position in the simulator as you move</li>
                <li>Complete all key points to finish the tour!</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}

export default TourExecution;