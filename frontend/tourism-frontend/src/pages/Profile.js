import React, { useState, useEffect, useCallback } from 'react';
import Layout from '../components/Layout';
import { authAPI } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Profile.css';

function Profile() {
  const { user } = useAuth();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  
  // Edit form state
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [profilePicture, setProfilePicture] = useState('');
  const [bio, setBio] = useState('');
  const [motto, setMotto] = useState('');

  const fetchProfile = useCallback(async () => {
    try {
      const response = await authAPI.getProfile(user.userId);
      if (response.data.success) {
        const profileData = response.data.profile;
        setProfile(profileData);
        // Set form values
        setFirstName(profileData.firstName || '');
        setLastName(profileData.lastName || '');
        setProfilePicture(profileData.profilePicture || '');
        setBio(profileData.bio || '');
        setMotto(profileData.motto || '');
      }
    } catch (err) {
      console.error('Failed to load profile:', err);
    } finally {
      setLoading(false);
    }
  }, [user.userId]);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);

    try {
      const response = await authAPI.updateProfile(user.userId, {
        firstName,
        lastName,
        profilePicture,
        bio,
        motto
      });

      if (response.data.success) {
        alert('Profile updated successfully!');
        setProfile(response.data.profile);
        setEditing(false);
      } else {
        alert(response.data.message);
      }
    } catch (err) {
      alert('Failed to update profile');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    // Reset form to current profile values
    setFirstName(profile.firstName || '');
    setLastName(profile.lastName || '');
    setProfilePicture(profile.profilePicture || '');
    setBio(profile.bio || '');
    setMotto(profile.motto || '');
    setEditing(false);
  };

  if (loading) return <Layout><div className="loading">Loading profile...</div></Layout>;
  if (!profile) return <Layout><div className="error">Profile not found</div></Layout>;

  return (
    <Layout>
      <div className="profile-page">
        <div className="page-header">
          <h1>My Profile</h1>
          {!editing && (
            <button onClick={() => setEditing(true)} className="btn-primary">
              Edit Profile
            </button>
          )}
        </div>

        <div className="profile-container">
          {/* Profile Picture */}
          <div className="profile-picture-section">
            {profile.profilePicture ? (
              <img src={profile.profilePicture} alt={profile.username} className="profile-picture" />
            ) : (
              <div className="profile-picture-placeholder">
                {profile.username.charAt(0).toUpperCase()}
              </div>
            )}
          </div>

          {/* Profile Info */}
          {!editing ? (
            // View Mode
            <div className="profile-info">
              <div className="info-section">
                <h2>{profile.firstName && profile.lastName 
                  ? `${profile.firstName} ${profile.lastName}` 
                  : profile.username}
                </h2>
                <p className="username-display">@{profile.username}</p>
                <span className={`role-badge ${profile.role}`}>{profile.role}</span>
              </div>

              <div className="info-section">
                <h3>Contact Information</h3>
                <div className="info-item">
                  <span className="label">Email:</span>
                  <span className="value">{profile.email}</span>
                </div>
              </div>

              {profile.motto && (
                <div className="info-section motto-section">
                  <h3>Motto</h3>
                  <p className="motto">"{profile.motto}"</p>
                </div>
              )}

              {profile.bio && (
                <div className="info-section">
                  <h3>Biography</h3>
                  <p className="bio">{profile.bio}</p>
                </div>
              )}

              {(!profile.firstName && !profile.lastName && !profile.bio && !profile.motto) && (
                <div className="empty-profile">
                  <p>Your profile is incomplete. Click "Edit Profile" to add more information!</p>
                </div>
              )}
            </div>
          ) : (
            // Edit Mode
            <div className="profile-form">
              <form onSubmit={handleSave}>
                <div className="form-row">
                  <div className="form-group">
                    <label>First Name</label>
                    <input
                      type="text"
                      value={firstName}
                      onChange={(e) => setFirstName(e.target.value)}
                      placeholder="John"
                      disabled={saving}
                    />
                  </div>
                  
                  <div className="form-group">
                    <label>Last Name</label>
                    <input
                      type="text"
                      value={lastName}
                      onChange={(e) => setLastName(e.target.value)}
                      placeholder="Doe"
                      disabled={saving}
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Profile Picture URL</label>
                  <input
                    type="text"
                    value={profilePicture}
                    onChange={(e) => setProfilePicture(e.target.value)}
                    placeholder="https://example.com/your-photo.jpg"
                    disabled={saving}
                  />
                </div>

                <div className="form-group">
                  <label>Motto</label>
                  <input
                    type="text"
                    value={motto}
                    onChange={(e) => setMotto(e.target.value)}
                    placeholder="Your personal motto or quote"
                    disabled={saving}
                  />
                </div>

                <div className="form-group">
                  <label>Biography</label>
                  <textarea
                    value={bio}
                    onChange={(e) => setBio(e.target.value)}
                    placeholder="Tell us about yourself..."
                    rows="5"
                    disabled={saving}
                  />
                </div>

                <div className="form-actions">
                  <button 
                    type="button" 
                    onClick={handleCancel}
                    className="btn-secondary"
                    disabled={saving}
                  >
                    Cancel
                  </button>
                  <button 
                    type="submit" 
                    className="btn-primary"
                    disabled={saving}
                  >
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </form>
            </div>
          )}
        </div>

        <div className="account-info">
          <h3>Account Information</h3>
          <div className="info-grid">
            <div className="info-item">
              <span className="label">User ID:</span>
              <span className="value">{profile.userId}</span>
            </div>
            <div className="info-item">
              <span className="label">Username:</span>
              <span className="value">{profile.username}</span>
            </div>
            <div className="info-item">
              <span className="label">Role:</span>
              <span className="value">{profile.role}</span>
            </div>
            <div className="info-item">
              <span className="label">Status:</span>
              <span className={`status ${profile.isBlocked ? 'blocked' : 'active'}`}>
                {profile.isBlocked ? 'Blocked' : 'Active'}
              </span>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}

export default Profile;