import React, { useState } from 'react';
import Layout from '../components/Layout';
import { tourAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './CreateTour.css';

function CreateTour() {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [difficulty, setDifficulty] = useState('easy');
  const [tags, setTags] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  const { user } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const tagArray = tags.split(',').map(tag => tag.trim()).filter(tag => tag);
      
      const response = await tourAPI.createTour({
        guideId: user.userId,
        name,
        description,
        difficulty,
        tags: tagArray
      });

      if (response.data.success) {
        alert('Tour created successfully!');
        navigate('/my-tours');
      } else {
        setError(response.data.message);
      }
    } catch (err) {
      setError('Failed to create tour');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="create-tour-page">
        <div className="page-header">
          <h1>Create New Tour</h1>
          <p>Design your tour and add key points</p>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit} className="tour-form">
          <div className="form-group">
            <label>Tour Name *</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g., Historic Belgrade Walking Tour"
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label>Description *</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe your tour..."
              rows="5"
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label>Difficulty *</label>
            <select 
              value={difficulty} 
              onChange={(e) => setDifficulty(e.target.value)}
              disabled={loading}
            >
              <option value="easy">Easy</option>
              <option value="medium">Medium</option>
              <option value="hard">Hard</option>
            </select>
          </div>

          <div className="form-group">
            <label>Tags (comma-separated)</label>
            <input
              type="text"
              value={tags}
              onChange={(e) => setTags(e.target.value)}
              placeholder="e.g., history, walking, culture"
              disabled={loading}
            />
            <small>Separate tags with commas</small>
          </div>

          <div className="form-actions">
            <button 
              type="button" 
              onClick={() => navigate('/my-tours')}
              className="btn-secondary"
              disabled={loading}
            >
              Cancel
            </button>
            <button 
              type="submit" 
              className="btn-primary"
              disabled={loading}
            >
              {loading ? 'Creating...' : 'Create Tour'}
            </button>
          </div>
        </form>

        <div className="info-box">
          <h3>📝 Next Steps</h3>
          <p>After creating your tour:</p>
          <ul>
            <li>Add key points (locations) to your tour</li>
            <li>Set a price and publish it</li>
            <li>Tourists will be able to purchase and explore your tour!</li>
          </ul>
        </div>
      </div>
    </Layout>
  );
}

export default CreateTour;