import React, { useState, useCallback } from 'react';
import Layout from '../components/Layout';
import { blogAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Blogs.css';

function Blogs() {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [images, setImages] = useState('');
  const [loading, setLoading] = useState(false);
  const { user } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const imageArray = images.split(',').map(img => img.trim()).filter(img => img);
      
      const response = await blogAPI.createBlog({
        userId: user.userId,
        username: user.username,
        title,
        description,
        images: imageArray
      });

      if (response.data.success) {
        alert('Blog created successfully!');
        setTitle('');
        setDescription('');
        setImages('');
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to create blog');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="blogs-page">
        <div className="page-header">
          <h1>Blogs</h1>
          <p>Share your travel experiences</p>
        </div>

        <div className="create-blog-section">
          <h2>Create New Blog Post</h2>
          <form onSubmit={handleSubmit} className="blog-form">
            <div className="form-group">
              <label>Title *</label>
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Give your blog a title"
                required
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label>Content *</label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Write your blog post..."
                rows="8"
                required
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label>Image URLs (comma-separated)</label>
              <input
                type="text"
                value={images}
                onChange={(e) => setImages(e.target.value)}
                placeholder="https://example.com/image1.jpg, https://example.com/image2.jpg"
                disabled={loading}
              />
              <small>Optional: Add image URLs separated by commas</small>
            </div>

            <button type="submit" className="btn-primary" disabled={loading}>
              {loading ? 'Publishing...' : 'Publish Blog'}
            </button>
          </form>
        </div>

        <div className="info-box">
          <h3>✨ Coming Soon</h3>
          <ul>
            <li>View all blogs from users you follow</li>
            <li>Like and comment on blog posts</li>
            <li>Follow/unfollow other users</li>
            <li>Get recommendations for users to follow</li>
          </ul>
        </div>
      </div>
    </Layout>
  );
}

export default Blogs;