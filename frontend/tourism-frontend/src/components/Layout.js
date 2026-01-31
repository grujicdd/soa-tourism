import React from 'react';
import Navbar from './Navbar';
import './Layout.css';

function Layout({ children }) {
  return (
    <div className="layout">
      <Navbar />
      <main className="main-content">
        {children}
      </main>
    </div>
  );
}

export default Layout;