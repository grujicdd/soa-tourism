import React from 'react';
import Layout from '../components/Layout';
import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import './Home.css';

function Home() {
  const { user } = useAuth();

  return (
    <Layout>
      <div className="home-dashboard">
        <div className="welcome-section">
          <h1>Welcome back, {user.username}! 👋</h1>
          <p>You're logged in as a <strong>{user.role}</strong></p>
        </div>

        <div className="quick-actions">
          <h2>Quick Actions</h2>
          <div className="action-cards">
            {user.role === 'guide' ? (
              <>
                <Link to="/create-tour" className="action-card">
                  <span className="card-icon">➕</span>
                  <h3>Create New Tour</h3>
                  <p>Design and publish a new tour</p>
                </Link>
                
                <Link to="/my-tours" className="action-card">
                  <span className="card-icon">📋</span>
                  <h3>My Tours</h3>
                  <p>Manage your existing tours</p>
                </Link>
                
                <Link to="/blogs" className="action-card">
                  <span className="card-icon">✍️</span>
                  <h3>Write Blog</h3>
                  <p>Share your experiences</p>
                </Link>
              </>
            ) : (
              <>
                <Link to="/tours" className="action-card">
                  <span className="card-icon">🗺️</span>
                  <h3>Browse Tours</h3>
                  <p>Discover amazing tours</p>
                </Link>
                
                <Link to="/cart" className="action-card">
                  <span className="card-icon">🛒</span>
                  <h3>Shopping Cart</h3>
                  <p>View your cart and checkout</p>
                </Link>
                
                <Link to="/blogs" className="action-card">
                  <span className="card-icon">📝</span>
                  <h3>Blogs</h3>
                  <p>Read and write blogs</p>
                </Link>
                
                <Link to="/my-executions" className="action-card">
                  <span className="card-icon">🚶</span>
                  <h3>My Tours</h3>
                  <p>View your active tours</p>
                </Link>
              </>
            )}
          </div>
        </div>
      </div>
    </Layout>
  );
}

export default Home;