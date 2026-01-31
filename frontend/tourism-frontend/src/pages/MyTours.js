import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './MyTours.css';

function MyTours() {
  const [tours, setTours] = useState([]);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();
  const navigate = useNavigate();

  const fetchMyTours = useCallback(async () => {
  try {
    const response = await tourAPI.getMyTours(user.userId);
    if (response.data.success) {
      setTours(response.data.tours);
    }
  } catch (err) {
    console.error('Failed to load tours:', err);
  } finally {
    setLoading(false);
  }
}, [user.userId]);

useEffect(() => {
  fetchMyTours();
}, [fetchMyTours]);

  const handlePublish = async (tourId) => {
    const price = prompt('Enter tour price:');
    if (!price) return;

    try {
      const response = await tourAPI.publishTour(tourId, {
        guideId: user.userId,
        price: parseFloat(price)
      });

      if (response.data.success) {
        alert('Tour published!');
        fetchMyTours();
      }
    } catch (err) {
      alert('Failed to publish tour');
      console.error(err);
    }
  };

  const handleAddKeyPoints = (tourId) => {
    navigate(`/tour/${tourId}/keypoints`);
  };

  if (loading) return <Layout><div className="loading">Loading...</div></Layout>;

  return (
    <Layout>
      <div className="my-tours-page">
        <div className="page-header">
          <h1>My Tours</h1>
          <button 
            onClick={() => navigate('/create-tour')}
            className="btn-primary"
          >
            + Create New Tour
          </button>
        </div>

        {tours.length === 0 ? (
          <div className="empty-state">
            <p>You haven't created any tours yet.</p>
            <button 
              onClick={() => navigate('/create-tour')}
              className="btn-primary"
            >
              Create Your First Tour
            </button>
          </div>
        ) : (
          <div className="tours-list">
            {tours.map((tour) => (
              <div key={tour.id} className="tour-item">
                <div className="tour-info">
                  <h3>{tour.name}</h3>
                  <p>{tour.description}</p>
                  <div className="tour-meta">
                    <span className={`status-badge ${tour.status}`}>
                      {tour.status}
                    </span>
                    <span className={`difficulty-badge ${tour.difficulty}`}>
                      {tour.difficulty}
                    </span>
                    {tour.isPublished && (
                      <span className="price-tag">${tour.price.toFixed(2)}</span>
                    )}
                  </div>
                </div>
                
                <div className="tour-actions">
                  <button 
                    onClick={() => handleAddKeyPoints(tour.id)}
                    className="btn-secondary"
                  >
                    Key Points
                  </button>
                  
                  {!tour.isPublished && (
                    <button 
                      onClick={() => handlePublish(tour.id)}
                      className="btn-primary"
                    >
                      Publish
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}

export default MyTours;