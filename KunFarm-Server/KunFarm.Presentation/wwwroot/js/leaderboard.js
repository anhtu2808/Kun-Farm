// Leaderboard JavaScript functionality
let currentLeaderboard = 'money';

// API Base URL
const API_BASE = '';

// Initialize page when document is ready
document.addEventListener('DOMContentLoaded', function() {
    // Load initial data
    loadMoneyLeaderboard();
    
    // Set up tab change handlers
    document.querySelectorAll('#leaderboardTabs button').forEach(tab => {
        tab.addEventListener('shown.bs.tab', function(event) {
            const target = event.target.getAttribute('data-bs-target').substring(1);
            currentLeaderboard = target;
            
            switch(target) {
                case 'money':
                    loadMoneyLeaderboard();
                    break;
                case 'health':
                    loadHealthLeaderboard();
                    break;
                case 'hunger':
                    loadHungerLeaderboard();
                    break;
            }
        });
    });
});

// Load money leaderboard
async function loadMoneyLeaderboard() {
    showLoading('money', true);
    try {
        const response = await fetch(`${API_BASE}/leaderboard/money?top=100`);
        const result = await response.json();
        
        if (result.code === 200) {
            renderLeaderboard('money', result.data.rankings);
            updateStatistics(result.data);
        } else {
            showError('money', result.message || 'Không thể tải bảng xếp hạng');
        }
    } catch (error) {
        console.error('Error loading money leaderboard:', error);
        showError('money', 'Có lỗi xảy ra khi tải dữ liệu');
    } finally {
        showLoading('money', false);
    }
}

// Load health leaderboard
async function loadHealthLeaderboard() {
    showLoading('health', true);
    try {
        const response = await fetch(`${API_BASE}/leaderboard/health?top=100`);
        const result = await response.json();
        
        if (result.code === 200) {
            renderLeaderboard('health', result.data.rankings);
            updateStatistics(result.data);
        } else {
            showError('health', result.message || 'Không thể tải bảng xếp hạng');
        }
    } catch (error) {
        console.error('Error loading health leaderboard:', error);
        showError('health', 'Có lỗi xảy ra khi tải dữ liệu');
    } finally {
        showLoading('health', false);
    }
}

// Load hunger leaderboard
async function loadHungerLeaderboard() {
    showLoading('hunger', true);
    try {
        const response = await fetch(`${API_BASE}/leaderboard/hunger?top=100`);
        const result = await response.json();
        
        if (result.code === 200) {
            renderLeaderboard('hunger', result.data.rankings);
            updateStatistics(result.data);
        } else {
            showError('hunger', result.message || 'Không thể tải bảng xếp hạng');
        }
    } catch (error) {
        console.error('Error loading hunger leaderboard:', error);
        showError('hunger', 'Có lỗi xảy ra khi tải dữ liệu');
    } finally {
        showLoading('hunger', false);
    }
}

// Render leaderboard table
function renderLeaderboard(type, rankings) {
    const tbody = document.getElementById(`${type}LeaderboardBody`);
    tbody.innerHTML = '';
    
    if (!rankings || rankings.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="px-4 py-8 text-center text-gray-500">
                    <i class="fas fa-inbox text-4xl mb-4 block text-gray-300"></i>
                    <p class="text-lg font-semibold">Không có dữ liệu</p>
                    <p class="text-sm">Chưa có người chơi nào trong bảng xếp hạng</p>
                </td>
            </tr>
        `;
        return;
    }
    
    rankings.forEach((player, index) => {
        const row = document.createElement('tr');
        row.className = 'hover:bg-gray-50 transition-colors duration-200 cursor-pointer';
        
        let primaryStat, secondaryStat;
        switch(type) {
            case 'money':
                primaryStat = formatMoney(player.money);
                secondaryStat = `${player.health}%`;
                break;
            case 'health':
                primaryStat = `${player.health}%`;
                secondaryStat = formatMoney(player.money);
                break;
            case 'hunger':
                primaryStat = `${player.hunger.toFixed(1)}%`;
                secondaryStat = `${player.health}%`;
                break;
        }
        
        row.innerHTML = `
            <td class="px-4 py-3">
                <span class="inline-flex items-center px-3 py-1 rounded-full text-sm font-semibold ${getRankBadgeClass(player.rank)}">
                    ${getRankIcon(player.rank)} ${player.rank}
                </span>
            </td>
            <td class="px-4 py-3">
                <div class="flex items-center">
                    <div class="w-8 h-8 bg-gradient-to-br from-farm-green to-farm-blue rounded-full flex items-center justify-center text-white font-bold text-sm mr-3">
                        ${player.username.charAt(0).toUpperCase()}
                    </div>
                    <div>
                        <div class="font-semibold text-gray-900">${player.username}</div>
                        ${!player.isActive ? '<span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800 mt-1">Offline</span>' : '<span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800 mt-1">Online</span>'}
                    </div>
                </div>
            </td>
            <td class="px-4 py-3 hidden sm:table-cell">
                <span class="text-gray-700">${player.displayName}</span>
            </td>
            <td class="px-4 py-3 text-right">
                <span class="text-lg font-bold ${getStatColor(type)}">${primaryStat}</span>
            </td>
            <td class="px-4 py-3 text-right hidden md:table-cell">
                <span class="text-gray-600">${secondaryStat}</span>
            </td>
            <td class="px-4 py-3 text-right hidden lg:table-cell">
                <span class="text-sm text-gray-500">${formatDate(player.lastSaved)}</span>
            </td>
        `;
        
        // Add click handler for player details
        row.addEventListener('click', () => showPlayerDetails(player));
        
        tbody.appendChild(row);
    });
}

// Show player details modal
async function showPlayerDetails(player) {
    const modalContent = document.getElementById('playerDetailContent');
    
    modalContent.innerHTML = `
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-4">
                <h4 class="text-lg font-bold text-gray-900 flex items-center">
                    <i class="fas fa-user text-farm-blue mr-2"></i>
                    Thông tin cơ bản
                </h4>
                <div class="space-y-3">
                    <div class="flex justify-between">
                        <span class="font-semibold text-gray-700">Tên đăng nhập:</span>
                        <span class="text-gray-900">${player.username}</span>
                    </div>
                    <div class="flex justify-between">
                        <span class="font-semibold text-gray-700">Tên hiển thị:</span>
                        <span class="text-gray-900">${player.displayName}</span>
                    </div>
                    <div class="flex justify-between">
                        <span class="font-semibold text-gray-700">Trạng thái:</span>
                        <span class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${player.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}">
                            <i class="fas fa-circle text-xs mr-1 ${player.isActive ? 'text-green-400' : 'text-gray-400'}"></i>
                            ${player.isActive ? 'Đang hoạt động' : 'Không hoạt động'}
                        </span>
                    </div>
                    <div class="flex justify-between">
                        <span class="font-semibold text-gray-700">Lần cuối chơi:</span>
                        <span class="text-gray-900">${formatDate(player.lastSaved)}</span>
                    </div>
                </div>
            </div>
            
            <div class="space-y-4">
                <h4 class="text-lg font-bold text-gray-900 flex items-center">
                    <i class="fas fa-gamepad text-farm-green mr-2"></i>
                    Thống kê game
                </h4>
                <div class="space-y-4">
                    <div>
                        <div class="flex justify-between items-center mb-2">
                            <span class="font-semibold text-gray-700">Số tiền:</span>
                            <span class="text-xl font-bold text-farm-yellow">${formatMoney(player.money)}</span>
                        </div>
                    </div>
                    
                    <div>
                        <div class="flex justify-between items-center mb-2">
                            <span class="font-semibold text-gray-700">Sức khỏe:</span>
                            <span class="text-lg font-semibold">${player.health}%</span>
                        </div>
                        <div class="w-full bg-gray-200 rounded-full h-3">
                            <div class="h-3 rounded-full transition-all duration-300 ${getHealthColor(player.health)}" 
                                 style="width: ${player.health}%"></div>
                        </div>
                    </div>
                    
                    <div>
                        <div class="flex justify-between items-center mb-2">
                            <span class="font-semibold text-gray-700">Độ đói:</span>
                            <span class="text-lg font-semibold">${player.hunger.toFixed(1)}%</span>
                        </div>
                        <div class="w-full bg-gray-200 rounded-full h-3">
                            <div class="h-3 rounded-full transition-all duration-300 ${getHungerColor(player.hunger)}" 
                                 style="width: ${player.hunger}%"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <div class="mt-6 pt-6 border-t border-gray-200">
            <h4 class="text-lg font-bold text-gray-900 flex items-center mb-4">
                <i class="fas fa-trophy text-farm-yellow mr-2"></i>
                Xếp hạng
            </h4>
            <div id="playerRanks">
                <div class="flex items-center justify-center py-8">
                    <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-farm-blue" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    <span class="text-farm-blue font-semibold">Đang tải thông tin xếp hạng...</span>
                </div>
            </div>
        </div>
    `;
    
    // Show modal
    document.getElementById('playerDetailModal').classList.remove('hidden');
    
    // Load player ranks
    loadPlayerRanks(player.userId);
}

// Load player ranks
async function loadPlayerRanks(userId) {
    try {
        const response = await fetch(`${API_BASE}/leaderboard/player/${userId}/ranks`);
        const result = await response.json();
        
        const ranksDiv = document.getElementById('playerRanks');
        
        if (result.code === 200) {
            const ranks = result.data;
            ranksDiv.innerHTML = `
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div class="bg-gradient-to-br from-yellow-50 to-yellow-100 rounded-xl p-4 text-center border border-yellow-200">
                        <div class="bg-farm-yellow p-3 rounded-full w-12 h-12 mx-auto mb-3 flex items-center justify-center">
                            <i class="fas fa-coins text-gray-900 text-xl"></i>
                        </div>
                        <h6 class="font-semibold text-gray-700 mb-2">Xếp hạng tiền</h6>
                        <div class="text-2xl font-bold text-farm-yellow">#${ranks.moneyRank || 'N/A'}</div>
                    </div>
                    <div class="bg-gradient-to-br from-red-50 to-red-100 rounded-xl p-4 text-center border border-red-200">
                        <div class="bg-red-500 p-3 rounded-full w-12 h-12 mx-auto mb-3 flex items-center justify-center">
                            <i class="fas fa-heart text-white text-xl"></i>
                        </div>
                        <h6 class="font-semibold text-gray-700 mb-2">Xếp hạng sức khỏe</h6>
                        <div class="text-2xl font-bold text-red-500">#${ranks.healthRank || 'N/A'}</div>
                    </div>
                    <div class="bg-gradient-to-br from-green-50 to-green-100 rounded-xl p-4 text-center border border-green-200">
                        <div class="bg-farm-green p-3 rounded-full w-12 h-12 mx-auto mb-3 flex items-center justify-center">
                            <i class="fas fa-drumstick-bite text-white text-xl"></i>
                        </div>
                        <h6 class="font-semibold text-gray-700 mb-2">Xếp hạng độ đói</h6>
                        <div class="text-2xl font-bold text-farm-green">#${ranks.hungerRank || 'N/A'}</div>
                    </div>
                </div>
            `;
        } else {
            ranksDiv.innerHTML = `
                <div class="bg-yellow-50 border border-yellow-200 rounded-xl p-4 text-center">
                    <i class="fas fa-exclamation-triangle text-yellow-500 text-2xl mb-2"></i>
                    <p class="text-yellow-700 font-semibold">Không thể tải thông tin xếp hạng</p>
                    <p class="text-yellow-600 text-sm">${result.message || 'Vui lòng thử lại sau'}</p>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading player ranks:', error);
        document.getElementById('playerRanks').innerHTML = `
            <div class="bg-red-50 border border-red-200 rounded-xl p-4 text-center">
                <i class="fas fa-times-circle text-red-500 text-2xl mb-2"></i>
                <p class="text-red-700 font-semibold">Có lỗi xảy ra khi tải thông tin xếp hạng</p>
                <p class="text-red-600 text-sm">Vui lòng kiểm tra kết nối và thử lại</p>
                <button onclick="loadPlayerRanks(${userId})" class="mt-3 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors duration-200 text-sm">
                    <i class="fas fa-redo mr-1"></i>Thử lại
                </button>
            </div>
        `;
    }
}

// Update statistics
function updateStatistics(data) {
    document.getElementById('totalPlayersCount').textContent = data.totalPlayers;
    document.getElementById('activePlayersCount').textContent = data.rankings.filter(p => p.isActive).length;
    document.getElementById('lastUpdated').textContent = formatDate(data.generatedAt);
}

// Show/hide loading indicator
function showLoading(type, show) {
    const indicator = document.getElementById(`${type}LoadingIndicator`);
    if (indicator) {
        if (show) {
            indicator.classList.remove('hidden');
        } else {
            indicator.classList.add('hidden');
        }
    }
}

// Show error message
function showError(type, message) {
    const tbody = document.getElementById(`${type}LeaderboardBody`);
    tbody.innerHTML = `
        <tr>
            <td colspan="6" class="px-4 py-8 text-center">
                <div class="flex flex-col items-center">
                    <i class="fas fa-exclamation-triangle text-4xl text-red-400 mb-4"></i>
                    <p class="text-lg font-semibold text-red-600 mb-2">Có lỗi xảy ra</p>
                    <p class="text-sm text-gray-600">${message}</p>
                    <button onclick="location.reload()" class="mt-4 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors duration-200">
                        <i class="fas fa-redo mr-2"></i>Thử lại
                    </button>
                </div>
            </td>
        </tr>
    `;
}

// Helper functions
function getRankBadgeClass(rank) {
    if (rank === 1) return 'bg-gradient-to-r from-yellow-400 to-yellow-500 text-gray-900'; // Gold
    if (rank === 2) return 'bg-gradient-to-r from-gray-300 to-gray-400 text-gray-900'; // Silver
    if (rank === 3) return 'bg-gradient-to-r from-orange-400 to-orange-500 text-white'; // Bronze
    return 'bg-gradient-to-r from-farm-blue to-blue-600 text-white';
}

function getRankIcon(rank) {
    if (rank === 1) return '🥇';
    if (rank === 2) return '🥈';
    if (rank === 3) return '🥉';
    return '';
}

function getStatColor(type) {
    switch(type) {
        case 'money': return 'text-farm-yellow';
        case 'health': return 'text-red-500';
        case 'hunger': return 'text-farm-green';
        default: return 'text-gray-900';
    }
}

function getHealthColor(health) {
    if (health >= 75) return 'bg-green-500';
    if (health >= 50) return 'bg-yellow-500';
    if (health >= 25) return 'bg-orange-500';
    return 'bg-red-500';
}

function getHungerColor(hunger) {
    if (hunger >= 75) return 'bg-green-500';
    if (hunger >= 50) return 'bg-yellow-500';
    if (hunger >= 25) return 'bg-orange-500';
    return 'bg-red-500';
}

function formatMoney(amount) {
    return amount.toLocaleString('vi-VN') + ' 💰';
}

function formatDate(dateString) {
    if (!dateString) return 'Không rõ';
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    
    if (diffMins < 1) return 'Vừa xong';
    if (diffMins < 60) return `${diffMins} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays < 7) return `${diffDays} ngày trước`;
    
    return date.toLocaleDateString('vi-VN');
} 