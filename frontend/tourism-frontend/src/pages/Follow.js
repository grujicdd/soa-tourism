import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { blogAPI, authAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Follow.css';

function Follow() {
  const [following, setFollowing] = useState([]);
  const [followers, setFollowers] = useState([]);
  const [recommendations, setRecommendations] = useState([]);
  const [searchUsername, setSearchUsername] = useState('');
  const [searchResult, setSearchResult] = useState(null);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();

  const fetchFollowData = useCallback(async () => {
    try {
      // Get who I'm following
      const followingRes = await blogAPI.getFollowing(user.userId);
      if (followingRes.data.success) {
        setFollowing(followingRes.data.users);
      }

      // Get my followers
      const followersRes = await blogAPI.getFollowers(user.userId);
      if (followersRes.data.success) {
        setFollowers(followersRes.data.users);
      }

      // Get recommendations
      const recsRes = await blogAPI.getRecommendations(user.userId, 5);
      if (recsRes.data.success) {
        setRecommendations(recsRes.data.recommendations);
      }
    } catch (err) {
      console.error('Failed to load follow data:', err);
    } finally {
      setLoading(false);
    }
  }, [user.userId]);

  useEffect(() => {
    fetchFollowData();
  }, [fetchFollowData]);

  const handleFollow = async (followeeId, followeeUsername) => {
    try {
      const response = await blogAPI.followUser(user.userId, {
        followerUsername: user.username,
        followeeId: followeeId,
        followeeUsername: followeeUsername
      });

      if (response.data.success) {
        alert(`You are now following ${followeeUsername}!`);
        // Refresh data
        fetchFollowData();
        setSearchResult(null);
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to follow user');
      console.error(err);
    }
  };

  const handleUnfollow = async (followeeId) => {
    try {
      const response = await blogAPI.unfollowUser(user.userId, followeeId);

      if (response.data.success) {
        alert('Unfollowed successfully');
        // Refresh data
        fetchFollowData();
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to unfollow user');
      console.error(err);
    }
  };

  const handleSearch = async (e) => {
    e.preventDefault();
    if (!searchUsername.trim()) return;

    try {
      // We don't have a search endpoint, so we'll need to get the user by username
      // This is a workaround - ideally we'd have a proper search endpoint
      alert('Search by username not implemented. Use recommendations or know the exact user ID.');
      // For now, users can only follow via recommendations
    } catch (err) {
      console.error('Search failed:', err);
    }
  };

  if (loading) return <Layout><div className="loading">Loading...</div></Layout>;

  return (
    <Layout>
      <div className="follow-page">
        <div className="page-header">
          <h1>Follow System</h1>
          <p>Connect with other travelers and guides</p>
        </div>

        {/* Stats */}
        <div className="follow-stats">
          <div className="stat-card">
            <div className="stat-number">{following.length}</div>
            <div className="stat-label">Following</div>
          </div>
          <div className="stat-card">
            <div className="stat-number">{followers.length}</div>
            <div className="stat-label">Followers</div>
          </div>
        </div>

        {/* Recommendations */}
        {recommendations.length > 0 && (
          <div className="section">
            <h2>💡 Recommended Users</h2>
            <p className="section-subtitle">Based on your network (friends of friends)</p>
            <div className="users-list">
              {recommendations.map((rec) => (
                <div key={rec.userId} className="user-card">
                  <div className="user-avatar">{rec.username.charAt(0).toUpperCase()}</div>
                  <div className="user-info">
                    <h3>{rec.username}</h3>
                    <p className="user-id">ID: {rec.userId}</p>
                  </div>
                  <button 
                    onClick={() => handleFollow(rec.userId, rec.username)}
                    className="btn-follow"
                  >
                    Follow
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Following List */}
        <div className="section">
          <h2>👥 Following ({following.length})</h2>
          {following.length === 0 ? (
            <div className="empty-state">
              <p>You're not following anyone yet. Check out the recommendations above!</p>
            </div>
          ) : (
            <div className="users-list">
              {following.map((user) => (
                <div key={user.userId} className="user-card">
                  <div className="user-avatar">{user.username.charAt(0).toUpperCase()}</div>
                  <div className="user-info">
                    <h3>{user.username}</h3>
                    <p className="user-id">ID: {user.userId}</p>
                  </div>
                  <button 
                    onClick={() => handleUnfollow(user.userId)}
                    className="btn-unfollow"
                  >
                    Unfollow
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Followers List */}
        <div className="section">
          <h2>❤️ Followers ({followers.length})</h2>
          {followers.length === 0 ? (
            <div className="empty-state">
              <p>No followers yet. Keep posting blogs to attract followers!</p>
            </div>
          ) : (
            <div className="users-list">
              {followers.map((user) => (
                <div key={user.userId} className="user-card">
                  <div className="user-avatar">{user.username.charAt(0).toUpperCase()}</div>
                  <div className="user-info">
                    <h3>{user.username}</h3>
                    <p className="user-id">ID: {user.userId}</p>
                  </div>
                  {!following.some(f => f.userId === user.userId) && (
                    <button 
                      onClick={() => handleFollow(user.userId, user.username)}
                      className="btn-follow"
                    >
                      Follow Back
                    </button>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="info-box">
          <h3>ℹ️ How it works</h3>
          <ul>
            <li>Follow other travelers and guides to see their content</li>
            <li>Recommendations are based on mutual connections (friends of friends)</li>
            <li>You can only comment on blogs from users you follow</li>
            <li>Build your network to discover more tours and experiences!</li>
          </ul>
        </div>
      </div>
    </Layout>
  );
}

export default Follow;