// Admin Users JavaScript functionality
let allUsers = [];
let filteredUsers = [];

// API Base URL
const API_BASE = '';

// Initialize page when document is ready
document.addEventListener('DOMContentLoaded', function() {
    loadUserStats();
    loadAllUsers();
});

// Load user statistics
async function loadUserStats() {
    try {
        const response = await fetch(`${API_BASE}/admin/users/stats`);
        const result = await response.json();
        
        if (result.success) {
            document.getElementById('totalUsers').textContent = result.data.totalUsers;
            document.getElementById('activeUsers').textContent = result.data.activeUsers;
            document.getElementById('inactiveUsers').textContent = result.data.inactiveUsers;
            // For online users, we'll just show active users for now
            document.getElementById('onlineNow').textContent = result.data.activeUsers;
        }
    } catch (error) {
        console.error('Error loading user stats:', error);
        showNotification('Không thể tải thống kê người dùng', 'error');
    }
}

// Load all users
async function loadAllUsers() {
    showLoading(true);
    try {
        const response = await fetch(`${API_BASE}/admin/users`);
        const result = await response.json();
        
        if (result.success) {
            allUsers = result.data;
            filteredUsers = [...allUsers];
            renderUsersTable();
        } else {
            showNotification(result.message || 'Không thể tải danh sách người dùng', 'error');
        }
    } catch (error) {
        console.error('Error loading users:', error);
        showNotification('Có lỗi xảy ra khi tải dữ liệu', 'error');
    } finally {
        showLoading(false);
    }
}

// Render users table
function renderUsersTable() {
    const tbody = document.getElementById('usersTableBody');
    tbody.innerHTML = '';
    
    if (filteredUsers.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="10" class="text-center">Không có dữ liệu</td>
            </tr>
        `;
        return;
    }
    
    filteredUsers.forEach(user => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${user.id}</td>
            <td>${user.username}</td>
            <td>${user.email}</td>
            <td>${user.displayName}</td>
            <td>
                <span class="badge ${user.role === 'ADMIN' ? 'bg-danger' : 'bg-primary'}">
                    ${user.role === 'ADMIN' ? 'Admin' : 'Player'}
                </span>
            </td>
            <td class="text-end">${user.money.toLocaleString()}</td>
            <td>
                <div class="progress" style="width: 60px;">
                    <div class="progress-bar ${getHealthColor(user.health)}" 
                         role="progressbar" style="width: ${user.health}%">
                        ${user.health}%
                    </div>
                </div>
            </td>
            <td>
                <span class="badge ${user.isActive ? 'bg-success' : 'bg-secondary'}">
                    ${user.isActive ? 'Hoạt động' : 'Không hoạt động'}
                </span>
            </td>
            <td>${formatDate(user.lastLoginAt)}</td>
            <td>
                <div class="btn-group btn-group-sm">
                    <button class="btn btn-outline-primary" onclick="editUser(${user.id})" title="Chỉnh sửa">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-info" onclick="viewUser(${user.id})" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button>
                    <button class="btn btn-outline-${user.isActive ? 'warning' : 'success'}" 
                            onclick="toggleUserStatus(${user.id}, ${!user.isActive})" 
                            title="${user.isActive ? 'Vô hiệu hóa' : 'Kích hoạt'}">
                        <i class="fas fa-${user.isActive ? 'ban' : 'check'}"></i>
                    </button>
                    <button class="btn btn-outline-danger" onclick="deleteUser(${user.id})" title="Xóa">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </td>
        `;
        tbody.appendChild(row);
    });
}

// Search users
function searchUsers() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    filterUsers();
}

// Filter users
function filterUsers() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const roleFilter = document.getElementById('roleFilter').value;
    const statusFilter = document.getElementById('statusFilter').value;
    
    filteredUsers = allUsers.filter(user => {
        const matchesSearch = !searchTerm || 
            user.username.toLowerCase().includes(searchTerm) ||
            user.email.toLowerCase().includes(searchTerm) ||
            user.displayName.toLowerCase().includes(searchTerm);
            
        const matchesRole = !roleFilter || user.role === roleFilter;
        const matchesStatus = statusFilter === '' || user.isActive.toString() === statusFilter;
        
        return matchesSearch && matchesRole && matchesStatus;
    });
    
    renderUsersTable();
}

// Edit user
async function editUser(userId) {
    try {
        const response = await fetch(`${API_BASE}/admin/users/${userId}`);
        const result = await response.json();
        
        if (result.success) {
            const user = result.data;
            
            // Fill form
            document.getElementById('editUserId').value = user.id;
            document.getElementById('editDisplayName').value = user.displayName;
            document.getElementById('editEmail').value = user.email;
            document.getElementById('editRole').value = user.role;
            document.getElementById('editIsActive').value = user.isActive.toString();
            
            if (user.playerState) {
                document.getElementById('editMoney').value = user.playerState.money;
                document.getElementById('editHealth').value = user.playerState.health;
                document.getElementById('editHunger').value = user.playerState.hunger;
            }
            
            // Show modal
            const modal = new bootstrap.Modal(document.getElementById('editUserModal'));
            modal.show();
        }
    } catch (error) {
        console.error('Error loading user details:', error);
        showNotification('Không thể tải thông tin người dùng', 'error');
    }
}

// Save user changes
async function saveUser() {
    const userId = document.getElementById('editUserId').value;
    const userUpdateData = {
        displayName: document.getElementById('editDisplayName').value,
        email: document.getElementById('editEmail').value,
        role: document.getElementById('editRole').value,
        isActive: document.getElementById('editIsActive').value === 'true'
    };
    
    const playerStateData = {
        money: parseInt(document.getElementById('editMoney').value) || 0,
        health: parseFloat(document.getElementById('editHealth').value) || 0,
        hunger: parseFloat(document.getElementById('editHunger').value) || 0
    };
    
    try {
        // Update user info
        const userResponse = await fetch(`${API_BASE}/admin/users/${userId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userUpdateData)
        });
        
        // Update player state
        const stateResponse = await fetch(`${API_BASE}/admin/users/${userId}/player-state`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(playerStateData)
        });
        
        if (userResponse.ok && stateResponse.ok) {
            showNotification('Cập nhật người dùng thành công', 'success');
            
            // Hide modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            modal.hide();
            
            // Reload data
            loadAllUsers();
            loadUserStats();
        } else {
            showNotification('Có lỗi xảy ra khi cập nhật', 'error');
        }
    } catch (error) {
        console.error('Error saving user:', error);
        showNotification('Có lỗi xảy ra khi lưu dữ liệu', 'error');
    }
}

// Toggle user status
async function toggleUserStatus(userId, activate) {
    const endpoint = activate ? 'activate' : 'deactivate';
    try {
        const response = await fetch(`${API_BASE}/admin/users/${userId}/${endpoint}`, {
            method: 'POST'
        });
        
        const result = await response.json();
        if (result.success) {
            showNotification(result.message, 'success');
            loadAllUsers();
            loadUserStats();
        } else {
            showNotification(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error toggling user status:', error);
        showNotification('Có lỗi xảy ra', 'error');
    }
}

// Delete user
async function deleteUser(userId) {
    if (!confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE}/admin/users/${userId}`, {
            method: 'DELETE'
        });
        
        const result = await response.json();
        if (result.success) {
            showNotification('Xóa người dùng thành công', 'success');
            loadAllUsers();
            loadUserStats();
        } else {
            showNotification(result.message || 'Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        console.error('Error deleting user:', error);
        showNotification('Có lỗi xảy ra', 'error');
    }
}

// View user details
function viewUser(userId) {
    window.location.href = `/admin/users/${userId}`;
}

// Refresh users
function refreshUsers() {
    loadAllUsers();
    loadUserStats();
}

// Export users (placeholder)
function exportUsers() {
    showNotification('Tính năng xuất Excel đang được phát triển', 'info');
}

// Show/hide loading indicator
function showLoading(show) {
    const indicator = document.getElementById('loadingIndicator');
    indicator.style.display = show ? 'block' : 'none';
}

// Helper functions
function getHealthColor(health) {
    if (health >= 75) return 'bg-success';
    if (health >= 50) return 'bg-warning';
    if (health >= 25) return 'bg-orange';
    return 'bg-danger';
}

function formatDate(dateString) {
    if (!dateString) return 'Chưa đăng nhập';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', {hour: '2-digit', minute: '2-digit'});
}

function showNotification(message, type) {
    // Simple notification - you can replace with a better notification library
    const alertClass = type === 'success' ? 'alert-success' : 
                      type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' : 'alert-info';
    
    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 5000);
} 