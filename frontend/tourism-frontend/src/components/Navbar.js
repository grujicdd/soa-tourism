import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Navbar.css';

function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/">🗺️ Tourism App</Link>
      </div>
      
      <div className="navbar-links">
        <Link to="/">Home</Link>
        
        {user.role === 'guide' ? (
          <>
            <Link to="/my-tours">My Tours</Link>
            <Link to="/create-tour">Create Tour</Link>
          </>
        ) : (
          <>
            <Link to="/tours">Browse Tours</Link>
            <Link to="/cart">Cart</Link>
            <Link to="/my-executions">My Tours</Link>
          </>
        )}
        
        <Link to="/blogs">Blogs</Link>
        <Link to="/profile">Profile</Link>
      </div>
      
      <div className="navbar-user">
        <span className="username">{user.username}</span>
        <span className="role-badge">{user.role}</span>
        <button onClick={handleLogout} className="btn-logout">Logout</button>
      </div>
    </nav>
  );
}

export default Navbar;