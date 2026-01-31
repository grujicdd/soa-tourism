import React from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './Home.css';

function Home() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="home-container">
      <header className="home-header">
        <h1>Tourism App</h1>
        <div className="user-info">
          <span>Welcome, <strong>{user.username}</strong></span>
          <span className="role-badge">{user.role}</span>
          <button onClick={handleLogout} className="btn-logout">Logout</button>
        </div>
      </header>
      
      <main className="home-content">
        <div className="welcome-card">
          <h2>🎉 Successfully Logged In!</h2>
          <p>Your account is set up and ready to go.</p>
          
          <div className="user-details">
            <p><strong>User ID:</strong> {user.userId}</p>
            <p><strong>Username:</strong> {user.username}</p>
            <p><strong>Role:</strong> {user.role}</p>
          </div>
          
          <div className="feature-preview">
            <h3>Coming Soon:</h3>
            <ul>
              {user.role === 'guide' ? (
                <>
                  <li>✨ Create and manage tours</li>
                  <li>📍 Add key points to your tours</li>
                  <li>📝 Write blog posts about your experiences</li>
                </>
              ) : (
                <>
                  <li>🗺️ Browse and purchase tours</li>
                  <li>🚶 Execute tours with GPS tracking</li>
                  <li>📝 Write blogs and follow other users</li>
                  <li>🛒 Manage your shopping cart</li>
                </>
              )}
            </ul>
          </div>
        </div>
      </main>
    </div>
  );
}

export default Home;