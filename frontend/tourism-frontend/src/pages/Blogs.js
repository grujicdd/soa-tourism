import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { blogAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Blogs.css';

function Blogs() {
  const [blogs, setBlogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [showCreateForm, setShowCreateForm] = useState(false);
  
  // Form state
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [imageUrls, setImageUrls] = useState('');
  
  const { user } = useAuth();

  const fetchBlogs = useCallback(async () => {
    try {
      const response = await blogAPI.getBlogs(user.userId);
      if (response.data.success) {
        setBlogs(response.data.blogs);
      }
    } catch (err) {
      console.error('Failed to load blogs:', err);
    } finally {
      setLoading(false);
    }
  }, [user.userId]);

  useEffect(() => {
    fetchBlogs();
  }, [fetchBlogs]);

  const handleCreateBlog = async (e) => {
    e.preventDefault();
    setCreating(true);

    try {
      const images = imageUrls.split('\n').filter(url => url.trim());
      
      const response = await blogAPI.createBlog({
        userId: user.userId,
        username: user.username,
        title,
        description: content,
        images
      });

      if (response.data.success) {
        alert('Blog created successfully!');
        setTitle('');
        setContent('');
        setImageUrls('');
        setShowCreateForm(false);
        fetchBlogs(); // Refresh list
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to create blog');
      console.error(err);
    } finally {
      setCreating(false);
    }
  };

  if (loading) return <Layout><div className="loading">Loading blogs...</div></Layout>;

  return (
    <Layout>
      <div className="blogs-page">
        <div className="page-header">
          <h1>Blogs</h1>
          <button 
            onClick={() => setShowCreateForm(!showCreateForm)}
            className="btn-primary"
          >
            {showCreateForm ? 'Cancel' : '+ Create Blog'}
          </button>
        </div>

        {showCreateForm && (
          <div className="create-blog-card">
            <h2>Create New Blog</h2>
            <form onSubmit={handleCreateBlog}>
              <div className="form-group">
                <label>Title *</label>
                <input
                  type="text"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="Enter blog title"
                  required
                  disabled={creating}
                />
              </div>

              <div className="form-group">
                <label>Content *</label>
                <textarea
                  value={content}
                  onChange={(e) => setContent(e.target.value)}
                  placeholder="Write your blog content..."
                  rows="8"
                  required
                  disabled={creating}
                />
              </div>

              <div className="form-group">
                <label>Image URLs (optional, one per line)</label>
                <textarea
                  value={imageUrls}
                  onChange={(e) => setImageUrls(e.target.value)}
                  placeholder="https://example.com/image1.jpg&#10;https://example.com/image2.jpg"
                  rows="3"
                  disabled={creating}
                />
              </div>

              <button type="submit" className="btn-submit" disabled={creating}>
                {creating ? 'Creating...' : 'Create Blog'}
              </button>
            </form>
          </div>
        )}

        <div className="blogs-list">
          {blogs.length === 0 ? (
            <div className="empty-state">
              <p>No blogs yet. Create your first blog to share your travel experiences!</p>
            </div>
          ) : (
            blogs.map((blog) => (
              <div key={blog.id} className="blog-card">
                <div className="blog-header">
                  <div className="author-info">
                    <div className="author-avatar">
                      {blog.username.charAt(0).toUpperCase()}
                    </div>
                    <div>
                      <h3 className="author-name">{blog.username}</h3>
                      <p className="blog-date">
                        {new Date(blog.createdAt).toLocaleDateString('en-US', {
                          year: 'numeric',
                          month: 'long',
                          day: 'numeric'
                        })}
                      </p>
                    </div>
                  </div>
                </div>

                <h2 className="blog-title">{blog.title}</h2>
                <p className="blog-content">{blog.description}</p>

                {blog.images && blog.images.length > 0 && (
                  <div className="blog-images">
                    {blog.images.map((img, index) => (
                      <img key={index} src={img} alt={`Blog ${index + 1}`} />
                    ))}
                  </div>
                )}

                <div className="blog-footer">
                  <div className="blog-stats">
                    <span>❤️ {blog.likeCount} likes</span>
                    <span>💬 {blog.commentCount} comments</span>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </Layout>
  );
}

export default Blogs;