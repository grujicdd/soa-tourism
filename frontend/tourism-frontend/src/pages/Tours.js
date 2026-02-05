import React, { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './Tours.css';

function Tours() {
  const [tours, setTours] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const { user } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    fetchTours();
  }, []);

  const fetchTours = async () => {
    try {
      const response = await tourAPI.getTours(true);
      if (response.data.success) {
        setTours(response.data.tours);
      }
    } catch (err) {
      setError('Failed to load tours');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddToCart = async (tourId) => {
    try {
      const response = await tourAPI.addToCart(user.userId, tourId);
      if (response.data.success) {
        alert('Tour added to cart!');
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to add to cart');
      console.error(err);
    }
  };

  const handleViewDetails = (tourId) => {
    navigate(`/tour/${tourId}`);
  };

  if (loading) return <Layout><div className="loading">Loading tours...</div></Layout>;
  if (error) return <Layout><div className="error">{error}</div></Layout>;

  return (
    <Layout>
      <div className="tours-page">
        <div className="page-header">
          <h1>Browse Tours</h1>
          <p>Discover amazing tours from our guides</p>
        </div>

        {tours.length === 0 ? (
          <div className="empty-state">
            <p>No tours available yet. Check back soon!</p>
          </div>
        ) : (
          <div className="tours-grid">
            {tours.map((tour) => (
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
                  <span className="tour-price">${tour.price.toFixed(2)}</span>
                  <div className="tour-actions">
                  
                    <button 
                      onClick={() => handleAddToCart(tour.id)}
                      className="btn-primary"
                    >
                      Add to Cart
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}

export default Tours;