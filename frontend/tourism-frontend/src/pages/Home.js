//import React from 'react';
import Layout from '../components/Layout';
import { useAuth } from '../context/AuthContext';
//import { Link } from 'react-router-dom';
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

        
      </div>
    </Layout>
  );
}

export default Home;