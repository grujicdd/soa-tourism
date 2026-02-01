import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './MyExecutions.css';

function MyExecutions() {
  const [purchasedTours, setPurchasedTours] = useState([]);
  const [activeExecutions, setActiveExecutions] = useState({}); // Add this
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();
  const navigate = useNavigate();

  const fetchPurchasedTours = useCallback(async () => {
  try {
    // Get all published tours
    const response = await tourAPI.getTours(true);
    if (response.data.success) {
      // Filter to show only tours we can access keypoints for (purchased)
      const toursWithAccess = [];
      
      for (const tour of response.data.tours) {
        const kpResponse = await tourAPI.getKeyPoints(tour.id, user.userId);
        if (kpResponse.data.isPurchased) {
          toursWithAccess.push(tour);
        }
      }
      
      setPurchasedTours(toursWithAccess);
    }
  } catch (err) {
    console.error('Failed to load purchased tours:', err);
  } finally {
    setLoading(false);
  }
}, [user.userId]);

  useEffect(() => {
    fetchPurchasedTours();
  }, [fetchPurchasedTours]);

  const handleStartTour = async (tour) => {
    // First, get current position
    try {
      const posResponse = await tourAPI.getPosition(user.userId);
      
      if (!posResponse.data.success || !posResponse.data.position) {
        alert('Please set your position in the Position Simulator first!');
        navigate('/position-simulator');
        return;
      }

      const position = posResponse.data.position;

      // Start tour execution
      const execResponse = await tourAPI.startExecution(tour.id, {
        touristId: user.userId,
        tourId: tour.id,
        startLatitude: position.latitude,
        startLongitude: position.longitude
      });

      if (execResponse.data.success) {
        const executionData = execResponse.data.execution;
        navigate(`/tour-execution/${executionData.id}`, {
          state: { execution: executionData, tour: tour }
        });
      } else {
        alert(execResponse.data.message);
      }
    } catch (err) {
      console.error('Failed to start tour:', err);
      alert('Failed to start tour. ' + (err.response?.data?.message || err.message));
    }
  };

  if (loading) return <Layout><div className="loading">Loading your tours...</div></Layout>;

  return (
    <Layout>
      <div className="my-executions-page">
        <div className="page-header">
          <h1>My Purchased Tours</h1>
          <p>Tours you own and can execute</p>
        </div>

        {purchasedTours.length === 0 ? (
          <div className="empty-state">
            <h2>📦 No purchased tours yet</h2>
            <p>Browse tours and purchase them to start exploring!</p>
            <button 
              onClick={() => navigate('/tours')}
              className="btn-primary"
            >
              Browse Tours
            </button>
          </div>
        ) : (
          <div className="tours-grid">
            {purchasedTours.map((tour) => (
              <div key={tour.id} className="tour-card">
                <div className="tour-header">
                  <h3>{tour.name}</h3>
                  <span className={`difficulty-badge ${tour.difficulty}`}>
                    {tour.difficulty}
                  </span>
                </div>
                
                <p className="tour-description">{tour.description}</p>
                
                <div className="tour-tags">
                  {tour.tags.map((tag, index) => (
                    <span key={index} className="tag">{tag}</span>
                  ))}
                </div>
                
                <div className="tour-footer">
                  <span className="purchased-badge">✓ Purchased</span>
                  <button 
                    onClick={() => handleStartTour(tour)}
                    className="btn-start"
                  >
                    Start / Continue Tour 🚶
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}

        <div className="info-box">
          <h3>💡 Before starting a tour:</h3>
          <ul>
            <li>Make sure you've set your position in the Position Simulator</li>
            <li>Have your map ready to navigate between key points</li>
            <li>The system will check your proximity every 10 seconds</li>
            <li>You'll be notified when you're near a key point!</li>
          </ul>
        </div>
      </div>
    </Layout>
  );
}

export default MyExecutions;