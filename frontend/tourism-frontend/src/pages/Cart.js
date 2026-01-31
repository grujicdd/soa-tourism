import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './Cart.css';

function Cart() {
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();
  const navigate = useNavigate();

  const fetchCart = useCallback(async () => {
    try {
      const response = await tourAPI.getCart(user.userId);
      if (response.data.success) {
        setCart(response.data.cart);
      }
    } catch (err) {
      console.error('Failed to load cart:', err);
    } finally {
      setLoading(false);
    }
  }, [user.userId]);

  useEffect(() => {
    fetchCart();
  }, [fetchCart]);

  const handleRemove = async (tourId) => {
    try {
      const response = await tourAPI.removeFromCart(user.userId, tourId);
      if (response.data.success) {
        setCart(response.data.cart);
      }
    } catch (err) {
      alert('Failed to remove item');
      console.error(err);
    }
  };

  const handleCheckout = async () => {
    if (!cart || cart.items.length === 0) return;

    try {
      const response = await tourAPI.checkout(user.userId);
      if (response.data.success) {
        alert(`Successfully purchased ${response.data.tokens.length} tours!`);
        navigate('/');
      }
    } catch (err) {
      alert('Checkout failed');
      console.error(err);
    }
  };

  if (loading) return <Layout><div className="loading">Loading cart...</div></Layout>;

  return (
    <Layout>
      <div className="cart-page">
        <div className="page-header">
          <h1>Shopping Cart</h1>
        </div>

        {!cart || cart.items.length === 0 ? (
          <div className="empty-cart">
            <h2>🛒 Your cart is empty</h2>
            <p>Browse tours and add them to your cart!</p>
            <button 
              onClick={() => navigate('/tours')}
              className="btn-primary"
            >
              Browse Tours
            </button>
          </div>
        ) : (
          <div className="cart-container">
            <div className="cart-items">
              {cart.items.map((item) => (
                <div key={item.tourId} className="cart-item">
                  <div className="item-info">
                    <h3>{item.tourName}</h3>
                    <p className="item-price">${item.price.toFixed(2)}</p>
                  </div>
                  <button 
                    onClick={() => handleRemove(item.tourId)}
                    className="btn-remove"
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>

            <div className="cart-summary">
              <h3>Order Summary</h3>
              <div className="summary-row">
                <span>Items ({cart.items.length})</span>
                <span>${cart.totalPrice.toFixed(2)}</span>
              </div>
              <div className="summary-total">
                <span>Total</span>
                <span>${cart.totalPrice.toFixed(2)}</span>
              </div>
              <button 
                onClick={handleCheckout}
                className="btn-checkout"
              >
                Proceed to Checkout
              </button>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default Cart;