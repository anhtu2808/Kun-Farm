// Leaderboard JavaScript functionality
let currentLeaderboard = 'money';

// API Base URL
const API_BASE = '';

// Initialize page when document is ready
document.addEventListener('DOMContentLoaded', function() {
    // Load initial data
    loadMoneyLeaderboard();
});

// Load money leaderboard
function loadMoneyLeaderboard() {
    showLoading('money', true);
    
    fetch(`${API_BASE}/leaderboard/money?top=100`)
        .then(response => response.json())
        .then(data => {
            if (data.code === 200 && data.data) {
                renderLeaderboard('money', data.data.rankings);
                renderPodium('money', data.data.rankings.slice(0, 3));
                updateStatistics(data.data);
            } else {
                console.error('Failed to load money leaderboard:', data.message);
                showError('money', data.message || 'Failed to load leaderboard');
            }
        })
        .catch(error => {
            console.error('Error loading money leaderboard:', error);
            showError('money', 'Network error occurred');
        })
        .finally(() => {
            showLoading('money', false);
        });
}

// Render podium for top 3 players
function renderPodium(type, topPlayers) {
    const podiumContainer = document.getElementById(`${type}-podium`);
    if (!podiumContainer || !topPlayers || topPlayers.length === 0) return;
    
    podiumContainer.innerHTML = '';
    
    // Order for podium display: 2nd, 1st, 3rd
    const podiumOrder = [1, 0, 2]; // indices for 2nd, 1st, 3rd place
    const podiumClasses = [
        'md:order-1', // 2nd place (left)
        'md:order-2', // 1st place (center) 
        'md:order-3'  // 3rd place (right)
    ];
    
    podiumOrder.forEach((playerIndex, displayIndex) => {
        if (playerIndex < topPlayers.length) {
            const player = topPlayers[playerIndex];
            const rank = player.rank;
            
            let heightClass = 'h-32';
            let bgGradient = 'from-farm-blue to-blue-600';
            let icon = '';
            
            if (rank === 1) {
                heightClass = 'h-40';
                bgGradient = 'from-yellow-400 to-yellow-500';
                icon = '👑';
            } else if (rank === 2) {
                heightClass = 'h-36';
                bgGradient = 'from-gray-300 to-gray-400';
                icon = '🥈';
            } else if (rank === 3) {
                heightClass = 'h-32';
                bgGradient = 'from-orange-400 to-orange-500';
                icon = '🥉';
            }
            
            const podiumCard = document.createElement('div');
            podiumCard.className = `podium-card ${podiumClasses[displayIndex]} bg-gradient-to-br ${bgGradient} rounded-xl p-4 text-white shadow-lg relative overflow-hidden`;
            
            podiumCard.innerHTML = `
                <div class="absolute top-2 right-2 text-2xl">${icon}</div>
                <div class="text-center ${heightClass} flex flex-col justify-center">
                    <div class="w-16 h-16 bg-white bg-opacity-20 rounded-full flex items-center justify-center text-2xl font-bold mx-auto mb-2">
                        ${player.username.charAt(0).toUpperCase()}
                    </div>
                    <h3 class="font-bold text-lg mb-1">${player.username}</h3>
                    <p class="text-sm opacity-90 mb-2">${player.displayName}</p>
                    <p class="text-xl font-bold">💰 ${formatMoney(player.money)}</p>
                    <p class="text-xs opacity-75 mt-1">#${rank}</p>
                </div>
            `;
            
            // Add click handler
            podiumCard.addEventListener('click', () => showPlayerDetails(player));
            
            podiumContainer.appendChild(podiumCard);
        }
    });
}

// Render leaderboard table
function renderLeaderboard(type, rankings) {
    const tbody = document.getElementById(`${type}LeaderboardBody`);
    if (!tbody || !rankings) return;
    
    tbody.innerHTML = '';
    
    if (rankings.length === 0) {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td colspan="5" class="px-4 py-8 text-center text-gray-500">
                <i class="fas fa-users text-3xl mb-2 block"></i>
                No players found
            </td>
        `;
        tbody.appendChild(row);
        return;
    }
    
    rankings.forEach((player, index) => {
        const row = document.createElement('tr');
        row.className = 'leaderboard-row hover:bg-gray-50 transition-colors duration-150';
        
        const primaryStat = formatMoney(player.money);
        
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
                <span class="text-lg font-bold text-farm-yellow">${primaryStat}</span>
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
function showPlayerDetails(player) {
    const modal = document.getElementById('playerDetailModal');
    const content = document.getElementById('playerDetailContent');
    
    content.innerHTML = `
        <div class="flex items-center mb-4">
            <div class="w-16 h-16 bg-gradient-to-br from-farm-green to-farm-blue rounded-full flex items-center justify-center text-white font-bold text-xl mr-4">
                ${player.username.charAt(0).toUpperCase()}
            </div>
            <div>
                <h4 class="text-xl font-bold text-gray-900">${player.username}</h4>
                <p class="text-gray-600">${player.displayName}</p>
                <span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${player.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'} mt-1">
                    ${player.isActive ? 'Online' : 'Offline'}
                </span>
            </div>
        </div>
        
        <div class="grid grid-cols-1 gap-4">
            <div class="bg-farm-yellow bg-opacity-10 rounded-lg p-4">
                <div class="flex items-center justify-between">
                    <div class="flex items-center">
                        <i class="fas fa-trophy text-farm-yellow text-xl mr-3"></i>
                        <span class="text-gray-700 font-medium">Rank</span>
                    </div>
                    <span class="text-xl font-bold text-gray-900">#${player.rank}</span>
                </div>
            </div>
            
            <div class="bg-green-50 rounded-lg p-4">
                <div class="flex items-center justify-between">
                    <div class="flex items-center">
                        <i class="fas fa-coins text-farm-yellow text-xl mr-3"></i>
                        <span class="text-gray-700 font-medium">Money</span>
                    </div>
                    <span class="text-xl font-bold text-green-600">${formatMoney(player.money)}</span>
                </div>
            </div>
            
            <div class="bg-blue-50 rounded-lg p-4">
                <div class="flex items-center justify-between">
                    <div class="flex items-center">
                        <i class="fas fa-clock text-farm-blue text-xl mr-3"></i>
                        <span class="text-gray-700 font-medium">Last Active</span>
                    </div>
                    <span class="text-sm text-gray-600">${formatDate(player.lastSaved)}</span>
                </div>
            </div>
        </div>
    `;
    
    modal.classList.remove('hidden');
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
    if (tbody) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="px-4 py-8 text-center text-red-500">
                    <i class="fas fa-exclamation-triangle text-3xl mb-2 block"></i>
                    Error: ${message}
                </td>
            </tr>
        `;
    }
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

function formatMoney(amount) {
    return new Intl.NumberFormat('en-US').format(amount) + ' 💰';
}

function formatDate(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffHours / 24);
    
    if (diffHours < 1) {
        return 'Just now';
    } else if (diffHours < 24) {
        return `${diffHours}h ago`;
    } else if (diffDays < 7) {
        return `${diffDays}d ago`;
    } else {
        return date.toLocaleDateString('en-US', { 
            month: 'short', 
            day: 'numeric',
            year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
        });
    }
} 