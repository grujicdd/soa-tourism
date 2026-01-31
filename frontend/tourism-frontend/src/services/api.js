import axios from 'axios';

const API_BASE_URL = 'http://localhost:8080/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests if it exists
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Auth endpoints
export const authAPI = {
  register: (data) => api.post('/auth/register', data),
  login: (data) => api.post('/auth/login', data),
  getProfile: (userId) => api.get(`/auth/profile/${userId}`),
  updateProfile: (userId, data) => api.put(`/auth/profile/${userId}`, data),
};

// Tour endpoints
export const tourAPI = {
  createTour: (data) => api.post('/tour', data),
  getTours: (publishedOnly = true) => api.get(`/tour?publishedOnly=${publishedOnly}`),
  getMyTours: (guideId) => api.get(`/tour/my/${guideId}`),
  getTourById: (tourId) => api.get(`/tour/${tourId}`),
  publishTour: (tourId, data) => api.put(`/tour/${tourId}/publish`, data),
  addKeyPoint: (tourId, data) => api.post(`/tour/${tourId}/keypoints`, data),
  getKeyPoints: (tourId, userId) => api.get(`/tour/${tourId}/keypoints?userId=${userId}`),
  
  // Position
  updatePosition: (data) => api.post('/tour/position', data),
  getPosition: (touristId) => api.get(`/tour/position/${touristId}`),
  
  // Shopping cart
  addToCart: (touristId, tourId) => api.post(`/tour/cart/${touristId}/items`, JSON.stringify(tourId)),
  removeFromCart: (touristId, tourId) => api.delete(`/tour/cart/${touristId}/items/${tourId}`),
  getCart: (touristId) => api.get(`/tour/cart/${touristId}`),
  checkout: (touristId) => api.post(`/tour/cart/${touristId}/checkout`),
  
  // Tour execution
  startExecution: (tourId, data) => api.post(`/tour/${tourId}/execute`, data),
  checkProximity: (executionId, data) => api.post(`/tour/executions/${executionId}/proximity`, data),
  completeTour: (executionId, touristId) => api.post(`/tour/executions/${executionId}/complete`, JSON.stringify(touristId)),
  abandonTour: (executionId, touristId) => api.post(`/tour/executions/${executionId}/abandon`, JSON.stringify(touristId)),
};

// Blog endpoints
export const blogAPI = {
  createBlog: (data) => api.post('/blog', data),
  getBlogs: (userId, skip = 0, limit = 20) => 
    api.get(`/blog?userId=${userId || ''}&skip=${skip}&limit=${limit}`),
  getBlogById: (blogId) => api.get(`/blog/${blogId}`),
  
  // Comments
  addComment: (blogId, data) => api.post(`/blog/${blogId}/comments`, data),
  getComments: (blogId) => api.get(`/blog/${blogId}/comments`),
  
  // Likes
  likeBlog: (blogId, userId) => api.post(`/blog/${blogId}/like`, JSON.stringify(userId)),
  unlikeBlog: (blogId, userId) => api.delete(`/blog/${blogId}/like`, { data: JSON.stringify(userId) }),
  
  // Follow
  followUser: (followerId, data) => api.post(`/blog/follow/${followerId}`, data),
  unfollowUser: (followerId, followeeId) => api.delete(`/blog/unfollow/${followerId}/${followeeId}`),
  getFollowing: (userId) => api.get(`/blog/following/${userId}`),
  getFollowers: (userId) => api.get(`/blog/followers/${userId}`),
  getRecommendations: (userId, limit = 5) => api.get(`/blog/recommendations/${userId}?limit=${limit}`),
};

export default api;