import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useParams, useNavigate } from 'react-router-dom';
import './KeyPoints.css';

function KeyPoints() {
  const [tour, setTour] = useState(null);
  const [keypoints, setKeypoints] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Form state
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [latitude, setLatitude] = useState('');
  const [longitude, setLongitude] = useState('');
  const [image, setImage] = useState('');
  const [order, setOrder] = useState(1);
  const [adding, setAdding] = useState(false);
  
  const { tourId } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();

  const fetchData = useCallback(async () => {
  try {
    // Fetch tour details
    const tourResponse = await tourAPI.getTourById(tourId);
    console.log('Tour Response:', tourResponse.data);
    if (tourResponse.data.success) {
      setTour(tourResponse.data.tour);
    }

    // Fetch keypoints
    const kpResponse = await tourAPI.getKeyPoints(tourId, user.userId);
    console.log('KeyPoints Response:', kpResponse.data);
    console.log('KeyPoints Array:', kpResponse.data.keyPoints);
    
    if (kpResponse.data.success) {
      setKeypoints(kpResponse.data.keyPoints);
      setOrder(kpResponse.data.keyPoints.length + 1);
    }
  } catch (err) {
    console.error('Failed to load data:', err);
  } finally {
    setLoading(false);
  }
}, [tourId, user.userId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setAdding(true);

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
        // Refresh data
        await fetchData();
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to add key point');
      console.error(err);
    } finally {
      setAdding(false);
    }
  };

  if (loading) return <Layout><div className="loading">Loading...</div></Layout>;
  if (!tour) return <Layout><div className="error">Tour not found</div></Layout>;

  return (
    <Layout>
      <div className="keypoints-page">
        <div className="page-header">
          <div>
            <h1>Manage Key Points</h1>
            <p className="tour-name">{tour.name}</p>
          </div>
          <button 
            onClick={() => navigate('/my-tours')}
            className="btn-secondary"
          >
            Back to My Tours
          </button>
        </div>

        <div className="keypoints-container">
          {/* Add Key Point Form */}
          <div className="add-keypoint-section">
            <h2>Add New Key Point</h2>
            <form onSubmit={handleSubmit} className="keypoint-form">
              <div className="form-row">
                <div className="form-group">
                  <label>Name *</label>
                  <input
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="e.g., Kalemegdan Fortress"
                    required
                    disabled={adding}
                  />
                </div>
                
                <div className="form-group">
                  <label>Order *</label>
                  <input
                    type="number"
                    value={order}
                    onChange={(e) => setOrder(parseInt(e.target.value))}
                    min="1"
                    required
                    disabled={adding}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Description *</label>
                <textarea
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Describe this location..."
                  rows="3"
                  required
                  disabled={adding}
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
                    placeholder="44.8176"
                    required
                    disabled={adding}
                  />
                </div>
                
                <div className="form-group">
                  <label>Longitude *</label>
                  <input
                    type="number"
                    step="0.000001"
                    value={longitude}
                    onChange={(e) => setLongitude(e.target.value)}
                    placeholder="20.4569"
                    required
                    disabled={adding}
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Image URL (optional)</label>
                <input
                  type="text"
                  value={image}
                  onChange={(e) => setImage(e.target.value)}
                  placeholder="https://example.com/image.jpg"
                  disabled={adding}
                />
              </div>

              <button type="submit" className="btn-primary" disabled={adding}>
                {adding ? 'Adding...' : 'Add Key Point'}
              </button>
            </form>

            <div className="helper-box">
              <h4>💡 Tips</h4>
              <ul>
                <li>Use <a href="https://www.google.com/maps" target="_blank" rel="noopener noreferrer">Google Maps</a> to find coordinates</li>
                <li>Right-click on a location → Click the coordinates to copy</li>
                <li>Order determines the sequence tourists will follow</li>
              </ul>
            </div>
          </div>

          {/* Existing Key Points List */}
          <div className="keypoints-list-section">
            <h2>Key Points ({keypoints.length})</h2>
            
            {keypoints.length === 0 ? (
              <div className="empty-state">
                <p>No key points added yet</p>
              </div>
            ) : (
              <div className="keypoints-list">
                {keypoints.map((kp) => (
                  <div key={kp.id} className="keypoint-card">
                    <div className="kp-order">#{kp.order}</div>
                    <div className="kp-content">
                      <h3>{kp.name}</h3>
                      <p>{kp.description}</p>
                      <div className="kp-coords">
                        📍 {kp.latitude.toFixed(6)}, {kp.longitude.toFixed(6)}
                      </div>
                      {kp.image && (
                        <div className="kp-image">
                          <img src={kp.image} alt={kp.name} />
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </Layout>
  );
}

export default KeyPoints;