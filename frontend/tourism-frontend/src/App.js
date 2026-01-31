import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Login from './pages/Login';
import Register from './pages/Register';
import Home from './pages/Home';
import Tours from './pages/Tours';
import CreateTour from './pages/CreateTour';
import MyTours from './pages/MyTours';
import Cart from './pages/Cart';
import Blogs from './pages/Blogs';
import KeyPoints from './pages/KeyPoints';

// Protected Route component
function ProtectedRoute({ children, requireGuide, requireTourist }) {
  const { isAuthenticated, loading, isGuide, isTourist } = useAuth();
  
  if (loading) {
    return <div>Loading...</div>;
  }
  
  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }
  
  if (requireGuide && !isGuide) {
    return <Navigate to="/" />;
  }
  
  if (requireTourist && !isTourist) {
    return <Navigate to="/" />;
  }
  
  return children;
}

// Public Route (redirect to home if already logged in)
function PublicRoute({ children }) {
  const { isAuthenticated, loading } = useAuth();
  
  if (loading) {
    return <div>Loading...</div>;
  }
  
  return !isAuthenticated ? children : <Navigate to="/" />;
}

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={
            <PublicRoute>
              <Login />
            </PublicRoute>
          } />
          
          <Route path="/register" element={
            <PublicRoute>
              <Register />
            </PublicRoute>
          } />
          
          {/* Protected Routes - All Users */}
          <Route path="/" element={
            <ProtectedRoute>
              <Home />
            </ProtectedRoute>
          } />
          
          {/* Protected Routes - Guide Only */}
          <Route path="/create-tour" element={
            <ProtectedRoute requireGuide>
              <CreateTour />
            </ProtectedRoute>
          } />
          
          <Route path="/my-tours" element={
            <ProtectedRoute requireGuide>
              <MyTours />
            </ProtectedRoute>
          } />
          
          {/* Protected Routes - Tourist Only */}
          <Route path="/tours" element={
            <ProtectedRoute requireTourist>
              <Tours />
            </ProtectedRoute>
          } />

          <Route path="/cart" element={
            <ProtectedRoute requireTourist>
              <Cart />
            </ProtectedRoute>
          } />

          <Route path="/tour/:tourId/keypoints" element={
            <ProtectedRoute requireGuide>
              <KeyPoints />
            </ProtectedRoute>
          } />
          
          {/* Protected Routes - All Users */}
          <Route path="/blogs" element={
            <ProtectedRoute>
              <Blogs />
            </ProtectedRoute>
          } />
          
          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;